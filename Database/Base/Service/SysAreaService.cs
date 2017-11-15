using Database.Base.Interface;
using Database.Base.Interface.Infrastructure;
using Database.Base.Model;
using Database.Base.Service.Infrastructure;

namespace DataService.EntityFramework.Base.Service
{
    public class SysAreaService : RepositoryBase<SysArea>, SysAreaInterface
    {
        public SysAreaService(IDatabaseFactory databaseFactory, CurrentUserInterface userInfo) 
            : base(databaseFactory, userInfo)
        {
        }
    }
}
