using ArcadeManager.Models;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArcadeManager.Services {

	/// <summary>
	/// CSV files management
	/// </summary>
	public class Csv {

		/// <summary>
		/// The accepted delimiters
		/// </summary>
		/// <remarks>
		/// Pipe is escaped because it'll be used in a regex
		/// </remarks>
		private static readonly string[] delimiters = { ";", ",", "\t", "\\|" };

		/// <summary>
		/// The header row of a DAT conversion
		/// </summary>
		private static readonly string headerDatRow = "name;description;year;manufacturer;is_parent;romof;is_clone;cloneof;sampleof";

		/// <summary>
		/// The header row of a INI conversion
		/// </summary>
		private static readonly string headerIniRow = "name;value;";

		/// <summary>
		/// The "name" column name
		/// </summary>
		private static readonly string nameColumn = "name";

		/// <summary>
		/// Converts a DAT file.
		/// </summary>
		/// <param name="main">The main file.</param>
		/// <param name="target">The target file.</param>
		/// <param name="progressor">The progress manager.</param>
		public static async Task ConvertDat(string main, string target, MessageHandler.Progressor progressor) {
			progressor.Init("DAT conversion");

			try {
				// read input file
				using (var reader = File.OpenRead(main)) {
					// deserialize file
					var datFile = Serializer.DeserializeXml<Models.DatFile.Datafile>(reader);

					// output stream
					using (var outStream = new FileStream(target, FileMode.Create, FileAccess.Write)) {
						using (var outStreamWriter = new StreamWriter(outStream)) {
							await outStreamWriter.WriteLineAsync(headerDatRow);

							// list entries depending on if it's a list of machines or games
							IEnumerable<Models.DatFile.BaseEntry> entries = datFile.Game != null && datFile.Game.Any()
								? datFile.Game
								: datFile.Machine;

							int total = entries.Count();
							int i = 0;
							foreach (var e in entries) {
								i++;
								progressor.Progress($"Converting {e.Name}", total, i);

								var sb = new StringBuilder();
								sb.Append($"{e.Name};");
								sb.Append($"{e.Description ?? "-"};");
								sb.Append($"{e.Year ?? "-"};");
								sb.Append($"{e.Manufacturer ?? "-"};");

								if (e is Models.DatFile.Game) {
									var eg = e as Models.DatFile.Game;

									sb.Append(string.IsNullOrEmpty(eg.Cloneof) ? "NO" : "YES").Append(';'); // is_parent
									sb.Append(eg.Romof ?? "-").Append(';'); // romof
									sb.Append(string.IsNullOrEmpty(eg.Cloneof) ? "YES" : "NO").Append(';'); // is_clone
									sb.Append(eg.Cloneof ?? "-").Append(';'); // cloneof
									sb.Append(eg.Sampleof ?? "-").Append(';'); // sampleof
								}
								else {
									sb.Append("-;-;-;-;-;");
								}

								await outStreamWriter.WriteLineAsync(sb.ToString());
							}
						}
					}

					progressor.Done("DAT file converted", target);
				}
			}
			catch (Exception ex) {
				progressor.Error(ex);
			}
		}

		/// <summary>
		/// Converts a INI file to CSV
		/// </summary>
		/// <param name="main">The main file</param>
		/// <param name="target">The target folder to create files into</param>
		/// <param name="progressor">The progress manager</param>
		public static async Task ConvertIni(string main, string target, MessageHandler.Progressor progressor) {
			progressor.Init("INI conversion");

			try {
				var data = new Dictionary<string, List<IniEntry>>();

				var mainInfo = new FileInfo(main);

				using (var source = new StreamReader(main)) {
					var isFolderSetting = false;
					var currentSection = "";

					while (!source.EndOfStream) {
						var line = (await source.ReadLineAsync()).Trim();

						// progress up to 50%
						progressor.Progress("Reading source file", 100, (int)(source.BaseStream.Position / mainInfo.Length * 50));

						// ignore empty lines and comments
						if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";")) {
							continue;
						}

						// ignore folder settings
						if (line == "[FOLDER_SETTINGS]") {
							isFolderSetting = true;
							continue;
						}

						if (isFolderSetting && !line.StartsWith("[")) {
							continue;
						}

						// we're out of folder settings
						isFolderSetting = false;

						// found a section
						if (line.StartsWith("[")) {
							currentSection = line;

							// add to data
							if (!data.ContainsKey(currentSection)) {
								data.Add(currentSection, new List<IniEntry>());
							}

							continue;
						}

						// we're in a section data
						if (line.IndexOf("=") > 0) {
							// game=value
							var split = line.Split("=", StringSplitOptions.TrimEntries);
							data[currentSection].Add(new IniEntry { game = split[0], value = split[1] });
						}
						else {
							// simple games list
							data[currentSection].Add(new IniEntry { game = line });
						}
					}
				}

				// create a file for each non-empty section
				var i = 0;
				foreach (var entry in data.Where(d => d.Value.Any())) {
					i++;

					// file name = sanitized section name, or source name if there's only one section
					var name = data.Count > 1 ? Sanitize(entry.Key.Trim("[]".ToCharArray())) : mainInfo.Name.Replace(".ini", "");
					name += ".csv";

					var path = Path.Join(target, name);

					// create increments to not overwrite existing conversions
					var j = 0;
					var finalPath = path;
					while (File.Exists(finalPath)) { finalPath = path.Replace(".csv", $" ({++j}).csv"); }
					path = finalPath;

					// progress from 50%
					progressor.Progress($"Creating file {name}", 100, 50 + (i / data.Count * 50));

					// write into file
					using (var output = new StreamWriter(path)) {
						await output.WriteLineAsync(headerIniRow);

						foreach (var iniEntry in entry.Value) {
							await output.WriteLineAsync($"{iniEntry.game};{iniEntry.value};");
						}
					}
				}

				progressor.Done("INI file converted", target);
			}
			catch (Exception ex) {
				progressor.Error(ex);
			}
		}

		/// <summary>
		/// Lists the files in a folder to a CSV file.
		/// </summary>
		/// <param name="main">The main folder.</param>
		/// <param name="target">The target CSV file.</param>
		/// <param name="progressor">The progressor.</param>
		/// <exception cref="DirectoryNotFoundException">Unable to find the folder {main}</exception>
		public static async Task ListFiles(string main, string target, MessageHandler.Progressor progressor) {
			progressor.Init("List files to a CSV");

			try {
				if (!Directory.Exists(main)) { throw new DirectoryNotFoundException($"Unable to find the folder {main}"); }

				var di = new DirectoryInfo(main);

				using (var output = new StreamWriter(target, false)) {
					await output.WriteLineAsync("name;");

					var files = di.GetFiles("*.zip", new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive });
					var total = files.Length;
					var i = 0;

					foreach (var f in files) {
						i++;
						progressor.Progress($"Listing file {f.Name}", total, i);

						await output.WriteLineAsync(f.Name.Replace(".zip", "", StringComparison.InvariantCultureIgnoreCase) + ";");
					}
				}

				progressor.Done("Files listed", target);
			}
			catch (Exception ex) {
				progressor.Error(ex);
			}
		}

		/// <summary>
		/// Merges the specified main and secondary files to the target file.
		/// </summary>
		/// <param name="main">The path to the main file.</param>
		/// <param name="secondary">The path to the secondary file.</param>
		/// <param name="target">The path to the target file.</param>
		/// <param name="progressor">The progressor.</param>
		public static async Task Merge(string main, string secondary, string target, MessageHandler.Progressor progressor) {
			progressor.Init("Merge two CSV files");

			try {
				var steps = 5;
				var current = 0;

				var fiMain = new FileInfo(main);
				var fiSecondary = new FileInfo(secondary);
				var fiTarget = new FileInfo(target);

				progressor.Progress($"file {fiMain.Name}", steps, ++current);
				var mainEntries = ReadFile(main, true);

				progressor.Progress($"file {fiSecondary.Name}", steps, ++current);
				var secondaryEntries = ReadFile(secondary, true);

				var result = mainEntries.ToList(); // the ToList() copies the list instead of creating a reference

				progressor.Progress($"files merging", steps, ++current);
				foreach (var e in secondaryEntries) {
					// copy entries in secondary that are not in main
					if (!mainEntries.Any(me => me.name == e.name)) {
						result.Add(e);
					}
				}

				progressor.Progress($"save to file {fiTarget.Name}", steps, ++current);
				await WriteFile(result, target);

				progressor.Done($"Files have been merged, result has {result.Count} entries", target);
			}
			catch (Exception ex) {
				progressor.Error(ex);
			}
		}

		/// <summary>
		/// Reads the provided CSV file
		/// </summary>
		/// <param name="filepath">The path to the file</param>
		/// <param name="getOtherValues">if set to <c>true</c> get the values other than the name.</param>
		/// <returns>
		/// The list of games in the CSV file
		/// </returns>
		public static IEnumerable<GameEntry> ReadFile(string filepath, bool getOtherValues) {
			using (var reader = new StreamReader(filepath)) {
				// check that the first line has a header
				var firstLine = reader.ReadLine();
				var (hasHeader, delimiter) = HasHeader(firstLine);

				// back to the beginning of the file
				reader.DiscardBufferedData();
				reader.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);

				// build CSV read options
				var conf = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture) {
					Delimiter = delimiter,
					HasHeaderRecord = hasHeader,
					IgnoreBlankLines = true
				};

				// read the file
				List<GameEntry> result = new();
				using (var csv = new CsvReader(reader, conf)) {
					var entries = csv.GetRecords<dynamic>().ToList();

					if (hasHeader) {
						foreach (var e in entries) {
							var ge = new GameEntry { name = e.name };

							if (getOtherValues) {
								// get all other columns ; see visualstudiomagazine.com/articles/2019/04/01/working-with-dynamic-objects.aspx
								ge.values = ((IDictionary<string, object>)e)
									.Where(elem => !elem.Key.Equals(nameColumn, StringComparison.InvariantCultureIgnoreCase))
									.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
							}

							result.Add(ge);
						}
					}
					else {
						// Without headers, CsvReader names the fields Field1, Field2, etc
						result.AddRange(entries.Select(e => new GameEntry { name = e.Field1 }));
					}
				}

				return result;
			}
		}

		/// <summary>
		/// Checks if the specified line has a header field
		/// </summary>
		/// <param name="line">The line to check</param>
		/// <returns>Whether a header has been found, and the delimiter, if any</returns>
		private static (bool, string) HasHeader(string line) {
			bool hasDelimiter = false;
			foreach (var d in delimiters) {
				// ",name," OR "name," OR ",name"
				var hasKeyword = new Regex($"{d}{nameColumn}{d}|^{nameColumn}{d}|{d}{nameColumn}$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
				// single column with just "name" and nothing else
				var hasKeywordAlone = new Regex($"^{d}$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

				if (hasKeyword.IsMatch(line)) {
					return (true, d.Replace("\\", ""));
				}

				if (hasKeywordAlone.IsMatch(line)) {
					return (false, d.Replace("\\", ""));
				}

				hasDelimiter |= line.IndexOf(d.Replace("\\", "")) > 0;
			}

			// no header column found, and no delimiter either
			if (!hasDelimiter) {
				return (false, delimiters[0]);
			}

			throw new FormatException("Your CSV file must have a 'name' column");
		}

		/// <summary>
		/// Sanitizes a file name
		/// </summary>
		/// <param name="name">The file name</param>
		/// <returns>The sanitized file name</returns>
		/// <remarks>
		/// Copied from stackoverflow.com/a/847251/6776
		/// </remarks>
		private static string Sanitize(string name) {
			string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
			string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

			return Regex.Replace(name, invalidRegStr, "_");
		}

		/// <summary>
		/// Writes the file to the specified target.
		/// </summary>
		/// <param name="result">The entries to write.</param>
		/// <param name="target">The target to write to.</param>
		/// <exception cref="NotImplementedException"></exception>
		private static async Task WriteFile(List<GameEntry> result, string target) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Represents an INI file entry
		/// </summary>
		private struct IniEntry {

			/// <summary>
			/// The game name
			/// </summary>
			public string game;

			/// <summary>
			/// The additional value, if any
			/// </summary>
			public string value;
		}
	}
}
