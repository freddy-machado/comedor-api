using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comedor.Core.Dtos.Auth
{
    public class RefreshValidateDto
    {
        public string UserId { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
