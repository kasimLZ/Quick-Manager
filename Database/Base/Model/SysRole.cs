using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Database.Base.Model
{
    public class SysRole : DbSetBase
    {
        // Methods
        public SysRole()
        {
            SystemId = "000";
        }
        
        [MaxLength(50), Required(ErrorMessage = "角色名称是必填项")]
        public string RoleName { get; set; }

        public virtual ICollection<SysRoleSysControllerSysAction> SysRoleSysControllerSysActions { get; set; }

        public virtual ICollection<SysRoleSysUserInfo> SysRoleSysUsers { get; set; }

        [MaxLength(50), Required]
        public string SystemId { get; set; }
    }
}
