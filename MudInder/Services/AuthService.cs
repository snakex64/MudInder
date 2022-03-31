using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MudInder.Services
{
    internal class AuthService
    {
        public bool IsAuthenticated => Token != null;

        public string? Token { get; set; }
    }
}
