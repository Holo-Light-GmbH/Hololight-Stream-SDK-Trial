/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

#pragma warning disable IDE0063 // Use simple 'using' statement // not supported in older C#/Unity versions

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace HoloLight.Isar.Signaling
{
	internal static class Tokens
	{
		public const string SDP = "Sdp";
		public const string SDP_TYPE = "Type";
		public const string SDP_TYPE_OFFER = "Offer";
		public const string SDP_TYPE_ANSWER = "Answer";

		public const string VERSION = "Version";

		public const string ICE_CANDIDATE = "IceCandidate";
		public const string ICE_MID = "sdp-mid";
		public const string ICE_MLINE_INDEX = "mline-index";
	}

	internal class XmlElement
	{
		public string Name;
		public string Content;
		public List<XmlAttribute> Attributes = new List<XmlAttribute>();
	}

	internal class XmlAttribute
	{
		public string Name;
		public string Value;
	}

	//internal class IsarXmlWriter
	//{
	//	private readonly StringBuilder _stringBuilder = new StringBuilder(1000);

	//	public string WriteVersion(uint version)
	//	{
	//		var output = _stringBuilder.Clear();
	//		using (var writer = XmlWriter.Create(output))
	//		{
	//			var type = writer.GetType();

	//			writer.WriteStartDocument();
	//			//writer.WriteStartElement(Tokens.VERSION);
	//			////writer.WriteAttributeString(elem.Name, elem.Value);
	//			////xmlElement.Attributes.ForEach(elem => writer.WriteAttributeString(elem.Name, elem.Value));
	//			//writer.WriteString(version.ToString(CultureInfo.InvariantCulture));
	//			//writer.WriteEndElement();
	//			writer.WriteElementString(Tokens.VERSION, version.ToString(CultureInfo.InvariantCulture));
	//			writer.WriteEndDocument();
	//			//writer.Close();
	//		}

	//		return output.ToString();
	//	}

	//	public string WriteSdp(string sdp)
	//	{
	//		var output = _stringBuilder.Clear();
	//		using (var writer = XmlWriter.Create(output))
	//		{
	//			writer.WriteStartDocument();
	//			{
	//				writer.WriteElementString(Tokens.SDP_TYPE, Tokens.SDP_TYPE_OFFER);
	//			}
	//			writer.WriteEndDocument();
	//			//writer.Close();
	//		}

	//		return output.ToString();
	//	}

	//	public string WriteIceCandidate(string mid, int mlineIndex, string candidate)
	//	{
	//		var output = _stringBuilder.Clear();
	//		using (var writer = XmlWriter.Create(output))
	//		{
	//			writer.WriteStartDocument();
	//			{
	//				writer.WriteStartElement(Tokens.ICE_CANDIDATE);
	//				{
	//					writer.WriteAttributeString(Tokens.ICE_MID, mid);
	//					writer.WriteAttributeString(Tokens.ICE_MLINE_INDEX, mlineIndex.ToString());

	//					writer.WriteString(candidate);
	//				}
	//				writer.WriteEndElement();
	//			}
	//			writer.WriteEndDocument();
	//		}

	//		return output.ToString();
	//	}
	//}

	internal static class IsarXmlReader /*: IDisposable*/
	{
		//private readonly XmlReader _reader;
		//public string ElementName
		//{
		//	get
		//	{
		//		bool readSuccessfully = _reader.Read();
		//		if (!readSuccessfully)
		//		{
		//			const string errorMsg = "Failed to parse XML. Unable to process message";
		//			Debug.WriteLine($"{nameof(DebugSignaling)}:" + errorMsg);
		//			return null;
		//		}

		//		return _reader.Name;
		//	}
		//}

		//public IsarXmlReader(in byte[] data)
		//{
		//	_reader = XmlReader.Create(new MemoryStream(data));
		//}

		//public void Dispose()
		//{
		//	_reader?.Dispose();
		//}

#if false
		public void ReadVersion(out uint version)
		{
			//string bullshit = reader.GetAttribute(Tokens.VERSION_REMOTING_LIB); // TODO: can throw
			string versionStr = _reader.ReadElementContentAsString(); // TODO: can throw
			Debug.Assert(!string.IsNullOrEmpty(versionStr));
			Debug.WriteLine($"Read <{Tokens.VERSION}>{versionStr}</{Tokens.VERSION}>");
			version = uint.Parse(versionStr); // TODO: can throw
		}

		public void ReadSdp(out string type, out string sdp)
		{
			type = _reader.GetAttribute(Tokens.SDP_TYPE); // TODO: can throw
			sdp = _reader.ReadElementContentAsString(); // TODO: can throw
			//Debug.WriteLine($"Received SDP message {{type: {type}, content: {sdp}}}");
			Debug.WriteLine($"Read <{Tokens.SDP} {Tokens.SDP_TYPE}\"{type}\">{sdp}</{Tokens.SDP}>");
		}

		//public void ReadSdp(out Tokens.Sdp type, out string sdp)
		//{
		//	string typeStr = _reader.GetAttribute(Tokens.SDP_TYPE);
		//	type = (Tokens.Sdp)Enum.Parse(typeof(Tokens.Sdp), typeStr); // TODO: can throw
		//	sdp = _reader.ReadElementContentAsString(); // TODO: can throw
		//	//Debug.WriteLine($"Received SDP message {{type: {type}, content: {sdp}}}");
		//	Debug.WriteLine($"Read <{Tokens.SDP} {Tokens.SDP_TYPE}\"{type}\">{sdp}</{Tokens.SDP}>");
		//}

		public void ReadIceCandidate(out string mid, out int mLineIndex, out string candidate)
		{
			mid = _reader.GetAttribute(Tokens.ICE_MID); // TODO: can throw
			string mLineIndexStr = _reader.GetAttribute(Tokens.ICE_MLINE_INDEX); // TODO: can throw
			Debug.Assert(!string.IsNullOrEmpty(mLineIndexStr));
			mLineIndex = int.Parse(mLineIndexStr);
			candidate = _reader.ReadElementContentAsString(); // TODO: can throw
			Debug.WriteLine(
				$"Read <{Tokens.ICE_CANDIDATE} {Tokens.ICE_MID}=\"{mid}\" {Tokens.ICE_MLINE_INDEX}=\"{mLineIndex}\">" +
				$"{candidate}" +
				$"</{Tokens.ICE_CANDIDATE}>");
		}
#endif
		///////////////////// static variants

		public static void ReadVersion(XmlReader reader, out uint version)
		{
			//string bullshit = reader.GetAttribute(Tokens.VERSION_REMOTING_LIB); // TODO: can throw
			string versionStr = reader.ReadElementContentAsString(); // TODO: can throw
			Debug.Assert(!string.IsNullOrEmpty(versionStr));
			Debug.Log($"Read <{Tokens.VERSION}>{versionStr}</{Tokens.VERSION}>");
			version = uint.Parse(versionStr); // TODO: can throw
		}

		public static void ReadSdp(XmlReader reader, out string type, out string sdp)
		{
			type = reader.GetAttribute(Tokens.SDP_TYPE); // TODO: can throw
			sdp = reader.ReadElementContentAsString(); // TODO: can throw
			//Debug.WriteLine($"Received SDP message {{type: {type}, content: {sdp}}}");
			Debug.Log($"Read <{Tokens.SDP} {Tokens.SDP_TYPE}\"{type}\">{sdp}</{Tokens.SDP}>");
		}

		//public static void ReadSdp(XmlReader reader, out Tokens.Sdp type, out string sdp)
		//{
		//	string typeStr = _reader.GetAttribute(Tokens.SDP_TYPE);
		//	type = (Tokens.Sdp)Enum.Parse(typeof(Tokens.Sdp), typeStr); // TODO: can throw
		//	sdp = reader.ReadElementContentAsString(); // TODO: can throw
		//	//Debug.WriteLine($"Received SDP message {{type: {type}, content: {sdp}}}");
		//	Debug.WriteLine($"Read <{Tokens.SDP} {Tokens.SDP_TYPE}\"{type}\">{sdp}</{Tokens.SDP}>");
		//}

		public static void ReadIceCandidate(XmlReader reader, out string mid, out int mLineIndex, out string candidate)
		{
			mid = reader.GetAttribute(Tokens.ICE_MID); // TODO: can throw
			string mLineIndexStr = reader.GetAttribute(Tokens.ICE_MLINE_INDEX); // TODO: can throw
			Debug.Assert(!string.IsNullOrEmpty(mLineIndexStr));
			mLineIndex = int.Parse(mLineIndexStr);
			candidate = reader.ReadElementContentAsString(); // TODO: can throw
			Debug.Log(
				$"Read <{Tokens.ICE_CANDIDATE} {Tokens.ICE_MID}=\"{mid}\" {Tokens.ICE_MLINE_INDEX}=\"{mLineIndex}\">" +
				$"{candidate}" +
				$"</{Tokens.ICE_CANDIDATE}>");
		}
	}

	internal class IsarXmlWriter
	{
		private readonly StringBuilder _stringBuilder = new StringBuilder(1000);

		public string WriteVersionAsString(uint version)
		{
			var element = new XmlElement
			{
				Name = Tokens.VERSION,
				Content = version.ToString(CultureInfo.InvariantCulture)
			};

			string xml = WriteElementAsString(element);
			return xml;
		}

		public int WriteVersionAsBytes(uint version, ref byte[] output, int offset)
		{
			var element = new XmlElement
			{
				Name = Tokens.VERSION,
				Content = version.ToString(CultureInfo.InvariantCulture)
			};

			return WriteElementAsBytes(element, ref output, offset);
		}

		public string WriteSdpAsString(string type, string sdp)
		{
			var element = new XmlElement {Name = Tokens.SDP, Content = sdp};
			element.Attributes.Add(new XmlAttribute { Name = Tokens.SDP_TYPE, Value = type});
			return WriteElementAsString(element);
		}

		//public string WriteSdp(Tokens.Sdp type, string sdp)
		//{
		//	var element = new XmlElement {Name = Tokens.SDP, Content = sdp};
		//	element.Attributes.Add(new XmlAttribute { Name = Tokens.SDP_TYPE, Value = type});
		//	string xml = WriteElement(element);
		//	return xml;
		//}

		public int WriteSdpAsBytes(string type, string sdp, ref byte[] output, int offset = 0)
		{
			var element = new XmlElement {Name = Tokens.SDP, Content = sdp};
			element.Attributes.Add(new XmlAttribute { Name = Tokens.SDP_TYPE, Value = type});
			return WriteElementAsBytes(element, ref output, offset);
		}

		public void WriteSdp(string type, string sdp, Stream stream)
		{
			var element = new XmlElement {Name = Tokens.SDP, Content = sdp};
			element.Attributes.Add(new XmlAttribute { Name = Tokens.SDP_TYPE, Value = type});
			WriteElement(element, stream);
		}

		public string WriteIceCandidateAsString(string mid, int mlineIndex, string candidate)
		{
			var element = new XmlElement
			{
				Name = Tokens.ICE_CANDIDATE,
				Attributes = new List<XmlAttribute>
				{
					new XmlAttribute {Name = Tokens.ICE_MID, Value = mid},
					new XmlAttribute
					{
						Name = Tokens.ICE_MLINE_INDEX,
						Value = mlineIndex.ToString(CultureInfo.InvariantCulture)
					}
				},
				Content = candidate
			};
			return WriteElementAsString(element);
		}

		public int WriteIceCandidateAsBytes(string mid, int mlineIndex, string candidate, ref byte[] output, int offset)
		{
			var element = new XmlElement
			{
				Name = Tokens.ICE_CANDIDATE,
				Attributes = new List<XmlAttribute>
				{
					new XmlAttribute {Name = Tokens.ICE_MID, Value = mid},
					new XmlAttribute
					{
						Name = Tokens.ICE_MLINE_INDEX,
						Value = mlineIndex.ToString(CultureInfo.InvariantCulture)
					}
				},
				Content = candidate
			};
			return WriteElementAsBytes(element, ref output, offset);
		}

		private string WriteElementAsString(XmlElement element)
		{
			StringBuilder output = _stringBuilder.Clear();

			using (var writer = XmlWriter.Create(output))
			{
				var type = writer.GetType();

				writer.WriteStartDocument();
				{
					writer.WriteStartElement(element.Name);
					{
						foreach (var attribute in element.Attributes)
						{
							writer.WriteAttributeString(attribute.Name, attribute.Value);
						}
						writer.WriteString(element.Content);
					}
					writer.WriteEndElement();
				}
				writer.WriteEndDocument();
			}

			return output.ToString();
		}

		private int WriteElementAsBytes(XmlElement element, ref byte[] output, int offset)
		{
			using (var stream = new MemoryStream(output, offset, output.Length - offset))
			{
				WriteElement(element, stream);
				return (int)stream.Position;
			}
		}

		private void WriteElement(XmlElement element, Stream stream)
		{
			using (var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true))
			using (var writer = XmlWriter.Create(streamWriter))
			{
				writer.WriteStartDocument();
				{
					writer.WriteStartElement(element.Name);
					{
						foreach (var attribute in element.Attributes)
						{
							writer.WriteAttributeString(attribute.Name, attribute.Value);
						}
						writer.WriteString(element.Content);
					}
					writer.WriteEndElement();
				}
				writer.WriteEndDocument();
			}
		}
	}
}

#pragma warning restore IDE0063 // Use simple 'using' statement
