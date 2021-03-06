﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Plato.Internal.Models.Schema;

namespace Plato.Internal.Stores.Abstractions.Schema
{
    public interface IConstraintStore
    {

        Task<IEnumerable<DbConstraint>> SelectConstraintsAsync();

    }

}
