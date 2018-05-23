using CRS.Implementation.Common;

namespace CRS.Implementation
{
    static class TransformationEngineFactory
    {
        public static OECDTransformationEngine Create(TransformationEngineTypeEnum type)
        {
            switch (type)
            {
                case TransformationEngineTypeEnum.OECD:
                    return new OECDTransformationEngine();
                default:
                    return null;
            }
        }
    }
}
