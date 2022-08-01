#if UNITY_2018_4_OR_NEWER
#define USING_UNITY
#endif

#if USING_UNITY
//#define VERBOSE_LOGS

#else
//#define VERBOSE_LOGS
//#define TEST_THROW_XML_EXCEPTION

#endif

/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

#pragma warning disable IDE0063 // Use simple 'using' statement // not supported in older C#/Unity versions

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Debug = HoloLight.Isar.Shared.Debug;
using Trace = HoloLight.Isar.Shared.Trace;
using Assertions = HoloLight.Isar.Shared.Assertions;

namespace HoloLight.Isar.Signaling
{
	/// <summary>
	/// This is a sample signaling implementation.
	/// Do NOT use this in production. It is intended merely as an example!
	/// For production environments it is recommended to use a dedicated
	/// signaling server together with encryption of the signaling messages.
	///
	/// Basically, this is a plain TCP connection where messages consist of length (32-bit int)
	/// and content (UTF8-encoded (without BOM) XML).
	///
	/// When receiving either SDP or ICE messages it fires corresponding events.
	/// It also advertises a service via DNS-SD so it can be discovered by name instead
	/// of hardcoding IP addresses.
	/// </summary>
	public class DebugSignaling
#if USING_UNITY
		: ISignaling
#endif
	{
		public const int DEFAULT_PORT = 9999;

		private TcpListener _tcpListener;
		private TcpClient _client;
		//private NetworkStream _stream;
		private byte[] _msgBuffer = new byte[1024];
		// TODO
		// private MemoryStream _memStream = new MemoryStream(1024);

		private Task _connectAndReceiveTask; // TODO: delete
		private Task _connectAndReceiveTask2; // TODO: delete

		/// <summary>
		/// Invoked when a signaling connection has been established.
		/// </summary>
		public event Action Connected;
		protected void OnConnected()
		{
			try
			{
				if (Connected != null)
				{
					Connected.Invoke();
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
			}
		}

		/// <summary>
		/// Invoked when a signaling connection has been closed.
		/// </summary>
		public event Action Disconnected;
		protected void OnDisconnected()
		{
			try
			{
				Disconnected?.Invoke();
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
			}
		}

		/// <summary>
		/// Invoked when the remote endpoint version was received. Only needed on the client
		/// </summary>
		/// <param name="version">ISAR protocol version sent from remote endpoint</param>
		public event Action<uint, bool> VersionReceived;
		protected void OnVersionReceived(uint version, bool hasDepthAlpha)
		{
			try
			{
				VersionReceived?.Invoke(version, hasDepthAlpha);
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
			}
		}

#if USING_UNITY
		public event Action<string> SdpAnswerReceived;
		protected void OnSdpAnswerReceived(string sdp)
		{
			try
			{
				SdpAnswerReceived?.Invoke(sdp);
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
			}
		}
#else
		/// <summary>
		/// Invoked when Session Description Protocol Message (SDP) was received.
		/// </summary>
		/// <param name="sdp">Session Description Protocol Message sent from signaling Server</param>
		public event Action<string, string> SdpReceived;
		protected void OnSdpReceived(string type, string sdp)
		{
			try
			{
				SdpReceived?.Invoke(type, sdp);
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
			}
		}
#endif

		/// <summary>
		/// Invoked when Interactive Connectivity Establishment (ICE) candidate
		/// message was received from signaling Server.
		/// </summary>
		/// <param name="mId"></param>
		/// <param name="mLineIndex"></param>
		/// <param name="candidate"></param>
		public event Action<string, int, string> IceCandidateReceived;
		protected void OnIceCandidateReceived(string mId, int mLineIndex, string candidate)
		{
			try
			{
				IceCandidateReceived?.Invoke(mId, mLineIndex, candidate);
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
			}
		}

#if USING_UNITY
		// TODO: move this into IsarSignalingSubsystem or whatever (ScriptableObject entirely created by the user)
		// basically what RemoteCamera used to be
		private IPAddress _signalingServerIpAddress = IPAddress.Any;
		private int _signalingServerPort = DEFAULT_PORT;

		// HACK: ISignaling
		public async void Listen()
		{
			Task listenTask = null;
			try
			{
				//for (int tries = 3; tries > 0; tries--)
				//{
				listenTask = Task.Run(async () => await ListenAsync(_signalingServerIpAddress, _signalingServerPort));
					//listenTask = /*_signaling.*/Listen(_signalingServerIpAddress, _signalingServerPort)/*.ConfigureAwait(false)*/;
					///*await*/ listenTask.Wait();
					await listenTask;
				//}
			}
			catch (TaskCanceledException)
			{
				Debug.Log("+++> Connecting to signaling server cancelled by the user.");
				// ignored - expected behavior
			}
			catch (ObjectDisposedException disposedEx)
			{
				Debug.Log("+++> tcp listener was shut down while waiting for connections: " + disposedEx.Message);
			}
			catch (InvalidOperationException invalidOpEx)
			{
				Trace.Log(invalidOpEx.Message);
				if (Debugger.IsAttached) Debugger.Break();
			}
			catch (SocketException socketEx)
			{
				// repro: start a second server while the first one is still signaling
				Debug.LogError(socketEx.Message);
			}
			catch (IOException ioEx)
			{
				// repro: kill client during signaling
				// Unable to read data from the transport connection: An existing connection was forcibly closed by the remote host.
				Debug.Log("IOException: " + ioEx.Message + "\n" + ioEx.StackTrace);
				//Debug.LogWarning(ex.Message);
				//Reset();
			}
			catch (Exception ex)
			{
				switch ((uint)ex.HResult)
				{
					case 0x80004005: // SocketException
					// AddressNotAvailable: "The requested address is not valid in its context"
					// AccessDenied: "An attempt was made to access a socket in a way forbidden by its access permissions"
					// AlreadyInUse
					//System.Diagnostics.Debugger.Break();
					//goto case 0x8007274D;
					case 0x80072AF9: // "No such host is known. (Exception from HRESULT: 0x80072AF9)"
					case 0x8007274D: // "No connection could be made because the target machine actively refused it. No connection could be made because the target machine actively refused it."
						Debug.LogError(ex.Message);
						//Debug.LogWarning((uint)ex.HResult + ": " + ex.Message);
						// TODO: there might be more cases where we need to Reset() (calls StartSignaling() recursively)
						//Reset();
						break;
					default:
						//Debug.LogError((uint)ex.HResult + ": " + ex.Message);
						if (Debugger.IsAttached) Debugger.Break();
						throw;
				}
			}

			if (listenTask?.Status == TaskStatus.RanToCompletion)
			{
				Trace.Log("There were no matching ice candidates, I think?");
				//if (Debugger.IsAttached) Debugger.Break();
			}
		}

#endif

		// NOTE: WIP, this used to be "Async over Sync" (see https://stackoverflow.com/a/24298425)
		// due to our UWP/C# version not supporting IAsyncDisposable or JoinableTaskFactory
		// TODO: clean up async/await or just get rid of it all together (it's a cancerous & half-baked feature)
		public async Task Listen(IPAddress ipAddress, int port)
		{
			_connectAndReceiveTask = Task.Run(async () =>
			{
#if VERBOSE_LOGS
				Debug.Log($"Task Id: {Task.CurrentId ?? -1}");
#endif
				_connectAndReceiveTask2 = ListenAsync(ipAddress, port);
				// NOTE: called after first await in _connectAndReceiveTask2 task
#if VERBOSE_LOGS
				Debug.Log($"{nameof(_connectAndReceiveTask2)}.Id: {_connectAndReceiveTask2.Id}");
#endif
				await _connectAndReceiveTask2.ConfigureAwait(false);
				Debug.Log($"Task Id: {Task.CurrentId ?? -1} Thread Id: {Thread.CurrentThread.ManagedThreadId}");
			});
			await _connectAndReceiveTask.ConfigureAwait(false);
#if VERBOSE_LOGS
			Debug.Log($"{nameof(_connectAndReceiveTask)}.Id: {_connectAndReceiveTask.Id}");
#endif
		}

		//private Task<TcpClient> connectTask;
		/// <summary>
		/// Start listening for a TCP connection.
		/// </summary>
		/// <param name="ipAddress">IP to listen on</param>
		/// <param name="port">Port to listen on</param>
		/// <param name="token">Task cancellation token</param>
		public async Task ListenAsync(IPAddress ipAddress, int port)
		{
#if VERBOSE_LOGS
			Debug.Log($"Task Id: {Task.CurrentId ?? -1} Thread Id: {Thread.CurrentThread.ManagedThreadId}");
#endif
			Debug.Assert(_tcpListener == null);
			_tcpListener = new TcpListener(ipAddress, port);
			_tcpListener.Start(1);

/* 			// TODO: merge
			//On UWP with Unity 2019.4 the DNS-SD library is kinda janky and throws exceptions which prevent signaling from working,
			//so we temporarily disable it.
#if UNITY_EDITOR || !UNITY_WSA
			StartDnssdServiceAdvertisement(port);
#endif
*/
			Trace.Log("==========> Waiting for a connection. <==========");
			var accept = _tcpListener.AcceptTcpClientAsync();

			_client = await accept/*.ConfigureAwait(false)*/;
			//if (_client!= null) _client.LingerState = new LingerOption(true, 1);

			Trace.Log("==========> Signaling Connected! <==========");
			OnConnected();

			try
			{
				await ReceiveLoopAsync()/*.ConfigureAwait(false)*/;
			}
			finally
			{
				// TODO: close client here?
				Trace.Log("==========> Signaling Disconnecting! <==========");
				OnDisconnected();
				Trace.Log("==========> Signaling Disconnected! <==========");
				//if (Debugger.IsAttached) Debugger.Break();
			}
		}

		// NOTE: WIP, this used to be "Async over Sync" (see https://stackoverflow.com/a/24298425)
		// due to our UWP/C# version not supporting IAsyncDisposable or JoinableTaskFactory
		// TODO: clean up async/await or just get rid of it all together (it's a cancerous & half-baked feature)
		public async Task Connect(IPAddress ipAddress, int port)
		{
			_connectAndReceiveTask = Task.Run(async () =>
			{
#if VERBOSE_LOGS
				Debug.Log($"Task Id: {Task.CurrentId ?? -1}");
#endif
				_connectAndReceiveTask2 = ConnectAsync(ipAddress, port);
				// NOTE: called after first await in _connectAndReceiveTask2 task
#if VERBOSE_LOGS
				Debug.Log($"{nameof(_connectAndReceiveTask2)}.Id: {_connectAndReceiveTask2.Id}");
#endif
				await _connectAndReceiveTask2.ConfigureAwait(false);

			});
			await _connectAndReceiveTask.ConfigureAwait(false);
#if VERBOSE_LOGS
			Debug.Log($"{nameof(_connectAndReceiveTask)}.Id: {_connectAndReceiveTask.Id}");
#endif
		}

		/// <summary>
		/// Start listening for a TCP connection.
		/// </summary>
		/// <param name="ipAddress">IP to listen on</param>
		/// <param name="port">Port to listen on</param>
		/// <param name="token">Task cancellation token</param>
		public async Task ConnectAsync(IPAddress ipAddress, int port)
		{
#if VERBOSE_LOGS
			Debug.Log($"Task Id: {Task.CurrentId} Thread Id: {Thread.CurrentThread.ManagedThreadId}");
#endif

			_client = new TcpClient(AddressFamily.InterNetwork);

			Trace.Log("==========> Signaling Connecting. <==========");

			await _client.ConnectAsync(ipAddress, port).ConfigureAwait(false);

			Trace.Log("==========> Signaling Connected! <==========");
			OnConnected();

			try
			{
				await ReceiveLoopAsync().ConfigureAwait(false);
			}
			finally
			{
				Trace.Log("ReceiveLoopAsync exited");
				// TODO: close client here?
				Trace.Log("==========> Signaling Disconnecting! <==========");
				OnDisconnected();
				Trace.Log("==========> Signaling Disconnected! <==========");
				//if (Debugger.IsAttached) Debugger.Break();
			}
		}

#if TEST_THROW_XML_EXCEPTION
		private static bool testOnce = true;
#endif

		private async Task<int> TryReadBytesAsync(byte[] dst, NetworkStream srcStream, int targetNumberOfBytes, int maxAttempts = 10)
		{
			Debug.Assert(dst.Length >= targetNumberOfBytes);

			int numberOfBytesReadTotal = 0;
			int numberOfAttempts = 0;

			while(numberOfBytesReadTotal < targetNumberOfBytes)
			{
				if(numberOfAttempts == maxAttempts)
				{
					Debug.Log("+++> Need more than " + maxAttempts + " attempts to read from network stream.");
					Debug.Log("TODO: return a value or exception so that the endpoint can react e.g. by restarting");
					if (Debugger.IsAttached) Debugger.Break();
					break;
				}
				numberOfAttempts += 1;
#if VERBOSE_LOGS
				Debug.Log("read pass " + numberOfAttempts);
				Debug.Log("client?.Connected: " + _client?.Connected +
				          " client?.Available: " + _client?.Available + " (amount of data received from the network and available to read)" +
				          " stream.DataAvailable: " + _client?.GetStream()?.DataAvailable);
				Debug.Log($"ReadAsync: Begin: Task Id: {Task.CurrentId ?? -1} Thread Id: {Thread.CurrentThread.ManagedThreadId}");
#endif
				int numberOfBytesRead = await srcStream.ReadAsync(dst, numberOfBytesReadTotal, targetNumberOfBytes - numberOfBytesReadTotal);
				numberOfBytesReadTotal += numberOfBytesRead;
#if VERBOSE_LOGS
				Debug.Log($"ReadAsync: End: Task Id: {Task.CurrentId ?? -1} Thread Id: {Thread.CurrentThread.ManagedThreadId}");
				Debug.Log("numberOfBytesRead: " + numberOfBytesRead);
				if (numberOfBytesReadTotal != targetNumberOfBytes)
					Debug.Log("bytes read so far: " + new UTF8Encoding(false, true).GetString(dst, 0, numberOfBytesReadTotal));
#endif
				// NOTE: await is a suspension point, which means that it is possible that the socket may be disposed or a cancellation requested while being suspended, this causes ReadAsync to return 0
				if (numberOfBytesRead == 0)
				{
					break;
				}

#if VERBOSE_LOGS
				Debug.Log($"ReadAsync: {numberOfBytesReadTotal}/{targetNumberOfBytes}");
#endif
			}
			//Debug.Assert(numberOfBytesReadTotal == dst.Length, "bytesReadTotal != dst.Length");
			return numberOfBytesReadTotal;
		}

		// TODO: cancel async calls cooperatively when all ice candidates are successfully exchanged
		private async Task ReceiveLoopAsync()
		{
#if VERBOSE_LOGS
			Debug.Log($"ReceiveLoopAsync: Task Id: {Task.CurrentId ?? -1} Thread Id: {Thread.CurrentThread.ManagedThreadId}");
#endif

			var stream = _client.GetStream();

#if VERBOSE_LOGS
			Debug.Log("_client?.ReceiveBufferSize: " + _client?.ReceiveBufferSize);
			Debug.Log("_client?.ReceiveTimeout: " + _client?.ReceiveTimeout);
			Debug.Log("_client?.SendBufferSize: " + _client?.SendBufferSize);
			Debug.Log("_client?.SendTimeout: " + _client?.SendTimeout);
#endif
			while (true)
			{
				Debug.Log("===== Waiting for signaling messages =====");

				int msgLength;
				{
					byte[] lengthBuffer = new byte[sizeof(int)];
					// TODO: look into ConfigureAwait(false) to stick to one receiving thread (seems to work on hololens client but not in unity...)
					int numberOfBytesRead = await TryReadBytesAsync(lengthBuffer, stream, lengthBuffer.Length);
					if (numberOfBytesRead != lengthBuffer.Length)
					{
						Trace.Log($"Unable to read entire message header ({numberOfBytesRead}/{lengthBuffer.Length})! exiting receive loop");
						return;
					}
					msgLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBuffer, 0));
				}

//#if VERBOSE_LOGS
				Debug.Log("message length: " + msgLength);
//#endif
				byte[] msgBuffer = new byte[msgLength];
				{
					// TODO: look into ConfigureAwait(false) to stick to one receiving thread (seems to work on hololens client but not in unity...)
					int numberOfBytesRead = await TryReadBytesAsync(msgBuffer, stream, msgBuffer.Length);
					if (numberOfBytesRead != msgBuffer.Length)
					{
						Trace.Log($"Unable to read entire message body ({numberOfBytesRead}/{msgBuffer.Length})! exiting receive loop");
						return;
					}
				}

				Assertions.Assert((char)msgBuffer[0] == '<' && (char)msgBuffer[msgLength - 1] == '>');

#if VERBOSE_LOGS
				var msg = new UTF8Encoding(false, true).GetString(msgBuffer, 0, msgLength);
				Trace.Log("\nreceived msg (length: " + msg.Length + "): |" + msg + "|");
#endif

#if TEST_THROW_XML_EXCEPTION
				if (testOnce)
				{
					testOnce = false;
					throw new XmlException("test");
				}
#endif

				ParseMessage(msgBuffer);

				// TODO: exit while loop here once ice candidates are successfully exchanged?
			}
		}

