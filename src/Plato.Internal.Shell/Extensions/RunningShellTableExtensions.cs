﻿using System;
using Microsoft.AspNetCore.Http;
using Plato.Internal.Models.Shell;
using Plato.Internal.Shell.Abstractions.Models;

namespace Plato.Internal.Shell.Extensions
{
    public static class RunningShellTableExtensions
    {

        public static ShellSettings Match(
        this IRunningShellTable table,
        HttpContext httpContext)
        {
            // use Host header to prevent proxy alteration of the orignal request
            try
            {
                var httpRequest = httpContext.Request;
                if (httpRequest == null)
                {
                    return null;
                }

                var host = httpRequest.Headers["Host"].ToString();

                return table.Match(host ?? string.Empty, httpRequest.Path);
            }
            catch (Exception)
            {
                // can happen on cloud service for an unknown reason
                return null;
            }
        }

    }
}
