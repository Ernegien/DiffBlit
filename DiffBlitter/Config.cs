using System.Configuration;

namespace DiffBlitter
{
    public static class Config
    {
        public static string RepoUri => ConfigurationManager.AppSettings["RepoUri"];

        public static string ContentPath => ConfigurationManager.AppSettings["ContentPath"];

        public static string VersionFilePath => ConfigurationManager.AppSettings["VersionFilePath"];   
    }
}
