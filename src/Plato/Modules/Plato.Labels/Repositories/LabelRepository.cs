﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plato.Labels.Models;
using Plato.Internal.Abstractions;
using Plato.Internal.Abstractions.Extensions;
using Plato.Internal.Data.Abstractions;

namespace Plato.Labels.Repositories
{

    public class LabelRepository<TLabel> : ILabelRepository<TLabel> where TLabel : class, ILabel
    {

        #region "Constructor"

        private readonly ILabelDataRepository<LabelData> _LabelDataRepository;
        private readonly IDbContext _dbContext;
        private readonly ILogger<LabelRepository<TLabel>> _logger;

        public LabelRepository(
            IDbContext dbContext,
            ILogger<LabelRepository<TLabel>> logger,
            ILabelDataRepository<LabelData> LabelDataRepository)
        {
            _dbContext = dbContext;
            _logger = logger;
            _LabelDataRepository = LabelDataRepository;
        }

        #endregion

        #region "Implementation"

        public async Task<TLabel> InsertUpdateAsync(TLabel model)
        {

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var id = await InsertUpdateInternal(
                model.Id,
                model.ParentId,
                model.FeatureId,
                model.Name,
                model.Description,
                model.Alias,
                model.IconCss,
                model.ForeColor,
                model.BackColor,
                model.SortOrder,
                model.CreatedUserId,
                model.CreatedDate,
                model.ModifiedUserId,
                model.ModifiedDate,
                model.Data);

            if (id > 0)
            {
                // return
                return await SelectByIdAsync(id);
            }

            return null;
        }

        public async Task<TLabel> SelectByIdAsync(int id)
        {

            TLabel Label = null;
            using (var context = _dbContext)
            {
                var reader = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "SelectLabelById", id);
                if ((reader != null) && (reader.HasRows))
                {
                    await reader.ReadAsync();
                    Label = ActivateInstanceOf<TLabel>.Instance();
                    Label.PopulateModel(reader);
                }

            }

            return Label;

        }

        public async Task<IPagedResults<TLabel>> SelectAsync(params object[] inputParams)
        {
            PagedResults<TLabel> output = null;
            using (var context = _dbContext)
            {

                _dbContext.OnException += (sender, args) =>
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.LogInformation($"SelectEntitiesPaged failed with the following error {args.Exception.Message}");
                };

                var reader = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "SelectCategoriesPaged",
                    inputParams
                );

