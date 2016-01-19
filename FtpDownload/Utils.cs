using System.Reflection;

namespace FtpDownload
{
    public sealed class StringUtils
    {
        public static string ExtractFileFromPath(string fileName, string pathSeparator)
        {
            int pos = fileName.LastIndexOf(pathSeparator);
            return fileName.Substring(pos+1);
        }

        public static string ExtractFolderFromPath(string fileName, string pathSeparator, bool includeSeparatorAtEnd)
        {
            int pos = fileName.LastIndexOf(pathSeparator);
            return fileName.Substring(0,(includeSeparatorAtEnd ? pos+1 : pos));
        }
    }

    public sealed class ReflectionUtils
    {
        public static string GetVersion()
        {
            AssemblyName nm= Assembly.GetExecutingAssembly().GetName();
            return nm.Version.Major.ToString() + "." + nm.Version.Minor.ToString() + "." +
                nm.Version.Revision.ToString() + "." + nm.Version.Build.ToString();
        }
    }
}
