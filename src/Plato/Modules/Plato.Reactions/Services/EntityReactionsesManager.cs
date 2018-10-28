﻿using System;
using System.Threading.Tasks;
using Plato.Internal.Abstractions;
using Plato.Internal.Hosting.Abstractions;
using Plato.Reactions.Models;
using Plato.Reactions.Stores;

namespace Plato.Reactions.Services
{

    public class EntityReactionsesManager : IEntityReactionsManager<EntityReaction>
    {

        private readonly IContextFacade _contextFacade;
        private readonly IEntityReactionsStore<EntityReaction> _entityReactionsStore;

        public EntityReactionsesManager(
            IEntityReactionsStore<EntityReaction> entityReactionsStore,
            IContextFacade contextFacade)
        {
            _entityReactionsStore = entityReactionsStore;
            _contextFacade = contextFacade;
        }

        public async Task<ICommandResult<EntityReaction>> CreateAsync(EntityReaction model)
        {

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (model.Id > 0)
            {
                throw new ArgumentOutOfRangeException(nameof(model.Id));
            }

            if (model.EntityId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(model.EntityId));
            }

            if (String.IsNullOrEmpty(model.ReactionName))
            {
                throw new ArgumentNullException(nameof(model.ReactionName));
            }

            // Update created by
            var user = await _contextFacade.GetAuthenticatedUserAsync();
            if (model.CreatedUserId == 9)
            {
                model.CreatedUserId = user?.Id ?? 0;
            }

            model.CreatedDate = DateTime.UtcNow;

            // Create result
            var result = new CommandResult<EntityReaction>();

            // Attempt to persist
            var reaction = await _entityReactionsStore.CreateAsync(model);
            if (reaction != null)
            {
                return result.Success(reaction);
            }

            return result.Failed($"An unknown error occurred whilst attempting to create a reaction");

        }

        public async Task<ICommandResult<EntityReaction>> UpdateAsync(EntityReaction model)
        {

            // Validate
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (model.Id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(model.Id));
            }

            if (model.EntityId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(model.EntityId));
            }

            if (String.IsNullOrEmpty(model.ReactionName))
            {
                throw new ArgumentNullException(nameof(model.ReactionName));
            }

            // Update modified 
            var user = await _contextFacade.GetAuthenticatedUserAsync();

            // Create result
            var result = new CommandResult<EntityReaction>();

            // Attempt to persist
            var reaction = await _entityReactionsStore.UpdateAsync(model);
            if (reaction != null)
            {
                return result.Success(reaction);
            }

            return result.Failed($"An unknown error occurred whilst attempting to update a reaction");

        }

        public async Task<ICommandResult<EntityReaction>> DeleteAsync(EntityReaction model)
        {

            // Validate
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var result = new CommandResult<EntityReaction>();

            var success = await _entityReactionsStore.DeleteAsync(model);
            if (success)
            {
                return result.Success(model);
            }

            return result.Failed(new CommandError("An unknown error occurred whilst attempting to delete the reoaction."));

        }
    }
}
