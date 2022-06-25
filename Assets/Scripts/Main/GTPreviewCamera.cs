using UnityEngine;
using System.Collections;

public class GTPreviewCamera : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(GTData.IsLaunched == false);
    }
}
