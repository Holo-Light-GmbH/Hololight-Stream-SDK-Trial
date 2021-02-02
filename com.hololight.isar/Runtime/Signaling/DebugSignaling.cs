#if UNITY_2018_4_OR_NEWER
#define USING_UNITY
#endif



#pragma warning disable IDE0063 
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

#if USING_UNITY
// TODO: merge // @nocheckin
using Unity.XR.Isar;
// using UnityEngine;
// using UnityEngine.Assertions;
#endif

namespace HoloLight.Isar.Signaling
{
#if WITH_CANCELLATION
	public static class BlackMagic
	{
		// ref: https://stackoverflow.com/questions/14524209/what-is-the-correct-way-to-cancel-an-async-operation-that-doesnt-accept-a-cance/14524565#14524565
		// ref: https://devblogs.microsoft.com/pfxteam/how-do-i-cancel-non-cancelable-async-operations/
		public static async Task<T> WithWaitCancellation<T>(this Task<T> task, CancellationToken token)
		{
			// The task completion source.
			var tcs = new TaskCompletionSource<bool>();

			// Register with the cancellation token.
			using(token.Register( s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs) )
			{
				// If the task waited on is the cancellation token...
				if (await Task.WhenAny(task, tcs.Task) != task)
				{
					throw new OperationCanceledException(token);
				}
			}

			// Wait for one or the other to complete.
			return await task;
		}

		// throws
		// NullReferenceException - The TaskAwaiter object was not properly initialized.
		// TaskCanceledException - The task was canceled.
		// Exception - The task completed in a Faulted state.
		public static Task<T> WithWaitCancellation2<T>(this Task<T> task, CancellationToken cancellationToken)
		{
			return task.IsCompleted
				? task
				: task.ContinueWith(
					completedTask => completedTask.GetAwaiter().GetResult(),
					cancellationToken,
					TaskContinuationOptions.ExecuteSynchronously,
					TaskScheduler.Default);
		}
	}
#endif // WITH_CANCELLATION

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
		private byte[] _msgBuffer = new byte[1024];


		private CancellationTokenSource _cancellation = new CancellationTokenSource();
		private Task _connectAndReceiveTask;
		private Task _connectAndReceiveTask2;

		private readonly Encoding _encoding = new UTF8Encoding(true, true); 
		private readonly IsarXmlWriter _isarXmlWriter = new IsarXmlWriter();

		/// <summary>
		/// Invoked when a signaling connection has been established.
		/// </summary>
		public event Func<Task> Connected;
		protected async Task OnConnected()
		{
			try
			{
				if (Connected != null)
				{
					await Connected.Invoke();
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
				throw;
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
				throw;
			}
		}

		/// <summary>
		/// Invoked when the remote endpoint version was received. Only needed on the client
		/// </summary>
		/// <param name="version">ISAR protocol version sent from remote endpoint</param>
		public event Action<uint> VersionReceived;
		protected void OnVersionReceived(uint version)
		{
			try
			{
				VersionReceived?.Invoke(version);
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
				throw;
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
				throw;
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
				throw;
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
				throw;
			}
		}

#if USING_UNITY
		// TODO: move this into IsarSignalingSubsystem or whatever (ScriptableObject entirely created by the user)
		// basically what RemoteCamera used to be
		private IPAddress _signalingServerIpAddress = IPAddress.Any;
		private int _signalingServerPort = DebugSignaling.DEFAULT_PORT;

		// @nocheckin TODO: HACK: ISignaling
		public async void Listen()
		{
			try
			{
				await /*_signaling.*/Listen(_signalingServerIpAddress, _signalingServerPort).ConfigureAwait(false);
			}
			catch (TaskCanceledException)
			{
				Debug.Log("+++> StartSignaling:\n" + " Connecting to signaling server cancelled by the user.");
				// ignored - expected behavior
			}
			catch (ObjectDisposedException disposedEx)
			{
				Debug.Log("+++> StartSignaling:\n" + "tcp listener was shut down while waiting for connections: " + disposedEx.Message);
			}
			catch (InvalidOperationException invalidOpEx)
			{
				Console.WriteLine("+++> StartSignaling:\n" + invalidOpEx.Message + "\n" + invalidOpEx.StackTrace);
				Debugger.Break();
			}
			catch (System.Net.Sockets.SocketException socketEx)
			{
				// repro: start a second server while the first one is still signaling
				Debug.LogError(socketEx.Message);
			}
			catch (IOException ioEx)
			{
				// repro: kill client during signaling
				// Unable to read data from the transport connection: An existing connection was forcibly closed by the remote host.
				Debug.Log(ioEx.Message);
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
						Debugger.Break();
						throw;
				}
			}
		}

#endif

