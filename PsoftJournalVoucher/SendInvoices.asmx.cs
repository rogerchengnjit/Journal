﻿using System;
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;      // for HttpWebRequest, WebRequest
using System.Net.Http;

namespace PsoftJournalVoucher
{
    /// <summary>
    /// Summary description for HelloRoger
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class SendInvoices : System.Web.Services.WebService
    {

        [WebMethod]
        public void GetXML()
        {
            string connStr = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;

            // Create our DB connection.
            SqlConnection conn = new SqlConnection(connStr);

            // This stored procedure returns four tables (Customer, Orders, 
            // OrderItems and CustNotes) [SEE NOTE AT TOP OF FILE]
            SqlCommand cmd = new SqlCommand("[sp_GetDataForXml2]", conn);
            DataSet ds = new DataSet();

            // Use a DataAdapter to fill the dataset with four tables at once
            SqlDataAdapter a = new SqlDataAdapter(cmd);
            a.Fill(ds);

            // These tables are returned from our stored proc
            DataTable tblHeaders = ds.Tables[0];
            tblHeaders.TableName = "JournalHeaders";
            DataTable tblItems = ds.Tables[1];
            tblItems.TableName = "JournalLineItems";
            DataRelation relHeaderItems = new DataRelation(
            "relHeaderItems",                          // relation name
            tblHeaders.Columns["JournalHeaderId"],     // parent column
            tblItems.Columns["HeaderId"]); // child column


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

                var headers = doc.SelectNodes("//N_DLAR_JRNL_HDR");//count headers in ds
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
                string[] businessUnitString = new string[headers.Count];
                string[] journalDateString = new string[headers.Count];
                string[] ledgerGroupString = new string[headers.Count];
                string[] ledgerString = new string[headers.Count];
                string[] descrString = new string[headers.Count];


                for (int i = 0; i < headers.Count; i++)
                {
                    tempDoc.Load("c:\\DLAR_JOURNAL_TEMP.xml");//load psoft xml file

                    var tempHeader = tempDoc.SelectSingleNode("//N_DLAR_JRNL_HDR");
                    var tempJournalLine = tempDoc.SelectSingleNode("//N_DLAR_JRNL_LIN ");
                    XmlDocument header = new XmlDocument();
                    String headerOuterXml = headers[i].OuterXml;
                    header.LoadXml(headerOuterXml);
                    //Insert Journal Header values to the template
                    tempDoc.SelectSingleNode("//BUSINESS_UNIT").InnerXml = businessUnit[i].InnerXml;
                    tempDoc.SelectSingleNode("//JOURNAL_DATE").InnerXml = journalDate[i].InnerXml;
                    tempDoc.SelectSingleNode("//LEDGER_GROUP").InnerXml = ledgerGroup[i].InnerXml;
                    tempDoc.SelectSingleNode("//LEDGER").InnerXml = ledger[i].InnerXml;
                    tempDoc.SelectSingleNode("//DESCR").InnerXml = descr[i].InnerXml;


                    //Journal Items
                    var accounts = header.SelectNodes("//ACCOUNT");
                    var deptIds = header.SelectNodes("//DEPTID");
                    var operatingUnits = header.SelectNodes("//OPERATING_UNIT");
                    var fundCodes = header.SelectNodes("//FUND_CODE");
                    var programCodes = header.SelectNodes("//PROGRAM_CODE");
                    var projectIds = header.SelectNodes("//PROJECT_ID");
                    var amounts = header.SelectNodes("//MONETARY_AMOUNT");
                    var currencies = header.SelectNodes("//CURRENCY_CD");
                    var lineDescrs = header.SelectNodes("//LINE_DESCR");
                    string[] accountsString = new string[accounts.Count];
                    string[] deptIdsString = new string[deptIds.Count];
                    string[] operatingUnitsString = new string[operatingUnits.Count];
                    string[] fundCodesString = new string[fundCodes.Count];
                    string[] programCodesString = new string[programCodes.Count];
                    string[] projectIdsString = new string[projectIds.Count];
                    string[] amountsString = new string[amounts.Count];
                    string[] currenciesString = new string[currencies.Count];
                    string[] lineDescrsString = new string[lineDescrs.Count];
                    //we have to know how many headers to clone in the tempDoc
                    //and append ADDITIONAL journal line items to temp
                    var tempAccounts = header.SelectNodes("//ACCOUNT");
                    string[] tempAccountsString = new string[tempAccounts.Count];
                    //Append line items!!!!

                    XmlDocument soapRequest = new XmlDocument();
                    XMLToString xmlToString = new XMLToString();
                    var headerNodes = doc.SelectNodes("//N_DLAR_JRNL_HDR");
                    // traditional approach
                    string[] transactions = new string[headerNodes.Count];
                    string[] responses = new string[headerNodes.Count];
                    for (int y = 0; y < accounts.Count; y++)
                    {
                        //accountsString[y] = accounts[y].InnerXml;
                        //tempAccounts[y].InnerXml = accounts[y].InnerXml;
                        var accountInLine = tempJournalLine.SelectSingleNode("//ACCOUNT");
                        accountInLine.InnerXml = accounts[y].InnerXml;
                        var deptIdInLine = tempJournalLine.SelectSingleNode("//DEPTID");
                        deptIdInLine.InnerXml = deptIds[y].InnerXml;
                        var operatingUnitInLine = tempJournalLine.SelectSingleNode("//OPERATING_UNIT");
                        operatingUnitInLine.InnerXml = operatingUnits[y].InnerXml;
                        var fundCodeInLine = tempJournalLine.SelectSingleNode("//FUND_CODE");
                        fundCodeInLine.InnerXml = fundCodes[y].InnerXml;
                        var programCodeInLine = tempJournalLine.SelectSingleNode("//PROGRAM_CODE");
                        programCodeInLine.InnerXml = programCodes[y].InnerXml;
                        var projectIdInLine = tempJournalLine.SelectSingleNode("//PROJECT_ID");
                        projectIdInLine.InnerXml = projectIds[y].InnerXml;
                        var amountInLine = tempJournalLine.SelectSingleNode("//MONETARY_AMOUNT");
                        amountInLine.InnerXml = amounts[y].InnerXml;
                        var currencyInLine = tempJournalLine.SelectSingleNode("//CURRENCY_CD");
                        currencyInLine.InnerXml = currencies[y].InnerXml;
                        var lineDescrInLine = tempJournalLine.SelectSingleNode("//LINE_DESCR");
                        lineDescrInLine.InnerXml = lineDescrs[y].InnerXml;
                        var cloneJournalLineNode = tempJournalLine.Clone();
                        tempHeader.AppendChild(cloneJournalLineNode);
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


                    tempDoc.Save("c:\\DLAR_JOURNAL_MSG.xml");
                    soapRequest.Load("c:\\DLAR_JOURNAL_MSG.xml");
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
                    string path = ("C:\\");
                    //System.IO.StreamWriter sw = System.IO.File.AppendText(
                    //GetTempPath() + "My Log File.txt");
                    System.IO.StreamWriter sw = System.IO.File.AppendText(
                        path + "My Log File.txt");
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


                    PsoftGetSoapXmlResponse psoftGetSoapXmlResponse = new PsoftGetSoapXmlResponse();
                    XMLResponse = PsoftGetSoapXmlResponse.PostXMLTransaction("http://pfwlcdcdvm003.nyumc.org:7710/PSIGW/PeopleSoftServiceListeningConnector", "N_DLAR_JOURNAL.v1", "c:\\DLAR_JOURNAL_MSG.xml");

                    //Now traverse through the Response Document to get the message we need


                    if (XMLResponse != null)
                    {
                        XmlNodeList node = XMLResponse.GetElementsByTagName("MsgData");

                        XmlNodeList ComboValidFalgNodes = XMLResponse.GetElementsByTagName("DESCR20");//Returns "Journal Received"
                        responses[i] = ComboValidFalgNodes[1].InnerText;
                        int x = i + 1;
                        //txtOutput.Text = ComboValidFalgNodes[1].InnerText;
                        //txtOutput.Text = i+1 + " " + ComboValidFalgNodes[1].InnerText;
                        //txtOutput.Text = responses[i] + "\r\n" + i + 1 + " Journal Entries Received.";
                        //txtOutput.AppendText (ComboValidFalgNodes[1].InnerText);

                        //NO OUTPUT, BELOW IS FOR WIN FORM APP
                        //txtOutput.AppendText(x + " " + responses[i] + "\r\n");


                    }
                    else
                    {
                        //ResultText.Text = "no value";
                        throw new ApplicationException("Something wrong happened while writing the XML content to the request stream: ");
                    }


                }//for (int i = 0; i < headers.Count; i++)
                //NO OUTPUT, BELOW IS FOR WIN FORM APP
                //txtOutput.AppendText("Total " + headers.Count + " Journal Entries Received.");
            }// using (var xmlTextWriter = XmlWriter.Create(stringWriter))
        }//end GetXML
        
    }
}
