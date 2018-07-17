﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Plato.Categories.Models;
using Plato.Categories.Repositories;
using Plato.Internal.Abstractions;
using Plato.Internal.Cache;
using Plato.Internal.Data.Abstractions;
using Plato.Internal.Modules.Abstractions;

namespace Plato.Categories.Stores
{
    public class CategoryStore : ICategoryStore<Category>
    {

        private readonly ICategoryRepository<Category> _categoryRepository;
        private readonly ICategoryDataStore<CategoryData> _categoryDataStore;
        private readonly ICacheManager _cacheManager;
        private readonly ILogger<CategoryStore> _logger;
        private readonly IDbQueryConfiguration _dbQuery;
        private readonly ITypedModuleProvider _typedModuleProvider;

        public CategoryStore(
            ICategoryRepository<Category> categoryRepository,
            ICacheManager cacheManager,
            ILogger<CategoryStore> logger,
            IDbQueryConfiguration dbQuery,
            ICategoryDataStore<CategoryData> categoryDataStore, ITypedModuleProvider typedModuleProvider)
        {
            _categoryRepository = categoryRepository;
            _cacheManager = cacheManager;
            _logger = logger;
            _dbQuery = dbQuery;
            _categoryDataStore = categoryDataStore;
            _typedModuleProvider = typedModuleProvider;
        }

        #region "Implementation"

        public async Task<Category> CreateAsync(Category model)
        {
            
            // transform meta data
            model.Data = await SerializeMetaDataAsync(model);
            
            var newCategory = await _categoryRepository.InsertUpdateAsync(model);
            if (newCategory != null)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Added new category with id {1}",
                        newCategory.Id);
                }

                _cacheManager.CancelTokens(this.GetType());
            }

            return newCategory;

        }

        public async Task<Category> UpdateAsync(Category model)
        {
            
            // transform meta data
            model.Data = await SerializeMetaDataAsync(model);
            
            var updatedCategory = await _categoryRepository.InsertUpdateAsync(model);
            if (updatedCategory != null)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Updated existing entity with id {1}",
                        updatedCategory.Id);
                }

                _cacheManager.CancelTokens(this.GetType());

            }

            return updatedCategory;

        }

        public async Task<bool> DeleteAsync(Category model)
        {

            var success = await _categoryRepository.DeleteAsync(model.Id);
            if (success)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Deleted category '{0}' with id {1}",
                        model.Name, model.Id);
                }
                _cacheManager.CancelTokens(this.GetType());
            }

            return success;

        }

        public async Task<Category> GetByIdAsync(int id)
        {
            var token = _cacheManager.GetOrCreateToken(this.GetType(), id);
            return await _cacheManager.GetOrCreateAsync(token, async (cacheEntry) =>
            {
                var category = await _categoryRepository.SelectByIdAsync(id);
                return await MergeCategoryData(category);
            });
        }

        public IQuery<Category> QueryAsync()
        {
            var query = new CategoryQuery(this);
            return _dbQuery.ConfigureQuery<Category>(query); ;
        }

        public async Task<IPagedResults<Category>> SelectAsync(params object[] args)
        {
            var token = _cacheManager.GetOrCreateToken(this.GetType(), args);
            return await _cacheManager.GetOrCreateAsync(token, async (cacheEntry) =>
            {

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Selecting categories for key '{0}' with the following parameters: {1}",
                        token.ToString(), args.Select(a => a));
                }

                var results =  await _categoryRepository.SelectAsync(args);
                if (results != null)
                {
                    results.Data = await MergeCategoryData(results.Data);
                }

                return results;

            });
        }
        
        public async Task<IEnumerable<Category>> GetByFeatureIdAsync(int featureId)
        {
            var token = _cacheManager.GetOrCreateToken(this.GetType(), featureId);
            return await _cacheManager.GetOrCreateAsync(token, async (cacheEntry) =>
            {

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Selecting categories for feature with Id '{0}'",
                        featureId);
                }

                var results = await _categoryRepository.SelectByFeatureIdAsync(featureId);
                if (results != null)
                {
                    results = await MergeCategoryData(results.ToList());
                }

                return results;

            });
        }

        #endregion

        #region "Private Methods"

        async Task<IEnumerable<CategoryData>> SerializeMetaDataAsync(Category category)
        {

            // Get all existing entity data
            var data = await _categoryDataStore.GetByCategoryIdAsync(category.Id);

            // Prepare list to search, use dummy list if needed
            var dataList = data?.ToList() ?? new List<CategoryData>();

            // Iterate all meta data on the supplied object,
            // check if a key already exists, if so update existing key 
            var output = new List<CategoryData>();
            foreach (var item in category.MetaData)
            {
                var key = item.Key.FullName;
                var entityData = dataList.FirstOrDefault(d => d.Key == key);
                if (entityData != null)
                {
                    entityData.Value = item.Value.Serialize();
                }
                else
                {
                    entityData = new CategoryData()
                    {
                        Key = key,
                        Value = item.Value.Serialize()
                    };
                }

                output.Add(entityData);
            }

            return output;

        }

        async Task<IList<Category>> MergeCategoryData(IList<Category> categories)
        {

            if (categories == null)
            {
                return null;
            }

            // Get all entity data matching supplied entity ids
            var results = await _categoryDataStore.QueryAsync()
                .Select<CategoryDataQueryParams>(q => { q.CategoryId.IsIn(categories.Select(e => e.Id).ToArray()); })
                .ToList();

            if (results == null)
            {
                return categories;
            }

            // Merge data into entities
            return await MergeCategoryData(categories, results.Data);

        }

        async Task<IList<Category>> MergeCategoryData(IList<Category> categories, IList<CategoryData> data)
        {

            if (categories == null || data == null)
            {
                return categories;
            }

            for (var i = 0; i < categories.Count; i++)
            {
                categories[i].Data = data.Where(d => d.CategoryId == categories[i].Id).ToList();
                categories[i] = await MergeCategoryData(categories[i]);
            }

            return categories;

        }

        async Task<Category> MergeCategoryData(Category category)
        {

            if (category == null)
            {
                return null;
            }

            if (category.Data == null)
            {
                return category;
            }

            foreach (var data in category.Data)
            {
                var type = await GetModuleTypeCandidateAsync(data.Key);
                if (type != null)
                {
                    var obj = JsonConvert.DeserializeObject(data.Value, type);
                    category.AddOrUpdate(type, (ISerializable)obj);
                }
            }

            return category;

        }

        async Task<Type> GetModuleTypeCandidateAsync(string typeName)
        {
            return await _typedModuleProvider.GetTypeCandidateAsync(typeName, typeof(ISerializable));
        }

        #endregion

    }
}