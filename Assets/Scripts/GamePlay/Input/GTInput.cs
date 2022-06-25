using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(LongPressRecognizer))]
[RequireComponent(typeof(PinchRecognizer))]
[RequireComponent(typeof(DragRecognizer))]
[RequireComponent(typeof(FingerDownDetector))]
[RequireComponent(typeof(FingerUpDetector))]
public class GTInput : GTMonoSingleton<GTInput>
{
    private PinchRecognizer     m_PinchRecognizer;
    private DragRecognizer      m_DragRecognizer;
    private LongPressRecognizer m_LongPressRecognizer;
    private FingerDownDetector  m_FingerDownDetector;
    private FingerUpDetector    m_FingerUpDetector;
    private DragGesture         m_Drag = null;

    public bool      Pinch
    {
        get; private set;
    }

    public float     PinchDelta
    {
        get; private set;
    }

    public bool      Drag
    {
        get; private set;
    }

    public Vector2   DragDelta
    {
        get; private set;
    }

//     public EFlyInput FlyInput
//     {
//         get; set;
//     }

    public bool      MoveJoystick
    {
        get; set;
    }

    public override void SetRoot(Transform parent)
    {
        base.SetRoot(parent);
        this.m_DragRecognizer      = gameObject.GET<DragRecognizer>();
        this.m_PinchRecognizer     = gameObject.GET<PinchRecognizer>();
        this.m_FingerDownDetector  = gameObject.GET<FingerDownDetector>();
        this.m_FingerUpDetector    = gameObject.GET<FingerUpDetector>();
        this.m_LongPressRecognizer = gameObject.GET<LongPressRecognizer>();
        this.m_DragRecognizer.     OnGesture    += OnDrag;
        this.m_PinchRecognizer.    OnGesture    += OnPinch;
        this.m_FingerDownDetector. OnFingerDown += OnFingerDown;
        this.m_FingerUpDetector.   OnFingerUp   += OnFingerUp;
        this.m_LongPressRecognizer.OnGesture    += OnLongPress;
    }

    private void OnFingerUp(FingerUpEvent eventData)
    {
        if (GTData.IsLaunched == false)
        {
            return;
        }
        if (GTCameraManager.Instance.MainCamera == null)
        {
            return;
        }
        if (Drag || Pinch)
        {
            return;
        }
        if (UICamera.Raycast(eventData.Position))
        {
            return;
        }
        if (UICamera.Raycast(eventData.Finger.Position))
        {
            return;
        }
        if (UICamera.Raycast(eventData.Finger.StartPosition))
        {
            return;
        }
        RaycastHit hit;
        Ray ray = GTCameraManager.Instance.MainCamera.ScreenPointToRay(eventData.Position);
        if(Physics.Raycast(ray, out hit, 1000))
        {          
            switch(hit.collider.gameObject.layer)
            {
                case GTLayer.LAYER_DEFAULT:
                    if (Application.isEditor)
                    {
                        GTWorld.Main.Get<ActorCommand>().GetCmd<CommandMove>().Update(hit.point, () =>
                        {
                            GTWorld.Main.Get<ActorCommand>().GetCmd<CommandIdle>().Do();
                        }).
                        Do();
                    }
                    break;
                case GTLayer.LAYER_PLAYER:
                case GTLayer.LAYER_PET:
                case GTLayer.LAYER_ACTOR:
                case GTLayer.LAYER_MONSTER:
                    {
                        Actor cc     = hit.transform.GetComponent<Actor>();
                        GTWorld.Instance.SetTarget(GTWorld.Main, cc);
                    }
                    break;
            }
        }

    }

    private void OnFingerDown(FingerDownEvent eventData)
    {
       
    }

    private void OnPinch(PinchGesture gesture)
    {
        for (int i = 0; i < gesture.Fingers.Count; i++)
        {
            if (UICamera.Raycast(gesture.Fingers[i].StartPosition))
            {
                Pinch = false;
                PinchDelta = 0;
                return;
            }
        }
        switch (gesture.Phase)
        {
            case ContinuousGesturePhase.Started:
                {
                    Pinch = true;
                    PinchDelta = gesture.Delta;
                }
                break;
            case ContinuousGesturePhase.Updated:
                if (Pinch)
                {
                    PinchDelta = gesture.Delta;
                }
                break;
            case ContinuousGesturePhase.None:
            case ContinuousGesturePhase.Ended:
                {
                    Pinch = false;
                    PinchDelta = 0;
                }
                break;
        }
    }

    private void OnDrag(DragGesture gesture)
    {
        if (Pinch)
        {
            DragDelta = Vector2.zero;
            return;
        }
        switch (gesture.Phase)
        {
            case ContinuousGesturePhase.Started:
                if (UICamera.Raycast(gesture.StartPosition) == false)
                {
                    m_Drag = gesture;
                    Drag = true;
                    DragDelta = gesture.DeltaMove;
                }
                break;
            case ContinuousGesturePhase.Updated:
                if (Drag && gesture.ClusterId == m_Drag.ClusterId)
                {
                    DragDelta = gesture.DeltaMove;
                }
                break;
            case ContinuousGesturePhase.Ended:
            case ContinuousGesturePhase.None:
                if (m_Drag != null && m_Drag.ClusterId == gesture.ClusterId)
                {
                    m_Drag = null;
                    Drag = false;
                    DragDelta = Vector2.zero;
                }
                break;
        }
    }

    private void OnLongPress(LongPressGesture gesture)
    {

    }
}
