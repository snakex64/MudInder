using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MudInder.AzureFunctions.Model
{
    internal class UserInfo
    {
        public Guid id { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public Profile? Profile { get; set; }

    }
}
