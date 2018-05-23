using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRS.Implementation.Common
{
    class Utility
    {
        public static string GenerateID()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
