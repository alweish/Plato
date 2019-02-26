﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Plato.Articles.Models;
using Plato.Articles.Services;
using Plato.Articles.ViewModels;
using Plato.Entities.Services;
using Plato.Entities.Stores;
using Plato.Entities.ViewModels;
using Plato.Internal.Navigation;
using Plato.Internal.Navigation.Abstractions;

namespace Plato.Articles.ViewComponents
{

    public class ArticleCommentListViewComponent : ViewComponent
    {

        private readonly IEntityStore<Article> _entityStore;
        private readonly IEntityReplyStore<ArticleComment> _entityReplyStore;

        private readonly IEntityReplyService<ArticleComment> _replyService;

        public ArticleCommentListViewComponent(
            IEntityReplyStore<ArticleComment> entityReplyStore,
            IEntityStore<Article> entityStore,
            IEntityReplyService<ArticleComment> replyService)
        {
            _entityReplyStore = entityReplyStore;
            _entityStore = entityStore;
            _replyService = replyService;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            EntityOptions options,
            PagerOptions pager)
        {

            if (options == null)
            {
                options = new EntityOptions();
            }

            if (pager == null)
            {
                pager = new PagerOptions();
            }
            
            return View(await GetViewModel(options, pager));

        }

        async Task<EntityViewModel<Article, ArticleComment>> GetViewModel(
            EntityOptions options,
            PagerOptions pager)
        {

            var topic = await _entityStore.GetByIdAsync(options.EntityId);
            if (topic == null)
            {
                throw new ArgumentNullException();
            }

            var results = await _replyService.GetRepliesAsync(options, pager);
            
            // Set total on pager
            pager.SetTotal(results?.Total ?? 0);

            // Return view model
            return new EntityViewModel<Article, ArticleComment>
            {
                Options = options,
                Pager = pager,
                Article = topic,
                Replies = results
        };

        }

    }

}
