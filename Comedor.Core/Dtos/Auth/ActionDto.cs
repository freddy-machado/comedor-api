using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comedor.Core.Dtos.Auth
{
    public class ActionDto
    {
        public int Id { get; set; }
        public string Key { get; set; } = null!;
        public string Title { get; set; } = null!;
    }
}
