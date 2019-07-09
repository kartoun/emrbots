using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace ICD10_Parser
{
    class Program
    {
        static Dictionary<string, string> ICD10s = new Dictionary<string, string>();

        static string DATA_SERVER = "TBD";
        static string DATABASE = "TBD";

        static void Main(string[] args)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("\\Path\\ICD10CM_FY2014_Full_XML_Tabular.xml");
                        
            XmlNodeList diag = xmlDoc.GetElementsByTagName("diag");

            foreach (XmlElement el1 in diag)
            {           
                ICD10s.Add(el1["name"].InnerText, el1["desc"].InnerText);
            }

            InsertPatients(ICD10s);
        }

        public static void InsertPatients(Dictionary<string, string> ICD10s)
        {
            try
            {
                DataTable listRows = new DataTable();
                listRows.Columns.Add("Code", typeof(String));
                listRows.Columns.Add("Description", typeof(String));
                
                foreach (var code in ICD10s)
                {
                    DataRow dr = listRows.NewRow();

                    dr["Code"] = code.Key;
                    dr["Description"] = code.Value;                    

                    listRows.Rows.Add(dr);
                }

                SqlConnection bulkSqlConnector =
                    new SqlConnection("integrated security=SSPI;persist security info=False;Trusted_Connection=Yes;server="
                        + DATA_SERVER + ";" + "database=" + DATABASE + ";connection timeout=0");

                using (SqlConnection connection = bulkSqlConnector)
                {
                    connection.Open();

                    using (SqlBulkCopy bulkcopy = new SqlBulkCopy(connection))
                    {
                        bulkcopy.DestinationTableName = "dbo." + "ICD10Table";
                        bulkcopy.BulkCopyTimeout = 30;

                        try
                        {
                            bulkcopy.WriteToServer(listRows);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }

                        connection.Close();
                    }
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Connection Functions

        static SqlConnection myConnectionMain;

        static void OpenConnectionMain()
        {
            myConnectionMain = new SqlConnection("integrated security=SSPI;persist security info=False;Trusted_Connection=Yes;server=" + DATA_SERVER + ";" + "database=" + DATABASE + ";connection timeout=0");

            try
            {
                myConnectionMain.Open();
            }
            catch (Exception e)
            {
                Console.Write("Connection problems...");
            }
        }

        static void CloseConnectionMain()
        {
            myConnectionMain.Dispose();
        }

        #endregion Connection
    }
}