		private void ParseMessage(in byte[] msgRaw)
		{
			try
			{
				using (XmlReader reader = XmlReader.Create(new MemoryStream(msgRaw)))
				{
					var xmlNodeType = reader.MoveToContent();
					if (xmlNodeType != XmlNodeType.Element)
					{
						Debug.LogError("Signaling: XML message doesn't contain an element.");
						return;
					}
					Debug.Log("parsing xml node <" + reader.Name + ">");
					switch (reader.Name)
					{
						case Tokens.VERSION:
							IsarXmlReader.ReadVersion(reader, out var version, out var caps);
							OnVersionReceived(version, caps);
							break;
						case Tokens.SDP:
							IsarXmlReader.ReadSdp(reader, out var type, out var sdp);
#if USING_UNITY
							// HACK: ISignaling
							if (type == Tokens.SDP_TYPE_OFFER)
							{
								//OnSdpOfferReceived(sdp);
							}
							else if (type == Tokens.SDP_TYPE_ANSWER)
							{
								OnSdpAnswerReceived(sdp);
							}
#else
							OnSdpReceived(type, sdp);
#endif
							break;
						case Tokens.ICE_CANDIDATE:
							IsarXmlReader.ReadIceCandidate(reader, out var mId, out var mLineIndex, out var candidate);
							OnIceCandidateReceived(mId, mLineIndex, candidate);
							break;
						default:
							Debug.LogError("Unexpected token. XML message cannot be parsed.");
							Debugger.Break();
							break;
					}
				}

			}
			catch (Exception e)
			{
				// TODO: we need to fix this, this should never happen but sometimes it does!!!
				// exception message: "'.', hexadecimal value 0x00, is an invalid character. Line 1, position 197."
				Debug.LogError(e.Message + "\nexception stacktrace: \n" + e.StackTrace);
				if (Debugger.IsAttached) Debugger.Break();
				throw;
			}
		}

