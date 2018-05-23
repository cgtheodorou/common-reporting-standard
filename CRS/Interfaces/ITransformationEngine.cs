using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRS.Implementation.OECD;

namespace CRS.Interfaces
{
    interface ITransformationEngine
    {
        IList<TransformationMessage> TransformToXml(string fileName);
    }
}
