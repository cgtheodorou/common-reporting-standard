using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRS.Implementation.Common;

namespace CRS.Implementation.OECD
{

    class CRSDocument
    {
        public Header Header { get; set; }
        public List<AccountHolderInfo> AccountHolderInfo { get; set; }
        public ControllingPersonDetail ControllingPersonDetail { get; set; }
    }

    public class Header
    {
        public string RowID; //message reference id
        public string ParentRowID;
        public string RowIDToCorrect; //message reference id to correct
        public string MessageType = "CRS"; // Always CRS, needed by spec. (not included in field guide)
        public CrsMessageTypeIndic_EnumType DataReportType; //MessageTypeIndic       
        public string ReportingPeriod;
        public string Timestamp;
        public CountryCode_Type TransmittingCountry = CountryCode_Type.CY;
        public CountryCode_Type ReceivingCountry = CountryCode_Type.CY;

        public string DocRefId { get { return Utility.GenerateID(); } }
        public OECDDocTypeIndic_EnumType DocTypeIndic { get { return OECDDocTypeIndic_EnumType.OECD1; } }
        public string ReportingFirmIdType { get { return "GIIN"; } } //InType
        public CountryCode_Type ReportingFirmIdIssuedBy; //TransmittingCountry, Reporting Firm Id Issued By
        public string ReportingFirmId; //SendingCompanyIN, RFIn
        public string ReportingFirmName;
        public OECDNameType_EnumType ReportingFirmNameType;
        public string ReportingFirmAddress;
        public CountryCode_Type ReportingFirmCountryCode;
    }

    public class AccountHolderInfo
    {
        public string DocRefId { get { return Utility.GenerateID(); } }
        public OECDDocTypeIndic_EnumType DocTypeIndic { get { return OECDDocTypeIndic_EnumType.OECD1; } }
        public string AccountNumber;
        public AcctNumberType_EnumType AccountNumberType;
        public string PreExistingAccount;
        public string UndocumentedAccount;
        public CrsAcctHolderType_EnumType AccountType;
        public string AccountMode;
        public string FirstName;
        public string MiddleName;
        public string LastName;
        public string NameType;
        public string TaxIdentificationNumber;
        public CountryCode_Type TaxIdIssuedBy;
        public CountryCode_Type CountryOfResidence;
        public string Address;
        public string City;
        public string AddressType;
        public string BirthDate;
        public string BirthCity;
        public string BirthCountry;
        public string AccountBalance;
        public currCode_Type AccountCurrency;

        public AccountPayment AccountPayment;
    }



    public class AccountPayment
    {
        public string PaymentCode;
        public CrsPaymentType_EnumType PaymentType;
        public string PaymentAmount;
        public currCode_Type PaymentCurrency;
    }

    public class ControllingPersonDetail
    {
        public string Type;
        public string FirstName;
        public string MiddleName;
        public string LastName;
        public string NameType;
        public string Country;
        public string TaxIdentificationNumber;
        public string TaxIdentificationIssuedBy;
        public string Address;
        public string AddressCity;
        public string AddressType;
        public string BirthDate;
        public string BirthCity;
        public string BirthCountry;
    }

}
