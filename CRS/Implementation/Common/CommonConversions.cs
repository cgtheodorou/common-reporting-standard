using System;
using CRS.Implementation.OECD;

namespace CRS.Implementation.Common
{
    class CommonConversions
    {
        public static CrsMessageTypeIndic_EnumType ConvertToMessageTypeIndicator(string val)
        {
            switch (val)
            {
                case "New Information Report":
                    return CrsMessageTypeIndic_EnumType.CRS701;
                case "Corrected Report":
                    return CrsMessageTypeIndic_EnumType.CRS702;
                case "Nil Report":
                    return CrsMessageTypeIndic_EnumType.CRS703;
                default:
                    throw new System.Exception("no type available to handle the message type indicator provided!");
            }
        }

        public static CrsAcctHolderType_EnumType ConvertToAccountType(string val)
        {
            switch (val)
            {
                case "Individual":
                    return CrsAcctHolderType_EnumType.CRS102;
                default:
                    throw new System.Exception("no type available to handle the account type provided!");
            }
        }

        public static OECDNameType_EnumType ConvertToNameType(string val)
        {
            return ParseEnum<OECDNameType_EnumType>(val);
        }

        public static CountryCode_Type ConvertToCountryCodeType(string val)
        {
            return ParseEnum<CountryCode_Type>(val);
        }

        public static AcctNumberType_EnumType ConvertToAccountNumberType(string val)
        {
            return ParseEnum<AcctNumberType_EnumType>(val);
        }

        public static currCode_Type ConvertToCurrencyCodeType(string val)
        {
            return ParseEnum<currCode_Type>(val);
        }

        public static CrsPaymentType_EnumType ConvertToPaymentType(string val)
        {
            return ParseEnum<CrsPaymentType_EnumType>(val);
        }

        public static T ParseEnum<T>(string value)
        {
            if(value.Length == 0) { throw new Exception($"Please check your data, a column of type {typeof(T).Name} has en empty value "); }
            return (T)Enum.Parse(typeof(T), value, true);
        }
    }
}
