﻿using System;
using System.Collections.Generic;
using Plato.Internal.Data.Abstractions;
using Plato.Internal.Data.Schemas.Abstractions;
using Plato.Internal.Data.Schemas.Abstractions.Builders;
using Plato.Internal.Data.Schemas.Builders;
using Plato.Internal.Text.Abstractions;

namespace Plato.Internal.Data.Schemas
{
    
    public class SchemaBuilder : ISchemaBuilder
    {

        public ICollection<string> Statements
        {
            get
            {

                var statements = new List<string>();

                foreach (var statement in TableBuilder.Statements)
                {
                    statements.Add(statement);
                }

                foreach (var statement in ProcedureBuilder.Statements)
                {
                    statements.Add(statement);
                }

                foreach (var statement in FullTextBuilder.Statements)
                {
                    statements.Add(statement);
                }

                foreach (var statement in IndexBuilder.Statements)
                {
                    statements.Add(statement);
                }

                return statements;

            }
        }

        public ITableBuilder TableBuilder { get; }

        public IProcedureBuilder ProcedureBuilder { get; }

        public IFullTextBuilder FullTextBuilder { get; }

        public IIndexBuilder IndexBuilder { get; }

        public SchemaBuilder(IDbContext dbContext, IPluralize pluralize) 
        {
            ProcedureBuilder = new ProcedureBuilder(dbContext, pluralize);
            TableBuilder = new TableBuilder(dbContext, pluralize);
            FullTextBuilder = new FullTextBuilder(dbContext, pluralize);
            IndexBuilder = new IndexBuilder(dbContext, pluralize);
        }
        
        public ISchemaBuilderBase Configure(Action<SchemaBuilderOptions> configure)
        {
            TableBuilder.Configure(configure);
            ProcedureBuilder.Configure(configure);
            FullTextBuilder.Configure(configure);
            IndexBuilder.Configure(configure);
            return this;
        }
        
        public void Dispose()
        {
            TableBuilder.Dispose();
            ProcedureBuilder.Dispose();
            FullTextBuilder.Dispose();
            IndexBuilder.Dispose();
        }

    }
    
}
