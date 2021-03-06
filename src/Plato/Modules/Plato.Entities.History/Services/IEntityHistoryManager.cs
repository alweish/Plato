﻿using Plato.Internal.Abstractions;

namespace Plato.Entities.History.Services
{

    public interface IEntityHistoryManager<TModel> : ICommandManager<TModel> where TModel : class
    {
    }

}
