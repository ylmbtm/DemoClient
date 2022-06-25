using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GTTest : MonoBehaviour
{
    public Font font;

    private void Start()
    {
        string str = string.Empty;
        string[] array = Font.GetOSInstalledFontNames();
        for (int i = 0; i < array.Length; i++)
        {
            str = array[i] + " ";
        }
        string path = Application.dataPath + "/chinesetext.txt";
        StreamWriter fs = File.CreateText(path);
        fs.Write(str);       
        fs.Close();
        fs.Dispose();
    }
}
