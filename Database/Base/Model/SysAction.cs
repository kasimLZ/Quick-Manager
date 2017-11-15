using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Database.Base.Model
{
    public class SysAction : DbSetBase
    {
        // Methods
        public SysAction()
        {
            SystemId = "000";
            ButtonIcon = string.Empty;
            ButtonStyle = string.Empty;
        }

        // Properties
        [MaxLength(40), Required]
        public string ActionDisplayName { get; set; }

        [MaxLength(40), Required]
        public string ActionName { get; set; }

        public ActionTypes ActionType { get; set; }

        [MaxLength(50)]
        public string ButtonIcon { get; set; }

        [MaxLength(50)]
        public string ButtonStyle { get; set; }

        public ButtonTypes ButtonType { get; set; }

        public virtual ICollection<SysControllerSysAction> SysControllerSysActions { get; set; }

        [MaxLength(50), Required]
        public string SystemId { get; set; }
    }
}
