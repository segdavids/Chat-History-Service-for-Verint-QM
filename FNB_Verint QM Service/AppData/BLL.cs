using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace FNB_Verint_QM_Service.AppData
{
    internal class BLL
    {
        public static void logger(Exception e)
        {
            StreamWriter loggerman = null;
            try
            {
                // loggerman = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Log.txt", true);
                loggerman = new StreamWriter(@"C:\EF\Text Capture\Log.txt", true);
                loggerman.WriteLine(DateTime.Now.ToString() + ":" + e.Source.ToString().Trim() + ";" + e.Message.ToString().Trim());
                loggerman.Flush();
                loggerman.Close();
            }
            catch(Exception ex)
            {
                
            }
        }
        /// <summary>
        /// GET DATA FROM DB MSSQL
        /// </summary>
        /// <param name="Query"></param>
        /// <returns></returns>
        public static DataTable GetRequest(string Query)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["VerintDB"].ConnectionString))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter(Query, conn))
                {
                    DataTable dt = new DataTable();
                    sda.Fill(dt);
                    return dt;
                }
            }
        }

        /// <summary>
        /// LOG THE PROGRESS IN FILE
        /// </summary>
        /// <param name="messageex"></param>
        public static void Logger(string messageex)
        {
            StreamWriter custommessage = null;
            //custommessage = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\CustomLog.txt", true);
            custommessage = new StreamWriter(@"C:\EF\Text Capture\CustomLog.txt", true);
            custommessage.WriteLine(DateTime.Now.ToString() + ":" + messageex);
            custommessage.Flush();
            custommessage.Close();
        }



    }
}

