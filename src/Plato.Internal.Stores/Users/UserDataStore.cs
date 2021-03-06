﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plato.Internal.Cache.Abstractions;
using Plato.Internal.Data.Abstractions;
using Plato.Internal.Models.Users;
using Plato.Internal.Repositories.Users;
using Plato.Internal.Stores.Abstractions.Users;

namespace Plato.Internal.Stores.Users
{

    public class UserDataStore : IUserDataStore<UserData>
    {

        public const string ByUser = "ByUser";

        private readonly IUserDataRepository<UserData> _userDataRepository;
        private readonly ILogger<UserDataStore> _logger;
        private readonly IDbQueryConfiguration _dbQuery;
        private readonly ICacheManager _cacheManager;

        public UserDataStore(
            IUserDataRepository<UserData> userDataRepository,
            ILogger<UserDataStore> logger,
            IDbQueryConfiguration dbQuery,
            ICacheManager cacheManager)
        {
            _userDataRepository = userDataRepository;
            _cacheManager = cacheManager;
            _dbQuery = dbQuery;
            _logger = logger;
        }
        
        public async Task<UserData> CreateAsync(UserData model)
        {
            var result =  await _userDataRepository.InsertUpdateAsync(model);
            if (result != null)
            {
                CancelTokens(result);
            }

            return result;

        }

        public async Task<UserData> UpdateAsync(UserData model)
        {
            var result = await _userDataRepository.InsertUpdateAsync(model);
            if (result != null)
            {
                CancelTokens(result);
            }

            return result;

        }

        public async Task<bool> DeleteAsync(UserData model)
        {
            var success = await _userDataRepository.DeleteAsync(model.Id);
            if (success)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Deleted user data with key '{0}' for user id {1}",
                        model.Key, model.UserId);
                }

                CancelTokens(model);

            }

            return success;
        }

        public async Task<UserData> GetByIdAsync(int id)
        {
            var token = _cacheManager.GetOrCreateToken(this.GetType(), id);
            return await _cacheManager.GetOrCreateAsync(token, async (cacheEntry) => await _userDataRepository.SelectByIdAsync(id));
        }

        public IQuery<UserData> QueryAsync()
        {
            var query = new UserDataQuery(this);
            return _dbQuery.ConfigureQuery<UserData>(query); ;
        }

        public async Task<IPagedResults<UserData>> SelectAsync(IDbDataParameter[] dbParams)
        {
            var token = _cacheManager.GetOrCreateToken(this.GetType(), dbParams.Select(p => p.Value).ToArray());
            return await _cacheManager.GetOrCreateAsync(token, async (cacheEntry) => await _userDataRepository.SelectAsync(dbParams));
        }

        public async Task<UserData> GetByKeyAndUserIdAsync(string key, int userId)
        {
            var token = _cacheManager.GetOrCreateToken(this.GetType(), key, userId);
            return await _cacheManager.GetOrCreateAsync(token, async (cacheEntry) => await _userDataRepository.SelectByKeyAndUserIdAsync(key, userId));
            
        }

        public async Task<IEnumerable<UserData>> GetByUserIdAsync(int userId)
        {
            var token = _cacheManager.GetOrCreateToken(this.GetType(), ByUser, userId);
            return await _cacheManager.GetOrCreateAsync(token, async (cacheEntry) => await _userDataRepository.SelectByUserIdAsync(userId));
        }

        public void CancelTokens(UserData model = null)
        {
            _cacheManager.CancelTokens(this.GetType());
        }

    }

}
