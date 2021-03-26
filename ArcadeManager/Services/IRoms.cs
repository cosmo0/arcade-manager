using ArcadeManager.Actions;
using System.Threading.Tasks;

namespace ArcadeManager.Services {

	/// <summary>
	/// Interface for the roms service
	/// </summary>
	public interface IRoms {

		/// <summary>
		/// Copies roms from a folder to another
		/// </summary>
		/// <param name="args">The arguments</param>
		/// <param name="progressor">The progress manager.</param>
		Task Add(RomsAction args, IMessageHandler progressor);

		/// <summary>
		/// Deletes roms from a folder
		/// </summary>
		/// <param name="args">The arguments</param>
		/// <param name="progressor">The progress manager.</param>
		Task Delete(RomsAction args, IMessageHandler progressor);

		/// <summary>
		/// Keeps only listed roms in a folder
		/// </summary>
		/// <param name="args">The arguments</param>
		/// <param name="progressor">The progress manager.</param>
		Task Keep(RomsAction args, IMessageHandler progressor);
	}
}
