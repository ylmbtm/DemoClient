using UnityEngine;
using System.Collections;
using DG.Tweening;
using System;

[DefaultExecutionOrder(1000)]
public class GTCameraFollow : MonoBehaviour
{
    private float           m_DefaultDistance = 12;     // 默认视距
    private float           m_MinDistance     = 8;
    private float           m_MaxDistance     = 20;
    private float           m_MinVertAngle    = -10f;
    private float           m_MaxVertAngle    = 70f;
    private float           m_CurrentXAngle   = 0;      // 摄像机当前绕X轴旋转的角度
    private float           m_ZoomRate        = 0.01f;
    private float           m_XSensitivity    = 0.10f;
    private float           m_YSensitivity    = 0.10f;
    private float[]         m_CullDistances   = new float[32];
    private LayerMask       m_LayMasks        = new LayerMask();
    private float           m_ClipMoveTime    = 0.05f;  // time taken to move when avoiding cliping (low value = fast, which it should be)
    private float           m_BackMoveTime    = 0.4f;   // time taken to move back towards desired position, when not clipping (typically should be a higher value than clipMoveTime)
    private float           m_SphereCastRadius = 0.1f;  // the radius of the sphere used to test for object between camera and target

 
    private float           m_MoveVelocity;             // the velocity at which the camera moved
    private Ray             m_Ray = new Ray();          // the ray used in the lateupdate for casting between the camera and the target
    private RaycastHit[]    m_Hits;                     // the hits between the camera and the target
    private RayHitComparer  m_RayHitComparer;           // variable to compare raycast hit distances


    private float           m_CurDistance;
    private float           m_OriginalDistance;         // the original distance to the camera before any modification are made
    private Quaternion      m_OriginalRotation;
    private float           m_X;
    private float           m_Y;
    private float           m_PinchDelta;
    private Camera          m_Cam;
    private Transform       m_Hero;
    private Transform       m_Pivot;                    // the point at which the camera pivots around
    private RaycastHit      m_HitInfo;

    private float           m_TotalDuration;
    private float           m_TotalStrength;
    private float           m_ShakeStrength;
    private float           m_ShakeStartTime;
    private bool            m_IsShake;
    private ECameraLookType m_CameraLookType;
    private ECameraState    m_CameraState;

    public enum ECameraState
    {
        TYPE_NORMAL_MODE = 0,
        TYPE_ROTATE_AUTO = 1,
        TYPE_SWITCH_LOOK = 2,
    }

    public enum ECameraLookType
    {
        TYPE_LOCK_LOOK  = 0,
        TYPE_FREE_LOOK  = 1,
    }

    void Start()
    {
        m_Cam = GetComponentInChildren<Camera>();
        m_Cam.layerCullSpherical = true;
        m_Cam.layerCullDistances = m_CullDistances;
        m_Pivot = m_Cam.transform.parent;
        m_OriginalDistance = m_Cam.transform.localPosition.magnitude;
        m_RayHitComparer = new RayHitComparer();
    }

    void Update()
    {
        if (m_Hero == null)
        {
            return;
        }
        if (GTInput.Instance.Drag)
        {
            if (GTData.NativeData != null)
            {
                switch((ECameraLookType)GTData.NativeData.LookType)
                {
                    case ECameraLookType.TYPE_LOCK_LOOK:
                        m_X =  0;
                        m_Y =  GTInput.Instance.DragDelta.x * m_YSensitivity;
                        break;
                    case ECameraLookType.TYPE_FREE_LOOK:
                        m_X = -GTInput.Instance.DragDelta.y * m_XSensitivity;
                        m_Y =  GTInput.Instance.DragDelta.x * m_YSensitivity;
                        break;
                }
            }
            else
            {
                m_X = -GTInput.Instance.DragDelta.y * m_XSensitivity;
                m_Y =  GTInput.Instance.DragDelta.x * m_YSensitivity;
            }
        }
        else
        {
            m_X = 0;
            m_Y = 0;
        }
        if (GTInput.Instance.Pinch)
        {
            m_PinchDelta = GTInput.Instance.PinchDelta;
        }
        else
        {
            m_PinchDelta = 0;
        }
        m_CurDistance += m_PinchDelta * -m_ZoomRate;
        m_CurDistance = Mathf.Clamp(m_CurDistance, m_MinDistance, m_MaxDistance);

        m_OriginalDistance += m_PinchDelta * -m_ZoomRate;
        m_OriginalDistance = Mathf.Clamp(m_OriginalDistance, m_MinDistance, m_MaxDistance);
    }