		private Task _lastSendTask = null;

		//private static int sendCount = 0;
		public async Task SendQueuedAsync(Task task)
		{
			if (_lastSendTask == null)
			{
				_lastSendTask = task;
				await _lastSendTask;
			}
			else
			{
#if false
				var previousTask = _lastSendTask;
				_lastSendTask = task;
				await previousTask.ContinueWith(
						async _ => await _lastSendTask,
						TaskContinuationOptions.OnlyOnRanToCompletion |
						TaskContinuationOptions.PreferFairness /*|*/
				//TaskContinuationOptions.ExecuteSynchronously |
				//TaskContinuationOptions.
				);
#else
				// TODO: I don't think this actually fixed anything
				Task continuation = _lastSendTask.ContinueWith(
					async _ => await task,
					TaskContinuationOptions.OnlyOnRanToCompletion |
					TaskContinuationOptions.PreferFairness /*|*/
					//TaskContinuationOptions.ExecuteSynchronously |
					//TaskContinuationOptions.
				).Unwrap();
				_lastSendTask = continuation;
				await _lastSendTask;
#endif
			}
			//Trace.Log("===== finished with send task " + sendCount);
			//sendCount++;
		}

		//public async Task SendVersionAsync(uint version) => await SendQueuedAsync(SendVersionAsync_OG(version));
		public async Task SendVersionAsync_OG(uint version)
		{
			var length = IsarXmlWriter.WriteVersionAsBytes(version, ref _msgBuffer, sizeof(int));
			await SendAsync(_msgBuffer, length);
		}

