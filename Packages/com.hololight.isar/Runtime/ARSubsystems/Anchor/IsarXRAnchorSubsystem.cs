using UnityEngine;

using UnityEngine.XR.ARSubsystems;
using HoloLight.Isar.Native;
using Unity.Collections;
using System.Runtime.InteropServices;
using System;
using System.Threading.Tasks;

namespace HoloLight.Isar.ARSubsystems
{
	public sealed class IsarXRAnchorSubsystem : XRAnchorSubsystem
	{
		private IsarProvider _provider;

#if !UNITY_2020_2_OR_NEWER
		protected override Provider CreateProvider()
		{
			_provider = new IsarProvider();
			return _provider;
		}
#else
		protected override void OnCreate()
		{
			base.OnCreate();
			_provider = (IsarProvider)provider;
		}
#endif

		static class NativeApi
		{
			[DllImport("remoting_unity")]
			public static extern bool CreateAnchorProvider(ref IntPtr nativeProvider);

			[DllImport("remoting_unity")]
			public static extern void DestroyAnchorProvider(ref IntPtr nativeProvider);

			[DllImport("remoting_unity")]
			public static unsafe extern int TryAddAnchor(IntPtr nativeProvider,
														 ref Pose pose, 
														 out XRAnchor anchor);

			[DllImport("remoting_unity")]
			public static extern int TryRemoveAnchor(IntPtr nativeProvider,
											         ref TrackableId anchorId);

			[DllImport("remoting_unity")]
			public static unsafe extern void AcquireChanges(IntPtr nativeProvider,
															out void* addedPtr,
															out int addedCount,
															out void* updatedPtr,
															out int updatedCount,
															out void* deletedPtr,
															out int deletedCount,
															out int elementSize);

			[DllImport("remoting_unity")]
			public static unsafe extern void ReleaseChanges(IntPtr nativeProvider,
															void* addedPtr,
															void* updatedPtr,
															void* deletedPtr);

			[DllImport("remoting_unity")]
			public static unsafe extern int ExportAnchors(IntPtr nativeProvider,
														  out IntPtr anchorData,
														  out ulong size,
														  uint timeoutMs);

			[DllImport("remoting_unity")]
			public static unsafe extern void ReleaseAnchorExportData(IntPtr nativeProvider);

			[DllImport("remoting_unity")]
			public static unsafe extern int ImportAnchors(IntPtr nativeProvider,
														  IntPtr anchorData,
														  ulong size,
														  uint timeoutMs);

			[DllImport("remoting_unity")]
			public static unsafe extern int EnumeratePersistedSpatialAnchorNames(IntPtr nativeProvider,
																				 out ulong size,
																				 out IntPtr namesPtr,
																				 uint timeoutMs);

			[DllImport("remoting_unity")]
			public static unsafe extern int CreateAnchorStoreConnection(IntPtr nativeProvider, 
																	    uint timeoutMs);

			[DllImport("remoting_unity")]
			public static unsafe extern int DestroyAnchorStoreConnection(IntPtr nativeProvider, 
																		 uint timeoutMs);

			[DllImport("remoting_unity")]
			public static unsafe extern int ClearAnchorStore(IntPtr nativeProvider, uint timeoutMs);

			[DllImport("remoting_unity")]
			public static extern int PersistSpatialAnchor(IntPtr nativeProvider, 
														  ref IntPtr namePtr, 
														  ref TrackableId anchorId, 
														  uint timeoutMs);

			[DllImport("remoting_unity")]
			public static extern int UnpersistSpatialAnchor(IntPtr nativeProvider, 
															ref IntPtr namePtr, 
															uint timeoutMs);

			[DllImport("remoting_unity")]
			public static extern int CreateAnchorFromPersistedName(IntPtr nativeProvider, 
																   ref IntPtr namePtr, 
																   out TrackableId anchorId, 
																   uint timeoutMs);
		}

		public class BinaryAnchorData
		{
			public bool isSuccess { get; set; }
			public byte[] data { get; set; }
		}

		public class EnumeratePersistedNamesData
		{
			public bool isSuccess { get; set; }
			public string[] data { get; set; }
		}

		public class AnchorFromPersistenceData
		{
			public bool isSuccess { get; set; }
			public TrackableId id{ get; set; }
		}

		public Task<BinaryAnchorData> ExportAnchors(uint timeoutMS)
		{
			return _provider.ExportAnchors(timeoutMS);
		}

		public Task<bool> ImportAnchors(byte [] anchorData, uint timeoutMs)
		{
			return _provider.ImportAnchors(anchorData, timeoutMs);
		}

		public Task<EnumeratePersistedNamesData> EnumeratePersistedAnchorNames(uint timeoutMs)
		{
			return _provider.EnumeratePersistedAnchorNames(timeoutMs);
		}