    void LateUpdate()
    {
        if (m_Hero == null)
        {
            return;
        }
        transform.rotation = m_OriginalRotation;
        if (m_X != 0 || m_Y != 0)
        {
            transform.Rotate(0, m_Y, 0, Space.World);
            m_CurrentXAngle += m_X;
            m_CurrentXAngle = Mathf.Clamp(m_CurrentXAngle, m_MinVertAngle, m_MaxVertAngle);
            transform.rotation = Quaternion.Euler(m_CurrentXAngle, transform.eulerAngles.y, transform.eulerAngles.z);
            m_OriginalRotation = transform.rotation;
        }
        if (m_IsShake)
        {
            Vector3 euler = UnityEngine.Random.insideUnitSphere * this.m_ShakeStrength;
            euler.z = 0f;
            float pastTime = Time.time - m_ShakeStartTime;
            float num = this.m_TotalDuration - (Time.time - m_ShakeStartTime) / this.m_TotalDuration;
            this.m_ShakeStrength = this.m_TotalStrength * num;
            this.transform.localRotation = Quaternion.Lerp(transform.localRotation, transform.localRotation * Quaternion.Euler(euler), Time.deltaTime);
            if (pastTime >= m_TotalDuration)
            {
                m_IsShake = false;
            }
        }
        else
        {
            this.transform.rotation = m_OriginalRotation;
        }


        float targetDist = m_OriginalDistance;
        m_Ray.origin = m_Pivot.position + m_Pivot.forward * m_SphereCastRadius;
        m_Ray.direction = -m_Pivot.forward;

        var cols = Physics.OverlapSphere(m_Ray.origin, m_SphereCastRadius);
        bool initialIntersect = false;
        for (int i = 0; i < cols.Length; i++)
        {
            if ((!cols[i].isTrigger))
            {
                initialIntersect = true;
                break;
            }
        }

        if (initialIntersect)
        {
            m_Ray.origin += m_Pivot.forward * m_SphereCastRadius;
            m_Hits = Physics.RaycastAll(m_Ray, m_OriginalDistance - m_SphereCastRadius);
        }
        else
        {
            m_Hits = Physics.SphereCastAll(m_Ray, m_SphereCastRadius, m_OriginalDistance + m_SphereCastRadius);
        }

        Array.Sort(m_Hits, m_RayHitComparer);
        float nearest = Mathf.Infinity;
        for (int i = 0; i < m_Hits.Length; i++)
        {
            if (m_Hits[i].distance < nearest && (!m_Hits[i].collider.isTrigger))
            {
                nearest = m_Hits[i].distance;
                targetDist = -m_Pivot.InverseTransformPoint(m_Hits[i].point).z;
            }
        }

        Vector3 dir = m_Cam.transform.localPosition.normalized;
        if (Physics.SphereCast(transform.position, 0.5f, transform.TransformDirection(dir), out m_HitInfo, m_CurDistance, m_LayMasks, QueryTriggerInteraction.Ignore))
        {
            m_CurDistance = m_HitInfo.distance;
        }

        m_CurDistance = Mathf.SmoothDamp(m_CurDistance, targetDist, ref m_MoveVelocity, m_CurDistance > targetDist ? m_ClipMoveTime : m_BackMoveTime);
        m_CurDistance = Mathf.Clamp(m_CurDistance, m_MinDistance, m_OriginalDistance);
        m_Cam.transform.localPosition = -Vector3.forward * m_CurDistance;
    }

    public void PlayShake(Int32 strength, float duration)
    {
        m_IsShake              = true;
        m_TotalStrength      = strength;
        m_TotalDuration      = duration;
        m_ShakeStrength      = strength;
        m_ShakeStartTime     = Time.time;
    }

    public void PlayRotateAuto()
    {

    }

    public void PlaySwitchLook(ECameraLookType type)
    {
        this.m_CameraLookType = type;
    }

    public void StopShake()
    {
        m_IsShake = false;
    }

    public void SetTarget(Transform target, float height = 0)
    {
        if (target == null)
        {
            return;
        }
        transform.parent = target.transform;
        m_Hero = target.transform;
        Vector3 euler = transform.eulerAngles;
        euler.z = 0;
        m_OriginalRotation      = Quaternion.Euler(euler);
        m_CurrentXAngle         = transform.eulerAngles.x;
        m_CurDistance           = m_DefaultDistance;
        transform.localPosition = new Vector3(0, height, 0);
    }

    public Quaternion OriginalRotation
    {
        get { return m_OriginalRotation; }
    }

    public Vector3    OriginalDirection
    {
        get { return m_OriginalRotation * Vector3.up; }
    }

    public class      RayHitComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            return ((RaycastHit)x).distance.CompareTo(((RaycastHit)y).distance);
        }
    }
}
