using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Helper
{
    public interface IAreaRegister
    {
        string AreaName { get; }

        void RegisterArea(IRouteBuilder route);
    }
}
