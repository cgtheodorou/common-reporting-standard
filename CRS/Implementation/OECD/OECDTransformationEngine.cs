using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CRS.Implementation.OECD;
using CRS.Implementation.OECD.XLS;
using CRS.Interfaces;

namespace CRS.Implementation
{
    class OECDTransformationEngine : ITransformationEngine
    {

        public IList<TransformationMessage> TransformToXml(string fileName)
        {

            var xlsLoader = new XLSLoader(fileName);
            var documentCollection = xlsLoader.Documents;

            IList<TransformationMessage> xsdMessages = new List<TransformationMessage>();
            foreach (var document in documentCollection)
            {

                var obj = new CRS_OECD();

                obj.MessageSpec = new MessageSpec_Type()
                {
                    MessageRefId = document.Header.RowID,
                    //CorrMessageRefId = document.Header.RowIDToCorrect,
                    MessageType = MessageType_EnumType.CRS,
                    MessageTypeIndic = document.Header.DataReportType,
                    MessageTypeIndicSpecified = true,
                    TransmittingCountry = document.Header.TransmittingCountry,
                    ReceivingCountry = document.Header.ReceivingCountry,
                    SendingCompanyIN = document.Header.ReportingFirmId,
                    Timestamp = Convert.ToDateTime(document.Header.Timestamp),
                    ReportingPeriod = Convert.ToDateTime(document.Header.ReportingPeriod),
                };

                var CrsBodyType = new CrsBody_Type();
                CrsBodyType.ReportingFI = new CorrectableOrganisationParty_Type();

                var IN = new OrganisationIN_Type()
                {
                    INType = document.Header.ReportingFirmIdType,
                    issuedBy = document.Header.ReportingFirmIdIssuedBy,
                    Value = document.Header.ReportingFirmId,
                    issuedBySpecified = true,
                };

                CrsBodyType.ReportingFI.IN = new OrganisationIN_Type[1];
                CrsBodyType.ReportingFI.IN[0] = IN;

                CrsBodyType.ReportingFI.Name = new NameOrganisation_Type[1];
                CrsBodyType.ReportingFI.Name[0] = new NameOrganisation_Type()
                {
                    nameType = document.Header.ReportingFirmNameType,
                    Value = document.Header.ReportingFirmName,
                };

                object RFIaddressObj = document.Header.ReportingFirmAddress.Replace(@"\n", "");
                CrsBodyType.ReportingFI.Address = new Address_Type[1];
                CrsBodyType.ReportingFI.Address[0] = new Address_Type()
                {
                    Items = new object[1] { RFIaddressObj },
                    CountryCode = document.Header.ReportingFirmCountryCode,
                    legalAddressTypeSpecified = false,
                };

                CrsBodyType.ReportingFI.DocSpec = new DocSpec_Type()
                {
                    DocRefId = document.Header.DocRefId,
                    DocTypeIndic = document.Header.DocTypeIndic,
                };
                CrsBodyType.ReportingFI.ResCountryCode = new CountryCode_Type[1];
                CrsBodyType.ReportingFI.ResCountryCode[0] = document.Header.ReportingFirmCountryCode;

                var CrsReportingGroup = new CrsBody_TypeReportingGroup();
                CrsReportingGroup.AccountReport = new CorrectableAccountReport_Type[document.AccountHolderInfo.Count];         
               
                for (int i = 0; i < document.AccountHolderInfo.Count; i++)
                {
                    var accountHolder = document.AccountHolderInfo[i];

                    var accountReport = new CorrectableAccountReport_Type();
                    
                    accountReport.DocSpec = new DocSpec_Type();
                    accountReport.DocSpec.DocRefId = accountHolder.DocRefId;
                    accountReport.DocSpec.DocTypeIndic = accountHolder.DocTypeIndic;

                    accountReport.AccountNumber = new FIAccountNumber_Type();
                    accountReport.AccountNumber.UndocumentedAccount = true;
                    accountReport.AccountNumber.UndocumentedAccountSpecified = true;
                    
                    accountReport.AccountNumber.AcctNumberType = accountHolder.AccountNumberType;
                    accountReport.AccountNumber.AcctNumberTypeSpecified = true;
                    accountReport.AccountNumber.Value = accountHolder.AccountNumber;

                    accountReport.AccountHolder = new AccountHolder_Type();
                    accountReport.AccountHolder.Items = new object[document.AccountHolderInfo.Count];

                    var accountHolderIndividual = new PersonParty_Type();

                    var Address = new Address_Type();
                    Address.CountryCode = accountHolder.CountryOfResidence;
                    object addressObj = accountHolder.Address;
                    Address.Items = new object[1] { addressObj };
                    //Address.legalAddressType = OECDLegalAddressType_EnumType.OECD301;
                    //Address.legalAddressTypeSpecified = false;

                    accountHolderIndividual.Address = new Address_Type[1];
                    accountHolderIndividual.Address[0] = Address;


                    var BirthInfo = new PersonParty_TypeBirthInfo();

                    DateTime birthDate;
                    if (DateTime.TryParse(accountHolder.BirthDate, out birthDate))
                    {
                        BirthInfo.BirthDate = birthDate;

                    }

                    BirthInfo.City = accountHolder.City;

                    accountHolderIndividual.BirthInfo = BirthInfo;

                    var AccountHolderIndividualName = new NamePerson_Type();
                    AccountHolderIndividualName.FirstName = new NamePerson_TypeFirstName()
                    {
                        Value = accountHolder.FirstName,
                    };
                    AccountHolderIndividualName.LastName = new NamePerson_TypeLastName()
                    {
                        Value = accountHolder.LastName,
                    };

                    accountHolderIndividual.Name = new NamePerson_Type[1];
                    accountHolderIndividual.Name[0] = AccountHolderIndividualName;
                    accountHolderIndividual.ResCountryCode = new CountryCode_Type[1];
                    accountHolderIndividual.ResCountryCode[0] = accountHolder.CountryOfResidence;

                    if(accountHolder.TaxIdentificationNumber.Trim().Length > 0)
                    {
                        var AccountHolderIndividualTIN = new TIN_Type();

                        AccountHolderIndividualTIN.issuedBySpecified = true;
                        AccountHolderIndividualTIN.issuedBy = accountHolder.TaxIdIssuedBy;
                        AccountHolderIndividualTIN.Value = accountHolder.TaxIdentificationNumber;

                        accountHolderIndividual.TIN = new TIN_Type[1];
                        accountHolderIndividual.TIN[0] = AccountHolderIndividualTIN;           
                    }

                    accountReport.AccountHolder.Items[0] = accountHolderIndividual;

                    accountReport.AccountBalance = new MonAmnt_Type();
                    accountReport.AccountBalance.currCode = accountHolder.AccountCurrency;
                    accountReport.AccountBalance.Value = Convert.ToDecimal(accountHolder.AccountBalance.Replace(".", ""));

                    var AccountReportPayment = new Payment_Type();
                    AccountReportPayment.Type = accountHolder.AccountPayment.PaymentType;
                    AccountReportPayment.PaymentAmnt = new MonAmnt_Type()
                    {
                        currCode = accountHolder.AccountPayment.PaymentCurrency,
                        Value = Convert.ToDecimal(accountHolder.AccountPayment.PaymentAmount.Replace(".", "")),
                    };

                    accountReport.Payment = new Payment_Type[1];
                    accountReport.Payment[0] = AccountReportPayment;
                    CrsReportingGroup.AccountReport[i] = accountReport;
                    CrsBodyType.ReportingGroup = new CrsBody_TypeReportingGroup[1];
                    CrsBodyType.ReportingGroup[0] = CrsReportingGroup;
                    obj.CrsBody = new CrsBody_Type[1];
                    obj.CrsBody[0] = CrsBodyType;
                }

                var xml = CreateXML(obj);
                var xmlbytes = Encoding.UTF8.GetBytes(xml);
                xsdMessages = ValidateXmlUsingXSD(xmlbytes);

                File.WriteAllText(DateTime.Now.Ticks + ".xml", xml);
                
            }

            return xsdMessages;
        }

