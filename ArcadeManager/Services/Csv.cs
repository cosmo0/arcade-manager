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

		/// <summary>
		/// Converts a DAT file.
		/// </summary>
		/// <param name="main">The main file.</param>
		/// <param name="target">The target file.</param>
		public static Task ConvertDat(string main, string target, MessageHandler.Progressor progressor) {
			throw new NotImplementedException();



			/*
			 
			 convertdat (dat, target) {
				this.emit('start.convert');

				this.emit('progress.convert', 100, 1, 'DAT file');

				// read DAT file
				fs.readFile(dat, { 'encoding': 'utf8' }, (err, datContents) => {
					if (err) throw err;

					// check that it's an XML file and it looks like something expected
					if (!datContents.startsWith('<?xml')) {
						throw 'Unable to read the DAT file, expected to start with <?xml version="1.0"?>';
					}
            
					parseString(datContents, (err, datXml) => {
						if (err) throw err;

						let gameNode;

						if (datXml.datafile && datXml.datafile.game) {
							// CLR MAME Pro format
							gameNode = 'game';
						}
						else if (datXml.datafile && datXml.datafile.machine) {
							// MAME format
							gameNode = 'machine';
						}
						else {
							throw 'DAT file needs to be in the MAME or CLRMAMEPRO format';
						}

						this.emit('log', 'DAT file has ' + datXml.datafile[gameNode].length + ' games');

						// create a file handler to write into
						fs.ensureFileSync(target);
						let stream = fs.createWriteStream(target, { 'encoding': 'utf8' });

						// write the header
						stream.write('name;description;year;manufacturer;is_parent;romof;is_clone;cloneof;sampleof\n');

						// for each game in the DAT, write a line in the CSV
						let requests = datXml.datafile[gameNode].reduce((promisechain, game, index) => {
							return promisechain.then(() => new Promise((resolve) => {
								this.emit('progress.convert', datXml.datafile[gameNode].length, index + 1, game.$.name);

								let line = '';
								line += game.$.name + ';';
								line += '"' + (game.description ? game.description[0] : '').replace(';', '-').replace('"', '') + '";';
								line += (game.year ? game.year[0] : '') + ';';
								line += '"' + (game.manufacturer ? game.manufacturer[0] : '') + '";';
                        
								line += (game.$.cloneof ? 'NO' : 'YES') + ';'; // is_parent
								line += (game.$.romof || '-') + ';'; // romof
								line += (game.$.cloneof ? 'YES' : 'NO') + ';'; // is_clone
								line += (game.$.cloneof || '-') + ';'; // cloneof
								line += (game.$.sampleof || '-'); // sampleof

								line += '\n';

								stream.write(line, () => {
									resolve();
								});
							}));
						}, Promise.resolve());

						requests.then(() => {
							console.log('done');
							this.emit('end.convert', target);
                    
							// close the file handler
							stream.end();
						});
					});
				});
			}
			 
			 */
		}
	}
}
