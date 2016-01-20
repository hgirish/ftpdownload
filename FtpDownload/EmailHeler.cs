using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Postal;

namespace FtpDownload
{
    public class EmailHeler
    {
        public void SendEmail(IEnumerable<FileDetail> fileList)
        {
            var viewsPath = Path.GetFullPath(@"..\..\Views\Emails");

            var engines = new ViewEngineCollection();
            engines.Add(new FileSystemRazorViewEngine(viewsPath));

            var service = new EmailService(engines);

            dynamic email = new Email("FtpFiles");
            email.To = "test@example.com";
            // Will look for Test.cshtml or Test.vbhtml in Views directory.
            email.Message = "Hello, non-asp.net world!";
            email.FileList = fileList;
            service.Send(email);
        } 
    }
}