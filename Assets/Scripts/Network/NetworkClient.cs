using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;
using Protocol;
using System.Collections.Generic;
public class NetworkClient
{
	private TcpClient       m_Tcp;
	public string           m_IP;
	public int              m_Port;
	private int             m_PacketHeaderSize = 28;
	private int             m_RecvLen          = 0;
	private int             m_DataLen          = 0;
	private byte[]          m_RecvBuffer       = new byte[8192];
	private byte[]          m_DataBuffer       = new byte[16384];

	private static Queue<byte[]> m_SendPackets = new Queue<byte[]>();

	private static bool m_IsSending = false;

	public Callback OnConnectSuccess = null;

	public NetworkClient(string ip, int port)
	{
		m_IP     = ip;
		m_Port   = port;
	}

	public void Connect()
	{
		m_Tcp = new TcpClient();
		IPAddress address = null;
		try
		{
			address = IPAddress.Parse(m_IP);
		}
		catch (ArgumentNullException e)
		{
			Debug.LogError(" ip address can not be null!!");
			return;
		}
		catch (FormatException e)
		{
			IPAddress[] tAddress = Dns.GetHostAddresses(m_IP);

			if (tAddress.Length <= 0)
			{
				Debug.LogError("invalid domain!");
				return;
			}

			address = tAddress[0];
		}
		catch (Exception e)
		{
			Debug.LogError(e.Message);
			return;
		}

		try
		{
			m_Tcp.BeginConnect(address, m_Port, OnConnect, m_Tcp);
		}
		catch (Exception e)
		{
			Debug.LogError(e.Message);
			return;
		}
		GTAsync.Instance.Run(() =>
		{
			GTEventCenter.FireEvent(GTEventID.TYPE_NETWORK_CONNECT);
		});
	}
	public void Close()
	{
		if(m_Tcp != null)
		{
			try
			{
				m_Tcp.GetStream().Close();
			}
			catch(Exception)
			{
			}
			m_Tcp.Close();
		}

		OnConnectSuccess = null;
		m_RecvLen = 0;
		m_DataLen = 0;
		m_SendPackets.Clear();
	}

	public bool IsConnectOK()
	{
		if (m_Tcp == null || m_Tcp.Client == null)
		{
			return false;
		}

		return m_Tcp.Connected;
	}

	void OnConnect(IAsyncResult ar)
	{
		if (m_Tcp.Client == null)
		{
			OnError(MessageRetCode.MRC_DISCONNECT);
			return;
		}
		if (!m_Tcp.Client.Connected)
		{
			OnError(MessageRetCode.MRC_DISCONNECT);
			return;
		}

		m_Tcp.NoDelay = true;

		m_Tcp.ReceiveBufferSize = 1024 * 512;

		m_Tcp.SendBufferSize = 1024 * 512;

		NetworkStream stream = m_Tcp.GetStream();
		if (!stream.CanRead)
		{
			Debug.LogError("OnConnect Exception can read from network!");
			return;
		}

		stream.BeginRead(m_RecvBuffer, 0, m_RecvBuffer.Length, new AsyncCallback(OnAsyncRead), stream);


		GTAsync.Instance.Run(() =>
		{
			if (OnConnectSuccess != null)
			{
				OnConnectSuccess.Invoke();

				OnConnectSuccess = null;
			}

			GTEventCenter.FireEvent(GTEventID.TYPE_NETWORK_CONNECT_SUCCESS);
		});
	}

	bool MakeRealPacket()
	{
		if (m_DataLen < m_PacketHeaderSize)
		{
			return false;
		}
		Byte CheckCode = m_DataBuffer[0];
		int nPacketSize = BitConverter.ToUInt16(m_DataBuffer, 8);
		if (nPacketSize > m_DataLen)
		{
			//暂时这样处理
			return false;
		}
		byte[] realPacket = new byte[nPacketSize];
		Array.Copy(m_DataBuffer, 0, realPacket, 0, nPacketSize);
		Array.Copy(m_DataBuffer, nPacketSize, m_DataBuffer, 0, m_DataLen - nPacketSize);
		m_DataLen = m_DataLen - nPacketSize;
		NetworkManager.Recv(this, realPacket);
		return true;
	}

	void OnAsyncRead(IAsyncResult ar)
	{
		if(m_Tcp == null)
		{
			return;
		}

		if(!m_Tcp.Connected)
		{
			return;
		}

		NetworkStream stream = null;
		try
		{
			stream = m_Tcp.GetStream();
			m_RecvLen = stream.EndRead(ar);
		}
		catch (System.IO.IOException)
		{
			OnError(MessageRetCode.MRC_DISCONNECT);
			return;
		}
		catch (ObjectDisposedException e)
		{
			OnError(MessageRetCode.MRC_DISCONNECT);
			return;
		}

		if (m_RecvLen <= 0)
		{
			OnError(MessageRetCode.MRC_DISCONNECT);
			return;
		}
		Array.Copy(m_RecvBuffer, 0, m_DataBuffer, m_DataLen, m_RecvLen);
		m_DataLen += m_RecvLen;
		m_RecvLen = 0;
		while(MakeRealPacket());
		stream.BeginRead(m_RecvBuffer, 0, m_RecvBuffer.Length, new AsyncCallback(OnAsyncRead), stream);
	}

	public void AddPacket(byte[] bytes)
	{
		lock (m_SendPackets)
		{
			m_SendPackets.Enqueue(bytes);

			if (!m_IsSending)
			{
				m_IsSending = true;
				byte[] tPacket = m_SendPackets.Dequeue();
				NetworkStream stream = m_Tcp.GetStream();
				stream.BeginWrite(tPacket, 0, tPacket.Length, new AsyncCallback(OnAsyncWrite), null);
			}
		}
	}

	void OnAsyncWrite(IAsyncResult ar)
	{
		NetworkStream stream = m_Tcp.GetStream();
		try
		{
			stream.EndWrite(ar);
		}
		catch (System.IO.IOException)
		{
			OnError(MessageRetCode.MRC_DISCONNECT);
			return;
		}

		lock (m_SendPackets)
		{
			if(m_SendPackets.Count <= 0)
			{
				m_IsSending = false;
				return;
			}
			else
			{
				byte[] tPacket = m_SendPackets.Dequeue();
				stream.BeginWrite(tPacket, 0, tPacket.Length, new AsyncCallback(OnAsyncWrite), null);
			}
		}
	}

	void OnError(MessageRetCode retCode)
	{
		GTAsync.Instance.Run(() =>
		{
			GTEventCenter.FireEvent(GTEventID.TYPE_NETWORK_CONNECT_FAIL, retCode);
		});
		OnClose();
	}

	void OnClose()
	{
		if (m_Tcp != null)
		{
			m_Tcp.Close();
		}
		m_Tcp = null;
	}
}
