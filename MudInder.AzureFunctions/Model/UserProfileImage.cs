using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MudInder.AzureFunctions.Model
{
    internal class UserProfileImage
    {
        public string id { get; set; } // id is UserId + Index

        public Guid UserId { get; set; }
        
        public int Index { get; set; }

        public byte[]? Data { get; set; }
    }
}
