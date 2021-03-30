using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcadeManager.Models {

	/// <summary>
	/// Represents the application data
	/// </summary>
	public class AppData {

		/// <summary>
		/// The list of CSV categories
		/// </summary>
		public IEnumerable<CsvCategory> Csv { get; set; }

		/// <summary>
		/// The default values
		/// </summary>
		public DefaultValues Defaults { get; set; }

		/// <summary>
		/// The list of overlay packages
		/// </summary>
		public IEnumerable<OverlayBundle> Overlays { get; set; }

		/// <summary>
		/// Represents a CSV category
		/// </summary>
		public class CsvCategory {

			/// <summary>
			/// Gets or sets the description.
			/// </summary>
			public string Description { get; set; }

			/// <summary>
			/// Gets or sets the details.
			/// </summary>
			public string Details { get; set; }

			/// <summary>
			/// Gets or sets the folder.
			/// </summary>
			public string Folder { get; set; }

			/// <summary>
			/// Gets or sets the image.
			/// </summary>
			public string Image { get; set; }

			/// <summary>
			/// Gets or sets the name.
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// Gets or sets the repository URL.
			/// </summary>
			public string Repository { get; set; }
		}

		/// <summary>
		/// Represents the default values for
		/// </summary>
		public class DefaultValues {

			/// <summary>
			/// Gets or sets the configs.
			/// </summary>
			public DistributionValues Configs { get; set; }

			/// <summary>
			/// Gets or sets the rom folders.
			/// </summary>
			public DistributionFolders RomFolders { get; set; }

			/// <summary>
			/// Gets or sets the roms.
			/// </summary>
			public DistributionValues Roms { get; set; }

			/// <summary>
			/// Represents the folders for a distributions
			/// </summary>
			public class DistributionFolders {

				/// <summary>
				/// Gets or sets the values for Recalbox.
				/// </summary>
				public string[] Recalbox { get; set; }

				/// <summary>
				/// Gets or sets the values for Retropie.
				/// </summary>
				public string[] Retropie { get; set; }
			}

			/// <summary>
			/// Represents the values for a distribution
			/// </summary>
			public class DistributionValues {

				/// <summary>
				/// Gets or sets the values for Recalbox.
				/// </summary>
				public Paths Recalbox { get; set; }

				/// <summary>
				/// Gets or sets the values for Retropie.
				/// </summary>
				public Paths Retropie { get; set; }

				/// <summary>
				/// Represents the system-dependent paths for the distribution
				/// </summary>
				public class Paths {

					/// <summary>
					/// Gets or sets the values for MacOS X.
					/// </summary>
					public string Darwin { get; set; }

					/// <summary>
					/// Gets or sets the values for Linux.
					/// </summary>
					public string Linux { get; set; }

					/// <summary>
					/// Gets or sets the values for Windows.
					/// </summary>
					public string Win32 { get; set; }
				}
			}
		}
	}

	/// <summary>
	/// Represents an overlay bundle
	/// </summary>
	public class OverlayBundle {

		/// <summary>
		/// Gets or sets the base values.
		/// </summary>
		public Distributions Base { get; set; }

		/// <summary>
		/// Gets or sets the base OS of the bundle.
		/// </summary>
		public string BaseOs { get; set; }

		/// <summary>
		/// Gets or sets the common files, if any.
		/// </summary>
		public SourceDest Common { get; set; }

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the image.
		/// </summary>
		public string Image { get; set; }

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the overlays paths.
		/// </summary>
		public SourceDest Overlays { get; set; }

		/// <summary>
		/// Gets or sets the repository URL.
		/// </summary>
		public string Repository { get; set; }

		/// <summary>
		/// Gets or sets the roms paths.
		/// </summary>
		public SourceDest Roms { get; set; }

		/// <summary>
		/// Represents a list of distributions
		/// </summary>
		public class Distributions {

			/// <summary>
			/// Gets or sets the value for Recalbox.
			/// </summary>
			public string Recalbox { get; set; }

			/// <summary>
			/// Gets or sets the value for Retropie.
			/// </summary>
			public string Retropie { get; set; }

			/// <summary>
			/// Accesses the value for the specified distribution
			/// </summary>
			/// <param name="distribution">The distribution</param>
			/// <returns>The distribution value</returns>
			public string this[string distribution] {
				get {
					var prop = this.GetType()
						.GetProperties()
						.Where(p => p.Name.Equals(distribution, StringComparison.InvariantCultureIgnoreCase))
						.FirstOrDefault();

					if (prop != null) {
						return prop.GetValue(this).ToString();
					}

					return string.Empty;
				}
			}
		}

		/// <summary>
		/// Represents a source/destination parameter
		/// </summary>
		public class SourceDest {

			/// <summary>
			/// Gets or sets the destination values.
			/// </summary>
			public Distributions Dest { get; set; }

			/// <summary>
			/// Gets or sets the source.
			/// </summary>
			public string Src { get; set; }
		}
	}
}
