using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Database.Base.Model
{
    public class SysArea : DbSetBase
    {
        public SysArea() { SortWeight = 0; }

        /// <summary>
        /// 排序权重
        /// </summary>
        [Display(Name = "Sort")]
        public int SortWeight { get; set; }

        /// <summary>
        /// 区域中文名称
        /// </summary>
        public string AreaDisplayName { get; set; }

        /// <summary>
        /// 区域名称（项目文件夹）
        /// </summary>
        public string AreaName { get; set; }

        /// <summary>
        /// 控制器外键
        /// </summary>
        public ICollection<SysController> SysControllers { get; set; }

        /// <summary>
        /// 是否显示
        /// </summary>
        public bool Display { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 启用权限认证
        /// </summary>
        public bool NeedAuth { get; set; }
    }
}
