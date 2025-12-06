using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comedor.Core.Dtos.Auth
{
    public partial class UserDto
    {
        public List<MenuDto> Menus { get; set; } = new();
    }
}
