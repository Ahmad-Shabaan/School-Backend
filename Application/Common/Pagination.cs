using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common
{
    public sealed class Pagination
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; } 
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

    }
}