		public async Task SendVersionAsync(uint version, bool supportDepthAlpha) => await SendQueuedAsync(SendVersionAsync_Stream(version, supportDepthAlpha));
		public async Task SendVersionAsync_Stream(uint version, bool supportDepthAlpha) =>
			await SendMessageAsync(stream => IsarXmlWriter.WriteVersion(version, supportDepthAlpha, stream), 4);

		// HACK: ISignaling
		public async Task SendOfferAsync(string sdp)
		{
#if VERBOSE_LOGS
			Debug.Log($"SendQueuedAsync: Begin: Task Id: {Task.CurrentId ?? -1} Thread Id: {Thread.CurrentThread.ManagedThreadId}");
#endif
			await SendQueuedAsync(SendSdpAsync(Tokens.SDP_TYPE_OFFER, sdp));
#if VERBOSE_LOGS
			Debug.Log($"SendQueuedAsync: End: Task Id: {Task.CurrentId ?? -1} Thread Id: {Thread.CurrentThread.ManagedThreadId}");
#endif
		}

		public async Task SendSdpAsync(string type, string sdp)
		{
			// TODO: actually test this out with 10 reconnects in release without debuggers attached
			//await SendMessageAsync( stream => IsarXmlWriter.WriteSdp(type, sdp, stream) );

			// TODO: maybe use a shared buffer
			using (var stream = new MemoryStream(1024))
			{
				stream.Seek(sizeof(int), SeekOrigin.Begin);
				IsarXmlWriter.WriteSdp(type, sdp, stream);
				await SendAsync(stream.GetBuffer(), (int)stream.Position - sizeof(int));
			}
		}

