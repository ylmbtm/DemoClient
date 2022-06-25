using UnityEngine;
using System.Collections;

public class GTPoolObjectDestroy : MonoBehaviour
{
    private float         m_DestroyTime;
    private float         m_DestroyStartTime;
    private System.Action m_Finish;

    public void Init(float destroyTime, System.Action onFinish)
    {
        this.m_DestroyTime      = destroyTime;
        this.m_Finish           = onFinish;
        this.m_DestroyStartTime = Time.realtimeSinceStartup;
        this.enabled = true;
    }

    void Update()
    {
        if (Time.realtimeSinceStartup - m_DestroyStartTime > m_DestroyTime)
        {
            if (m_Finish != null)
            {
                m_Finish();
                m_Finish = null;
            }
            GameObject.Destroy(gameObject);
        }
    }
}
