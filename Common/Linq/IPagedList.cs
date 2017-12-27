using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Linq
{
    public interface IPagedList
    {
        // Properties
        int PageIndex { get; set; }
        int PageSize { get; set; }
        string Target { get; set; }
        int TotalCount { get; set; }
    }
}
