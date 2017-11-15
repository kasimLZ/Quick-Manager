using Database.Base.Interface.Infrastructure;
using Database.Base.Model;
using System.Collections.Generic;

namespace Database.Base.Interface
{
    public interface SysControllerInterface : IRepository<SysController>
    {
        IEnumerable<SysController> GetBreadcrumbActions(string controller, string action);
    }
}
