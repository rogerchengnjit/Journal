using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;       // XmlNamespace and others
using System.Xml.Linq;  // LINQ for XML
using System.Xml.XPath; // XPath extensions
using System.IO;// for Stream, StreamReader
// Custom includes:
using System.Net;     // for HttpWebRequest, WebRequest
using System.Web;
using System.Web.Http;

namespace PsoftJournalVoucher
{
    public class PsoftGetSoapXmlResponse
    {

        public static XmlDocument PostXMLTransaction(string strURL, string SOAPAction, string XMLDocLocation)
        {

            //This function is to get XML response for Fund, Dept_ID, Project, Program and operating unit SOAP request
            string pageName = strURL;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(pageName);
            req.Method = "POST";
            req.ContentType = "text/xml;charset=UTF-8";
            req.Headers.Add("SOAPAction", SOAPAction);

            // The parameter values. In a real-world app these would of
            // course be gotten from user input or some other input.

            //string countryName = "United States";

            // Now for the XML. Just build it by brute force.
            XmlDocument soapRequest = new XmlDocument();
            XMLToString xmlToString = new XMLToString();
            //Read XML doc from file
            soapRequest.Load(XMLDocLocation);
            //Assign XML node "REQUESTTYPE" to an object
            //XmlNode nodeDeptId = soapRequest.GetElementsByTagName("REQUESTTYPE").Item(0);

            //Replace the value of "CityName" node
            //string DeptID = nodeDeptId.FirstChild.Value;

            string xmlRequest = xmlToString.GetXMLAsString(soapRequest);


            // Pull the XML request into a UTF-8 byte array for two
            // reasons:
            // 1. We need to set the content length to the byte length.
            // 2. The XML will be pushed into the request stream, which
            //    handles bytes, not characters.
            byte[] reqBytes = new UTF8Encoding().GetBytes(xmlRequest);

            // Now that the request is encoded to a byte array, we can
            // get its byte length. Set the remaining HTTP header value,
            // which is the content-length:
            req.ContentLength = reqBytes.Length;

            // Write the XML to the request stream.
            // Write the request content (the XML) to the request stream.
            try
            {
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(reqBytes, 0, reqBytes.Length);
                }
            }
            catch (WebException e)
            {
                Console.WriteLine("This program is expected to throw WebException on successful run." +
                          "\n\nException Message :" + e.Message);
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    Console.WriteLine("Status Code : {0}", ((HttpWebResponse)e.Response).StatusCode);
                    Console.WriteLine("Status Description : {0}", ((HttpWebResponse)e.Response).StatusDescription);
                }
            } // endcatch (WebException wex)



            // Not sure if this is right, but now begin async call to web request.
            IAsyncResult asyncResult = req.BeginGetResponse(null, null);
            // suspend this thread until call is complete. You might want to
            // do something usefull here like update your UI.
            asyncResult.AsyncWaitHandle.WaitOne();
            // At this point, the HTTP headers are set and the XML
            // content is set. It's time to call the service.
            XmlDocument XMLResponse = new XmlDocument();
                //using (WebResponse webResponse = req.EndGetResponse(asyncResult))
                //{
                    try{
                    HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                    Stream objResponseStream = null;
                    objResponseStream = resp.GetResponseStream();
                    XmlTextReader objXMLReader;
                    objXMLReader = new XmlTextReader(objResponseStream);
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(objXMLReader);
                    //XmlDocument XMLResponse = new XmlDocument();
                    XMLResponse = xmldoc;
                   
                    }

                    catch (System.Net.WebException ex)
                    {
                        var response = (HttpWebResponse)ex.Response;

                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.NotFound: // 404
                                break;

                            case HttpStatusCode.InternalServerError: // 500
                                break;

                            default:
                                throw;
                        }
                    }
                    return XMLResponse;
                //}//using (WebResponse webResponse = req.EndGetResponse(asyncResult))
         
            
        }

    }
}