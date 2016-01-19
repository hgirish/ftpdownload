using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;

namespace FtpDownload
{
    class Program
    {
        private static FoxproHelper _dbHelper;
        static void Main(string[] args)
        {
            //ReadFoxproTable();
            //return;
             _dbHelper = new FoxproHelper();
            //InsertRecord();
            //return;
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
            

        }

        private static void InsertRecord()
        {
            FileDetail detail = new FileDetail();
            detail.DownloadTime = DateTime.Now;
            detail.FileDate = DateTime.Now;
            detail.FileName = "TEST";
            detail.Folder = "TEST";
            detail.Records = 99;
            _dbHelper.InsertFtpRecord(detail);
        }
        private static void ReadFoxproTable()
        {
            FoxproHelper dbHelper = new FoxproHelper();
            var table = dbHelper.GetYourData();
            foreach (DataColumn column in table.Columns)
            {
                Console.WriteLine(column.ColumnName);
            }
            foreach (DataRow r in table.Rows)
            {
                Console.WriteLine($"{r[0]}\t{r[1]}\t{r[2]}");
            }
            foreach (DataRow row in table.Rows)
            {
                foreach (var item in row.ItemArray)
                {
                    Console.Write("Value:" + item + "\t");
                }
            }

            return;
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
                            var lineCount = File.ReadLines(ftpFileName).Count();
                            // minus 2 for Header line and blank line after header line

                            FileDetail fileDetail = new FileDetail
                            {
                                FileName = fileName,
                                DownloadTime = DateTime.Now,
                                FileDate = fileTime,
                                Folder = folder,
                                Records = lineCount - 2
                            };

                            _dbHelper.InsertFtpRecord(fileDetail);
                        }
                    }
                }
            }
        }
    }

        
    }

