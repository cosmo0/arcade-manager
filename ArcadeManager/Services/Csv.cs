using ArcadeManager.Actions;
using ArcadeManager.Models;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
		/// The "name" column name
		/// </summary>
		private static readonly string nameColumn = "name";

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
