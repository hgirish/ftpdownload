using System;
using System.Configuration;
using System.IO;

namespace FtpDownload
{
    class Program
    {
        static void Main(string[] args)
        {
            var siteurl = ConfigurationManager.AppSettings["ftpUrl"];
            var ftpUserName = ConfigurationManager.AppSettings["ftpUserName"];
            var ftpPassword = ConfigurationManager.AppSettings["ftpPassword"];
              Ftp ftp = new Ftp();
            //  Ftper ftper = new Ftper();
            // var files = ftper.Connect(siteurl, ftpUserName, ftpPassword);
            var files = ftp.Connect(siteurl, ftpUserName, ftpPassword);
            foreach (var ftpinfo in files)
            {
               // Console.WriteLine($"File Name: {ftpinfo.Filename}, File Type: {ftpinfo.FileType}");
                GetFtpFiles(ftpinfo, ftp, siteurl);
            }
            return;
            //FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(siteurl);
            //var networkCredential = new NetworkCredential(ftpUserName, ftpPassword);
            //ftpRequest.Credentials = networkCredential;
            //ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            //FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
            //StreamReader streamReader = new StreamReader(response.GetResponseStream());
            //List<string> directories = new List<string>();

            //string line = streamReader.ReadLine();
            //while (!string.IsNullOrEmpty(line))
            //{
            //    directories.Add(line);
            //    line = streamReader.ReadLine();
            //}
            //streamReader.Close();


            //using (WebClient ftpClient = new WebClient())
            //{
            //    ftpClient.Credentials = networkCredential;

            //    for (int i = 0; i <= directories.Count - 1; i++)
            //    {
            //        var s = directories[i];
            //        if (s.Contains("."))
            //        {

            //            string path = siteurl + s;
            //            string trnsfrpth = @"H:\\tmp\" + s;
            //            ftpClient.DownloadFile(path, trnsfrpth);
            //        }
            //    }
        }

        private static void GetFtpFiles(Ftpinfo ftpinfo, Ftp ftp, string siteurl)
        {
          
            if (ftpinfo.FileType == DirectoryEntryTypes.Directory)
            {
                var subFiles = ftp.Browse(siteurl + ftpinfo.Filename);
                if (subFiles == null)
                {
                    return;
                }
                foreach (var subFile in subFiles)
                {
                   
                   // Console.WriteLine($"File Name: {subFile.Filename}, File Type: {subFile.FileType}");
                    if (subFile.FileType == DirectoryEntryTypes.Directory)
                    {
                        var path = subFile.Path;
                        if (!path.EndsWith("/"))
                        {
                            path = path + "/";
                        }
                        GetFtpFiles(subFile, ftp, path );
                    }
                    else
                    {
                        var fileTime = subFile.FileDateTime;
                        var kftpfolder = ConfigurationManager.AppSettings["ftpSaveDestination"]; 
                        var fileName = subFile.Filename;
                        var fileDate = fileTime.Date;
                        var arr = subFile.Path.Split('/');
                        var folder = arr[arr.Length-1];
                        var folderFullPath = Path.Combine(kftpfolder, folder, fileDate.ToString("MMddyyyy"));
                        if (!Directory.Exists(folderFullPath))
                        {
                            Directory.CreateDirectory(folderFullPath);
                        }
                        var ftpFileName = Path.Combine(folderFullPath, fileName);
                        Console.WriteLine($"Name: {subFile.Filename}\tSize: {subFile.Size}");
                        if (File.Exists(ftpFileName))
                        {
                           // Console.WriteLine($"{ftpFileName} already exists");
                        }
                        else
                        {
                            Console.WriteLine($"New file found: {fileName}");
                            var path = subFile.Path;
                            if (!path.EndsWith("/"))
                            {
                                path = path + "/";
                            }
                            FtpDownload.Fileinfo fileInfo = new FtpDownload.Fileinfo(path + fileName,ftpFileName,DirectionEnum.Down,true);
                           // ftp.AddFileToDownloadQueue(path + fileName, ftpFileName);
                           // ftp.StartProcessing();
                            ftp.Download(fileInfo);
                        }
                    }
                }
            }
        }
    }

        
    }

