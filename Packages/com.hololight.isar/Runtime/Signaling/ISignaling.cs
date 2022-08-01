using System;
using System.Threading.Tasks;

namespace HoloLight.Isar.Signaling
{
	/// <summary>
	/// An interface for different signaling implementations. IsarXRLoader instantiates one of them via reflection and
	/// calls its functions/listens for events. IDisposable.Dispose is called during the XR loader's lifecycle Stop().
	/// </summary>
	public interface ISignaling : IDisposable
	{
		/// <summary>
		/// Call this when signaling is ready to start the offer/answer exchange. This can be for example when
		/// a socket has been connected and messages can be exchanged. XR loader listens for this and reacts
		/// by calling CreateOffer.
		/// </summary>
		event Action Connected;

		/// <summary>
		/// Tells a subscriber that we received an SDP answer.
		/// </summary>
		event Action<string> SdpAnswerReceived;

		/// <summary>
		/// Tells a subscriber that we received an ICE candidate.
		/// </summary>
		event Action<string, int, string> IceCandidateReceived;

		/// <summary>
		/// Sends the server version to the client side.
		/// </summary>
		/// <param name="serverApiVersion"></param>
		/// <returns></returns>
		Task SendVersionAsync(uint serverApiVersion, bool hasDepthAlpha); // TODO: get rid of this an let the implementors send a byte array to allow arbitrary user data

		/// <summary>
		/// Sends an SDP offer to the client side.
		/// </summary>
		/// <param name="offerSdp">The SDP string</param>
		/// <returns></returns>
		Task SendOfferAsync(string offerSdp);

		/// <summary>
		/// Sends an ICE candidate to the client side.
		/// </summary>
		/// <param name="mid"></param>
		/// <param name="mlineIndex"></param>
		/// <param name="candidate"></param>
		/// <returns></returns>
		Task SendIceCandidateAsync(string mId, int mLineIndex, string candidate);

		/// <summary>
		/// Starts listening for a client connection. XR loader calls this as part of its lifecycle Start() method.
		/// </summary>
		/// <param name="address">Implementation-specific address. Can be an IP address, HTTP URI or something else entirely.</param>
		void Listen(/*string address*/);
	}
}
