﻿using Plato.Internal.Repositories;

namespace Plato.Email.Repositories
{
    public interface IEmailRepository<T> : IRepository<T> where T : class
    {

    }

}