		public async Task Listen(IPAddress ipAddress, int port)
		{
			_connectAndReceiveTask = Task.Run(async () =>
			{
				Debug.Log($"{nameof(Listen)}: Task.Run: Task Id: {Task.CurrentId ?? -1}");
				_connectAndReceiveTask2 = ListenAsync(ipAddress, port, _cancellation.Token);
				Debug.Log($"{nameof(Listen)}: Task.Run: {nameof(_connectAndReceiveTask2)}.Id: {_connectAndReceiveTask2.Id}");
				await _connectAndReceiveTask2.ConfigureAwait(false);

			}, _cancellation.Token);
			await _connectAndReceiveTask.ConfigureAwait(false);
			Debug.Log($"{nameof(Listen)}: Task.Run: {nameof(_connectAndReceiveTask)}.Id: {_connectAndReceiveTask.Id}");
		}

		/// <summary>
		/// Start listening for a TCP connection.
		/// </summary>
		/// <param name="ipAddress">IP to listen on</param>
		/// <param name="port">Port to listen on</param>
		/// <param name="token">Task cancellation token</param>
		public async Task ListenAsync(IPAddress ipAddress, int port, CancellationToken token)
		{
			Debug.Log($"+++> {nameof(ListenAsync)} begin");
			Debug.Log($"{nameof(ListenAsync)}: Task Id: {Task.CurrentId} Thread Id: {Thread.CurrentThread.ManagedThreadId}");

			Debug.Assert(_tcpListener == null);
			_tcpListener = new TcpListener(ipAddress, port);
			_tcpListener.Start(1);


			Debug.Log("==========> Waiting for a connection. <==========");
			var accept = _tcpListener.AcceptTcpClientAsync();
#if WITH_CANCELLATION
			//connectTask = _tcpListener.AcceptTcpClientAsync(/*cancellation.Token*/);
			//_client = await connectTask.ConfigureAwait(false);

			//cancellation.Token.ThrowIfCancellationRequested();
			_client = await accept.WithWaitCancellation2(token).ConfigureAwait(false);
#else
			_client = await accept.ConfigureAwait(true);
#endif

			Debug.Log("==========> Connected! <==========");
			await OnConnected();

			try
			{
				await ReceiveLoopAsync(token).ConfigureAwait(true);
			}

			finally
			{
				OnDisconnected();
				Debug.Log("==========> Disconnected! <==========");
			}
		}


		public async Task Connect(IPAddress ipAddress, int port)
		{
			_connectAndReceiveTask = Task.Run(async () =>
			{
				Debug.Log($"{nameof(Connect)}: Task.Run: Task Id: {Task.CurrentId ?? -1}");
				_connectAndReceiveTask2 = ConnectAsync(ipAddress, port, _cancellation.Token);
				Debug.Log($"{nameof(Connect)}: Task.Run: {nameof(_connectAndReceiveTask2)}.Id: {_connectAndReceiveTask2.Id}");
				await _connectAndReceiveTask2.ConfigureAwait(false);

			}, _cancellation.Token);
			await _connectAndReceiveTask.ConfigureAwait(false);
			Debug.Log($"{nameof(Connect)}: Task.Run: {nameof(_connectAndReceiveTask)}.Id: {_connectAndReceiveTask.Id}");
		}
//#endif