		public async Task SendMessageAsync(Action<Stream> write, int bufferSize = 1024)
		{
			// TODO: maybe use a shared buffer, sending is single-threaded/sequential anyway
			using (var stream = new MemoryStream(bufferSize))
			{
				stream.Seek(sizeof(int), SeekOrigin.Begin);
				write(stream);
				await SendAsync(stream.GetBuffer(), (int)stream.Position - sizeof(int));
			}
		}

		// HACK: ISignaling
		public async Task SendIceCandidateAsync(string mId, int mLineIndex, string candidate)
		{
#if VERBOSE_LOGS
			Debug.Log($"SendQueuedAsync: Begin: Task Id: {Task.CurrentId ?? -1} Thread Id: {Thread.CurrentThread.ManagedThreadId}");
#endif
			await SendQueuedAsync(SendIceCandidateAsync_OG(mId, mLineIndex, candidate));
#if VERBOSE_LOGS
			Debug.Log($"SendQueuedAsync: End: Task Id: {Task.CurrentId ?? -1} Thread Id: {Thread.CurrentThread.ManagedThreadId}");
#endif
		}

#if false
		/// <summary>
		/// Sends an ICE message to the remote peer.
		/// </summary>
		/// <param name="mId">SDP mid</param>
		/// <param name="mLineIndex">SDP m-line index</param>
		/// <param name="candidate">SDPized candidate</param>
		public async Task SendIceCandidateAsync_OG(string mId, int mLineIndex, string candidate)
		{
			var length = IsarXmlWriter.WriteIceCandidateAsBytes(mId, mLineIndex, candidate, ref _msgBuffer, sizeof(int));
			await SendAsync(_msgBuffer, length);
		}
#else
		/// <summary>
		/// Sends an ICE message to the remote peer.
		/// </summary>
		/// <param name="mId">SDP mid</param>
		/// <param name="mLineIndex">SDP m-line index</param>
		/// <param name="candidate">SDPized candidate</param>
		public async Task SendIceCandidateAsync_OG(string mId, int mLineIndex, string candidate)
		{
			using (var stream = new MemoryStream(1024))
			{
				stream.Seek(sizeof(int), SeekOrigin.Begin);
				IsarXmlWriter.WriteIceCandidate(mId, mLineIndex, candidate, stream);
				await SendAsync(stream.GetBuffer(), (int)stream.Position - sizeof(int));
			}
		}
#endif

