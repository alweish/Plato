﻿using System.Threading.Tasks;
using Plato.Internal.Repositories;

namespace Plato.Mentions.Repositories
{

    public interface IEntityMentionsRepository<T> : IRepository<T> where T : class
    {

        Task<bool> DeleteByEntityIdAsync(int entityId);

        Task<bool> DeleteByEntityReplyIdAsync(int entityReplyId);

    }

}
