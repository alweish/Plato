﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Plato.Internal.Hosting;
using Plato.Internal.Navigation;

namespace Plato.Features
{
    public class Startup : StartupBase
    {
    
        public override void ConfigureServices(IServiceCollection services)
        {
            // register navigation provider
            services.AddScoped<INavigationProvider, AdminMenu>();

        }

        public override void Configure(
            IApplicationBuilder app,
            IRouteBuilder routes,
            IServiceProvider serviceProvider)
        {

            //routes.MapAreaRoute(
            //    name: "AdminFeatures",
            //    areaName: "Plato.Features",
            //    template: "admin/features/{action}/{id?}",
            //    defaults: new { controller = "Admin", action = "Index" }
            //);

            //routes.MapAreaRoute(
            //    name: "AdminEnableFeatures",
            //    areaName: "Plato.Features",
            //    template: "admin/features/{action}/{id?}",
            //    defaults: new { controller = "Admin", action = "Enable" }
            //);

        }
    }
}