		/// <summary>
		/// Start listening for a TCP connection.
		/// </summary>
		/// <param name="ipAddress">IP to listen on</param>
		/// <param name="port">Port to listen on</param>
		/// <param name="token">Task cancellation token</param>
		public async Task ConnectAsync(IPAddress ipAddress, int port, CancellationToken token)
		{
			Debug.Log($"+++> {nameof(ConnectAsync)} begin");
			Debug.Log($"{nameof(ListenAsync)}: Task Id: {Task.CurrentId} Thread Id: {Thread.CurrentThread.ManagedThreadId}");

			_client = new TcpClient(AddressFamily.InterNetwork);

			Debug.Log("==========> Connecting. <==========");
#if WITH_CANCELLATION
			//cancellation.Token.ThrowIfCancellationRequested();
			_client = await accept.WithWaitCancellation2(token)/*.ConfigureAwait(false)*/;
#else
			await _client.ConnectAsync(ipAddress, port).ConfigureAwait(true);
#endif



			Debug.Log("==========> Connected! <==========");
			await OnConnected();

			try
			{
				await ReceiveLoopAsync(token).ConfigureAwait(true);
			}
			catch(Exception ex)
			{
				Debug.Log("Exception thrown which i dont care");
			}

			finally
			{
				OnDisconnected();
				Debug.Log("==========> Disconnected! <==========");
			}
		}

		private async Task ReceiveLoopAsync(CancellationToken token)
		{
			Debug.Log($"ReceiveLoopAsync: Task Id: {Task.CurrentId} Thread Id: {Thread.CurrentThread.ManagedThreadId}");

				var stream = _client.GetStream();
				Debug.Log("_client?.ReceiveBufferSize: " + _client?.ReceiveBufferSize);
				Debug.Log("_client?.ReceiveTimeout: " + _client?.ReceiveTimeout);
				Debug.Log("_client?.SendBufferSize: " + _client?.SendBufferSize);
				Debug.Log("_client?.SendTimeout: " + _client?.SendTimeout);

				while (/*_client.Connected*/ true
#if WITH_CANCELLATION
				       && !cancellation.IsCancellationRequested
#endif
				)
				{
					try
					{

					Debug.Log("===== recv =====");
					byte[] lengthBuffer = new byte[sizeof(int)];
				
					int numberOfBytesRead = 0;
				
					Debug.Log("client?.Connected: " + _client?.Connected +
					          " client?.Available: " + _client?.Available + " (amount of data received from the network and available to read)" +
					          " stream.DataAvailable: " + _client?.GetStream()?.DataAvailable);

						Debug.Log("ThreadId: " + Thread.CurrentThread.ManagedThreadId);
						numberOfBytesRead = await stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length, token).ConfigureAwait(true);
						Debug.Log("ThreadId: " + Thread.CurrentThread.ManagedThreadId);
		
					Debug.Log("numberOfBytesRead: " + numberOfBytesRead);



					Debug.Log("client?.Connected: " + _client?.Connected +
					          " client?.Available: " + _client?.Available + " (amount of data received from the network and available to read)" +
					          " stream.DataAvailable: " + _client?.GetStream()?.DataAvailable);

					if (numberOfBytesRead == 0)
					{

						return;
					}
					Debug.Assert(numberOfBytesRead == lengthBuffer.Length, "numberOfBytesRead != lengthBuffer.Length");

					Debug.Log("lengthBuffer: " + BitConverter.ToInt32(lengthBuffer, 0).ToString("x8") + " (== little endian of msgLength)");
					int msgLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBuffer, 0));
				
					Debug.Log("msgLength: " + msgLength);

					byte[] msgBuffer = new byte[msgLength];

					const int MAX_TRIES = 3;
					int tries = MAX_TRIES;
					numberOfBytesRead = 0;
					int numberOfBytesReadSum = 0;
					do
					{
						Debug.Log("read pass " + (MAX_TRIES - tries));
					

						Debug.Log("ThreadId: " + Thread.CurrentThread.ManagedThreadId);
						numberOfBytesRead = await stream.ReadAsync(msgBuffer, numberOfBytesRead, msgLength - numberOfBytesRead, token).ConfigureAwait(true);
						numberOfBytesReadSum += numberOfBytesRead;
						Debug.Log($"ReadAsync: {numberOfBytesReadSum}/{msgLength}");
						Debug.Log("ThreadId: " + Thread.CurrentThread.ManagedThreadId);

					Debug.Log("numberOfBytesRead: " + numberOfBytesRead);


						Debug.Log("client?.Connected: " + _client?.Connected +
						          " client?.Available: " + _client?.Available + " (amount of data received from the network and available to read)" +
						          " stream.DataAvailable: " + _client?.GetStream()?.DataAvailable);

						if (numberOfBytesReadSum != msgLength)
							Debug.Log("msgBuffer (so far): " + Encoding.Default.GetString(msgBuffer, 0, numberOfBytesReadSum));

						if (numberOfBytesRead == 0)
						{
							
							Debugger.Break();
							return;
						}

						tries--;
					} while (numberOfBytesReadSum < msgLength && tries > 0);

