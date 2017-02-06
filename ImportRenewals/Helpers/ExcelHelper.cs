using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Web;

namespace ImportRenewals.Helpers
{
    public class ExcelHelper
    {
        public static DataSet ExcelToDataSet(String fileName, String fileExtension)
        {
            try
            {
                string conStr = "";
                switch (fileExtension.ToLower())
                {
                    case ".xls": //Excel 97-03
                                 /* conStr = ConfigurationManager.ConnectionStrings["Excel03ConString"]
                                           .ConnectionString;*/
                        conStr = ConfigurationManager.ConnectionStrings["Excel07ConString"]
                         .ConnectionString;

                        break;
                    case ".xlsx": //Excel 07
                        conStr = ConfigurationManager.ConnectionStrings["Excel07ConString"]
                                  .ConnectionString;
                        break;
                    case ".xlt": //Excel Model
                        conStr = ConfigurationManager.ConnectionStrings["Excel07ConString"]
                                  .ConnectionString;
                        break;
                    default:
                        break;
                }

                conStr = String.Format(conStr, fileName, "No", "1");
                OleDbConnection connection = new OleDbConnection(conStr);
                connection.Open();

                DataTable dtExcelSchema = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                DataRow row = dtExcelSchema.Rows[0];
                string SheetName = row["TABLE_NAME"].ToString();

                DataSet ds = new DataSet();
                OleDbCommand cmdExcel = new OleDbCommand();
                OleDbDataAdapter oda = new OleDbDataAdapter();
                cmdExcel.Connection = connection;
                cmdExcel.CommandText = "SELECT * From [" + SheetName + "]";
                oda.SelectCommand = cmdExcel;
                oda.Fill(ds);
                connection.Close();

                return ds;
            }
            catch (Exception e)
            {
                throw e;
            }

        }
    }
}