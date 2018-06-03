using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace ActionProviderImplementation
{
    public class EntityFrameworkActionProvider: ActionProvider
    {
        DbContext _dbContext;

        public EntityFrameworkActionProvider(DbContext dbContext) : base(dbContext, new EntityFrameworkParameterMarshaller()) {
            _dbContext = dbContext;
        }
    }
}
