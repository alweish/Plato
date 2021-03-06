﻿using Microsoft.AspNetCore.Hosting;
using Plato.Internal.Hosting.Abstractions;

namespace Plato.Internal.Hosting
{
    public class HostEnvironment : IHostEnvironment
    {

        private readonly IHostingEnvironment _hostingEnvironment;

        protected HostEnvironment(
            IHostingEnvironment hostingEnvrionment)
        {
            _hostingEnvironment = hostingEnvrionment;
        }

        public string MapPath(string virtualPath)
        {
            return _hostingEnvironment.ContentRootPath + 
                virtualPath.Replace("~/", "");
        }

    }
}
