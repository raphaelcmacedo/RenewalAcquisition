using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace ImportRenewals.Email
{
    public class _base
    {
        public void EnviarEmail(List<string> mailTo, string nomeSistema, string assunto, string body, List<string> cc = null, List<string> attachs = null, List<string> bcc = null)
        {
            MailMessage mail = new MailMessage();
            List<string> address;

            //remetente do email
            mail.From = new MailAddress("noreply@westcon.com.br");

            //preenche endereços para quais o email será enviado
            mail.To.Clear();
            if (mailTo != null && mailTo.Count > 0)
            {
                for (int i = 0; i < mailTo.Count; i++)
                {
                    //valida endereço recebido
                    address = this.FormatEmailAddress(mailTo[i]);
                    for (int x = 0; x < address.Count; x++)
                    {
                        if (address[x] != "") mail.To.Add(address[x]);
                    }
                    //if (mailTo[i] != "") mail.To.Add(mailTo[i]);
                }
            }

            //preenche campo para cópias de email
            mail.CC.Clear();
            if (cc != null && cc.Count > 0)
            {
                for (int i = 0; i < cc.Count; i++)
                {
                    //valida endereço recebido
                    address = this.FormatEmailAddress(cc[i]);
                    for (int x = 0; x < address.Count; x++)
                    {
                        if (address[x] != "") mail.CC.Add(address[x]);
                    }
                    //if (cc[i] != "") mail.CC.Add(cc[i]);
                }
            }

            //preenche campo para cópias ocultas de email
            mail.Bcc.Clear();
            if (bcc != null && bcc.Count > 0)
            {
                for (int i = 0; i < bcc.Count; i++)
                {
                    //valida endereço recebido
                    address = this.FormatEmailAddress(bcc[i]);
                    for (int x = 0; x < address.Count; x++)
                    {
                        if (address[x] != "") mail.Bcc.Add(address[x]);
                    }
                }
            }
            //preenche anexos
            if (attachs != null && attachs.Count > 0)
            {
                Attachment file;
                for (int i = 0; i < attachs.Count; i++)
                {
                    file = new Attachment(attachs[i]);
                    mail.Attachments.Add(file);
                }
            }

            //assunto do email
            mail.Subject = nomeSistema + " - " + assunto;

            //corpo do email
            mail.IsBodyHtml = true;
            mail.Body = body;

            //preenche dados do SMTP
            SmtpClient mailClient = new SmtpClient(Helpers.Settings.ServidorSMTP, 25);
            mailClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            //mailClient.Credentials = new System.Net.NetworkCredential("westcon\\TrackitUser", "zD8ivsRo");

            //realiza tentativa de envio de email
            mailClient.Timeout = 600000;
            mailClient.Send(mail);
            mail.Attachments.Dispose();
        }

        private List<string> FormatEmailAddress(string email)
        {
            List<string> mailList = new List<string>();

            //caso email passado não possua "@", retornar lista vazia
            if (!email.Contains("@")) return mailList;

            //caso endereço de email não possua ";" como separador, adiciona-lo à lista e retorná-la
            if (!email.Contains(";"))
            {
                mailList.Add(email);
                return mailList;
            }

            //caso email contenha ";" como separador, quebrar endereços
            if (email.Contains(";"))
            {
                string[] mails = email.Split(';');
                for (int i = 0; i < mails.Length; i++)
                {
                    if (mails[i] != "" && mails[i].Contains('@')) mailList.Add(mails[i].Trim());
                }
            }

            return mailList;
        }
    }
}