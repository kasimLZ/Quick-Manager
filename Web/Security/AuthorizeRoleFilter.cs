using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Security
{
    public class AuthorizeRoleFilter : AuthorizeFilter
    {
        private static AuthorizationPolicy _policy_ = new AuthorizationPolicy(new[] { new DenyAnonymousAuthorizationRequirement() }, new string[] { });

        public AuthorizeRoleFilter() : base(_policy_) { }

        public AuthorizeRoleFilter(AuthorizationPolicy policy) : base(policy) { }

        public override async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            await base.OnAuthorizationAsync(context);
            if (context.Filters.Any(item => item is IAllowAnonymousFilter)){
                
            }
            else
            {
                Console.WriteLine("没有权限");
            }
        }
    }
}
