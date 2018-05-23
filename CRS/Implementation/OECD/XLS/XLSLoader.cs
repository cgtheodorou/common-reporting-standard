using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CRS.Implementation.Common;

namespace CRS.Implementation.OECD.XLS
{
    class XLSLoader
    {
        public List<CRSDocument> Documents = new List<CRSDocument>();

        public XLSLoader(string fileName)
        {
            var workbook = new XLWorkbook(fileName);
            var xlWorksheet = workbook.Worksheets.FirstOrDefault();

            int rowCount = xlWorksheet.Rows().Count();
            int colCount = xlWorksheet.Columns().Count();

            CRSDocument doc = null;
            for (int i = 2; i <= rowCount; i++)
            {
                try
                {
                    var rowName = xlWorksheet.Cell(i, 1).Value.ToString();

                    //FIELD MAPPING
                    switch (rowName)
                    {
                        case "crsHeader":
                            if (doc == null)
                            {
                                doc = CreateNewDocument();
                                doc.Header = CreateHeader(xlWorksheet, i);
                            }
                            break;
                        case "crsAccountHolderInfo":
                            var accountHolder = CreateAccountHolder(xlWorksheet, i);
                            i++; //get next row for payment details
                            accountHolder.AccountPayment = CreateAccountPayment(xlWorksheet, i);
                            doc.AccountHolderInfo.Add(accountHolder);
                            break;
                        case "crsAccountPayments":
                            //do nothing, handled in crsAccountHolderInfo
                            break;
                        case "crsControllingPersonDetails":
                            throw new NotImplementedException("not implemented.");
                        default:
                            throw new NotImplementedException("Invalid case!");
                    }

                    if (i == rowCount)
                    {
                        Documents.Add(doc);
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception($"{ex.Message} on row number {i} ");
                }
            }
        }

        public CRSDocument CreateNewDocument()
        {
            var doc = new CRSDocument()
            {
                Header = null,
                AccountHolderInfo = new List<AccountHolderInfo>(),
            };
            return doc;
        }

        public Header CreateHeader(IXLWorksheet xlWorksheet, int currentRow)
        {
            var crsHeader = new Header()
            {
                RowID = xlWorksheet.Cell(currentRow, 2).Value.ToString(),
                ParentRowID = xlWorksheet.Cell(currentRow, 3).Value.ToString(),
                DataReportType = CommonConversions.ConvertToMessageTypeIndicator(xlWorksheet.Cell(currentRow, 4).Value.ToString()),
                TransmittingCountry = CountryCode_Type.CY,
                ReceivingCountry = CountryCode_Type.CY,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd"),

                ReportingFirmName = xlWorksheet.Cell(currentRow, 7).Value.ToString(),
                ReportingFirmNameType = CommonConversions.ConvertToNameType(xlWorksheet.Cell(currentRow, 8).Value.ToString()),
                ReportingFirmCountryCode = CommonConversions.ConvertToCountryCodeType(xlWorksheet.Cell(currentRow, 9).Value.ToString()),
                //ReportingFirmIdType = xlWorksheet.Cell(currentRow, 10).Value.ToString(),
                ReportingFirmId = xlWorksheet.Cell(currentRow, 11).Value.ToString(),
                ReportingFirmIdIssuedBy = CommonConversions.ConvertToCountryCodeType(xlWorksheet.Cell(currentRow, 12).Value.ToString()),
                ReportingFirmAddress = xlWorksheet.Cell(currentRow, 13).Value.ToString(),

                RowIDToCorrect = !string.IsNullOrEmpty(xlWorksheet.Cell(currentRow, 15).Value.ToString().Trim()) ? xlWorksheet.Cell(currentRow, 15).Value.ToString() : "",
            };

            DateTime reportPeriod;
            ParseReportingPeriod(xlWorksheet.Cell(currentRow, 5).Value.ToString(), out reportPeriod);
            crsHeader.ReportingPeriod = reportPeriod.ToString("yyyy-MM-dd");

            return crsHeader;
        }

        public AccountHolderInfo CreateAccountHolder(IXLWorksheet xlWorksheet, int currentRow)
        {
            var account = new AccountHolderInfo()
            {
                AccountNumber = xlWorksheet.Cell(currentRow, 16).Value.ToString(),
                AccountNumberType = CommonConversions.ConvertToAccountNumberType(xlWorksheet.Cell(currentRow, 17).Value.ToString()),
                PreExistingAccount = xlWorksheet.Cell(currentRow, 18).Value.ToString(),
                UndocumentedAccount = xlWorksheet.Cell(currentRow, 19).Value.ToString(),
                AccountType = CommonConversions.ConvertToAccountType(xlWorksheet.Cell(currentRow, 20).Value.ToString()),
                AccountMode = xlWorksheet.Cell(currentRow, 21).Value.ToString(),
                FirstName = xlWorksheet.Cell(currentRow, 22).Value.ToString(),
                MiddleName = xlWorksheet.Cell(currentRow, 23).Value.ToString(),
                LastName = xlWorksheet.Cell(currentRow, 24).Value.ToString(),
                NameType = xlWorksheet.Cell(currentRow, 25).Value.ToString(),
                TaxIdentificationNumber = xlWorksheet.Cell(currentRow, 26).Value.ToString(),

                CountryOfResidence = CommonConversions.ConvertToCountryCodeType(xlWorksheet.Cell(currentRow, 28).Value.ToString()),
                Address = xlWorksheet.Cell(currentRow, 29).Value.ToString(),
                City = xlWorksheet.Cell(currentRow, 30).Value.ToString(),
                AddressType = xlWorksheet.Cell(currentRow, 31).Value.ToString(),
                BirthDate = xlWorksheet.Cell(currentRow, 32).Value.ToString(),
                BirthCity = xlWorksheet.Cell(currentRow, 33).Value.ToString(),
                BirthCountry = xlWorksheet.Cell(currentRow, 34).Value.ToString(),

                AccountBalance = xlWorksheet.Cell(currentRow, 45).Value.ToString(),
                AccountCurrency = CommonConversions.ConvertToCurrencyCodeType(xlWorksheet.Cell(currentRow, 46).Value.ToString())
            };

            if (xlWorksheet.Cell(currentRow, 27).Value.ToString().Length > 0)
            {
                account.TaxIdIssuedBy = CommonConversions.ConvertToCountryCodeType(xlWorksheet.Cell(currentRow, 27).Value.ToString());
            }

            return account;
        }

        public AccountPayment CreateAccountPayment(IXLWorksheet xlWorksheet, int currentRow)
        {
            var payment = new AccountPayment()
            {
                PaymentCode = xlWorksheet.Cell(currentRow, 47).Value.ToString(),
                PaymentType = CommonConversions.ConvertToPaymentType(xlWorksheet.Cell(currentRow, 48).Value.ToString()),
                PaymentAmount = xlWorksheet.Cell(currentRow, 49).Value.ToString(),
                PaymentCurrency = CommonConversions.ConvertToCurrencyCodeType(xlWorksheet.Cell(currentRow, 50).Value.ToString()),
            };
            return payment;
        }

        public ControllingPersonDetail CreateControllingPersonDetails(IXLWorksheet xlWorksheet, int currentRow)
        {
            return null;
        }

        public void ParseReportingPeriod(string datetimeStr, out DateTime reportingPeriod)
        {
            var isDatetimeStr = Regex.IsMatch(datetimeStr, @"^(?:(?:31(\/|-|\.)(?:0?[13578]|1[02]))\1|(?:(?:29|30)(\/|-|\.)(?:0?[1,3-9]|1[0-2])\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:29(\/|-|\.)0?2\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\d|2[0-8])(\/|-|\.)(?:(?:0?[1-9])|(?:1[0-2]))\4(?:(?:1[6-9]|[2-9]\d)?\d{2})$");
            if (isDatetimeStr)
            {
                DateTime.TryParse(datetimeStr, out reportingPeriod);
            }
            else
            {
                var year = int.Parse(datetimeStr);
                reportingPeriod = new DateTime(year, 12, 31);
            }
        }
    }
}
