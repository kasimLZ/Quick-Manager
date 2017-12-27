using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Linq
{
    public class PagedList<T> : List<T>, IPagedList
    {
        // Methods
        public PagedList(IQueryable source, int index, int pageSize, string target = "Main")
        {
            TotalCount = source.Count();
            PageSize = pageSize;
            PageIndex = index;
            Target = target;
            AddRange((IEnumerable<T>)source.Skip(((index - 1) * pageSize)).Take(pageSize));
        }

        // Properties
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public string Target { get; set; }

        public int TotalCount { get; set; }
    }


}
