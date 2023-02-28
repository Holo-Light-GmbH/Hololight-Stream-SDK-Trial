using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoloLight.Isar.Native
{
	enum HlrAnchorTrackingState
	{
		NONE = 0,
		LIMITED,
		TRACKING
	}

	public enum HlrAnchorOpStatus
	{
		NOT_CONNECTED = 0,
		SUCCESS,
		REQUEST_SEND_FAILURE,
		BAD_RESPONSE,
		COULD_NOT_ADD_ANCHOR,
		ANCHOR_ALREADY_EXIST,
		PENDING_REQUEST,
		ANCHOR_NOT_FOUND,
		EXISTING_STORE_CONNECTION,
		NO_STORE_CONNECTION,
		BAD_TIMEOUT_VALUE,
		ZERO_RESULT,
		TIMEOUT,
		FAILURE,
	}

	struct HlrAnchorId
	{
		UInt64 sub_id_first;
		UInt64 sub_id_second;

	}
	struct HlrAnchor
	{
		HlrAnchorId id;
		HlrPose pose;
		HlrAnchorTrackingState tracking_state;
	}

	public struct HlrAnchorStatusChecker
	{
		public static string CheckAnchoringError(HlrAnchorOpStatus status)
		{
			switch (status)
			{
				case HlrAnchorOpStatus.NOT_CONNECTED:
					return "ISAR client is not connected.";
				case HlrAnchorOpStatus.REQUEST_SEND_FAILURE:
					return "Cannot send request to client (Possible network failure).";
				case HlrAnchorOpStatus.BAD_RESPONSE:
					return "Client responded with unexpected response.";
				case HlrAnchorOpStatus.COULD_NOT_ADD_ANCHOR:
					return "Anchor could not be added. Possible cause is,there could be pending request for that anchor on current update ( deletion etc. ).";
				case HlrAnchorOpStatus.ANCHOR_ALREADY_EXIST:
					return "Anchor already exists in system.";
				case HlrAnchorOpStatus.PENDING_REQUEST:
					return "There is already a pending request for the requested operation.";
				case HlrAnchorOpStatus.ANCHOR_NOT_FOUND:
					return "Anchor could not be found on the system.";
				case HlrAnchorOpStatus.EXISTING_STORE_CONNECTION:
					return "Cannot create store connection, there is already an established store conenction.";
				case HlrAnchorOpStatus.NO_STORE_CONNECTION:
					return "There is not an existing store connection.";
				case HlrAnchorOpStatus.BAD_TIMEOUT_VALUE:
					return "The timeout value that is provided to operation is invalid.";
				case HlrAnchorOpStatus.ZERO_RESULT:
					return "Operation returned with zero number of anchors";
				case HlrAnchorOpStatus.TIMEOUT:
					return "Operation timed out.";
				case HlrAnchorOpStatus.FAILURE:
					return "Operation failed unexpectedly.";
				default:
					return "Unexpected error code";
			}
		}
	}


}
