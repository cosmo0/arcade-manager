using ArcadeManager.Actions;
using System.Threading.Tasks;

namespace ArcadeManager.Services {

	/// <summary>
	/// Interface for the overlays service
	/// </summary>
	public interface IOverlays {

		/// <summary>
		/// Downloads an overlay pack
		/// </summary>
		/// <param name="data">The parameters</param>
		/// <param name="messageHandler">The message handler.</param>
		/// <returns></returns>
		Task Download(OverlaysAction data, IMessageHandler messageHandler);
	}
}
