using Database.Base.Interface;
using Database.Base.Interface.Infrastructure;
using Database.Base.Service.Infrastructure;
using Database.Interface;
using Database.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Database.Service
{
    public class ArticalService : RepositoryBase<Artical>, ArticalInterface
    {
        public ArticalService(IDatabaseFactory databaseFactory, CurrentUserInterface userInfo) 
            : base(databaseFactory, userInfo)
        {
        }
    }
}