        public string CreateXML(object obj)
        {
            try
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("crs", "urn:oecd:ties:crs:v1");
                ns.Add("isocrstypes", "urn:oecd:ties:isocrstypes:v1");
                ns.Add("fatca", "urn:oecd:ties:fatca:v1");
                ns.Add("stf", "urn:oecd:ties:stf:v4");
                ns.Add("commontypesfatcacrs", "urn:oecd:ties:commontypesfatcacrs:v1");

                XmlSerializer serializer = new XmlSerializer(typeof(CRS_OECD));
                var xml = "";

                using (var memStm = new MemoryStream())
                using (var xw = XmlWriter.Create(memStm, new XmlWriterSettings() { Indent=true, Encoding = Encoding.UTF8 }))
                {
                    serializer.Serialize(xw, obj, ns);
                    var utf8 = memStm.ToArray();
                    xml = Encoding.UTF8.GetString(utf8);
                }

                return xml;
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        private IList<TransformationMessage> ValidateXmlUsingXSD(byte[] xml)
        {
            var results = new List<TransformationMessage>();

            XmlReaderSettings settings = new XmlReaderSettings()
            {
                ValidationType = ValidationType.Schema,
                ConformanceLevel = ConformanceLevel.Document,
                ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings,
                Schemas = LoadEmbeddedXsdFiles(),
                CloseInput = true
            };
            settings.ValidationEventHandler += (sender, e) =>
            {
                results.Add(new TransformationMessage()
                {
                    Level = e.Severity == XmlSeverityType.Warning ? MessageLevel.Warning : MessageLevel.Error,
                    MessageType = MessageType.XsdValidation,
                    InvalidValue = e.Message
                });
            };

            using (var reader = XmlReader.Create(new MemoryStream(xml), settings))
            {
                while (reader.Read())
                { }
            }

            return results;
        }

        public XmlSchemaSet LoadEmbeddedXsdFiles()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName().Name;
            XmlSchemaSet xsd = new XmlSchemaSet();

            using (Stream stream = assembly.GetManifestResourceStream(assemblyName + ".CRS_schema_v1._0.CrsXML_v1.0.xsd"))
            using (var xmlReader = XmlReader.Create(stream))
            {
                xsd.Add(null, xmlReader);
            }
            using (Stream stream = assembly.GetManifestResourceStream(assemblyName + ".CRS_schema_v1._0.CommonTypesFatcaCrs_v1.1.xsd"))
            using (var xmlReader = XmlReader.Create(stream))
            {
                xsd.Add(null, xmlReader);
            }
            using (Stream stream = assembly.GetManifestResourceStream(assemblyName + ".CRS_schema_v1._0.FatcaTypes_v1.1.xsd"))
            using (var xmlReader = XmlReader.Create(stream))
            {
                xsd.Add(null, xmlReader);
            }
            using (Stream stream = assembly.GetManifestResourceStream(assemblyName + ".CRS_schema_v1._0.isocrstypes_v1.0.xsd"))
            using (var xmlReader = XmlReader.Create(stream))
            {
                xsd.Add(null, xmlReader);
            }
            using (Stream stream = assembly.GetManifestResourceStream(assemblyName + ".CRS_schema_v1._0.oecdtypes_v4.1.xsd"))
            using (var xmlReader = XmlReader.Create(stream))
            {
                xsd.Add(null, xmlReader);
            }
            return xsd;
        }

    }
}
