using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Protocol;
using System;

public class MLLogin : GTSingleton<MLLogin>
{
    private List<ClientServerNode> m_ServerList = new List<ClientServerNode>();
	private ClientServerNode                m_CurrServer;

    public List<ClientServerNode> GetServerList()
    {
        return m_ServerList;
    }

    public void                   SetServerList(List<ClientServerNode> value)
    {
        m_ServerList = value;
    }

	public ClientServerNode                GetCurrServer()
    {
        return m_CurrServer;
    }

    public int                    GetCurrServerID()
    {
		return m_CurrServer == null ? 0 : m_CurrServer.SvrID;
    }       

    public void                   SetCurrServer(ClientServerNode server)
    {
		m_CurrServer = server;
    }

    public string                 LastUsername
    {
        get { return GTData.NativeData.Username; }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            GTData.NativeData.Username = value;
        }
    }

    public string                 LastPassword
    {
        get { return GTData.NativeData.Password; }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            GTData.NativeData.Password = value;
        }
    }

    public UInt64                 LastAccountID
    {
        get; set;
    }
}
