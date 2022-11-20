using ArcadeManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcadeManager.Services {

	/// <summary>
	/// Interface for the CSV service
	/// </summary>
	public interface ICsv {

		/// <summary>
		/// Converts a DAT file.
		/// </summary>
		/// <param name="main">The main file.</param>
		/// <param name="target">The target file.</param>
		/// <param name="messageHandler">The message handler.</param>
		/// <returns></returns>
		Task ConvertDat(string main, string target, IMessageHandler messageHandler);

		/// <summary>
		/// Converts a INI file to CSV
		/// </summary>
		/// <param name="main">The main file</param>
		/// <param name="target">The target folder to create files into</param>
		/// <param name="messageHandler">The message handler.</param>
		/// <returns></returns>
		Task ConvertIni(string main, string target, IMessageHandler messageHandler);

		/// <summary>
		/// Keeps files that are listed in both files
		/// </summary>
		/// <param name="main">The path to the main file.</param>
		/// <param name="secondary">The path to the secondary file.</param>
		/// <param name="target">The path to the target file.</param>
		/// <param name="messageHandler">The message handler.</param>
		/// <returns></returns>
		Task Keep(string main, string secondary, string target, IMessageHandler messageHandler);

		/// <summary>
		/// Lists the files in a folder to a CSV file.
		/// </summary>
		/// <param name="main">The main folder.</param>
		/// <param name="target">The target CSV file.</param>
		/// <param name="messageHandler">The message handler.</param>
		/// <returns></returns>
		Task ListFiles(string main, string target, IMessageHandler messageHandler);

		/// <summary>
		/// Merges the specified main and secondary files to the target file.
		/// </summary>
		/// <param name="main">The path to the main file.</param>
		/// <param name="secondary">The path to the secondary file.</param>
		/// <param name="target">The path to the target file.</param>
		/// <param name="messageHandler">The message handler.</param>
		/// <returns></returns>
		Task Merge(string main, string secondary, string target, IMessageHandler messageHandler);

		/// <summary>
		/// Reads the provided CSV file
		/// </summary>
		/// <param name="filepath">The path to the file</param>
		/// <param name="getOtherValues">if set to <c>true</c> get the values other than the name.</param>
		/// <returns>
		/// The list of games in the CSV file
		/// </returns>
		Task<CsvGamesList> ReadFile(string filepath, bool getOtherValues);

		/// <summary>
		/// Removes entries in the main file that are in the secondary
		/// </summary>
		/// <param name="main">The path to the main file.</param>
		/// <param name="secondary">The path to the secondary file.</param>
		/// <param name="target">The path to the target file.</param>
		/// <param name="messageHandler">The message handler.</param>
		/// <returns></returns>
		Task Remove(string main, string secondary, string target, IMessageHandler messageHandler);
	}
}