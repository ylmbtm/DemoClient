using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class EEffectRenderQueue : MonoBehaviour
{
    [SerializeField]
    public int                  queueOffset = 1;
    [SerializeField]
    public UIPanel              panel;
    [HideInInspector]
    public List<ParticleSystem> particles = new List<ParticleSystem>();

    void OnEnable()
    {
        Calc();
    }

    [ContextMenu("计算层级")]
    void Calc()
    {
        panel = NGUITools.FindInParents<UIPanel>(gameObject);
        GetComponents<ParticleSystem>(particles);
        GetComponentsInChildren<ParticleSystem>(particles);
        if (panel == null)
        {
            return;
        }
        int thisQueue = panel.startingRenderQueue + queueOffset;
        for (int i = 0; i < particles.Count; i++)
        {
            Renderer render = particles[i].GetComponent<Renderer>();
            if (render != null && render.material != null)
            {
                render.material.renderQueue = thisQueue;
            }
        }
    }
}
