﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Plato.Entities.Models;
using Plato.Entities.Stores;
using Plato.Entities.ViewModels;
using Plato.Internal.Abstractions.Extensions;
using Plato.Internal.Data.Abstractions;
using Plato.Internal.Hosting.Abstractions;
using Plato.Internal.Navigation.Abstractions;
using Plato.Search.Models;
using Plato.Search.Services;
using Plato.Search.Stores;
using Plato.Search.ViewModels;
using Plato.WebApi.Controllers;
using Plato.WebApi.Models;

namespace Plato.Search.Controllers
{

    public class SearchController : BaseWebApiController
    {

        private readonly IEntityStore<Entity> _entityStore;
        private readonly IContextFacade _contextFacade;
        private readonly ISearchSettingsStore<SearchSettings> _searchSettingsStore;

        private readonly ISearchService _searchService;

        public SearchController(
            IUrlHelperFactory urlHelperFactory,
            IContextFacade contextFacade,
            IEntityStore<Entity> entityStore,
            ISearchSettingsStore<SearchSettings> searchSettingsStore,
            ISearchService searchService)
        {
            _contextFacade = contextFacade;
            _entityStore = entityStore;
            _searchSettingsStore = searchSettingsStore;
            _searchService = searchService;
        }
        
        [HttpGet, ResponseCache(NoStore = true)]
        public async Task<IActionResult> Get(
            int page = 1,
            int size = 10,
            string keywords = "",
            SortBy sort = SortBy.LastReply,
            OrderBy order = OrderBy.Desc)
        {

            // Get results
            var entities = await _searchService.GetResultsAsync(new EntityIndexOptions()
            {
                Search = keywords,
                Sort = sort,
                Order = order
            }, new PagerOptions()
            {
                Page = page,
                PageSize = size
            });
            
            IPagedResults<SearchApiResult> results = null;
            if (entities != null)
            {
                results = new PagedResults<SearchApiResult>
                {
                    Total = entities.Total
                };

                var baseUrl = await _contextFacade.GetBaseUrlAsync();
                foreach (var entity in entities.Data)
                {

                    var url = baseUrl + _contextFacade.GetRouteUrl(new RouteValueDictionary()
                    {
                        ["Area"] = "Plato.Discuss",
                        ["Controller"] = "Home",
                        ["Action"] = "Topic",
                        ["Id"] = entity.Id,
                        ["Alias"] = entity.Alias
                    });

                    results.Data.Add(new SearchApiResult()
                    {
                        Id = entity.Id,
                        CreatedBy = new UserApiResult()
                        {
                            Id = entity.CreatedBy.Id,
                            DisplayName = entity.CreatedBy.DisplayName,
                            UserName = entity.CreatedBy.UserName,
                            Avatar = entity.CreatedBy.Avatar,
                            Url = baseUrl + _contextFacade.GetRouteUrl(new RouteValueDictionary()
                            {
                                ["Area"] = "Plato.Users",
                                ["Controller"] = "Home",
                                ["Action"] = "Display",
                                ["Id"] = entity.CreatedBy.Id,
                                ["Alias"] = entity.CreatedBy.Alias
                            })
                        },
                        ModifiedBy = new UserApiResult()
                        {
                            Id = entity.ModifiedBy.Id,
                            DisplayName = entity.ModifiedBy.DisplayName,
                            UserName = entity.ModifiedBy.UserName,
                            Avatar = entity.ModifiedBy.Avatar,
                            Url = baseUrl + _contextFacade.GetRouteUrl(new RouteValueDictionary()
                            {
                                ["Area"] = "Plato.Users",
                                ["Controller"] = "Home",
                                ["Action"] = "Display",
                                ["Id"] = entity.ModifiedBy.Id,
                                ["Alias"] = entity.ModifiedBy.Alias
                            })
                        },
                        LastReplyBy = new UserApiResult()
                        {
                            Id = entity.LastReplyBy.Id,
                            DisplayName = entity.LastReplyBy.DisplayName,
                            UserName = entity.LastReplyBy.UserName,
                            Avatar = entity.LastReplyBy.Avatar,
                            Url = baseUrl + _contextFacade.GetRouteUrl(new RouteValueDictionary()
                            {
                                ["Area"] = "Plato.Users",
                                ["Controller"] = "Home",
                                ["Action"] = "Display",
                                ["Id"] = entity.LastReplyBy.Id,
                                ["Alias"] = entity.LastReplyBy.Alias
                            })
                        },
                        Title = entity.Title,
                        Excerpt = entity.Abstract,
                        Url = url,
                        CreatedDate = new FriendlyDate()
                        {
                            Text = entity.CreatedDate.ToPrettyDate(),
                            Value = entity.CreatedDate
                        },
                        ModifiedDate = new FriendlyDate()
                        {
                            Text = entity.ModifiedDate.ToPrettyDate(),
                            Value = entity.ModifiedDate
                        },
                        LastReplyDate = new FriendlyDate()
                        {
                            Text = entity.LastReplyDate.ToPrettyDate(),
                            Value = entity.LastReplyDate
                        },
                        Relevance = entity.Relevance
                    });

                }

            }

            IPagedApiResults<SearchApiResult> output = null;
            if (results != null)
            {
                output = new PagedApiResults<SearchApiResult>()
                {
                    Page = page,
                    Size = size,
                    Total = results.Total,
                    TotalPages = results.Total.ToSafeCeilingDivision(size),
                    Data = results.Data
                };
            }

            return output != null
                ? base.Result(output)
                : base.NoResults();

        }
      
    }

}
