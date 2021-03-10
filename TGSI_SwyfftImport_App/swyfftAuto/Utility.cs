using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
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
                //client.Port = 25;
                //client.DeliveryMethod = SmtpDeliveryMethod.Network;
                //client.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["SMTPUserName"], ConfigurationManager.AppSettings["SMTPPassword"]);
                //client.UseDefaultCredentials = true;
                //client.Host = ConfigurationManager.AppSettings.Get("ExchangeServer");
                //client.EnableSsl = true;
                mail.Subject = subject;
                mail.Body = body;
                client.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.Read();
            }

        }
    }
}
