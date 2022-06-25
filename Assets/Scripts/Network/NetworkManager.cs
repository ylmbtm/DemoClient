using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Net;
using Protocol;
using ProtoBuf;
using System.Linq;

public class NetworkManager : GTSingleton<NetworkManager>
{
	private NetworkClient m_Client = null;
	private static Dictionary<MessageID, MessageCallbackData>      m_MessageDispatchs = new Dictionary<MessageID, MessageCallbackData>();
	private static List<MessageRecvData>                           m_RecvPackets      = new List<MessageRecvData>();

	private NetworkCtrl                                            m_NetworkCtrl      = new NetworkCtrl();
	private static int                                             m_nWritePos        = 0;
	private static byte[]                                          m_PacketHeader     = new byte[28];
	private string                                                 m_LoginIP;
	private int                                                    m_Port;
	private ulong                                                  m_MainGuid;

	public override void Init()
	{
		m_NetworkCtrl.AddHandlers();
	}

	public void SetIPAddress(string ip, int port)
	{
		m_LoginIP = ip;
		m_Port    = port;
	}

	public void SetMainGuid(ulong uGuid)
	{
		m_MainGuid = uGuid;
	}

	public void Execute()
	{
		lock (m_RecvPackets)
		{
			if (m_RecvPackets.Count <= 0)
			{
				return;
			}

			for (int i = 0; i < m_RecvPackets.Count; i++)
			{
				MessageRecvData recv = m_RecvPackets[i];
				DispatchPacket(recv);
			}

			m_RecvPackets.Clear();
		}
	}

	public void ConnectServer(string ip, int port, Callback onConnect)
	{
		m_Client = new NetworkClient(ip, port);
		m_Client.OnConnectSuccess = onConnect;
		m_Client.Connect();
	}

	public void ReConnect()
	{
		if (m_Client != null && m_Client.IsConnectOK())
		{
			m_Client.Close();
			m_Client.Connect();
		}
	}

	public void Send<T>(MessageID messageID, T obj, UInt64 u64TargetID, UInt32 dwUserData)
	{
		if (m_Client == null)
		{
			return;
		}

		if (!m_Client.IsConnectOK())
		{
			Debug.LogError("m_Client IsConnect False");
			return;
		}

		Send(m_Client, messageID, obj, u64TargetID, dwUserData);
	}

	public void Send<T>(MessageID messageID, T obj)
	{
		Send<T>(messageID, obj, m_MainGuid, 0);
	}

	public void Close()
	{
		if (m_Client == null)
		{
			return;
		}

		m_Client.Close();
	}


	public static void Recv(NetworkClient client, byte[] bytes)
	{
		MemoryStream ms = new MemoryStream(bytes);
		MessageRecvData evet = new MessageRecvData();
		evet.Data = bytes.Skip(28).ToArray();
		evet.Client = client;
		evet.MsgID = (MessageID)BitConverter.ToInt32(bytes, 4);
		evet.UesrData = BitConverter.ToUInt32(bytes, 24);
		evet.TargetID = BitConverter.ToUInt64(bytes, 16);
		lock (m_RecvPackets)
		{
			m_RecvPackets.Add(evet);
		}
	}

	public static void AddListener(MessageID id, Action<MessageRecvData> handle)
	{
		MessageCallbackData d = null;
		m_MessageDispatchs.TryGetValue(id, out d);
		if (d == null)
		{
			d = new MessageCallbackData();
			m_MessageDispatchs[id] = d;
		}
		d.Handler += handle;
		d.ID = id;
	}

	public static void RemoveListener(MessageID id, Action<MessageRecvData> handle)
	{
		m_MessageDispatchs.Remove(id);
	}

	static void DispatchPacket(MessageRecvData recv)
	{
		Debug.Log ("Receive Message ID : " + recv.MsgID);
		MessageCallbackData d = null;
		m_MessageDispatchs.TryGetValue(recv.MsgID, out d);
		if (d != null)
		{
			d.Handler(recv);
		}
	}

	static void Send<T>(NetworkClient client, MessageID messageID, T obj, UInt64 u64TargetID, UInt32 dwUserData)
	{
		Debug.Log("Send Message ID : " + messageID);
		byte[] bytes = null;
		Pack<T>(messageID, obj, u64TargetID, dwUserData, ref bytes);

		client.AddPacket(bytes);
	}

	static void Pack<T>(MessageID messageID, T obj, UInt64 u64TargetID, UInt32 dwUserData, ref byte[] bytes)
	{
		MemoryStream byteMs = new MemoryStream();
		MakePacketHeader(messageID, u64TargetID, dwUserData);
		byteMs.Write(m_PacketHeader, 0, 28);
		Serializer.Serialize<T>(byteMs, obj);
		bytes = byteMs.ToArray();
		UInt32 nLen = (UInt32)bytes.Length;
		int nPos = 8;
		for (int i = 0; i < 4; i++)
		{
			bytes[nPos++] = (Byte)(nLen >> i * 8 & 0xff);
		}
	}

	static public void WriteUInt32(UInt32 v)
	{
		for (int i = 0; i < 4; i++)
		{
			m_PacketHeader[m_nWritePos++] = (Byte)(v >> i * 8 & 0xff);
		}
	}

	static public void WriteUInt64(UInt64 v)
	{
		byte[] getdata = BitConverter.GetBytes(v);
		for (int i = 0; i < getdata.Length; i++)
		{
			m_PacketHeader[m_nWritePos++] = getdata[i];
		}
	}

	static public bool MakePacketHeader(MessageID messageID, UInt64 u64Target, UInt32 dwUserData)
	{
		m_nWritePos = 0;
		UInt32 CheckCode = 0x88888888;
		UInt32 dwMsgID = (UInt32)messageID;
		UInt32 dwSize = 3;
		UInt32 dwPacketNo = 0;	//生成序号 = wCommandID^dwSize+index(每个包自动增长索引); 还原序号 = pHeader->dwPacketNo - pHeader->wCommandID^pHeader->dwSize;
		WriteUInt32(CheckCode);
		WriteUInt32(dwMsgID);
		WriteUInt32(dwSize);
		WriteUInt32(dwPacketNo);
		WriteUInt64(u64Target);
		WriteUInt32(dwUserData);
		return true;
	}
}