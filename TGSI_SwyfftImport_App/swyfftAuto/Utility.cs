using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace swyfftAuto
{
    public static class Utility
    {
        public static string PDFName()
        {
            return "true";
        }

        public static void SendMail(string subject, string body)
        {
            try
            {
                string toAddress = ConfigurationManager.AppSettings["ToAddress"];
                string fromAddress = ConfigurationManager.AppSettings["FromAddress"];
                string ccAddress = ConfigurationManager.AppSettings["ccAddress"];
                MailMessage mail = new MailMessage(fromAddress, toAddress);
                SmtpClient client = new SmtpClient();
                client.Port = 25;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Host = ConfigurationManager.AppSettings.Get("ExchangeServer");

                mail.Subject = subject;
                mail.Body = body;
                //client.Send(mail);
            }
            catch (Exception ex)
            {
                
            }

        }
    }
}
