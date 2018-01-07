using System;
using System.Configuration;

namespace DiffBlitter
{
    /// <summary>
    /// Application configuration settings.
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// The repo uri path.
        /// </summary>
        public static string RepoUri => ConfigurationManager.AppSettings["RepoUri"];

        /// <summary>
        /// The local file content path.
        /// </summary>
        public static string ContentPath => ConfigurationManager.AppSettings["ContentPath"];

        /// <summary>
        /// The relative path of the file to use for quick version detection.
        /// </summary>
        public static string VersionFilePath => ConfigurationManager.AppSettings["VersionFilePath"];

        /// <summary>
        /// Indicates whether or not content should be validated before package application.
        /// </summary>
        public static bool ValidateBeforePackageApply => Convert.ToBoolean(ConfigurationManager.AppSettings["ValidateBeforePackageApply"]);

        /// <summary>
        /// Indicates whether or not content should be validated after package application.
        /// </summary>
        public static bool ValidateAfterPackageApply => Convert.ToBoolean(ConfigurationManager.AppSettings["ValidateAfterPackageApply"]);
    }
}
