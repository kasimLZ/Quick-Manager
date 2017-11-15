using System;
using System.ComponentModel.DataAnnotations;

namespace Database.Base.Model
{
    public class SysUserInfo : DbSetBase
    {
        public SysUserInfo()
        {
            Enable = true;
        }

        [StringLength(30, MinimumLength = 4, ErrorMessage = "{0}长度为{2}-{1}个字符")]
        [Display(Name = "帐号")]
        public string Login { get; set; }

        [DataType(DataType.Password)]
        [StringLength(256, ErrorMessage = "{0}长度少于{1}个字符")]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "名称")]
        public string UserName { get; set; }

        [ScaffoldColumn(false)]
        public string RealName { get; set; }

        [Display(Name = "头像")]
        public string HeadIcon { get; set; }

        public int sex { get; set; }

        [Display(Name = "登陆时间")]
        //[HiddenInput(DisplayValue = false)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? LastLoginTime { get; set; }

        public bool Enable { get; set; }
    }
}
