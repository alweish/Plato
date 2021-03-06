﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Plato.Internal.Text.Abstractions;
using Plato.Internal.Text.Abstractions.Diff;
using Plato.Internal.Text.Alias;
using Plato.Internal.Text.Diff;
using Plato.Internal.Text.UriExtractors;

namespace Plato.Internal.Text.Extensions
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddPlatoText(
            this IServiceCollection services)
        {


            services.TryAddSingleton<IAliasCreator, AliasCreator>();
            
            services.TryAddSingleton<IImageUriExtractor, ImageUriExtractor>();
            services.TryAddSingleton<IAnchorUriExtractor, AnchorUriExtractor>();
            services.TryAddSingleton<IKeyGenerator, KeyGenerator>();
            services.TryAddSingleton<IDefaultHtmlEncoder, DefaultHtmlEncoder>();
            services.TryAddSingleton<ITextParser, TextParser>();
            services.TryAddTransient<IPluralize, Pluralize>();

            services.TryAddSingleton<IDiffer, Differ>();
            services.TryAddSingleton<IInlineDiffBuilder, InlineDiffBuilder>();
            services.TryAddSingleton<ISideBySideDiffBuilder, SideBySideDiffBuilder>();
            
            return services;

        }


    }
}
