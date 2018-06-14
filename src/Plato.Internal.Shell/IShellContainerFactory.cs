﻿using System;
using Plato.Internal.Models.Shell;
using Plato.Internal.Shell.Abstractions.Models;

namespace Plato.Internal.Shell
{
    public interface IShellContainerFactory
    {
        IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint);
    }
}
