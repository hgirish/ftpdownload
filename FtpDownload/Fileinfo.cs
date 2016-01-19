namespace FtpDownload
{
    public class Fileinfo
    {
        public DirectionEnum Direction;
        public string CompleteFileName;	//local or remote file name
        public string Destination;
        public bool MkdirFlag; //boolean value to indicate if the specified folder is to be created locally/remotely
        //public ftpinfo ftpstats=null;	//applicable only for remote files
        //public FileInfo filestats=null;	//applicable only for local files
		
        //to upload
        public Fileinfo(string fileName, string destination, DirectionEnum direction, bool mkdirFlag)
        {
            CompleteFileName = fileName;
            Destination = destination;
            Direction = direction;
            MkdirFlag = mkdirFlag;
        }

        public Fileinfo(string fileName, string destination, DirectionEnum direction)
            :this(fileName,destination,direction,false)
        {
        }
    }
}