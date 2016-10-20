using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;  // LINQ for XML

using System.Text;
using System.Net;      // for HttpWebRequest, WebRequest
using System.Net.Http;
using System.Collections;


using System.Web.Services.Protocols;
using System.Xml.Serialization;


namespace PsoftJournalVoucher
{
    /// <summary>
    /// Summary description for SendInvoicesService
    /// </summary>
    [WebService(Namespace = "http://nyumc.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class SendInvoicesService : System.Web.Services.WebService
    {

        [WebMethod]
        public string GetXML(string BillingPeriodId, string busUnit)
        {
            string DBConnection = Properties.Settings.Default.DBConnection;
            string connStr = ConfigurationManager.ConnectionStrings[DBConnection].ConnectionString;
            // Create our DB connection.
            SqlConnection conn = new SqlConnection(connStr);

            
            SqlCommand cmd = new SqlCommand("[dbo].[generateInvoicesByBillingPeriod]", conn);
            cmd.CommandTimeout = 300;
            DataSet ds = new DataSet();

            SqlDataAdapter a = new SqlDataAdapter();
            DataTable dt = new DataTable();


            cmd.Parameters.Add(new SqlParameter("@BillingPeriodId", BillingPeriodId));
            cmd.Parameters.Add(new SqlParameter("@BusUnit", busUnit));
            cmd.CommandType = CommandType.StoredProcedure;
            a.SelectCommand = cmd;
            string responseText = "";
            
            // Use a DataAdapter to fill the dataset with four tables at once
            //SqlDataAdapter a = new SqlDataAdapter(cmd);
            a.Fill(ds);
            //for testing
            /*using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter))
            {
                ds.DataSetName = "Transaction";
                ds.Tables[0].TableName = "N_DLAR_JRNL_HDR";
                ds.WriteXml(xmlTextWriter);
                xmlTextWriter.Close();

                XmlDocument doc = new XmlDocument();

                string dsXML = ds.GetXml();
                
            }*/

            // These tables are returned from our stored proc
            DataTable tblHeaders = ds.Tables[0];
            tblHeaders.TableName = "JournalHeaders";
            DataTable tblItems = ds.Tables[1];
            tblItems.TableName = "JournalLineItems";
            
            /*if (ds.Tables[1].Rows.Count == 0)
            {
                return "No Journal Sent!";
                //throw new SoapException("No Journals Sent.", SoapException.ServerFaultCode);
            }*/
            //RELATE BILLING PERIOD
            /*DataRelation relHeaderItems = new DataRelation(
            "relHeaderItems",                          // relation name
            tblHeaders.Columns["JournalHeaderId"],     // parent column
            tblItems.Columns["HeaderId"]); // child column
             * */
            DataRelation relHeaderItems = new DataRelation(
            "relHeaderItems",                          // relation name
            tblHeaders.Columns["Billing_Period_Id"],     // parent column
            tblItems.Columns["Billing_Period_Id"]); // child column

            // Set the "Nested" property on all the relations we created

            //no more check  box, always true.....
            //relHeaderItems.Nested = cbNestedRelation.Checked;
            relHeaderItems.Nested = true;

            // Lastly, add the relations to the dataset
            ds.Relations.Add(relHeaderItems);
            
            using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter))
            {
                ds.DataSetName = "Transaction";
                ds.Tables[0].TableName = "N_DLAR_JRNL_HDR";
                ds.WriteXml(xmlTextWriter);
                xmlTextWriter.Close();

                XmlDocument doc = new XmlDocument();

                string dsXML = ds.GetXml();

                doc.LoadXml(dsXML);
                XmlDocument tempDoc = new XmlDocument();

                var headers = doc.SelectNodes("//N_DLAR_JRNL_HDR");//count headers in ds, alway 1 in this case
                /*
                tempDoc.Load("c:\\DLAR_JOURNAL_TEMP.xml");//load psoft xml file
                
                var tempHeader = tempDoc.SelectSingleNode("//N_DLAR_JRNL_HDR");
                var tempJournalLine = tempDoc.SelectSingleNode("//N_DLAR_JRNL_LIN ");
                 * */

                //Deal with the dataset
                //Journal Headers variable for both ds xml and template
                var businessUnit = doc.SelectNodes("//BUSINESS_UNIT");
                //var tempBusinessUnit = tempDoc.SelectNodes("//BUSINESS_UNIT");//BUSINESS_UNIT FROM TEMPLATE
                var journalDate = doc.SelectNodes("//JOURNAL_DATE");
                //var tempJournalDate = tempDoc.SelectNodes("//JOURNAL_DATE");
                var ledgerGroup = doc.SelectNodes("//LEDGER_GROUP");
                //var tempLedgerGroup = tempDoc.SelectNodes("//LEDGER_GROUP");
                var ledger = doc.SelectNodes("//LEDGER");
                //var tempLedger = tempDoc.SelectNodes("//LEDGER");
                var descr = doc.SelectNodes("//DESCR");
                //var tempDescr = tempDoc.SelectNodes("//DESCR");
                string[] headersString = new string[headers.Count];
                //hard coded business unit
                
                string[] journalDateString = new string[headers.Count];
                string[] ledgerGroupString = new string[headers.Count];
                string[] ledgerString = new string[headers.Count];
                string[] descrString = new string[headers.Count];

                /*tempDoc.Load("c:\\DLAR_JOURNAL_TEMP.xml");//load psoft xml file

                var tempHeader = tempDoc.SelectSingleNode("//N_DLAR_JRNL_HDR");
                var tempJournalLine = tempDoc.SelectSingleNode("//N_DLAR_JRNL_LIN ");
                XmlDocument header = new XmlDocument();

                String headerOuterXml = headers[0].OuterXml;

                header.LoadXml(headerOuterXml);*/
                //see if there is HOS01 in the dataset
                //string[] businessUnitString = new string[2] { "SOM01", "HOS01" };
                //ArrayList businessUnitString = new ArrayList();
                //only one business unit is allowed at once for pSoft
                //List<string> businessUnitString = new List<string>{"HOS01"};
                List<string> businessUnitString = new List<string> { busUnit };
                XMLToString xmlToString = new XMLToString();
                /*bool hasHOS01 = false;
                for (int c= 0; c < businessUnit.Count; c++)
                {

                    
                    if (businessUnit[c].InnerXml == "HOS01")
                    {
                        hasHOS01 = true;
                        //break;
                    }
                    
                }
                if (hasHOS01 == true)
                {
                    businessUnitString = new List<string>(new string[] { "SOM01", "HOS01" });
                }
               */
                //for (int i = 0; i < headers.Count; i++)

                //string responseText = "";
                for (int i = 0; i < businessUnitString.Count; i++)
                
                {
                    tempDoc.Load("c:\\SendVouchers\\DLAR_JOURNAL_TEMP.xml");//load psoft xml file
                    
                    var tempHeader = tempDoc.SelectSingleNode("//N_DLAR_JRNL_HDR");
                    var tempJournalLine = tempDoc.SelectSingleNode("//N_DLAR_JRNL_LIN ");
                    XmlDocument header = new XmlDocument();
                    if (headers[i].InnerXml == "")
                    {
                        responseText = "No journals sent.";
                        return responseText;
                    }
                    String headerOuterXml = headers[i].OuterXml;

                    header.LoadXml(headerOuterXml);

                    //Insert Journal Header values to the template
                    //tempDoc.SelectSingleNode("//BUSINESS_UNIT").InnerXml = businessUnit[i].InnerXml;
                    //Now hard coded Business Unit for the header
                    
                    tempDoc.SelectSingleNode("//BUSINESS_UNIT").InnerXml = businessUnitString[i];
                    if (journalDate.Count != 0)
                    {
                        tempDoc.SelectSingleNode("//JOURNAL_DATE").InnerXml = journalDate[0].InnerXml;
                    }
                    else tempDoc.SelectSingleNode("//JOURNAL_DATE").InnerXml = "";
                    if (ledgerGroup.Count != 0)
                    {
                        tempDoc.SelectSingleNode("//LEDGER_GROUP").InnerXml = ledgerGroup[0].InnerXml;
                    }
                    else
                        tempDoc.SelectSingleNode("//LEDGER_GROUP").InnerXml = "";
                    if (ledger.Count != 0)
                    {
                        tempDoc.SelectSingleNode("//LEDGER").InnerXml = ledger[0].InnerXml;
                    }
                    else
                        tempDoc.SelectSingleNode("//LEDGER").InnerXml = "";
                    if (descr.Count != 0)
                    {
                        tempDoc.SelectSingleNode("//DESCR").InnerXml = descr[0].InnerXml;
                    }
                    else tempDoc.SelectSingleNode("//DESCR").InnerXml = "";
                    //}//if (businessUnitString.Length < 1)
                 
                    //Journal Items
                    var businessUnits = header.SelectNodes("//BUSINESS_UNIT");
                    //ACCOUNTS NOW SHOULD BE COUNTED BY CHART STRINGS TO GET ACCURATE COUNT
                    var accounts = header.SelectNodes("//ACCOUNT");
                    var glaccounts = header.SelectNodes("//glAccount");
                    var deptIds = header.SelectNodes("//DEPTID");
                    var operatingUnits = header.SelectNodes("//OPERATING_UNIT");
                    var fundCodes = header.SelectNodes("//FUND_CODE");
                    var programCodes = header.SelectNodes("//PROGRAM_CODE");
                    var projectIds = header.SelectNodes("//PROJECT_ID");
                    var amounts = header.SelectNodes("//MONETARY_AMOUNT");
                    var lines = header.SelectNodes("//LINE_DESCR");
                    //NO MORE CURRENCY ADN LINE DESCRIPTION
                    //var currencies = header.SelectNodes("//CURRENCY_CD");
                    //var lineDescrs = header.SelectNodes("//LINE_DESCR");
                    string[] accountsString = new string[accounts.Count];
                    string[] deptIdsString = new string[deptIds.Count];
                    string[] operatingUnitsString = new string[operatingUnits.Count];
                    string[] fundCodesString = new string[fundCodes.Count];
                    string[] programCodesString = new string[programCodes.Count];
                    string[] projectIdsString = new string[projectIds.Count];
                    string[] amountsString = new string[amounts.Count];
                    //string[] currenciesString = new string[currencies.Count];
                    //string[] lineDescrsString = new string[lineDescrs.Count];
                    //we have to know how many headers to clone in the tempDoc
                    //and append ADDITIONAL journal line items to temp
                    var tempAccounts = header.SelectNodes("//ACCOUNT");
                    string[] tempAccountsString = new string[tempAccounts.Count];
                    //Append line items!!!!

                    XmlDocument soapRequest = new XmlDocument();
                    //XMLToString xmlToString = new XMLToString();
                    var headerNodes = doc.SelectNodes("//N_DLAR_JRNL_HDR");
                    // traditional approach
                    string[] transactions = new string[headerNodes.Count];
                    string[] responses = new string[headerNodes.Count];
                    for (int y = 0; y < glaccounts.Count; y++)
                    {
                        if (businessUnits[y].InnerXml == businessUnitString[i])
                        {
                            //accountsString[y] = accounts[y].InnerXml;
                            //tempAccounts[y].InnerXml = accounts[y].InnerXml;
                            var accountInLine = tempJournalLine.SelectSingleNode("//ACCOUNT");
                            if (accounts.Count != 0)
                            {
                                accountInLine.InnerXml = accounts[y].InnerXml;
                            }
                            else
                                accountInLine.InnerXml = "";
                            var deptIdInLine = tempJournalLine.SelectSingleNode("//DEPTID");
                            if (deptIds.Count != 0)
                            {
                                deptIdInLine.InnerXml = deptIds[y].InnerXml;
                            }
                            else
                                deptIdInLine.InnerXml = "";

                            var operatingUnitInLine = tempJournalLine.SelectSingleNode("//OPERATING_UNIT");
                            if (operatingUnits.Count != 0)
                            {
                                operatingUnitInLine.InnerXml = operatingUnits[y].InnerXml;
                            }
                            else
                                operatingUnitInLine.InnerXml = "";

                            var fundCodeInLine = tempJournalLine.SelectSingleNode("//FUND_CODE");
                            if (fundCodes.Count != 0)
                            {
                                fundCodeInLine.InnerXml = fundCodes[y].InnerXml;
                            }
                            else
                                fundCodeInLine.InnerXml = "";
                            var programCodeInLine = tempJournalLine.SelectSingleNode("//PROGRAM_CODE");
                            if (programCodes.Count != 0)
                            {
                                programCodeInLine.InnerXml = programCodes[y].InnerXml;
                            }
                            else
                                programCodeInLine.InnerXml = "";
                            var projectIdInLine = tempJournalLine.SelectSingleNode("//PROJECT_ID");
                            if (projectIds.Count != 0)
                            {
                                projectIdInLine.InnerXml = projectIds[y].InnerXml;
                            }
                            else
                                projectIdInLine.InnerXml = "";
                            var amountInLine = tempJournalLine.SelectSingleNode("//MONETARY_AMOUNT");
                            if (amounts.Count != 0)
                            {
                                amountInLine.InnerXml = amounts[y].InnerXml;
                            }
                            else
                                amountInLine.InnerXml = "";
                            var currencyInLine = tempJournalLine.SelectSingleNode("//CURRENCY_CD");
                            //hard coded for currency and Line Description
                            //currencyInLine.InnerXml = currencies[y].InnerXml;
                            currencyInLine.InnerXml = "USD";
                            var lineDescrInLine = tempJournalLine.SelectSingleNode("//LINE_DESCR");
                            if (lines.Count != 0)
                            {
                                lineDescrInLine.InnerXml = lines[y].InnerXml;
                            }
                            else
                                lineDescrInLine.InnerXml = "";
                            //lineDescrInLine.InnerXml = lineDescrs[y].InnerXml;
                            //lineDescrInLine.InnerXml = "Line Description";
                            var cloneJournalLineNode = tempJournalLine.Clone();
                            tempHeader.AppendChild(cloneJournalLineNode);
                        }//if (businessUnits[y].InnerXml == businessUnitString[i])
                    }

                    var journalLinesInTemp = tempDoc.SelectNodes("//N_DLAR_JRNL_LIN");

                    //VERY IMPORTEANT:REMOVE THE VERY FIRST LINE IN tempDoc!!!!
                    for (int z = 0; z < journalLinesInTemp.Count; z++)
                    {

                        if (z == 0)
                        {
                            journalLinesInTemp[0].ParentNode.RemoveChild(journalLinesInTemp[0]);
                        }

                    }
                    //Now we have the temp xml doc ready we can insert the values from the string arrays above to
                    //the corresponding nodes in the temp xml doc

                    //Now we have to know how many items to append to each header


                    tempDoc.Save("c:\\SendVouchers\\DLAR_JOURNAL_MSG.xml");
                    soapRequest.Load("c:\\SendVouchers\\DLAR_JOURNAL_MSG.xml");
                    transactions[i] = headerNodes[i].ParentNode.InnerXml;
                    //soapRequest.GetElementsByTagName("Transaction").Item(0).InnerXml = doc.GetElementsByTagName("Transaction").Item(0).InnerXml;
                    soapRequest.GetElementsByTagName("Transaction").Item(0).InnerXml = transactions[i];
                    //soapRequest.GetElementsByTagName("N_DLAR_JRNL_HDR").Item(0).InnerXml = headerNodes[i].InnerXml;
                    //soapRequest.Save(@"c:\\DLAR_JOURNAL.xml");
                    //Now we can send the soap request transaction by transaction
                    XmlDocument XMLResponse = new XmlDocument();
                    string xmlRequest = xmlToString.GetXMLAsString(soapRequest);
                    //LogMessageToFile(xmlRequest);
                    //Log Sent Soap Message to file
                    string msg = xmlRequest;
                    string path = ("C:\\SendVouchers\\");
                    //System.IO.StreamWriter sw = System.IO.File.AppendText(
                    //GetTempPath() + "My Log File.txt");
                    System.IO.StreamWriter sw = System.IO.File.AppendText(
                        path + "Send Journals Message Log File.txt");
                    try
                    {
                        string logLine = System.String.Format(
                            "{0:G}: {1}.", System.DateTime.Now, msg);
                        sw.WriteLine(logLine);
                    }
                    finally
                    {
                        sw.Close();
                    }
                    //EndPoint
                    string endPoint = Properties.Settings.Default.EndPoint;
                    
                    //string endPointUrl = ConfigurationManager.ConnectionStrings[devType].ConnectionString;
                    string endPointUrl = ConfigurationManager.AppSettings[endPoint];
                    PsoftGetSoapXmlResponse psoftGetSoapXmlResponse = new PsoftGetSoapXmlResponse();
                    //XMLResponse = PsoftGetSoapXmlResponse.PostXMLTransaction("http://pfwlcdcdvm003.nyumc.org:7710/PSIGW/PeopleSoftServiceListeningConnector", "N_DLAR_JOURNAL.v1", "c:\\DLAR_JOURNAL_MSG.xml");
                    //NEW UAT
                    //XMLResponse = PsoftGetSoapXmlResponse.PostXMLTransaction("http://peoplesoftfscmuat.nyumc.org:8115/PSIGW/PeopleSoftServiceListeningConnector", "N_DLAR_JOURNAL.v1", "c:\\DLAR_JOURNAL_MSG.xml");
                    XMLResponse = PsoftGetSoapXmlResponse.PostXMLTransaction(endPointUrl, "N_DLAR_JOURNAL.v1", "c:\\SendVouchers\\DLAR_JOURNAL_MSG.xml");

                    //Now traverse through the Response Document to get the message we need

                    if (XMLResponse != null)
                    {
                        XmlNodeList node = XMLResponse.GetElementsByTagName("MsgData");

                        XmlNodeList ComboValidFalgNodes = XMLResponse.GetElementsByTagName("DESCR20");//Returns "Journal Received"
                        if (ComboValidFalgNodes.Count == 0)
                        {
                            responseText = "Peoplesoft service is down. Please try again later.";
                            return responseText;
                        }
                        //responses[i] = ComboValidFalgNodes[1].InnerText;
                        responseText = ComboValidFalgNodes[1].InnerText;
                        //int x = i + 1;
                        //txtOutput.Text = ComboValidFalgNodes[1].InnerText;
                        //txtOutput.Text = i+1 + " " + ComboValidFalgNodes[1].InnerText;
                        //txtOutput.Text = responses[i] + "\r\n" + i + 1 + " Journal Entries Received.";
                        //txtOutput.AppendText (ComboValidFalgNodes[1].InnerText);

                        //NO OUTPUT, BELOW IS FOR WIN FORM APP
                        //txtOutput.AppendText(x + " " + responses[i] + "\r\n");
                        //save the response in log?????
                        string responseMsg = responseText;
                        string responsePath = ("C:\\SendVouchers\\");
                        //System.IO.StreamWriter sw = System.IO.File.AppendText(
                        //GetTempPath() + "My Log File.txt");
                        System.IO.StreamWriter swR = System.IO.File.AppendText(
                            responsePath + "Journal Response Message Log File.txt");
                        try
                        {
                            string logLine = System.String.Format(
                                "{0:G}: {1}.", System.DateTime.Now, responseMsg);
                            swR.WriteLine(logLine);
                        }
                        finally
                        {
                            swR.Close();
                        }
                        
                    }
                    else
                    {
                        //ResultText.Text = "no value";
                        throw new ApplicationException("Something wrong happened while writing the XML content to the request stream: ");
                    }
                    
                    
                }//for (int i = 0; i < businessUnitString.Length; i++)
                //NO OUTPUT, BELOW IS FOR WIN FORM APP
                //txtOutput.AppendText("Total " + headers.Count + " Journal Entries Received.");
                return responseText;
            }// using (var xmlTextWriter = XmlWriter.Create(stringWriter))
            
        }//end GetXML
    }
}
