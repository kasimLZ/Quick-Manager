using Database.Base.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Database.Model
{
    public class Artical : DbSetBase
    {
        public string Title { get; set; }

        public string Content { get; set; }

        public string Auth { get; set; }

        public DateTime PublishTime { get; set; }
    }
}
