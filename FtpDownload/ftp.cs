/* Author: Prahlad Yeri
 * Created: 18-07-2012
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace FtpDownload
{
    /// <summary>
    /// Abstraction of the ftp file transfer functionality
    /// </summary>
    public class Ftp
    {
        public event EventHandler<DownloadCompleteEventArgs> DownloadComplete;
        public event EventHandler<UploadCompleteEventArgs> UploadComplete;
        public event EventHandler<DownloadProgressEventArgs> DownloadProgress;
        public event EventHandler<UploadProgressEventArgs> UploadProgress;
        public event EventHandler<StatusChangeEventArgs> StatusChange;

        public SynchronizationContext Context;

        internal bool IsConnected = false;


        private string _username = "";
        private string _password = "";

        #region eventStuff

        void OnDownloadComplete(string filename)
        {
            //save delegate field in a temporary field for thread-safety
            EventHandler<DownloadCompleteEventArgs> temp = DownloadComplete;
            Console.WriteLine($"{filename} DownloadComplete");
            if (temp != null)
            {
                //if there is a listener who as asked for notification, then give away!
                DownloadCompleteEventArgs e = new DownloadCompleteEventArgs();
                e.Filename = filename;
                //temp(this, e);
                Context.Post(delegate { DownloadComplete(this, e); }, null);
            }
        }

        void OnUploadComplete(string filename)
        {
            //save delegate field in a temporary field for thread-safety
            EventHandler<UploadCompleteEventArgs> temp = UploadComplete;

            if (temp != null)
            {
                //if there is a listener who as asked for notification, then give away!
                UploadCompleteEventArgs e = new UploadCompleteEventArgs();
                e.Filename = filename;
                //uploadComplete(this, e);
                Context.Post(delegate { UploadComplete?.Invoke(this, e); }, null);
            }
        }

        void OnDownloadProgress(string filename, long bytesTransferred)
        {
            Console.WriteLine($"{filename} Progress: {bytesTransferred}");
            EventHandler<DownloadProgressEventArgs> temp = DownloadProgress;
            if (temp != null)
            {
                DownloadProgressEventArgs e = new DownloadProgressEventArgs();
                e.Filename = filename;
                e.BytesTransferred = bytesTransferred;
                //downloadProgress(this, e);
                //temp(this, e);
                Context.Post(delegate { DownloadProgress?.Invoke(this, e); }, null);
            }
        }

        void OnUploadProgress(string filename, long bytesTransferred)
        {
            EventHandler<UploadProgressEventArgs> temp = UploadProgress;
            if (temp != null)
            {
                UploadProgressEventArgs e = new UploadProgressEventArgs();
                e.Filename = filename;
                e.BytesTransferred = bytesTransferred;
                //uploadProgress(this, e);
                Context.Post(delegate { UploadProgress?.Invoke(this, e); }, null);
            }
        }

        internal void OnStatusChange(string message, long uploaded, long downloaded)
        {
            //save delegate field in a temporary field for thread-safety
            EventHandler<StatusChangeEventArgs> temp = StatusChange;

            if (temp != null)
            {
                //if there is a listener who as asked for notification, then give away!
                StatusChangeEventArgs e = new StatusChangeEventArgs();
                e.Message = message;
                e.BytesUploaded = uploaded;
                e.BytesDownloaded = downloaded;
                //statusChange(this, e);
                Context.Post(delegate { StatusChange?.Invoke(this, e); }, null);
            }
        }
        #endregion

        internal Ftp()
        {
            Context = SynchronizationContext.Current;
        } //hide constructor from the outside world.

        //tests connection and browse to home-directory
        public List<Ftpinfo> Connect(string host, string username, string password)
        {
            _username = username;
            _password = password;

            //FtpWebRequest.Create(host); //test connect;
            Context = SynchronizationContext.Current;

            return Browse(host);
        }

        //public bool exists(string remotefilename)
        //{
        //    return true;
        //}

        /// <summary>
        /// Fetch all files/folders in this directory and return the ftpinfo array.
        /// </summary>
        public List<Ftpinfo> Browse(string path) //eg: "ftp.xyz.org", "ftp.xyz.org/ftproot/etc"
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            List<Ftpinfo> files = new List<Ftpinfo>();

            //request.Proxy = System.Net.WebProxy.GetDefaultProxy();
            //request.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
            request.Credentials = new NetworkCredential(_username, _password);
            Stream rs = request.GetResponse().GetResponseStream();

            OnStatusChange("CONNECTED: " + path, 0, 0);

            if (rs != null)
            {
                StreamReader sr = new StreamReader(rs);
                string strList = sr.ReadToEnd();
                string[] lines = null;

                if (strList.Contains("\r\n"))
                {
                    lines = strList.Split(new[] { "\r\n" }, StringSplitOptions.None);
                }
                else if (strList.Contains("\n"))
                {
                    lines = strList.Split(new[] { "\n" }, StringSplitOptions.None);
                }

                //now decode this string array

                if (lines == null || lines.Length == 0)
                    return null;

                foreach (string line in lines)
                {
                    if (line.Length == 0)
                        continue;
                    //parse line
                    Match m = GetMatchingRegex(line);
                    if (m == null)
                    {
                        //failed
                        throw new ApplicationException("Unable to parse line: " + line);
                    }

                    Ftpinfo item = new Ftpinfo();
                    item.Filename = m.Groups["name"].Value.Trim('\r');
                    item.Path = path;
                    if (m.Groups["size"] != null && !string.IsNullOrWhiteSpace(m.Groups["size"].Value))
                        item.Size = Convert.ToInt64(m.Groups["size"].Value);
                    if ((m.Groups["permission"] != null))
                        item.Permission = m.Groups["permission"].Value;

                    string dir = m.Groups["dir"].Value;
                    if (dir.Length > 0 && dir != "-")
                    {
                        item.FileType = DirectoryEntryTypes.Directory;
                    }
                    else
                    {
                        item.FileType = DirectoryEntryTypes.File;
                    }

                    try
                    {
                        item.FileDateTime = DateTime.Parse(m.Groups["timestamp"].Value);
                    }
                    catch
                    {
                        item.FileDateTime = DateTime.MinValue; //null;
                    }

                    files.Add(item);
                }
            }

            return files;
        }

        public string CreateRemoteDirectory(Fileinfo file)
        {
            //System.IO.FileInfo info = new FileInfo(file.completeFileName);
            //This is actually a directory:
            //  string filename = file.CompleteFileName;//.Substring(file.completeFileName.LastIndexOf(@"\") + 1);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(file.Destination);
            request.Credentials = new NetworkCredential(_username, _password);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            request.UseBinary = true;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            return response.StatusDescription;
            //request.UseBinary = true;
            //request.ContentLength = info.Length;
        }

        public void Download(Fileinfo file)
        {
            FileInfo info = new FileInfo(file.Destination);
            if (info.Exists)
            {
                return;
                // info.Delete();
            }

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(file.CompleteFileName);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.UseBinary = true;
            request.Credentials = new NetworkCredential(_username, _password);
            //FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            //Stream responseStream = response.GetResponseStream();
            //StreamReader reader = new StreamReader(responseStream);

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        using (StreamWriter destination = new StreamWriter(file.Destination))
                        {
                            destination.Write(reader.ReadToEnd());
                            destination.Flush();
                        }
                    }

                }
            }


            //using (Stream rs = response.GetResponseStream())
            //{
            //    using (FileStream fs = info.OpenWrite())
            //    {
            //        try
            //        {
            //            int bufferSize = 2048;
            //            byte[] bytes = new byte[bufferSize];
            //            int totBytes = 0;
            //            int read = 0;
            //            do
            //            {
            //                if (rs != null) read = rs.Read(bytes, 0, bufferSize);
            //                fs.Write(bytes, 0, read);
            //                totBytes += read;
            //                //TODO: Raise status event to notify the bytes transferred
            //                OnDownloadProgress(file.CompleteFileName, totBytes);
            //            } while (read > 0 );
            //            rs?.Flush();
            //            OnDownloadComplete(file.CompleteFileName);
            //        }
            //        catch (Exception ex)
            //        {
            //            OnStatusChange("Error occured: " + ex.Message, 0, 0);
            //        }
            //        finally
            //        {
            //            fs.Close();
            //            rs?.Close();
            //        }  
            //    }
            //}
        }

        public void Upload(Fileinfo file)
        {
            FileInfo info = new FileInfo(file.CompleteFileName);
            string filename = file.CompleteFileName.Substring(file.CompleteFileName.LastIndexOf(@"\") + 1);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(file.Destination + "/" + filename);
            request.Credentials = new NetworkCredential(_username, _password);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.UseBinary = true;
            request.ContentLength = info.Length;

            int bufferSize = 2048;
            byte[] bytes = new byte[bufferSize];
            int read = 0;
            long totBytes = 0;
            Stream rs = request.GetRequestStream();
            using (FileStream fs = info.OpenRead())
            {
                try
                {
                    do
                    {
                        read = fs.Read(bytes, 0, bufferSize);
                        rs.Write(bytes, 0, read);
                        totBytes += read;
                        //TODO: Raise status event to notify the bytes transferred
                        OnUploadProgress(file.CompleteFileName, totBytes);
                    } while (read == bufferSize);
                    OnUploadComplete(file.CompleteFileName);
                }
                catch { }
                finally
                {
                    fs.Close();
                }
            }
            rs.Close();
            request = null;
            return;
        }

        private Match GetMatchingRegex(string line)
        {
            string[] formats = {
                        @"(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\w+\s+\w+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d{4})\s+(?<name>.+)" ,
                        @"(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\d+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d{4})\s+(?<name>.+)" ,
                        @"(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\d+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d{1,2}:\d{2})\s+(?<name>.+)" ,
                        @"(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\w+\s+\w+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d{1,2}:\d{2})\s+(?<name>.+)" ,
                        @"(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})(\s+)(?<size>(\d+))(\s+)(?<ctbit>(\w+\s\w+))(\s+)(?<size2>(\d+))\s+(?<timestamp>\w+\s+\d+\s+\d{2}:\d{2})\s+(?<name>.+)" ,
                        @"(?<timestamp>\d{2}\-\d{2}\-\d{2}\s+\d{2}:\d{2}[Aa|Pp][mM])\s+(?<dir>\<\w+\>){0,1}(?<size>\d+){0,1}\s+(?<name>.+)"};
            Regex rx;
            Match m;
            for (int i = 0; i < formats.Length; i++)  //As Integer = 0 To formats.Length - 1
            {
                rx = new Regex(formats[i]);
                m = rx.Match(line);
                if (m.Success)
                {
                    return m;
                }
            }
            return null;
        }
    }
}
