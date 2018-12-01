﻿using System;
using System.Text;
using System.Threading.Tasks;
using Plato.Internal.Data.Abstractions;
using Plato.Internal.Stores.Abstractions;
using Plato.Notifications.Models;

namespace Plato.Notifications.Stores
{

    #region "UserNotificationsQuery"

    public class UserNotificationsQuery : DefaultQuery<UserNotification>
    {

        private readonly IStore<UserNotification> _store;

        public UserNotificationsQuery(IStore<UserNotification> store)
        {
            _store = store;
        }

        public UserNotificationsQueryParams Params { get; set; }

        public override IQuery<UserNotification> Select<T>(Action<T> configure)
        {
            var defaultParams = new T();
            configure(defaultParams);
            Params = (UserNotificationsQueryParams)Convert.ChangeType(defaultParams, typeof(UserNotificationsQueryParams));
            return this;
        }

        public override async Task<IPagedResults<UserNotification>> ToList()
        {

            var builder = new UserNotificationsQueryBuilder(this);
            var populateSql = builder.BuildSqlPopulate();
            var countSql = builder.BuildSqlCount();

            var data = await _store.SelectAsync(
                PageIndex,
                PageSize,
                populateSql,
                countSql,
                Params.NotificationName.Value
            );

            return data;
        }

    }

    #endregion

    #region "UserNotificationsQueryParams"

    public class UserNotificationsQueryParams
    {

        private WhereInt _userId;
        private WhereString _notificationName;

        public WhereInt UserId
        {
            get => _userId ?? (_userId = new WhereInt());
            set => _userId = value;
        }
        
        public WhereString NotificationName
        {
            get => _notificationName ?? (_notificationName = new WhereString());
            set => _notificationName = value;
        }

    }

    #endregion

    #region "UserNotificationsQueryBuilder"

    public class UserNotificationsQueryBuilder : IQueryBuilder
    {
        #region "Constructor"

        private readonly string _userNotificationsTableName;
        private readonly string _usersTableName;

        private readonly UserNotificationsQuery _query;

        public UserNotificationsQueryBuilder(UserNotificationsQuery query)
        {
            _query = query;
            _userNotificationsTableName = GetTableNameWithPrefix("UserNotifications");
            _usersTableName = GetTableNameWithPrefix("Users");
        }

        #endregion

        #region "Implementation"

        public string BuildSqlPopulate()
        {
            var whereClause = BuildWhereClause();
            var orderBy = BuildOrderBy();
            var sb = new StringBuilder();
            sb.Append("SELECT ")
                .Append(BuildPopulateSelect())
                .Append(" FROM ")
                .Append(BuildTables());
            if (!string.IsNullOrEmpty(whereClause))
                sb.Append(" WHERE (").Append(whereClause).Append(")");
            sb.Append(" ORDER BY ")
                .Append(!string.IsNullOrEmpty(orderBy)
                    ? orderBy
                    : "Id ASC");
            sb.Append(" OFFSET @RowIndex ROWS FETCH NEXT @PageSize ROWS ONLY;");
            return sb.ToString();
        }

        public string BuildSqlCount()
        {
            var whereClause = BuildWhereClause();
            var sb = new StringBuilder();
            sb.Append("SELECT COUNT(un.Id) FROM ")
                .Append(BuildTables());
            if (!string.IsNullOrEmpty(whereClause))
                sb.Append(" WHERE (").Append(whereClause).Append(")");
            return sb.ToString();
        }

        string BuildPopulateSelect()
        {
            var sb = new StringBuilder();
            sb.Append("un.*, ")
                .Append("u.UserName, ")
                .Append("u.NormalizedUserName, ")
                .Append("u.DisplayName, ")
                .Append("u.FirstName, ")
                .Append("u.LastName, ")
                .Append("u.Alias, ")
                .Append("c.UserName AS CreatedUserName, ")
                .Append("c.NormalizedUserName AS CreatedNormalizedUserName, ")
                .Append("c.DisplayName AS CreatedDisplayName, ")
                .Append("c.FirstName AS CreatedFirstName, ")
                .Append("c.LastName AS CreatedLastName, ")
                .Append("c.Alias AS CreatedAlias");
            return sb.ToString();

        }

        string BuildTables()
        {

            var sb = new StringBuilder();
            sb.Append(_userNotificationsTableName)
                .Append(" un WITH (nolock) LEFT OUTER JOIN ")
                .Append(_usersTableName)
                .Append(" u ON un.UserId = u.Id LEFT OUTER JOIN ")
                .Append(_usersTableName)
                .Append(" c ON un.CreatedUserId = c.Id");

            return sb.ToString();

        }

        #endregion

        #region "Private Methods"

        private string GetTableNameWithPrefix(string tableName)
        {
            return !string.IsNullOrEmpty(_query.Options.TablePrefix)
                ? _query.Options.TablePrefix + tableName
                : tableName;
        }

        private string BuildWhereClause()
        {
            var sb = new StringBuilder();

            // UserId
            if (_query.Params.UserId.Value > 0)
            {
                if (!string.IsNullOrEmpty(sb.ToString()))
                    sb.Append(_query.Params.UserId.Operator);
                sb.Append(_query.Params.UserId.ToSqlString("un.UserId"));
            }
            
            if (!string.IsNullOrEmpty(_query.Params.NotificationName.Value))
            {
                if (!string.IsNullOrEmpty(sb.ToString()))
                    sb.Append(_query.Params.NotificationName.Operator);
                sb.Append(_query.Params.NotificationName.ToSqlString("un.NotificationName", "Keywords"));
            }

            return sb.ToString();

        }

        string GetQualifiedColumnName(string columnName)
        {
            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            return columnName.IndexOf('.') >= 0
                ? columnName
                : "un." + columnName;
        }

        private string BuildOrderBy()
        {
            if (_query.SortColumns.Count == 0) return null;
            var sb = new StringBuilder();
            var i = 0;
            foreach (var sortColumn in _query.SortColumns)
            {
                sb.Append(GetQualifiedColumnName(sortColumn.Key));
                if (sortColumn.Value != OrderBy.Asc)
                    sb.Append(" DESC");
                if (i < _query.SortColumns.Count - 1)
                    sb.Append(", ");
                i += 1;
            }
            return sb.ToString();
        }

        #endregion

    }

    #endregion

}