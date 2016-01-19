/* Author: Prahlad Yeri
 * Created: 18-07-2012
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FtpDownload
{
	/// <summary>
	/// Primary interface class to use the ftp commands
	/// </summary>
	public class Ftper
	{
		public bool LoggingEnabled=false;

		public Ftp Ftpobject=new Ftp();
		private Hashtable _queue=new Hashtable();
		private bool _threadRunning;

		~Ftper()
		{
			//make sure to clean up all variables
			Ftpobject=null;
			_queue=null;
		}
		
		public bool IsProcessing()
		{
			return _threadRunning; //check if a thread is running for up/download
		}

		public List<Ftpinfo> Connect(string host, string username, string password)
		{
			return Ftpobject.Connect(host,username,password);
		}
		
		
		public void Disconnect()
		{
			if (_threadRunning)
			{
				_threadRunning=false;
			}
			
			int timeout=60; //seconds
			DateTime start=DateTime.Now;
			while(_queue.Count==0) //wait till running up/download threads complete.
			{
				if (DateTime.Now.Subtract(start).Seconds>timeout)
					break;
			}
		}

        public List<Ftpinfo> Browse(string path)
        {
            return Ftpobject.Browse(path);
        }
		

        //remote destination should be entire path including name of the folder to be
        //transferred. eg: C:\foo   =>  /remoteftpsite/blah-folder/foo
        /// <summary>
        /// Adds the specified folder to the upload queue
        /// </summary>
        /// <param name="path">Complete path of the local folder</param>
        /// <param name="remoteDestination">Complete path of remote destination including the local folder name</param>
		public void AddFolderToUploadQueue(string path, string remoteDestination)
		{
            //path must be a valid directory. curse thru it.
            //List<ftpinfo> contents = ftpobject.browse(path);
            string[] contents = Directory.GetFiles(path);
            for (int i = 0; i < contents.Length; i++)
            {
                AddFileToUploadQueue(contents[i], remoteDestination);
            }

            contents = Directory.GetDirectories(path);
            for (int i = 0; i < contents.Length; i++)
            {
                string filePart = StringUtils.ExtractFileFromPath(contents[i],@"\");
                AddFolderToUploadQueue(contents[i], remoteDestination + "/" +  filePart);
            }
        }

		
		public void AddFileToUploadQueue(string localFileName, string remoteDestination)
		{
			if (File.Exists(localFileName))
			{
				//uploadQ.Enqueue(localFileName);
                _queue.Add(remoteDestination, new Fileinfo(remoteDestination,remoteDestination,DirectionEnum.Up,true)); //ensure that the directory exists
				_queue.Add(localFileName,new Fileinfo(localFileName,remoteDestination, DirectionEnum.Up));
			}
			else
			{
				throw new Exception("Incorrect file path: " + localFileName);
			}
		}
		
		public void RemoveFilesFromUploadQueue(string[] localFileName)
		{
			foreach(string s in localFileName)
			{
				if (_queue.ContainsKey(s))
				{
					_queue.Remove(s);
				}
			}
		}
		
		public void AddFolderToDownloadQueue(string path, string localDestination)
		{
            if (Directory.Exists(localDestination) == false)
            {
                Directory.CreateDirectory(localDestination);
            }
            //path must be a valid directory. curse thru it.
            List<Ftpinfo> contents = Ftpobject.Browse(path);
            if (contents == null)
                return;
            for (int i = 0; i < contents.Count; i++)
            {
                if (contents[i].FileType == DirectoryEntryTypes.File)
                {
                    AddFileToDownloadQueue(contents[i].Path + "/" + contents[i].Filename, localDestination);
                }
                else
                {
                    AddFolderToDownloadQueue(path + "/" + contents[i].Filename,localDestination + @"\" + contents[i].Filename);
                }
            }
		}

        public void AddFileToDownloadQueue(string remotefilename, string localDestination)
		{
            _queue.Add(remotefilename, new Fileinfo(remotefilename, localDestination, DirectionEnum.Down));
		}
		
		public void RemoveFilesFromDownloadQueue(string[] remotefilename)
		{
			foreach(string s in remotefilename)
			{
				if (_queue.Contains(s))
				{
					_queue.Remove(s);
				}
				else
				{
					throw new Exception("File does not exist: " + s);
				}
			}
		}
		
		//start the processing thread
		public void StartProcessing()
		{
			_threadRunning=true;
			ThreadPool.QueueUserWorkItem(ThreadForProcessQueue);
		}
		
		//stop the processing thread
		public void StopProcessing()
		{
			_threadRunning=false;
		}
		
		private void ThreadForProcessQueue(object stateInfo)
		{
			// No state object was passed to QueueUserWorkItem, so 
        	// stateInfo is null.

        	try
        	{
				while(_threadRunning && _queue.Count>0)
				{
					//process next queue item
                    object[] keys = new object[_queue.Keys.Count];
                    _queue.Keys.CopyTo(keys, 0);
					Fileinfo nextitem = (Fileinfo)_queue[keys[0]]; //process first item in the queue
                    if (nextitem.MkdirFlag)
                    {
                        Ftpobject.CreateRemoteDirectory(nextitem);
                    }
                    else
                    {
                        if (nextitem.Direction == DirectionEnum.Down)
                        {
                            Ftpobject.Download(nextitem);
                        }
                        else
                        {
                            Ftpobject.Upload(nextitem);
                        }
                    }
					//remove item from queue after processing it
					_queue.Remove(nextitem.CompleteFileName);
				}
        	}
        	catch(Exception ex)
            {
                Ftpobject.OnStatusChange("Error occured: " + ex.Message, 0, 0);
        	}
        	finally{
				_threadRunning=false;
        	}
		}
	}

    /// <summary>
    /// Hold information about the direction of transfer i.e. upload/download
    /// </summary>
	public enum DirectionEnum{
		Up,
		Down
	}
	
	public enum DirectoryEntryTypes
    {
		File =0,
		Directory=1
	}

	/// <summary>
	/// Hold information about the listed file/folder
	/// </summary>
	public class Ftpinfo
	{
		public string Filename;
	    public string Path;
	    public DirectoryEntryTypes FileType;
	    public long Size;
	    public string Permission;
	    public DateTime FileDateTime;
	}

}