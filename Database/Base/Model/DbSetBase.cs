using Common.DataTool;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Base.Model
{
    public abstract class DbSetBase : IDbSetBase
    {
        protected DbSetBase()
        {
            Id = SFID.NewID();
            CreatedDate = DateTime.Now;
            Deleted = false;
        }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true), ScaffoldColumn(false)]
        public DateTime CreatedDate { get; set; }

        [ScaffoldColumn(false)]
        public DateTime? UpdatedDate { get; set; }

        [ScaffoldColumn(false)]
        public bool Deleted { get; set; }

        [Key, ScaffoldColumn(false),DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }


    }

    public interface IDbSetBase
    {
        /// <summary>
        /// 主键
        /// </summary>
        long Id { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        DateTime CreatedDate { get; set; }

        /// <summary>
        /// 修改日期
        /// </summary>
        DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// 逻辑删除标记
        /// </summary>
        bool Deleted { get; set; }
    }
}
