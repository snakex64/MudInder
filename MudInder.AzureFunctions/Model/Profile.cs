using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MudInder.AzureFunctions.Model
{
    internal class Profile
    {
        public int Age { get; set; }

        public string DisplayedName { get; set; }

        public string Description { get; set; }

        public int NbImages { get; set; } = 0;
    }
}
