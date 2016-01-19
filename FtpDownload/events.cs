/* Author: Prahlad Yeri
 * Created: 18-07-2012
 */

using System;

namespace FtpDownload
{
    public class DownloadCompleteEventArgs : EventArgs
    {
        public string Filename;
    }

    public class UploadCompleteEventArgs : EventArgs
    {
        public string Filename;
    }
    public class DownloadProgressEventArgs : EventArgs
    {
        public string Filename;
        public long BytesTransferred;
    }

    public class UploadProgressEventArgs : EventArgs
    {
        public string Filename;
        public long BytesTransferred;
    }

    public class StatusChangeEventArgs : EventArgs
    {
        public string Message;
        public long BytesUploaded = 0;
        public long BytesDownloaded = 0;
    }
}
