using System.ComponentModel.DataAnnotations;

namespace Database.Base.Model
{
    public enum ActionTypes
    {
        [Display(Name = "默认")]
        Default = 0,
        [Display(Name = "事件")]
        Event = 2,
        [Display(Name = "链接")]
        Link = 1
    }

    public enum ButtonTypes
    {
        [Display(Name = "无")]
        None = 0,
        [Display(Name = "页面按钮")]
        PageButton = 1,
        [Display(Name = "行内按钮")]
        InlineButton = 2
    }

    public enum HttpMethod
    {
        Unknow = 0,
        Post = 1,
        Get = 2
    }

    public enum LogMode
    {
        [Display(Name = "数据库")]
        DataBase = 0,
        [Display(Name = "文件")]
        File = 1
    }

    public enum Log
    {
        [Display(Name = "数据库")]
        DataBase = 0,
        [Display(Name = "文件")]
        File = 1
    }
}
