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
		private static readonly string[] delimiters = { ";", ",", "\t", "|" };

		/// <summary>
		/// The header row of a DAT conversion
		/// </summary>
		private static readonly string headerDatRow = "name;description;year;manufacturer;is_parent;romof;is_clone;cloneof;sampleof";

		/// <summary>
		/// The "name" column name
		/// </summary>
		private static readonly string nameColumn = "name";

		/// <summary>
		/// Converts a DAT file.
		/// </summary>
		/// <param name="main">The main file.</param>
		/// <param name="target">The target file.</param>
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
		/// Reads the provided CSV file
		/// </summary>
		/// <param name="filepath">The path to the file</param>
		/// <returns>The list of games in the CSV file</returns>
		public static IEnumerable<Models.GameEntry> ReadFile(string filepath) {
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
				using (var csv = new CsvReader(reader, conf)) {
					return csv.GetRecords<Models.GameEntry>().ToList();
				}
			}
		}

		/// <summary>
		/// Checks if the specified line has a header field
		/// </summary>
		/// <param name="line">The line to check</param>
		/// <returns>Whether a header has been found, and the delimiter, if any</returns>
		private static (bool, string) HasHeader(string line) {
			foreach (var d in delimiters) {
				// ",name," OR "name," OR ",name"
				var hasKeyword = new Regex($"{d}{nameColumn}{d}|^{nameColumn}{d}|{d}{nameColumn}$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
				// single column with just "name" and nothing else
				var hasKeywordAlone = new Regex($"^{d}$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

				if (hasKeyword.IsMatch(line)) {
					return (true, d);
				}

				if (hasKeywordAlone.IsMatch(line)) {
					return (false, d);
				}
			}

			throw new FormatException("Your CSV file must have a 'name' column");
		}
	}
}
