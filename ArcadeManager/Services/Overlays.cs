using ArcadeManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArcadeManager.Services {

	public class Overlays {

		/// <summary>
		/// Downloads an overlay pack
		/// </summary>
		/// <param name="data">The parameters</param>
		/// <param name="progressor">The progressor</param>
		public static async Task Download(Actions.OverlaysAction data, MessageHandler.Progressor progressor) {
			progressor.Init("Download overlay pack");

			try {
				var os = ArcadeManagerEnvironment.SettingsOs;
				var pack = ArcadeManagerEnvironment.AppData.Overlays.Where(o => o.Name == data.pack).First();

				// check if the destination of rom cfg is the rom folder
				var romCfgFolder = pack.Roms.Dest[os] == "roms"
					? null // save rom cfg directly into rom folder(s)
					: Path.Join(data.configFolder, pack.Roms.Dest[os]); // save rom cfg in config folder

				// list the available rom configs
				progressor.Progress("Listing files to download", 1, 100);
				var romConfigs = await Downloader.ListFiles(pack.Repository, pack.Roms.Src);

				// download common files
				progressor.Progress("Downloading common files", 1, 100);
				if (pack.Common != null && !string.IsNullOrWhiteSpace(pack.Common.Src)) {
					await DownloadCommon(pack, Path.Join(data.configFolder, pack.Common.Dest[os]), data.overwrite, data.ratio, progressor, 100, 1);
				}

				if (MessageHandler.MustCancel) { return; }

				// check that thvere is a matching game in any of the roms folders
				progressor.Progress("Listing games to process", 1, 100);
				var processedOverlays = new List<string>();
				var romsToProcess = GetRomsToProcess(data.romFolders, romConfigs.tree);

				var total = romConfigs.tree.Count;
				var current = 0;
				var installed = 0;

				foreach (var r in romsToProcess) {
					if (MessageHandler.MustCancel) { return; }

					current++;

					var game = r.Game;
					var zip = $"{game}.zip";
					var romcfg = $"{game}.zip.cfg";

					progressor.Progress($"Processing {r.Game}", total, current);

					// download the rom config and extract the overlay file name
					var romConfigContent = await Downloader.DownloadFileText(pack.Repository, $"{pack.Roms.Src}/{game}.zip.cfg");
					foreach (var romFolder in r.TargetFolder) {
						if (MessageHandler.MustCancel) { return; }

						var romFile = Path.Join(romFolder, zip);
						if (data.overwrite || !File.Exists(romFile)) {
							await File.WriteAllTextAsync(romFile, romConfigContent);
							installed++;
						}
					}

					if (MessageHandler.MustCancel) { return; }

					var overlayFileName = GetCfgData(romConfigContent, "input_overlay");

					// download the overlay file name and extract the image file name

					// download the image

					// resize the overlay coordinates if necessary

					// fix the paths in the config files
				}

				progressor.Done($"Installed {installed} overlays", null);
			}
			catch (Exception ex) {
				progressor.Error(ex);
			}
		}

		/// <summary>
		/// Gets data from the specified config file.
		/// </summary>
		/// <param name="fileContent">The content of the file.</param>
		/// <param name="key">The key to look for.</param>
		/// <returns>
		/// The config value
		/// </returns>
		public static string GetCfgData(string fileContent, string key) {
			var match = Regex.Match(fileContent, BuildCfgRegex(key), RegexOptions.Multiline);
			if (match.Success && match.Captures.Any()) {
				return match.Groups[1].Value.Trim();
			}

			return null;
		}

		/// <summary>
		/// Builds a regex string to get the specified key value
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>The corresponding value</returns>
		private static string BuildCfgRegex(string key) {
			/// searched value looks like:
			/// key = "value"
			/// with or without spaces, with or without quotes, with or without trailing spaces
			return $"^{key}\\s*=\\s?\"?([^\"\\n]*)\"?\\s*$";
		}

		/// <summary>
		/// Changes the resolution in a file content.
		/// </summary>
		/// <param name="content">The file content.</param>
		/// <param name="ratio">The ratio.</param>
		/// <returns>The modified content</returns>
		private static string ChangeResolution(string content, float ratio) {
			if (ratio == 1) {
				return content;
			}

			var parameters = new List<string> {
				"custom_viewport_width",
				"custom_viewport_height",
				"custom_viewport_x",
				"custom_viewport_y",
				"video_fullscreen_x",
				"video_fullscreen_y"
			};

			foreach (var p in parameters) {
				var regex = new Regex(BuildCfgRegex(p), RegexOptions.Multiline | RegexOptions.IgnoreCase);
				var foundValue = regex.Match(content).Groups[1].Value;
				if (double.TryParse(foundValue, out var value)) {
					value = Math.Round(value * ratio, 0);
					content = regex.Replace(content, $"{p} = {(int)value}");
				}
			}

			return content;
		}

		/// <summary>
		/// Downloads the common files.
		/// </summary>
		/// <param name="pack">The overlay pack to download.</param>
		/// <param name="destination">The destination path.</param>
		/// <param name="overwrite">if set to <c>true</c> overwrite existing files.</param>
		/// <param name="ratio">The ratio to change resolution.</param>
		/// <param name="progressor">The progressor.</param>
		/// <returns>
		/// How many files have been downloaded
		/// </returns>
		private static async Task DownloadCommon(OverlayBundle pack, string destination, bool overwrite, float ratio, MessageHandler.Progressor progressor, int total, int current) {
			if (MessageHandler.MustCancel) { return; }

			if (pack.Common != null && !string.IsNullOrEmpty(pack.Common.Src)) {
				IEnumerable<string> files = await Downloader.DownloadFolder(pack.Repository, pack.Common.Src, destination, overwrite, (entry) => {
					progressor.Progress($"downloading {entry.path}", total, current);
				});

				foreach (var f in files.Where(f => f.EndsWith(".cfg", StringComparison.InvariantCultureIgnoreCase))) {
					var fi = new FileInfo(f);
					progressor.Progress($"fixing {fi.Name}", total, current);

					if (overwrite || !fi.Exists) {
						var content = await File.ReadAllTextAsync(f);

						content = ChangeResolution(content, ratio);
						content = FixPaths(content, pack);

						await File.WriteAllTextAsync(f, content);
					}
				};
			}
		}

		/// <summary>
		/// Fixes the paths in a config.
		/// </summary>
		/// <param name="content">The config content.</param>
		/// <param name="pack">The pack to download.</param>
		/// <returns>
		/// The fixed content
		/// </returns>
		private static string FixPaths(string content, OverlayBundle pack) {
			if (ArcadeManagerEnvironment.SettingsOs != pack.BaseOs) {
				return content.Replace(pack.Base[pack.BaseOs], pack.Base[ArcadeManagerEnvironment.SettingsOs], StringComparison.InvariantCultureIgnoreCase);
			}

			return content;
		}

		/// <summary>
		/// Gets the list of roms to process and their folder(s).
		/// </summary>
		/// <param name="romFolders">The rom folders.</param>
		/// <param name="entries">The available entries.</param>
		/// <returns>
		/// The roms to process
		/// </returns>
		private static IEnumerable<RomToProcess> GetRomsToProcess(string[] romFolders, IEnumerable<GithubTree.Entry> entries) {
			var result = new List<RomToProcess>();

			// list all the folders (arcade, fba, mame...)
			foreach (var folder in romFolders) {
				var di = new DirectoryInfo(folder);
				// get all rom files
				foreach (var fi in di.GetFiles("*.zip")) {
					var game = fi.Name.Replace(".zip", "");

					// only process files that are in the overlays pack
					if (entries.Any(e => e.path.EndsWith($"{game}.zip.cfg", StringComparison.InvariantCultureIgnoreCase))) {
						var existing = result.Where(r => r.Game.Equals(game, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
						if (existing != null) {
							existing.TargetFolder.Add(fi.DirectoryName);
						}
						else {
							result.Add(new RomToProcess { Game = game, TargetFolder = new List<string> { fi.DirectoryName } });
						}
					}
				}
			}

			return result;
		}

		/// <summary>
		/// A rom to process
		/// </summary>
		private class RomToProcess {

			/// <summary>
			/// Gets or sets the name of the game.
			/// </summary>
			public string Game { get; set; }

			/// <summary>
			/// Gets or sets the folders in which the game is present.
			/// </summary>
			public List<string> TargetFolder { get; set; } = new();
		}
	}
}
