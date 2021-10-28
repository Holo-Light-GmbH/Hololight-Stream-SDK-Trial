using HoloLight.Isar;
using HoloLight.Isar.Native;
using UnityEngine;

namespace HoloLight.Isar
{
	//KL: Isar has connection state event already and subclasses do parts of the API.
	//We could convert this one to be a MonoBehaviour that includes Isar for (maybe) easier usage.
	public abstract class IsarEventListener : MonoBehaviour
	{
		protected Isar _server;
		private bool _isRegistered = false;

		private void RemotingServer_OnConnectionStateChanged(HlrConnectionState state)
		{
			if (state == HlrConnectionState.Connected)
			{
				OnConnected();
			}
			else //if (state == Connection.State.kDisconnected)
			{
				OnDisconnected();
			}
		}

		private void RegisterEvents()
		{
			if (_isRegistered) return;
			if (_server == null) return; // assert?

			_server.ConnectionStateChanged += RemotingServer_OnConnectionStateChanged;
			if (_server.IsConnected)
			{
				OnConnected();
			}

			_isRegistered = true;
		}

		private void UnregisterRemotingEvents()
		{
			if (!_isRegistered) return;
			if (_server == null) return; // assert?

			if (_server.IsConnected)
			{
				OnDisconnected();
			}
			_server.ConnectionStateChanged -= RemotingServer_OnConnectionStateChanged;

			_isRegistered = false;
		}

		protected abstract void OnConnected();
		//{
		//}

		protected abstract void OnDisconnected();
		//{
		//}

		#region MonoBehaviour

		// NOTE: is executed even when script is disabled
		//protected void Awake()
		//{
		//}

		protected void Start()
		{
#if ISAR_LEGACY
			_server = Camera.main.GetComponent<RemoteCamera>().Server;
#endif
			RegisterEvents();
		}

		protected void OnEnable()
		{
			RegisterEvents();
		}

		protected void OnDisable()
		{
			UnregisterRemotingEvents();
		}

		//protected void OnDestroy()
		//{
		//	UnregisterRemotingEvents();
		//}

		#endregion MonoBehaviour
	}
}
