using System;
using System.Configuration;

namespace ArcadeManager.Behavior
{
    /// <summary>
    /// Settings management
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// Gets or sets the OS (Retropie/Recalbox)
        /// </summary>
        public static string Os {
            get
            {
                return ConfigurationManager.AppSettings["os"];
            }
            set
            {
                ConfigurationManager.AppSettings["os"] = value;
            }
        }
    }
}
