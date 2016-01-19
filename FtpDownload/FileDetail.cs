using System;

namespace FtpDownload
{
    public class FileDetail
    {
        public string FileName { get; set; }
        public string Folder { get; set; }
        public DateTime FileDate { get; set; }
        public DateTime DownloadTime { get; set; }
        public int Records { get; set; }
    }
}