		public async Task SendAsync(byte[] message, int length)
		{
			// prepend length in front of array
			int networkLength = IPAddress.HostToNetworkOrder(length);
			byte[] encodedMessageLength = BitConverter.GetBytes(networkLength);
			encodedMessageLength.CopyTo(message, 0);
			//message[0] = (byte)  networkLength;
			//message[1] = (byte) (networkLength >> 8);
			//message[2] = (byte) (networkLength >> 16);
			//message[3] = (byte) (networkLength >> 24);

#if VERBOSE_LOGS
			Trace.Log(">>> Sending(" + length + "): |" + new UTF8Encoding(false, true).GetString(message, sizeof(int), length) + "|");
#endif
			var first = (char)message[0 + sizeof(int)];
			var last = (char)message[length - 1 + sizeof(int)];
			Assertions.Assert(first == '<' && last == '>');

			await _client?.GetStream()?.WriteAsync(message, 0, length + sizeof(int));
		}

		public void Dispose()
		{
#if VERBOSE_LOGS
				Debug.Log("Signaling.Dispose: Thread Id: " + Thread.CurrentThread.ManagedThreadId);
#endif
//				try
//				{
//					if (_client?.Connected == true)
//					{
//#if VERBOSE_LOGS
//						Debug.Log("DebugSignaling is getting disposed while stream is still active; flushing stream.");
//#endif
//						//_client?.GetStream()?.FlushAsync().Wait(); // NetworkStream.FlushAsync doesn't do anything
//						// TODO: _client?.Close();
//						//_client?.GetStream()?.Flush();
//						//Debug.Log("Signaling.Dispose: waiting for stream operations to finish (deadlock potential)");
//						//TODO: not being able to cancel TcpListener.ListenAsync is really annoying, we should separate the initial tcp connection establishment from the receive loop to enable task cancellation
//						//_cancellation.Cancel(true);
//						//_lastSendTask.Wait();
//						//_connectAndReceiveTask.Wait();
//#if VERBOSE_LOGS
//						Debug.Log("Signaling.Dispose: finished waiting for send operations to finish");
//#endif
//					}
//				}
//				catch (Exception e)
//				{
//					Debug.LogError(e.Message);
//					if (Debugger.IsAttached) Debugger.Break();
//					throw;
//				}

			try {
				// TODO:
				//_client.Close();
				_client?.Dispose();
			}
			catch(Exception e)
			{
				Debug.LogError(e.Message);
				if (Debugger.IsAttached) Debugger.Break();
				throw;
			}

			try
			{
				_tcpListener?.Stop();

			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
				if (Debugger.IsAttached) Debugger.Break();
				throw;
			}
			_tcpListener = null;
		}
	}
}

#pragma warning restore IDE0063 // Use simple 'using' statement
