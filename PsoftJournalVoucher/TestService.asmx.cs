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

namespace PsoftJournalVoucher
{
    /// <summary>
    /// Summary description for TestService
    /// </summary>
    [WebService(Namespace = "http://nyumc.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class TestService : System.Web.Services.WebService
    {

        [WebMethod]
        public string TestConnection(string EndDate)
        {
            string connStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            
            // Create our DB connection.
            SqlConnection conn = new SqlConnection(connStr);
            //SqlCommand cmd = new SqlCommand("select top 5 * from dbo.[__IACUC Study_CustomAttributesManager]", conn);
            
            //using new procedure
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataTable dt = new DataTable();

            cmd = new SqlCommand("dbo.invoicesByDate", conn);
            cmd.Parameters.Add(new SqlParameter("@EndDate", EndDate));
            cmd.CommandType = CommandType.StoredProcedure;
            da.SelectCommand = cmd;
            DataSet ds = new DataSet();

            // Use a DataAdapter to fill the dataset with four tables at once
            SqlDataAdapter a = new SqlDataAdapter(cmd);
            a.Fill(ds);



            //cmd.Dispose();
            //conn.Close();
         

            using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter))
            {
                ds.DataSetName = "Transaction";
                ds.Tables[0].TableName = "N_DLAR_JRNL_HDR";
                ds.WriteXml(xmlTextWriter);
                xmlTextWriter.Close();

                XmlDocument doc = new XmlDocument();

                string dsXML = ds.GetXml();
                return dsXML;
                //doc.LoadXml(dsXML);
            }
        }
    }
}
