using UnityEngine;
using System;
using System.Collections;
using Protocol;

public class MessageCallbackData
{
    public MessageID       ID;
	public Action<MessageRecvData> Handler;
}
