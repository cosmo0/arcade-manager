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

						progressor.Progress($"{game} ({FileSystem.HumanSize(fi.Length)})", total, i);

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

							progressor.Progress($"Copying {game} CHD ({FileSystem.HumanSize(FileSystem.DirectorySize(sourceChd))})", total, i);

							FileSystem.DirectoryCopy(sourceChd, targetChd, args.overwrite, false);
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
	}
}
