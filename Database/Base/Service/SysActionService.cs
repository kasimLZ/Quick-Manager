using Database.Base.Interface;
using Database.Base.Model;
using Database.Base.Service.Infrastructure;
using Database.Base.Interface.Infrastructure;

namespace DataService.EntityFramework.Base.Service
{
    public class SysActionService : RepositoryBase<SysAction>, SysActionInterface
    {
        public SysActionService(IDatabaseFactory databaseFactory, CurrentUserInterface userInfo) 
            : base(databaseFactory, userInfo)
        {
        }
        
    }
}
