using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;
using Postal;

namespace FtpDownload
{
    public class EmailHeler
    {
        private string _emailRecipients;

        public EmailHeler()
        {
            _emailRecipients = ConfigurationManager.AppSettings["emailReceivers"];
        }
        // Sending email using postal.mvc5 package
        public void SendEmail(IEnumerable<FileDetail> fileList)
        {
            var viewsPath = Path.GetFullPath(@"..\..\Views\Emails");

            var engines = new ViewEngineCollection();
            engines.Add(new FileSystemRazorViewEngine(viewsPath));

            var service = new EmailService(engines);

            dynamic email = new Email("FtpFiles");
            email.To = _emailRecipients;
            // Will look for Test.cshtml or Test.vbhtml in Views directory.
            email.Message = "Hello, non-asp.net world!";
            email.FileList = fileList;
            service.Send(email);
        }
        // Sending mail without using outside packages
        public void SendMail(IEnumerable<FileDetail> fileList)
        {
            MailMessage message = new MailMessage();
           
            message.To.Add(_emailRecipients);
            message.Subject = "FTP File Downloaded today";
            StringBuilder builder = new StringBuilder();
            builder.Append("<table border='1' cellpadding='10' cellspacing='5' style='border-collapse:collapse;mso-table-lspace:0; mso-table-rspace:0;'>");
            builder.Append("<thead><tr><th>File Name</th><th>Records</th><th>Upload Time</th></tr></thead><tbody>");
            foreach (var detail in fileList)
            {
                builder.Append($"<tr><td>{detail.FileName}</td><td>{detail.Records}</td><td>{detail.FileDate}</td></tr>");
            }
            builder.Append("</tbody></table>");
            message.Body = builder.ToString();
            message.IsBodyHtml = true;
            SmtpClient client = new SmtpClient();
            client.Send(message);
        }
    }
}