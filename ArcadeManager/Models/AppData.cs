using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace ArcadeManager.Models
{
    /// <summary>
    /// Represents the application data
    /// </summary>
    public class AppData
    {
        private static AppData _current;

        /// <summary>
        /// Gets the current AppData values
        /// </summary>
        public static AppData Current
        {
            get
            {
                if (_current != null) { return _current; }

                string content = File.ReadAllText(Path.Join(ArcadeManagerEnvironment.BasePath, Path.Join("Data", "appdata.json")));
                _current = Services.Serializer.Deserialize<AppData>(content);

                return _current;
            }
        }

        /// <summary>
        /// The list of CSV categories
        /// </summary>
        public IEnumerable<CsvCategory> Csv { get; set; }

        /// <summary>
        /// The list of overlay packages
        /// </summary>
        public IEnumerable<OverlayBundle> Overlays { get; set; }

        /// <summary>
        /// The default values
        /// </summary>
        public DefaultValues Defaults { get; set; }

        /// <summary>
        /// Represents a CSV category
        /// </summary>
        public class CsvCategory
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public string Image { get; set; }

            public string Repository { get; set; }

            public string Folder { get; set; }

            public string Details { get; set; }
        }

        /// <summary>
        /// Represents an overlay bundle
        /// </summary>
        public class OverlayBundle
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public string Image { get; set; }

            public string Repository { get; set; }

            public SourceDest Roms { get; set; }

            public SourceDest Overlays { get; set; }

            public SourceDest Common { get; set; }

            public Distributions Base { get; set; }

            /// <summary>
            /// Represents a source/destination parameter
            /// </summary>
            public class SourceDest
            {
                public string Src { get; set; }

                public Distributions Dest { get; set; }
            }

            /// <summary>
            /// Represents a list of distributions
            /// </summary>
            public class Distributions
            {
                public string Retropie { get; set; }

                public string Recalbox { get; set; }
            }
        }

        /// <summary>
        /// Represents the default values for 
        /// </summary>
        public class DefaultValues
        {
            public DistributionValues Roms { get; set; }

            public DistributionValues Configs { get; set; }

            public DistributionFolders RomFolders { get; set; }

            /// <summary>
            /// Represents the values for a distribution
            /// </summary>
            public class DistributionValues
            {
                public Paths Retropie { get; set; }

                public Paths Recalbox { get; set; }

                /// <summary>
                /// Represents the system-dependent paths for the distribution
                /// </summary>
                public class Paths
                {
                    public string Win32 { get; set; }

                    public string Darwin { get; set; }

                    public string Linux { get; set; }
                }
            }

            /// <summary>
            /// Represents the folders for a distributions
            /// </summary>
            public class DistributionFolders
            {
                public string[] Retropie { get; set; }

                public string[] Recalbox { get; set; }
            }
        }
    }
}