		public Task<bool> CreateStoreConnection(uint timeoutMs)
		{
			return _provider.CreateStoreConnection(timeoutMs);
		}

		public Task<bool> DestroyStoreConnection(uint timeoutMs)
		{
			return _provider.DestroyStoreConnection(timeoutMs);
		}

		public Task<bool> ClearStore(uint timeoutMs)
		{
			return _provider.ClearStore(timeoutMs);
		}
		public Task<bool> PersistAnchor(string name, TrackableId anchorId, uint timeoutMs)
		{
			return _provider.PersistAnchor(name, anchorId, timeoutMs);
		}

		public Task<bool> UnpersistAnchor(string name, uint timeoutMs)
		{
			return _provider.UnpersistAnchor(name, timeoutMs);
		}

		public Task<AnchorFromPersistenceData> CreateAnchorFromPersistedName(string name, uint timeoutMS)
		{
			return _provider.CreateAnchorFromPersistedName(name, timeoutMS);
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void RegisterDescriptor()
		{
			XRAnchorSubsystemDescriptor.Create(new XRAnchorSubsystemDescriptor.Cinfo
			{
				id = "ISAR Anchor",
#if UNITY_2020_2_OR_NEWER
				providerType = typeof(IsarXRAnchorSubsystem.IsarProvider),
				subsystemTypeOverride = typeof(IsarXRAnchorSubsystem),
#else
				subsystemImplementationType = typeof(IsarXRAnchorSubsystem),
#endif
				supportsTrackableAttachments = false
			});
		}

		class IsarProvider : Provider
		{
			private IntPtr _nativeProvider;
			private bool _isNativeAnchorSystemRunning;

			public IsarProvider()
			{
				_isNativeAnchorSystemRunning = NativeApi.CreateAnchorProvider(ref _nativeProvider);
			}

			public override void Start()
			{
				_isNativeAnchorSystemRunning = true;
			}

			public override void Stop()
			{ 
				_isNativeAnchorSystemRunning = false;
			}

			public override void Destroy()
			{
				NativeApi.DestroyAnchorProvider(ref _nativeProvider);
			}

			public override bool TryAddAnchor(Pose pose, out XRAnchor anchor)
			{
				anchor = new XRAnchor();

				if (!_isNativeAnchorSystemRunning)
				{
					Debug.LogWarning("Anchor addition failed: ISAR native anchor system is not running");
					return false;
				}

				HlrAnchorOpStatus status = (HlrAnchorOpStatus)NativeApi.TryAddAnchor(_nativeProvider, ref pose, out anchor);

				if (status != HlrAnchorOpStatus.SUCCESS)
				{
					Debug.LogWarning("Anchor addition returned with status: " + HlrAnchorStatusChecker.CheckAnchoringError(status));
					return false;
				}
				else
					return true;
			}

			public override bool TryRemoveAnchor(TrackableId anchorId)
			{
				if (!_isNativeAnchorSystemRunning)
				{
					Debug.LogWarning("Anchor removal failed: ISAR native anchor system is not running");
					return false;
				}

				HlrAnchorOpStatus status = (HlrAnchorOpStatus)NativeApi.TryRemoveAnchor(_nativeProvider, ref anchorId);
				if (status != HlrAnchorOpStatus.SUCCESS)
				{
					Debug.LogWarning("Anchor removal returned with status: " + HlrAnchorStatusChecker.CheckAnchoringError(status));
					return false;
				}
				else
					return true;

			}

			public override unsafe TrackableChanges<XRAnchor> GetChanges(XRAnchor defaultAnchor, Allocator allocator)
			{

				if (!_isNativeAnchorSystemRunning)
					return new TrackableChanges<XRAnchor>();

				int addedCount, updatedCount, removedCount, elementSize;
				void* addedPtr, updatedPtr, removedPtr;

				NativeApi.AcquireChanges(_nativeProvider,
										out addedPtr, out addedCount,
										out updatedPtr, out updatedCount,
										out removedPtr, out removedCount,
										out elementSize);

				try
				{
					var added = NativeCopyUtility.PtrToNativeArrayWithDefault<XRAnchor>(defaultAnchor, addedPtr, elementSize, addedCount, allocator);
					var updated = NativeCopyUtility.PtrToNativeArrayWithDefault<XRAnchor>(defaultAnchor, updatedPtr, elementSize, updatedCount, allocator);
					var removed = NativeCopyUtility.PtrToNativeArrayWithDefault<TrackableId>(default(TrackableId), removedPtr, elementSize, removedCount, allocator);

					var ret = TrackableChanges<XRAnchor>.CopyFrom(
						added,
						updated,
						removed,
						allocator);

					added.Dispose();
					updated.Dispose();
					removed.Dispose();

					return ret;
				}
				finally
				{
					NativeApi.ReleaseChanges(_nativeProvider, addedPtr, updatedPtr, removedPtr);
				}
			}


			public async Task<BinaryAnchorData> ExportAnchors(uint timeoutMs)
			{
				BinaryAnchorData returnVal = new BinaryAnchorData();

				if (!_isNativeAnchorSystemRunning)
				{
					Debug.LogWarning("Exporting anchor data failed: ISAR native anchor system is not running");
					return returnVal;
				}

				await Task.Run(() =>
				{
					IntPtr binaryData;
					ulong size;
					HlrAnchorOpStatus status = (HlrAnchorOpStatus)NativeApi.ExportAnchors(_nativeProvider, out binaryData, out size, timeoutMs);

					if (status == HlrAnchorOpStatus.SUCCESS)
					{
						returnVal.data = new byte[size];
						Marshal.Copy(binaryData, returnVal.data, 0, (int)size);
						returnVal.isSuccess = true;
						NativeApi.ReleaseAnchorExportData(_nativeProvider);
					}
					else
					{
						Debug.LogWarning("Exporting anchor data is returned with status: " + HlrAnchorStatusChecker.CheckAnchoringError(status));
					}
				});

				return returnVal;
			}
			public async Task<bool> ImportAnchors(byte[] anchorData, uint timeoutMs)
			{
				bool returnVal = false;

				if (!_isNativeAnchorSystemRunning)
				{
					Debug.LogWarning("Importing anchor data failed: ISAR native anchor system is not running");
					return returnVal;
				}

				await Task.Run(() =>
				{
					IntPtr unmanagedPointer = Marshal.AllocHGlobal(anchorData.Length);
					Marshal.Copy(anchorData, 0, unmanagedPointer, anchorData.Length);
					HlrAnchorOpStatus status = (HlrAnchorOpStatus)NativeApi.ImportAnchors(_nativeProvider, unmanagedPointer, (ulong)(anchorData.Length), timeoutMs);

					if (status == HlrAnchorOpStatus.SUCCESS)
					{
						returnVal = true;
					}
					else
					{
						Debug.LogWarning("Importing anchor data is returned with status: " + HlrAnchorStatusChecker.CheckAnchoringError(status));
					}

				});

				return returnVal;
			}

			public async Task<EnumeratePersistedNamesData> EnumeratePersistedAnchorNames(uint timeoutMs)
			{
				EnumeratePersistedNamesData resultData = new EnumeratePersistedNamesData();
				ulong size = 0;
				IntPtr enumeratedNames;
				bool isSucceded = false;

				if (!_isNativeAnchorSystemRunning)
				{
					Debug.LogWarning("Enumerating persisted anchor names failed: ISAR native anchor system is not running");
					return resultData;
				}

				await Task.Run(() =>
				{
					HlrAnchorOpStatus status = (HlrAnchorOpStatus)NativeApi.EnumeratePersistedSpatialAnchorNames(_nativeProvider, out size, out enumeratedNames, timeoutMs);

					if (status == HlrAnchorOpStatus.SUCCESS)
					{
						isSucceded = (size > 0);
						IntPtr[] ptrArray = new IntPtr[size];
						Marshal.Copy(enumeratedNames, ptrArray, 0, (int)size);
						resultData.data = new string[size];
						int arrIndex = 0;

						foreach (var ptr in ptrArray)
						{
							string str = Marshal.PtrToStringAnsi(ptr);
							resultData.data[arrIndex] = str;
							arrIndex++;
						}
					}
					else 
					{
						Debug.LogWarning("Enumerating persisted anchor returned with status: " + HlrAnchorStatusChecker.CheckAnchoringError(status));
					}
				});

				resultData.isSuccess = isSucceded;
				return resultData;
			}

			public async Task<bool> CreateStoreConnection(uint timeoutMs)
			{
				bool returnVal = false;

				if (!_isNativeAnchorSystemRunning)
				{
					Debug.LogWarning("Enumerating persisted anchor names failed: ISAR native anchor system is not running");
					return returnVal;
				}

				await Task.Run(() =>
				{
					HlrAnchorOpStatus status = (HlrAnchorOpStatus)NativeApi.CreateAnchorStoreConnection(_nativeProvider, timeoutMs);

					if (status == HlrAnchorOpStatus.SUCCESS)
					{
						returnVal = true;
					}
					else
					{
						Debug.LogWarning("Creating anchor store connection returned with status: " + HlrAnchorStatusChecker.CheckAnchoringError(status));
					}

				});

				return returnVal;
			}

			public async Task<bool> DestroyStoreConnection(uint timeoutMs)
			{
				bool returnVal = false;

				if (!_isNativeAnchorSystemRunning)
				{
					Debug.LogWarning("Destroying anchor store connection failed: ISAR native anchor system is not running");
					return returnVal;
				}

				await Task.Run(() =>
				{
					HlrAnchorOpStatus status = (HlrAnchorOpStatus)NativeApi.DestroyAnchorStoreConnection(_nativeProvider, timeoutMs);
					if (status == HlrAnchorOpStatus.SUCCESS)
					{
						returnVal = true;
					}
					else
					{
						Debug.LogWarning("Destroying anchor store connection returned with status: " + HlrAnchorStatusChecker.CheckAnchoringError(status));
					}
				});

				return returnVal;
			}

			public async Task<bool> ClearStore(uint timeoutMs)
			{
				bool returnVal = false;

				if (!_isNativeAnchorSystemRunning)
				{
					Debug.LogWarning("Clearing anchor store failed: ISAR native anchor system is not running");
					return returnVal;
				}

				await Task.Run(() =>
				{
					HlrAnchorOpStatus status = (HlrAnchorOpStatus)NativeApi.ClearAnchorStore(_nativeProvider, timeoutMs);
					if (status == HlrAnchorOpStatus.SUCCESS)
					{
						returnVal = true;
					}
					else
					{
						Debug.LogWarning("Clearing anchor store returned with status: " + HlrAnchorStatusChecker.CheckAnchoringError(status));
					}
				});

				return returnVal;
			}

			public async Task<bool> PersistAnchor(string name, TrackableId anchorId, uint timeoutMs)
			{
				bool returnVal = false;

				if (!_isNativeAnchorSystemRunning)
				{
					Debug.LogWarning("Persisting anchor failed: ISAR native anchor system is not running");
					return returnVal;
				}

				await Task.Run(() => 
				{
					IntPtr charPtr = Marshal.StringToCoTaskMemAnsi(name);
					HlrAnchorOpStatus status = (HlrAnchorOpStatus)NativeApi.PersistSpatialAnchor(_nativeProvider, ref charPtr, ref anchorId,  timeoutMs);
					Marshal.FreeCoTaskMem(charPtr);

					if (status == HlrAnchorOpStatus.SUCCESS)
					{
						returnVal = true;
					}
					else
					{
						Debug.LogWarning("Persisting anchor with name:" + name + 
										 "and id: " + anchorId + 
										 "returned with status: " + HlrAnchorStatusChecker.CheckAnchoringError(status));
					}
				});

				return returnVal;

			}

			public async Task<bool> UnpersistAnchor(string name, uint timeoutMs)
			{
				bool returnResult = false;

				if (!_isNativeAnchorSystemRunning)
				{
					Debug.LogWarning("Unpersisting anchor failed: ISAR native anchor system is not running");
					return returnResult;
				}

				await Task.Run(() =>
				{
					IntPtr charPtr = Marshal.StringToCoTaskMemAnsi(name);
					HlrAnchorOpStatus status = (HlrAnchorOpStatus)NativeApi.UnpersistSpatialAnchor(_nativeProvider, ref charPtr, timeoutMs);
					Marshal.FreeCoTaskMem(charPtr);

					if (status == HlrAnchorOpStatus.SUCCESS)
					{
						returnResult = true;
					}
					else
					{
						Debug.LogWarning("Unpersisting anchor with name:" + name +
										 "returned with status: " + HlrAnchorStatusChecker.CheckAnchoringError(status));
					}
				});

				return returnResult;
			}

			public async Task<AnchorFromPersistenceData> CreateAnchorFromPersistedName(string name, uint timeoutMs)
			{
				AnchorFromPersistenceData result = new AnchorFromPersistenceData();
				result.isSuccess = false;
				result.id = new TrackableId();

				if (!_isNativeAnchorSystemRunning)
				{
					Debug.LogWarning("Creating anchor from persisted name failed: ISAR native anchor system is not running");
					return result;
				}
				
				await Task.Run(() =>
				{
					IntPtr charPtr = Marshal.StringToCoTaskMemAnsi(name);
					TrackableId tid = new TrackableId();
					HlrAnchorOpStatus status = (HlrAnchorOpStatus)NativeApi.CreateAnchorFromPersistedName(_nativeProvider, ref charPtr, out tid, timeoutMs);
					Marshal.FreeCoTaskMem(charPtr);

					if (status == HlrAnchorOpStatus.SUCCESS)
					{
						result.isSuccess = true;
						result.id = tid;
					}
					else
					{
						Debug.LogWarning("Creating anchor from persisted name:" + name +
										 "returned with status: " + HlrAnchorStatusChecker.CheckAnchoringError(status));
					}
				});

				return result;
			}
		}
	}

}