					Debug.Assert(numberOfBytesReadSum==msgLength);

					if (tries == 0)
					{
						Debug.Log("+++> Need more than " + MAX_TRIES + "passes to read message. Pulling the plug, the message is too big.");
						Debug.Log("TODO: return a value or exception so that the endpoint can react e.g. by restarting");
						return;
					}

					Debug.Assert(numberOfBytesReadSum == msgLength, "numberOfBytesRead != msgLength");

					var msg = Encoding.UTF8.GetString(msgBuffer);
					Debug.Log("\nreceived msg (length: " + msg.Length + "): |" + msg + "|");

					var msg2 = msg.Replace("\0", "");
					if (msg != msg2) Debug.Log("\nmsg2 (length: " + msg2.Length + "): |" + msg2 + "|");
					ParseMessage(msgBuffer);


					}
					catch (IOException e)
					{
						Debug.Log(e.Message + "\n" + e.StackTrace);
						Console.WriteLine(e.Message + "\n" + e.StackTrace);
						throw;
					}
				}
		}

#if WITH_CANCELLATION
		public void Cancel()
		{
			cancellation.Cancel();
			//cancellation.Cancel(true);
		}
#endif


		private void ParseMessage(in byte[] msgRaw)
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
						IsarXmlReader.ReadVersion(reader, out var version);
						OnVersionReceived(version);
						break;
					case Tokens.SDP:
						IsarXmlReader.ReadSdp(reader, out var type, out var sdp);
