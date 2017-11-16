using Database.Base.Interface.Infrastructure;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Database.Base.Service.Infrastructure
{
    public class DatabaseFactory : IDatabaseFactory
    {
        // Fields
        private IApplicationDB _dataContext;

        // Methods
        public DatabaseFactory(IApplicationDB applicationDb)
        {
            _dataContext = applicationDb;
        }

        public IApplicationDB DataContext
        {
            get { return _dataContext; }
          
        }
        
    }

}