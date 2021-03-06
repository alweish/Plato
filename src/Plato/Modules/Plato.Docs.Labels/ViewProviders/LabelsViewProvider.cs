﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Plato.Internal.Features.Abstractions;
using Plato.Internal.Layout.ViewProviders;
using Plato.Internal.Data.Abstractions;
using Plato.Labels.Stores;
using Plato.Labels.ViewModels;
using Plato.Entities.ViewModels;
using Plato.Docs.Labels.Models;
using Plato.Docs.Models;

namespace Plato.Docs.Labels.ViewProviders
{

    public class LabelViewProvider : BaseViewProvider<Label>
    {
        
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ILabelStore<Label> _labelStore;
        private readonly IFeatureFacade _featureFacade;

        public LabelViewProvider(
            IActionContextAccessor actionContextAccessor,
            ILabelStore<Label> labelStore,
            IFeatureFacade featureFacade)
        {
            _actionContextAccessor = actionContextAccessor;
            _featureFacade = featureFacade;
            _labelStore = labelStore;
        }
        
        public override Task<IViewProviderResult> BuildIndexAsync(Label label, IViewProviderContext context)
        {

            // Get index view model from context
            var viewModel = context.Controller.HttpContext.Items[typeof(LabelIndexViewModel<Label>)] as LabelIndexViewModel<Label>;
            if (viewModel == null)
            {
                throw new Exception($"A view model of type {typeof(LabelIndexViewModel<Label>).ToString()} has not been registered on the HttpContext!");
            }

            return Task.FromResult(Views(
                View<LabelIndexViewModel<Label>>("Home.Index.Header", model => viewModel).Zone("header").Order(1),
                View<LabelIndexViewModel<Label>>("Home.Index.Tools", model => viewModel).Zone("tools").Order(1),
                View<LabelIndexViewModel<Label>>("Home.Index.Content", model => viewModel).Zone("content").Order(1)
            ));

        }

        public override async Task<IViewProviderResult> BuildDisplayAsync(Label label, IViewProviderContext context)
        {

            // Get topic index view model from context
            var viewModel = _actionContextAccessor.ActionContext.HttpContext.Items[typeof(EntityIndexViewModel<Doc>)] as EntityIndexViewModel<Doc>;
            if (viewModel == null)
            {
                throw new Exception($"A view model of type {typeof(EntityIndexViewModel<Doc>).ToString()} has not been registered on the HttpContext!");
            }
            
            var indexViewModel = new EntityIndexViewModel<Doc>
            {
                Options = viewModel?.Options,
                Pager = viewModel?.Pager
            };
            
            // Get labels for feature
            var labels = await _labelStore.QueryAsync()
                .Take(1, 10)
                .Select<LabelQueryParams>(async q =>
                {
                    q.FeatureId.Equals(await GetFeatureIdAsync());
                })
                .OrderBy("Entities", OrderBy.Desc)
                .ToList();

            return Views(
                View<Label>("Home.Display.Header", model => label).Zone("header").Order(1),
                View<Label>("Home.Display.Tools", model => label).Zone("tools").Order(1),
                View<EntityIndexViewModel<Doc>>("Home.Display.Content", model => indexViewModel).Zone("content").Order(1),
                View<LabelsViewModel<Label>>("Doc.Labels.Index.Sidebar", model =>
                {
                    model.SelectedLabelId = label?.Id ?? 0;
                    model.Labels = labels?.Data;
                    return model;
                }).Zone("sidebar").Order(1)
            );
            
        }

        public override Task<IViewProviderResult> BuildEditAsync(Label model, IViewProviderContext context)
        {
            return Task.FromResult(default(IViewProviderResult));
        }

        public override Task<IViewProviderResult> BuildUpdateAsync(Label model, IViewProviderContext context)
        {
            return Task.FromResult(default(IViewProviderResult));
        }

        async Task<int> GetFeatureIdAsync()
        {
            var feature = await _featureFacade.GetFeatureByIdAsync("Plato.Docs");
            if (feature != null)
            {
                return feature.Id;
            }

            throw new Exception($"Could not find required feature registration for Plato.Docs");
        }
        
    }

}
