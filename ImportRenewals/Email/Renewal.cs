using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ImportRenewals.Email
{
    public class Renewal:_base
    {
        public void Success(List<string> message,int success,int error,string email)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.Append("<head>");
            sb.Append("     <title>Renewals</title>");
            sb.Append("     <meta http-equiv='Content-Type' content='text/html; charset=iso-8859-1'>");
            sb.Append("     <style type='text/css'>");
            sb.Append("         <!-- td{font-family:Verdana; font-size:10px}-->");
            sb.Append("     </style>");
            sb.Append("</head>");
            sb.Append("<body>");
            sb.Append("<table width='547' border='1' cellspacing='0' cellpadding='5'>");
            sb.Append("     <tr>");
            sb.Append("         <td colspan='3'> The data has been successfully read and saved in our database</td>");
            sb.Append("     </tr>");
            sb.Append("     <tr>");
            sb.Append("         <td colspan='3'> We have saved "+ success.ToString() +" lines and found "+error.ToString()+" errors </td>");
            sb.Append("     </tr>");
            sb.Append("     <tr>");
            sb.Append("         <td colspan='3'>&nbsp;</td>");
            sb.Append("     </tr>");
            if (error > 0)
            {
                sb.Append("     <tr>");
                sb.Append("         <td colspan='3'>Errors</td>");
                sb.Append("     </tr>");
                sb.Append("     <tr>");
                sb.Append("         <td colspan='3'>&nbsp;</td>");
                sb.Append("     </tr>");
                for (int i =0,len = message.Count; i < len; i++)
                {
                    sb.Append("     <tr>");
                    sb.Append("         <td colspan='3'>" + message[i] + "</td>");
                    sb.Append("     </tr>");
                }
            }
            sb.Append("     <tr>");
            sb.Append("         <td colspan='3' height='1' bgcolor='#175BA4'></td>");
            sb.Append("     </tr>");
            sb.Append("</table>");
            sb.Append("<br/><br/>");
            sb.Append("</body>");
            sb.Append("</html>");

            this.EnviarEmail(new List<string> { email }, "Renewals", "CSV file reading", sb.ToString());
        }

        public void Error(string message)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.Append("<head>");
            sb.Append("     <title>Renewals Error</title>");
            sb.Append("     <meta http-equiv='Content-Type' content='text/html; charset=iso-8859-1'>");
            sb.Append("     <style type='text/css'>");
            sb.Append("         <!-- td{font-family:Verdana; font-size:10px}-->");
            sb.Append("     </style>");
            sb.Append("</head>");
            sb.Append("<body>");
            sb.Append("<table width='547' border='1' cellspacing='0' cellpadding='5'>");
            sb.Append("     <tr>");
            sb.Append("         <td colspan='3'> We encountered an error while we're reading the file</td>");
            sb.Append("     </tr>");
            sb.Append("     <tr>");
            sb.Append("         <td colspan='3'> "+ message + " </td>");
            sb.Append("     </tr>");            
            sb.Append("     <tr>");
            sb.Append("         <td colspan='3' height='1' bgcolor='#175BA4'></td>");
            sb.Append("     </tr>");
            sb.Append("</table>");
            sb.Append("<br/><br/>");
            sb.Append("</body>");
            sb.Append("</html>");

            this.EnviarEmail(new List<string> { "Hugo.Souza@westcon.com","Raphael.Macedo@westcon.com" }, "Renewals", "Error at Renewals", sb.ToString());
        }

    }
}