                if ((reader != null) && (reader.HasRows))
                {
                    output = new PagedResults<TLabel>();
                    while (await reader.ReadAsync())
                    {
                        var Label = ActivateInstanceOf<TLabel>.Instance();
                        Label.PopulateModel(reader);
                        output.Data.Add(Label);
                    }

                    if (await reader.NextResultAsync())
                    {
                        await reader.ReadAsync();
                        output.PopulateTotal(reader);
                    }

                }
            }

            return output;
        }

        public async Task<bool> DeleteAsync(int id)
        {

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation($"Deleting Label with id: {id}");
            }

            var success = 0;
            using (var context = _dbContext)
            {
                success = await context.ExecuteScalarAsync<int>(
                    CommandType.StoredProcedure,
                    "DeleteLabelById", id);
            }

            return success > 0 ? true : false;

        }

        public async Task<IEnumerable<TLabel>> SelectByFeatureIdAsync(int featureId)
        {

            List<TLabel> output = null;
            using (var context = _dbContext)
            {

                _dbContext.OnException += (sender, args) =>
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.LogInformation($"SelectEntitiesPaged failed with the following error {args.Exception.Message}");
                };

                var reader = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "SelectCategoriesByFeatureId",
                    featureId
                );

                if ((reader != null) && (reader.HasRows))
                {
                    output = new List<TLabel>();
                    while (await reader.ReadAsync())
                    {
                        var Label = ActivateInstanceOf<TLabel>.Instance();
                        Label.PopulateModel(reader);
                        output.Add(Label);
                    }

                }
            }

            return output;
        }

        #endregion

        #region "Private Methods"

        async Task<int> InsertUpdateInternal(
            int id,
            int parentId,
            int featureId,
            string name,
            string description,
            string alias,
            string iconCss,
            string foreColor,
            string backColor,
            int sortOrder,
            int createdUserId,
            DateTime? createdDate,
            int modifiedUserId,
            DateTime? modifiedDate,
            IEnumerable<LabelData> data)
        {

            var LabelId = 0;
            using (var context = _dbContext)
            {

                context.OnException += (sender, args) =>
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.LogInformation(
                            id == 0
                                ? $"Insert for Label with name '{name}' failed with the following error '{args.Exception.Message}'"
                                : $"Update for Label with Id {id} failed with the following error {args.Exception.Message}");
                    throw args.Exception;
                };

                LabelId = await context.ExecuteScalarAsync<int>(
                    CommandType.StoredProcedure,
                    "InsertUpdateLabel",
                    id,
                    parentId,
                    featureId,
                    name.ToEmptyIfNull().TrimToSize(255),
                    description.ToEmptyIfNull().TrimToSize(500),
                    alias.ToEmptyIfNull().TrimToSize(255),
                    iconCss.ToEmptyIfNull().TrimToSize(50),
                    foreColor.ToEmptyIfNull().TrimToSize(50),
                    backColor.ToEmptyIfNull().TrimToSize(50),
                    sortOrder,
                    createdUserId,
                    createdDate.ToDateIfNull(),
                    modifiedUserId,
                    modifiedDate.ToDateIfNull());
            }

            // Add Label data
            if (LabelId > 0)
            {
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        item.LabelId = LabelId;
                        await _LabelDataRepository.InsertUpdateAsync(item);
                    }
                }

            }

            return LabelId;

        }

        #endregion

    }


    //public class LabelRepository : ILabelRepository<Label>
    //{
     
    //    #region "Constructor"

    //    private readonly ILabelDataRepository<LabelData> _LabelDataRepository;

    //    private readonly IDbContext _dbContext;
    //    private readonly ILogger<LabelRepository> _logger;
      
    //    public LabelRepository(
    //        IDbContext dbContext,
    //        ILogger<LabelRepository> logger,
    //        ILabelDataRepository<LabelData> LabelDataRepository)
    //    {
    //        _dbContext = dbContext;
    //        _logger = logger;
    //        _LabelDataRepository = LabelDataRepository;
    //    }

    //    #endregion
        
    //    #region "Implementation"

    //    public async Task<Label> InsertUpdateAsync(Label model)
    //    {

    //        if (model == null)
    //            throw new ArgumentNullException(nameof(model));

    //        var id = await InsertUpdateInternal(
    //            model.Id,
    //            model.ParentId,
    //            model.FeatureId,
    //            model.Name,
    //            model.Description,
    //            model.Alias,
    //            model.IconCss,
    //            model.ForeColor,
    //            model.BackColor,
    //            model.SortOrder,
    //            model.CreatedUserId,
    //            model.CreatedDate,
    //            model.ModifiedUserId,
    //            model.ModifiedDate,
    //            model.Data);

    //        if (id > 0)
    //        {
    //            // return
    //            return await SelectByIdAsync(id);
    //        }

    //        return null;
    //    }

    //    public async Task<Label> SelectByIdAsync(int id)
    //    {

    //        Label Label = null;
    //        using (var context = _dbContext)
    //        {
    //            var reader = await context.ExecuteReaderAsync(
    //                CommandType.StoredProcedure,
    //                "SelectLabelById", id);
    //            if ((reader != null) && (reader.HasRows))
    //            {
    //                await reader.ReadAsync();
    //                Label = new Label();
    //                Label.PopulateModel(reader);
    //            }

    //        }

    //        return Label;

    //    }
        
    //    public async Task<IPagedResults<Label>> SelectAsync(params object[] inputParams)
    //    {
    //        PagedResults<Label> output = null;
    //        using (var context = _dbContext)
    //        {

    //            _dbContext.OnException += (sender, args) =>
    //            {
    //                if (_logger.IsEnabled(LogLevel.Error))
    //                    _logger.LogInformation($"SelectEntitiesPaged failed with the following error {args.Exception.Message}");
    //            };

    //            var reader = await context.ExecuteReaderAsync(
    //                CommandType.StoredProcedure,
    //                "SelectCategoriesPaged",
    //                inputParams
    //            );

    //            if ((reader != null) && (reader.HasRows))
    //            {
    //                output = new PagedResults<Label>();
    //                while (await reader.ReadAsync())
    //                {
    //                    var entity = new Label();
    //                    entity.PopulateModel(reader);
    //                    output.Data.Add(entity);
    //                }

    //                if (await reader.NextResultAsync())
    //                {
    //                    await reader.ReadAsync();
    //                    output.PopulateTotal(reader);
    //                }
                    
    //            }
    //        }

    //        return output;
    //    }
        
    //    public async Task<bool> DeleteAsync(int id)
    //    {

    //        if (_logger.IsEnabled(LogLevel.Information))
    //        {
    //            _logger.LogInformation($"Deleting Label with id: {id}");
    //        }

    //        var success = 0;
    //        using (var context = _dbContext)
    //        {
    //            success = await context.ExecuteScalarAsync<int>(
    //                CommandType.StoredProcedure,
    //                "DeleteLabelById", id);
    //        }

    //        return success > 0 ? true : false;

    //    }
        
    //    public async Task<IEnumerable<Label>> SelectByFeatureIdAsync(int featureId)
    //    {

    //        List<Label> output = null;
    //        using (var context = _dbContext)
    //        {

    //            _dbContext.OnException += (sender, args) =>
    //            {
    //                if (_logger.IsEnabled(LogLevel.Error))
    //                    _logger.LogInformation($"SelectEntitiesPaged failed with the following error {args.Exception.Message}");
    //            };

    //            var reader = await context.ExecuteReaderAsync(
    //                CommandType.StoredProcedure,
    //                "SelectCategoriesByFeatureId",
    //                featureId
    //            );

    //            if ((reader != null) && (reader.HasRows))
    //            {
    //                output = new List<Label>();
    //                while (await reader.ReadAsync())
    //                {
    //                    var Label = new Label();
    //                    Label.PopulateModel(reader);
    //                    output.Add(Label);
    //                }
                    
    //            }
    //        }

    //        return output;
    //    }
        
    //    #endregion

    //    #region "Private Methods"

    //    async Task<int> InsertUpdateInternal(
    //        int id,
    //        int parentId,
    //        int featureId,
    //        string name,
    //        string description,
    //        string alias,
    //        string iconCss,
    //        string foreColor,
    //        string backColor,
    //        int sortOrder,
    //        int createdUserId,
    //        DateTime? createdDate,
    //        int modifiedUserId,
    //        DateTime? modifiedDate,
    //        IEnumerable<LabelData> data)
    //    {

    //        var LabelId = 0;
    //        using (var context = _dbContext)
    //        {

    //            context.OnException += (sender, args) =>
    //            {
    //                if (_logger.IsEnabled(LogLevel.Error))
    //                    _logger.LogInformation(
    //                        id == 0
    //                            ? $"Insert for Label with name '{name}' failed with the following error '{args.Exception.Message}'"
    //                            : $"Update for Label with Id {id} failed with the following error {args.Exception.Message}");
    //                throw args.Exception;
    //            };

    //            LabelId = await context.ExecuteScalarAsync<int>(
    //                CommandType.StoredProcedure,
    //                "InsertUpdateLabel",
    //                id,
    //                parentId,
    //                featureId,
    //                name.ToEmptyIfNull().TrimToSize(255),
    //                description.ToEmptyIfNull().TrimToSize(500),
    //                alias.ToEmptyIfNull().TrimToSize(255),
    //                iconCss.ToEmptyIfNull().TrimToSize(50),
    //                foreColor.ToEmptyIfNull().TrimToSize(50),
    //                backColor.ToEmptyIfNull().TrimToSize(50),
    //                sortOrder,
    //                createdUserId,
    //                createdDate.ToDateIfNull(),
    //                modifiedUserId,
    //                modifiedDate.ToDateIfNull());
    //        }

    //        // Add Label data
    //        if (LabelId > 0)
    //        {
    //            if (data != null)
    //            {
    //                foreach (var item in data)
    //                {
    //                    item.LabelId = LabelId;
    //                    await _LabelDataRepository.InsertUpdateAsync(item);
    //                }
    //            }

    //        }

    //        return LabelId;

    //    }

    //    #endregion

    //}

}