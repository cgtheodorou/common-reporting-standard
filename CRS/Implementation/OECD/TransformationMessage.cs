using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRS.Implementation.OECD
{
    public enum MessageLevel
    {
        Warning,
        Error
    }

    public enum MessageType
    {
        DataPointValueMissing,
        DataPointValueInvalidFormat,
        DataPointValueLengthWasLargerThanSpecsFormat,
        DataPointValueWasTruncated,
        DataPointEnumerationValueNotSupported,
        XsdValidation
    }

    public class TransformationMessage
    {
        public MessageLevel Level { get; set; }
        public decimal SortId { get; set; }
        public IList<string> XPath { get; set; }
        public string DataPoint { get; set; }
        public MessageType MessageType { get; set; }
        public string InvalidValue { get; set; }
        public IList<KeyValuePair<string, string>> Data { get; set; }
    }
}
