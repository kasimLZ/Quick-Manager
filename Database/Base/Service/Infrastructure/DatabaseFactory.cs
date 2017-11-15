using Database.Base.Interface.Infrastructure;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Database.Base.Service.Infrastructure
{
    public class DatabaseFactory : IDatabaseFactory
    {
        // Fields
        private IApplicationDb _dataContext;

        // Methods
        public DatabaseFactory(IApplicationDb applicationDb)
        {
            _dataContext = applicationDb;
        }

        public IApplicationDb DataContext
        {
            get { return _dataContext; }
          
        }
        
    }

}