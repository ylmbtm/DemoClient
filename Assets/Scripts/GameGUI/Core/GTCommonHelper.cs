using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Protocol;

public class GTCommonHelper
{
	public static bool ShowErrorCodeInfo(uint errorCode)
	{
		if (errorCode != 0)
		{
			GTItemHelper.ShowTip(string.Format("错误：{0}", ReadCfgLocalString.GetDataById(errorCode)));
			return false;
		}
	
		return true;
	}

}
