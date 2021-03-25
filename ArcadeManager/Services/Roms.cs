using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeManager.Services {

	/// <summary>
	/// Roms management
	/// </summary>
	public class Roms {

		/// <summary>
		/// Copies roms
		/// </summary>
		/// <param name="args">The arguments</param>
		/// <param name="progressor">The progress manager.</param>
		/// <exception cref="FileNotFoundException">Unable to find main CSV file</exception>
		/// <exception cref="DirectoryNotFoundException">Unable to find romset folder {args.romset}</exception>
		public static async Task Add(Actions.RomsAction args, MessageHandler.Progressor progressor) {
			progressor.Init("Copying roms");

			try {
				// check files and folders
				if (!File.Exists(args.main)) { throw new FileNotFoundException("Unable to find main CSV file", args.main); }
				if (!Directory.Exists(args.romset)) { throw new DirectoryNotFoundException($"Unable to find romset folder {args.romset}"); }
				if (!Directory.Exists(args.selection)) { Directory.CreateDirectory(args.selection); }

				// read CSV file
				var content = await Csv.ReadFile(args.main, false);

				var total = content.Count();
				var i = 0;
				var copied = 0;

				// copy each file found in CSV
				foreach (var f in content) {
					if (MessageHandler.MustCancel) { break; }

					i++;

					// build vars
					var game = f.name;
					var zip = $"{game}.zip";
					var sourceRom = Path.Join(args.romset, zip);
					var destRom = Path.Join(args.selection, zip);

					if (File.Exists(sourceRom)) {
						var fi = new FileInfo(sourceRom);

						progressor.Progress($"{game} ({HumanSize(fi.Length)})", total, i);

						// copy rom
						if (!File.Exists(destRom) || args.overwrite) {
							File.Copy(sourceRom, destRom, true);
							copied++;
						}

						// try to copy chd if it can be found
						var sourceChd = Path.Join(args.romset, game);
						var targetChd = Path.Join(args.selection, game);
						if (Directory.Exists(sourceChd)) {
							if (MessageHandler.MustCancel) { break; }

							progressor.Progress($"Copying {game} CHD ({HumanSize(DirectorySize(sourceChd))})", total, i);

							DirectoryCopy(sourceChd, targetChd, args.overwrite, false);
						}
					}
				}

				progressor.Done($"Copied {copied} file(s)", args.selection);
			}
			catch (Exception ex) {
				progressor.Error(ex);
			}
		}

		/// <summary>
		/// Deletes roms from a folder
		/// </summary>
		/// <param name="args">The arguments</param>
		/// <param name="progressor">The progress manager.</param>
		/// <exception cref="FileNotFoundException">Unable to find main CSV file</exception>
		/// <exception cref="DirectoryNotFoundException">Unable to find selection folder {args.selection}</exception>
		public static async Task Delete(Actions.RomsAction args, MessageHandler.Progressor progressor) {
			progressor.Init("Deleting roms");

			try {
				// check files and folders
				if (!File.Exists(args.main)) { throw new FileNotFoundException("Unable to find main CSV file", args.main); }
				if (!Directory.Exists(args.selection)) { throw new DirectoryNotFoundException($"Unable to find selection folder {args.selection}"); }

				// read CSV file
				var content = await Csv.ReadFile(args.main, false);

				var total = content.Count();
				var i = 0;
				var deleted = 0;

				foreach (var f in content) {
					if (MessageHandler.MustCancel) { break; }
					i++;

					// build vars
					var game = f.name;
					var zip = $"{game}.zip";
					var filePath = Path.Join(args.selection, zip);

					progressor.Progress(game, total, i);

					if (File.Exists(filePath)) {
						File.Delete(filePath);
						deleted++;
					}
				}

				progressor.Done($"Deleted {deleted} file(s)", args.selection);
			}
			catch (Exception ex) {
				progressor.Error(ex);
			}
		}

		/// <summary>
		/// Keeps only listed roms in a folder
		/// </summary>
		/// <param name="args">The arguments</param>
		/// <param name="progressor">The progress manager.</param>
		/// <exception cref="FileNotFoundException">Unable to find main CSV file</exception>
		/// <exception cref="DirectoryNotFoundException">Unable to find selection folder {args.selection}</exception>
		public static async Task Keep(Actions.RomsAction args, MessageHandler.Progressor progressor) {
			progressor.Init("Filtering roms");

			try {
				// check files and folders
				if (!File.Exists(args.main)) { throw new FileNotFoundException("Unable to find main CSV file", args.main); }
				if (!Directory.Exists(args.selection)) { throw new DirectoryNotFoundException($"Unable to find selection folder {args.selection}"); }

				// read CSV file
				var content = await Csv.ReadFile(args.main, false);

				// get list of files
				var files = (new DirectoryInfo(args.selection)).GetFiles("*.zip");

				var total = content.Count();
				var i = 0;
				var deleted = 0;

				// check if files exist in games list
				foreach (var f in files) {
					if (MessageHandler.MustCancel) { break; }
					i++;

					progressor.Progress(f.Name, total, i);

					if (!content.Any(c => $"{c.name}.zip" == f.Name)) {
						File.Delete(f.FullName);
						deleted++;
					}
				}

				progressor.Done($"Deleted {deleted} files", args.selection);
			}
			catch (Exception ex) {
				progressor.Error(ex);
			}
		}

		/// <summary>
		/// Copies a directory
		/// </summary>
		/// <param name="sourceDirName">The source directory name</param>
		/// <param name="destDirName">The destination directory name</param>
		/// <param name="overwrite">Wether to overwrite existing files</param>
		/// <param name="copySubDirs">Whether to copy the sub-directories</param>
		/// <exception cref="DirectoryNotFoundException">Source directory does not exist or could not be found</exception>
		/// <remarks>
		/// From docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
		/// </remarks>
		private static void DirectoryCopy(string sourceDirName, string destDirName, bool overwrite, bool copySubDirs) {
			// Get the subdirectories for the specified directory.
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);

			if (!dir.Exists) {
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName);
			}

			DirectoryInfo[] dirs = dir.GetDirectories();

			// If the destination directory doesn't exist, create it.
			if (!Directory.Exists(destDirName)) {
				Directory.CreateDirectory(destDirName);
			}

			// Get the files in the directory and copy them to the new location.
			FileInfo[] files = dir.GetFiles();
			foreach (FileInfo file in files) {
				string tempPath = Path.Combine(destDirName, file.Name);
				file.CopyTo(tempPath, overwrite);
			}

			// If copying subdirectories, copy them and their contents to new location.
			if (copySubDirs) {
				foreach (DirectoryInfo subdir in dirs) {
					string tempPath = Path.Combine(destDirName, subdir.Name);
					DirectoryCopy(subdir.FullName, tempPath, overwrite, copySubDirs);
				}
			}
		}

		/// <summary>
		/// Computes a directory size
		/// </summary>
		/// <param name="directory">The path to the directory</param>
		/// <returns>
		/// The directory size
		/// </returns>
		/// <remarks>
		/// From stackoverflow.com/a/468131/6776
		/// </remarks>
		private static long DirectorySize(string directory) {
			var d = new DirectoryInfo(directory);

			long size = 0;
			// Add file sizes.
			FileInfo[] fis = d.GetFiles();
			foreach (FileInfo fi in fis) {
				size += fi.Length;
			}

			// Add subdirectory sizes.
			DirectoryInfo[] dis = d.GetDirectories();
			foreach (DirectoryInfo di in dis) {
				size += DirectorySize(di.FullName);
			}

			return size;
		}

		/// <summary>
		/// Makes a file size human-readable
		/// </summary>
		/// <param name="size">The source file size</param>
		/// <returns>
		/// The human-readable file size
		/// </returns>
		/// <remarks>
		/// From stackoverflow.com/a/4975942/6776
		/// </remarks>
		private static string HumanSize(long size) {
			string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
			if (size == 0)
				return $"0 {suf[0]}";
			long bytes = Math.Abs(size);
			int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
			double num = Math.Round(bytes / Math.Pow(1024, place), 1);
			return $"{Math.Sign(size) * num} {suf[place]}";
		}
	}
}
