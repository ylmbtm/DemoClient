using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EJoystick : MonoBehaviour
{
    public delegate void JoystickEventHandler(EJoystick joystick);
    public event JoystickEventHandler On_JoystickMove;
    public event JoystickEventHandler On_JoystickMoveEnd;

    private Vector3       m_OriTouchPos     = Vector3.zero;
    private Vector3       m_OriPos          = Vector3.zero;
    private UIWidget      m_Root;
    private UISprite      m_Area;
    private UISprite      m_Touch;
    private bool          m_IsPress          = false;
    private int           m_TouchID          = -1000;

    public int            joystickRadius     = 100;
    public float          joystickHideAlpha  = 0.3f;
    public Vector2        joystickAxis       = Vector2.zero;
    public float          moveRangeRadius    = 200f;
    public EMoveRangeType moveRangeType      = EMoveRangeType.None;
    public Rect           moveRangeRect      = new Rect(0, 0, 400, 400);

    public enum EMoveRangeType
    {
        None,
        Circle,
        Rect
    }

    void Awake()
    {
        m_Root = this.GetComponent<UIWidget>();
        m_Area = transform.Find("Area").GetComponent<UISprite>();
        m_Touch = transform.Find("Touch").GetComponent<UISprite>();
    }

    void Start()
    {
        m_Area.transform.localPosition = Vector3.zero;
        m_Touch.transform.localPosition = Vector3.zero;
        m_OriTouchPos = m_Touch.transform.localPosition;
        m_OriPos = transform.localPosition;
        moveRangeRect.center = transform.localPosition;
        Lighting(joystickHideAlpha);
    }

    void Update()
    {
        if (m_Touch == null || m_Area == null || m_Root == null)
        {
            return;
        }
        if (m_IsPress)
        {
            if (Vector3.Magnitude(m_Touch.transform.localPosition - m_OriTouchPos) > 0.01f)
            {
                Lighting(1f);
                OnJoystickMove();
            }
        }
    }

    void OnPress(bool isPressed)
    {
        if(NGUITools.GetActive(this) == false || !enabled)
        {
            return;
        }
        if (isPressed)
        {
            if(m_IsPress == false)
            {
                m_TouchID = UICamera.currentTouchID;
                m_IsPress = true;
                Lighting(1f);
                CalculateJoystickAxis();
            }
        }
        else
        {
            if(m_TouchID == UICamera.currentTouchID && m_IsPress)
            {
                CalculateJoystickAxis();
                FadeOut(joystickHideAlpha);
                OnJoystickMoveEnd();
            }
        }
    }

    void OnJoystickMove()
    {
        if (On_JoystickMove != null)
        {
            On_JoystickMove(this);
        }
    }

    void OnJoystickMoveEnd()
    {
        if (On_JoystickMoveEnd != null)
        {
            On_JoystickMoveEnd(this);
        }
        m_Touch.transform.localPosition = m_OriTouchPos;
        transform.localPosition = m_OriPos;
        m_IsPress = false;
        m_TouchID = -1000;
        joystickAxis = Vector2.zero;
    }

    void OnDrag(Vector2 delta)
    {
        if (NGUITools.GetActive(this) == false || !enabled)
        {
            return;
        }
        Lighting(1f);
        CalculateJoystickAxis();
    }

    void CalculateJoystickAxis()
    {
        Vector3 offset = NGUIMath.ScreenToParentPixels(UICamera.currentTouch.pos, m_Touch.transform);
        switch (moveRangeType)
        {
            case EMoveRangeType.None:
                if (offset.sqrMagnitude > joystickRadius * joystickRadius)
                {
                    offset = offset.normalized * joystickRadius;
                }
                break;
            case EMoveRangeType.Circle:
                if (offset.sqrMagnitude > joystickRadius * joystickRadius)
                {
                    Vector3 pos = transform.localPosition + offset.normalized * (offset.magnitude - joystickRadius);
                    Vector3 dir = pos - m_OriPos;
                    if (dir.magnitude < moveRangeRadius)
                    {
                        transform.localPosition = pos;
                    }
                    else
                    {
                        dir.Normalize();
                        pos = m_OriPos + dir * moveRangeRadius;
                        transform.localPosition = pos;
                    }
                    offset = offset.normalized * joystickRadius;
                }
                break;
            case EMoveRangeType.Rect:
                if (offset.sqrMagnitude > joystickRadius * joystickRadius)
                {
                    Vector3 pos = transform.localPosition + offset.normalized * (offset.magnitude - joystickRadius);
                    Vector3 dir = pos - m_OriPos;
                    if (pos.x < moveRangeRect.xMin) pos.x = moveRangeRect.xMin;
                    if (pos.y < moveRangeRect.yMin) pos.y = moveRangeRect.yMin;
                    if (pos.x > moveRangeRect.xMax) pos.x = moveRangeRect.xMax;
                    if (pos.y > moveRangeRect.yMax) pos.y = moveRangeRect.yMax;
                    transform.localPosition = pos;
                    offset = offset.normalized * joystickRadius;
                }
                break;
        }

        m_Touch.transform.localPosition = offset;
        joystickAxis.x = offset.x / joystickRadius;
        joystickAxis.y = offset.y / joystickRadius;
    }

    public float Axis2Angle(bool inDegree = true)
    {
        float angle = Mathf.Atan2(joystickAxis.x, joystickAxis.y);
        if (inDegree)
        {
            return angle * Mathf.Rad2Deg;
        }
        else
        {
            return angle;
        }
    }

    public float Axis2Angle(Vector2 axis, bool inDegree = true)
    {
        float angle = Mathf.Atan2(axis.x, axis.y);

        if (inDegree)
        {
            return angle * Mathf.Rad2Deg;
        }
        else
        {
            return angle;
        }
    }

    public void  ForceToEnd(bool callMoveEnd)
    {
        if (On_JoystickMoveEnd != null && callMoveEnd)
        {
            On_JoystickMoveEnd(this);
        }
        m_Touch.transform.localPosition = m_OriTouchPos;
        m_TouchID                       = -1000;
        m_IsPress                       = false;
        transform.localPosition         = m_OriPos;
        joystickAxis                    = Vector2.zero;
    }

    public bool  IsJoystickMove()
    {
        return m_IsPress;
    }

    Vector3 ScreenPos_to_NGUIPos(Vector3 screenPos)
    {
        Vector3 uiPos = UICamera.currentCamera.ScreenToWorldPoint(screenPos);
        uiPos = UICamera.currentCamera.transform.InverseTransformPoint(uiPos);
        return uiPos;
    }

    Vector3 ScreenPos_to_NGUIPos(Vector2 screenPos)
    {
        return ScreenPos_to_NGUIPos(new Vector3(screenPos.x, screenPos.y, 0f));
    }

    void Lighting(float alpha)
    {
        m_Root.alpha = alpha;
    }

    void FadeOut(float toAlpha)
    {
        TweenAlpha.Begin(gameObject, 0.2f, toAlpha);
    }
}