#if USING_UNITY
						// @nocheckin TODO: HACK: ISignaling
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

		public async Task SendVersionAsync(uint serverApiVersion) => await SendQueuedAsync(SendVersionAsync_OG(serverApiVersion));
		public async Task SendVersionAsync_OG(uint serverApiVersion)
		{
#if WRITE_STRING
			await SendAsync(_isarXmlWriter.WriteVersionAsString(serverApiVersion));
#else
			var length = _isarXmlWriter.WriteVersionAsBytes(serverApiVersion, ref _msgBuffer, sizeof(int));
			await SendAsync(_msgBuffer, length);
#endif
		}

		/// <summary>
		/// Sends a offer SDP (session description) message to the remote peer.
		/// </summary>
		/// <param name="sdp">Offer Session description</param>

		/// <summary>
		/// Sends a answer SDP (session description) message to the remote peer.
		/// </summary>
		/// <param name="sdp">Offer Session description</param>

		/// <summary>
		/// Sends an SDP (session description) message to the remote peer.
		/// </summary>
		/// <param name="sdp">Offer Session description</param>
		public async Task SendSdpAsync2(string type, string sdp)
		{
#if WRITE_STRING
			await SendAsync(_isarXmlWriter.WriteSdpAsString(type, sdp));
#else
			var length = _isarXmlWriter.WriteSdpAsBytes(type, sdp, ref _msgBuffer, sizeof(int));
			await SendAsync(_msgBuffer, length);
#endif
		}

		private Task _lastSendTask = null;



		public async Task SendQueuedAsync(Task task)
		{
			if (_lastSendTask == null)
			{
				_lastSendTask = task;
				Debug.Log("pre await SendQueuedAsync: ThreadId: " + Thread.CurrentThread.ManagedThreadId);
				await _lastSendTask;
				Debug.Log("post await SendQueuedAsync: ThreadId: " + Thread.CurrentThread.ManagedThreadId);
			}
			else
			{
				var previousTask = _lastSendTask;
				_lastSendTask = task;
				Debug.Log("pre await SendQueuedAsync: ThreadId: " + Thread.CurrentThread.ManagedThreadId);
				await previousTask.ContinueWith( _ => _lastSendTask );
				Debug.Log("post await SendQueuedAsync: ThreadId: " + Thread.CurrentThread.ManagedThreadId);
			}
		}

		public async Task SendOfferAsync(string sdp)
		{
			Debug.Log("pre await SendOfferAsync: ThreadId: " + Thread.CurrentThread.ManagedThreadId);
			await SendQueuedAsync(SendSdpAsync(Tokens.SDP_TYPE_OFFER, sdp));
			Debug.Log("post await SendOfferAsync: ThreadId: " + Thread.CurrentThread.ManagedThreadId);
		}


		public async Task SendSdpAsync(string type, string sdp)
		{
#if WRITE_STRING
			await SendAsync(_isarXmlWriter.WriteSdpAsString(type, sdp));
#else
			using (var stream = new MemoryStream(1024))
			{
				stream.Seek(sizeof(int), SeekOrigin.Begin);
				_isarXmlWriter.WriteSdp(type, sdp, stream);
				await SendAsync(stream.GetBuffer(), (int)stream.Position - sizeof(int));
			}
#endif
		}

		public async Task SendIceCandidateAsync(string mId, int mLineIndex, string candidate)
		{
			Debug.Log("pre await SendIceCandidateAsync: ThreadId: " + Thread.CurrentThread.ManagedThreadId);
			await SendQueuedAsync(SendIceCandidateAsync_OG(mId, mLineIndex, candidate));
			Debug.Log("post await SendIceCandidateAsync: ThreadId: " + Thread.CurrentThread.ManagedThreadId);
		}


		/// <summary>
		/// Sends an ICE message to the remote peer.
		/// </summary>
		/// <param name="mId">SDP mid</param>
		/// <param name="mLineIndex">SDP m-line index</param>
		/// <param name="candidate">SDPized candidate</param>
		public async Task SendIceCandidateAsync_OG(string mId, int mLineIndex, string candidate)
		{
#if WRITE_STRING
			await SendAsync(_isarXmlWriter.WriteIceCandidateAsString(mId, mLineIndex, candidate));
#else
			var length = _isarXmlWriter.WriteIceCandidateAsBytes(mId, mLineIndex, candidate, ref _msgBuffer, sizeof(int));
			await SendAsync(_msgBuffer, length);
#endif
		}


		public async Task SendAsync(string message)
		{
			Debug.Assert(!string.IsNullOrEmpty(message));

			byte[] encodedMessage = _encoding.GetBytes(message);
			byte[] encodedMessageLength = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(encodedMessage.Length));

			byte[] networkMessage = new byte[encodedMessageLength.Length + encodedMessage.Length];
			encodedMessageLength.CopyTo(networkMessage, 0);
			encodedMessage.CopyTo(networkMessage, encodedMessageLength.Length);

			await _client?.GetStream()?.WriteAsync(networkMessage, 0, networkMessage.Length, _cancellation.Token);
		}

		public async Task SendAsync(byte[] message, int length)
		{
			int networkLength = IPAddress.HostToNetworkOrder(length);

			message[0] = (byte)  networkLength;
			message[1] = (byte) (networkLength >> 8);
			message[2] = (byte) (networkLength >> 16);
			message[3] = (byte) (networkLength >> 24);
			Debug.Log(">>> Sending(" + length + "): |" + Encoding.Default.GetString(message, sizeof(int), length) + "|");

			await _client?.GetStream()?.WriteAsync(message, 0, length + sizeof(int), _cancellation.Token);
			await _client?.GetStream()?.FlushAsync(_cancellation.Token);
		}

		public void WaitForExplosion()
		{
			if (_connectAndReceiveTask == null) return;

			Debug.Log($"+++> waiting for _connectAndReceiveTask. Status: {_connectAndReceiveTask.Status}");
			Debug.Log($"+++> waiting for _connectAndReceiveTask2. Status: {_connectAndReceiveTask2.Status}");
			Debug.Log("===> TODO: handle sending properly");
			Debug.Log($"+++> waiting for lastSendTask. Status: {_lastSendTask?.Status}");

			Task[] tasks = {_connectAndReceiveTask, _lastSendTask ?? Task.CompletedTask};
			try
			{
				if (!Task.WaitAll(tasks, TimeSpan.FromSeconds(1)))
				{
					Debug.Log("===> TODO: Tasks didn't finish in time...");

					Debug.Log($"+++> waiting for _connectAndReceiveTask. Status: {_connectAndReceiveTask.Status}");
					Debug.Log($"+++> waiting for _connectAndReceiveTask2. Status: {_connectAndReceiveTask2.Status}");
					Debug.Log("===> TODO: handle sending properly");
					Debug.Log($"+++> waiting for lastSendTask. Status: {_lastSendTask?.Status}");
					Debugger.Break();
				}
				else
				{
					_connectAndReceiveTask.Dispose();
					_connectAndReceiveTask = null;

					_lastSendTask?.Dispose();
					_lastSendTask = null;
				}
			}
			catch (Exception ex)
			{
				Debug.Log("BOOM: " + ex.Message);
			}

			if (_connectAndReceiveTask != null)
				Debug.Log("leaking _connectAndReceiveTask");
			_connectAndReceiveTask = null;

			if (_lastSendTask != null)
				Debug.Log("leaking lastSendTask");
			_lastSendTask = null;
		}

		private static void WaitAndDispose(ref Task task)
		{
			if (task == null) return;
			Debug.Log($"+++> waiting for task. Status: {task.Status}");

			try
			{
				task.GetAwaiter().GetResult();
			}
			catch (TaskCanceledException ex)
			{
				Debug.Log(ex.Message);
			}
			finally
			{
				try
				{
					task.Dispose();
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
				finally
				{
					task = null;
				}
			}
		}

		public void Close()
		{
			_cancellation?.Cancel();
			WaitAndDispose(ref _connectAndReceiveTask);
			WaitAndDispose(ref _lastSendTask);
		}

		public void Close_old()
		{
			_cancellation?.Cancel();
			if (_connectAndReceiveTask == null) return;

			Debug.Log($"+++> waiting for _connectAndReceiveTask. Status: {_connectAndReceiveTask.Status}");
			Debug.Log($"+++> waiting for _connectAndReceiveTask2. Status: {_connectAndReceiveTask2.Status}");
			Debug.Log($"+++> waiting for lastSendTask. Status: {_lastSendTask?.Status}");

			try
			{
				Task[] tasks = {_connectAndReceiveTask, _lastSendTask ?? Task.CompletedTask};
				if (Task.WaitAll(tasks, TimeSpan.FromTicks(3)))
				{
					Debug.Log("TODO: Tasks didn't finish in time...");
					Debugger.Break();
				}
			}

			catch (AggregateException ex)
			{
				Debug.Log(ex.Message);
				Debug.Log("AggregateException: " + ex.InnerException?.Message);
			}
			catch (Exception ex)
			{
				Debug.Log(ex.Message);
			}
			finally
			{
				Debug.Log($"+++> finished waiting for _connectAndReceiveTask. Status: {_connectAndReceiveTask.Status}");
				Debug.Log($"+++> finished waiting for _connectAndReceiveTask2. Status: {_connectAndReceiveTask2.Status}");
				Debug.Log($"+++> finished waiting for lastSendTask. Status: {_lastSendTask?.Status}");

				_connectAndReceiveTask.Dispose();
				_connectAndReceiveTask = null;

				_lastSendTask?.Dispose();
				_lastSendTask = null;
			}
		}

#if WITH_CANCELLATION
		public void Dispose()
		{
			//if (_client == null)
			//{
			// there is no connection, StartAsync is blocked by AcceptTcpClientAsync
			Connected = null;
			IceCandidateReceived = null;
			SdpReceived = null;
			VersionReceived = null;

			Close();

			//Cancelled = null;
			//}
			//else
			//{
				//_stream.Dispose();
			_client?.Dispose();
			_tcpListener?.Stop();
			_tcpListener = null;

			//}
			Disconnected = null;

			_tcpListener?.Stop();
			_tcpListener = null;

			cancellation.Dispose();
		}
#else
		public void Dispose()
		{

				try
				{
					if (_client?.Connected == true)
					{
						Debug.Log("DebugSignaling is getting disposed while stream is still active; flushing stream.");
						_client?.GetStream()?.FlushAsync().Wait();

						Debug.Log("Signaling.Dispose: ThreadId: " + Thread.CurrentThread.ManagedThreadId);
						Debug.Log("Signaling.Dispose: waiting for stream operations to finish (deadlock potential)");

						Debug.Log("Signaling.Dispose: finished waiting for send operations to finish");
					}
				}
				catch (Exception e)
				{
					Debug.LogError(e.Message);
					if (Debugger.IsAttached) Debugger.Break();
					throw;
				}
				_client?.Dispose();
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


			Disconnected = null;
		}
#endif


		}
}

#pragma warning restore IDE0063 // Use simple 'using' statement
