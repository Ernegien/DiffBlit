using System;
using System.Configuration;
using DiffBlit.Core.Logging;

namespace DiffBlitter
{
    /// <summary>
    /// Application configuration settings.
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// The log verbosity level.
        /// </summary>
        public static LogLevel LogLevel
        {
            get
            {
                try
                {
                    return (LogLevel) Enum.Parse(typeof(LogLevel), ConfigurationManager.AppSettings["LogLevel"], true);
                }
                catch
                {
                    return LogLevel.Info;
                }
            }
        }

        /// <summary>
        /// The content repo uri path.
        /// </summary>
        public static string ContentRepoUri => ConfigurationManager.AppSettings["ContentRepoUri"];

        /// <summary>
        /// The updater repo uri path.
        /// </summary>
        public static string UpdaterRepoUri => ConfigurationManager.AppSettings["UpdaterRepoUri"];

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

        /// <summary>
        /// Determines whether or not to prompt the user for permission to update itself.
        /// </summary>
        public static bool UpdaterAutoUpdate => Convert.ToBoolean(ConfigurationManager.AppSettings["UpdaterAutoUpdate"]);
    }
}
