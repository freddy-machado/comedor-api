using System.Collections.Generic;

namespace Comedor.Core.Dtos
{
    public class PagedResultDto<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
