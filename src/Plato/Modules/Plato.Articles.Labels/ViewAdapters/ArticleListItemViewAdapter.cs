﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Plato.Articles.Models;
using Plato.Entities.Services;
using Plato.Entities.ViewModels;
using Plato.Internal.Data.Abstractions;
using Plato.Internal.Features.Abstractions;
using Plato.Internal.Layout.ViewAdapters;
using Plato.Labels.Models;
using Plato.Labels.Stores;
using Label = Plato.Articles.Labels.Models.Label;

namespace Plato.Articles.Labels.ViewAdapters
{

    public class ArticleListItemViewAdapter : BaseAdapterProvider
    {
           
        private readonly IEntityLabelStore<EntityLabel> _entityLabelStore;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IEntityService<Article> _entityService;
        private readonly ILabelStore<Label> _labelStore;
        private readonly IFeatureFacade _featureFacade;

        public ArticleListItemViewAdapter(
            IEntityLabelStore<EntityLabel> entityLabelStore,
            IActionContextAccessor actionContextAccessor,
            IEntityService<Article> entityService,
            ILabelStore<Label> labelStore,
            IFeatureFacade featureFacade)
        {
            _actionContextAccessor = actionContextAccessor;
            _entityLabelStore = entityLabelStore;
            _entityService = entityService;
            _featureFacade = featureFacade;
            _labelStore = labelStore;
            ViewName = "ArticleListItem";
        }

        private IDictionary<int, IList<Label>> _lookUpTable;

        public override async Task<IViewAdapterResult> ConfigureAsync(string viewName)
        {
            
            if (!viewName.Equals(ViewName, StringComparison.OrdinalIgnoreCase))
            {
                return default(IViewAdapterResult);
            }

            if (_lookUpTable == null)
            {

                // Get feature
                var feature = await _featureFacade.GetFeatureByIdAsync("Plato.Articles");
                if (feature == null)
                {
                    // Feature not found
                    return default(IViewAdapterResult);
                }

                // Get all labels for feature
                var labels = await _labelStore.GetByFeatureIdAsync(feature.Id);
                if (labels == null)
                {
                    // No labels available to adapt the view 
                    return default(IViewAdapterResult);
                }

                // Build a dictionary we can use below within our AdaptModel
                // method to add the correct labels for each displayed entity
                _lookUpTable = await BuildLookUpTable(labels.ToList());

            }

            // Plato.Articles does not have a dependency on Plato.Articles.Labels
            // Instead we update the model for the entity list item view component
            // here via our view adapter to include the label data for the entity
            // This way the label data is only ever populated if the labels feature is enabled
            return await Adapt(ViewName, v =>
            {
                v.AdaptModel<EntityListItemViewModel<Article>>(model  =>
                {
                    if (model.Entity == null)
                    {
                        // Return an anonymous type as we are adapting a view component
                        return new
                        {
                            model
                        };
                    }

                    // No need to modify if we don't have a lookup table
                    if (_lookUpTable == null)
                    {
                        // Return an anonymous type as we are adapting a view component
                        return new
                        {
                            model
                        };
                    }

                    // No need to modify the model if no labels have been found
                    if (!_lookUpTable.ContainsKey(model.Entity.Id))
                    {
                        // Return an anonymous type as we are adapting a view component
                        return new
                        {
                            model
                        };
                    }

                    // Get labels for entity
                    var entityLabels = _lookUpTable[model.Entity.Id];

                    // Add labels to the model from our dictionary
                    var modelLabels = new List<Label>();
                    foreach (var label in entityLabels)
                    {
                        modelLabels.Add(label);
                    }

                    model.Labels = modelLabels;

                    // Return an anonymous type as we are adapting a view component
                    return new
                    {
                        model
                    };

                });
            });

        }
        
        async Task<IDictionary<int, IList<Label>>> BuildLookUpTable(IEnumerable<Label> labels)
        {
            
            // Get topic index view model from context
            var viewModel = _actionContextAccessor.ActionContext.HttpContext.Items[typeof(EntityIndexViewModel<Article>)] as EntityIndexViewModel<Article>;
            if (viewModel == null)
            {
                return null;
            }

            // Get all entities for our current view
            var entities = await _entityService.GetResultsAsync(
                viewModel?.Options, 
                viewModel?.Pager);

            // Get all entity label relationships for displayed entities
            IPagedResults<EntityLabel> entityLabels = null;
            if (entities?.Data != null)
            {
                entityLabels = await _entityLabelStore.QueryAsync()
                    .Select<EntityLabelQueryParams>(q =>
                    {
                        q.EntityId.IsIn(entities.Data.Select(e => e.Id).ToArray());
                    })
                    .ToList();
            }

            // Build a dictionary of entity and label relationships
            var output = new ConcurrentDictionary<int, IList<Label>>();
            if (entityLabels?.Data != null)
            {
                var labelList = labels.ToList();
                foreach (var entityLabel in entityLabels.Data)
                {
                    var label = labelList.FirstOrDefault(l => l.Id == entityLabel.LabelId);
                    if (label != null)
                    {
                        output.AddOrUpdate(entityLabel.EntityId, new List<Label>()
                        {
                            label
                        }, (k, v) =>
                        {
                            v.Add(label);
                            return v;
                        });
                    }
                }
            }

            return output;

        }
        
    }
    
}
