﻿using System.Threading.Tasks;
using Plato.Discuss.Tags.Models;
using Plato.Follows.Stores;
using Plato.Follows.ViewModels;
using Plato.Internal.Hosting.Abstractions;
using Plato.Internal.Layout.ViewProviders;
using Plato.Tags.Models;
using Plato.Tags.Stores;

namespace Plato.Discuss.Tags.Follow.ViewProviders
{
    public class TagViewProvider : BaseViewProvider<DiscussTag>
    {

        private readonly ITagStore<Tag> _tagStore;
        private readonly IContextFacade _contextFacade;
        private readonly IFollowStore<Follows.Models.Follow> _followStore;

        public TagViewProvider(
            ITagStore<Tag> tagStore, 
            IContextFacade contextFacade,
            IFollowStore<Follows.Models.Follow> followStore)
        {
            _tagStore = tagStore;
            _contextFacade = contextFacade;
            _followStore = followStore;
        }

        public override async Task<IViewProviderResult> BuildDisplayAsync(DiscussTag tag, IViewProviderContext context)
        {

            var existingTag = await _tagStore.GetByIdAsync(tag.Id);
            if (existingTag == null)
            {
                return await BuildIndexAsync(tag, context);
            }

            var followType = FollowTypes.Tag;
            var isFollowing = false;

            var currentUser = await _contextFacade.GetAuthenticatedUserAsync();
            if (currentUser != null)
            {
                var existingFollow = await _followStore.SelectFollowByNameThingIdAndCreatedUserId(
                    followType.Name,
                    existingTag.Id,
                    currentUser.Id);
                if (existingFollow != null)
                {
                    isFollowing = true;
                }
            }
            
            return Views(
                View<FollowViewModel>("Follow.Display.Tools", model =>
                {
                    model.FollowType = followType;
                    model.ThingId = existingTag.Id;
                    model.IsFollowing = isFollowing;
                    return model;
                }).Zone("tools").Order(1)
            );

        }

        public override Task<IViewProviderResult> BuildIndexAsync(DiscussTag model, IViewProviderContext context)
        {
            return Task.FromResult(default(IViewProviderResult));
        }

        public override Task<IViewProviderResult> BuildEditAsync(DiscussTag discussUser, IViewProviderContext context)
        {
            return Task.FromResult(default(IViewProviderResult));
        }

        public override Task<IViewProviderResult> BuildUpdateAsync(DiscussTag model, IViewProviderContext context)
        {
            return Task.FromResult(default(IViewProviderResult));
        }
    }

}
