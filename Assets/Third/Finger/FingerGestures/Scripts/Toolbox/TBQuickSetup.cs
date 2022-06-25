using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu( "FingerGestures/Toolbox/Quick Setup" )]
public class TBQuickSetup : MonoBehaviour
{
    public GameObject MessageTarget = null;     // default to this game object
    public int MaxSimultaneousGestures = 2;
    ScreenRaycaster screenRaycaster;

    // Finger Event Detectors
    [HideInInspector] 
    public FingerDownDetector FingerDown;
    [HideInInspector] 
    public FingerUpDetector FingerUp;
    [HideInInspector] 
    public FingerHoverDetector FingerHover;
    [HideInInspector] 
    public FingerMotionDetector FingerMotion;

    // Gesture Recognizers
    [HideInInspector]
    public DragRecognizer Drag;
    [HideInInspector]
    public LongPressRecognizer LongPress;
    [HideInInspector]
    public SwipeRecognizer Swipe;
    [HideInInspector] 
    public TapRecognizer Tap;
    [HideInInspector] 
    public PinchRecognizer Pinch;
    [HideInInspector] 
    public TwistRecognizer Twist;
    [HideInInspector] 
    public TapRecognizer DoubleTap;
    [HideInInspector] 
    public DragRecognizer TwoFingerDrag;
    [HideInInspector] 
    public TapRecognizer TwoFingerTap;
    [HideInInspector] 
    public SwipeRecognizer TwoFingerSwipe;
    [HideInInspector] 
    public LongPressRecognizer TwoFingerLongPress;

    GameObject CreateChildNode( string name )
    {
        GameObject go = new GameObject( name );
        Transform tf = go.transform;
        tf.parent = this.transform;
        tf.localPosition = Vector3.zero;
        tf.localRotation = Quaternion.identity;
        return go;
    }

    void Start()
    {
        if( !MessageTarget )
            MessageTarget = this.gameObject;

        screenRaycaster = GetComponent<ScreenRaycaster>();
        if( !screenRaycaster )
            screenRaycaster = gameObject.AddComponent<ScreenRaycaster>();

        // Create the FG instance if not already available
        if( !FingerGestures.Instance )
            gameObject.AddComponent<FingerGestures>();

        GameObject fingerEventsNode = CreateChildNode( "Finger Event Detectors" );
        {
            FingerDown = AddFingerEventDetector<FingerDownDetector>( fingerEventsNode );
            FingerUp = AddFingerEventDetector<FingerUpDetector>( fingerEventsNode );
            FingerMotion = AddFingerEventDetector<FingerMotionDetector>( fingerEventsNode );
            FingerHover = AddFingerEventDetector<FingerHoverDetector>( fingerEventsNode );
        }

        GameObject singleFingerGestureNode = CreateChildNode( "Single Finger Gestures" );
        {
            Drag = AddSingleFingerGesture<DragRecognizer>( singleFingerGestureNode );
            Tap = AddSingleFingerGesture<TapRecognizer>( singleFingerGestureNode );
            Swipe = AddSingleFingerGesture<SwipeRecognizer>( singleFingerGestureNode );
            LongPress = AddSingleFingerGesture<LongPressRecognizer>( singleFingerGestureNode );

            DoubleTap = AddSingleFingerGesture<TapRecognizer>( singleFingerGestureNode );
            DoubleTap.RequiredTaps = 2;
            DoubleTap.EventMessageName = "OnDoubleTap";
        }

        GameObject twoFingerGestures = CreateChildNode( "Two-Finger Gestures" );
        {
            Pinch = AddTwoFingerGesture<PinchRecognizer>( twoFingerGestures );
            Twist = AddTwoFingerGesture<TwistRecognizer>( twoFingerGestures );
            TwoFingerDrag = AddTwoFingerGesture<DragRecognizer>( twoFingerGestures, "OnTwoFingerDrag" );
            TwoFingerTap = AddTwoFingerGesture<TapRecognizer>( twoFingerGestures, "OnTwoFingerTap" );
            TwoFingerSwipe = AddTwoFingerGesture<SwipeRecognizer>( twoFingerGestures, "OnTwoFingerSwipe" );
            TwoFingerLongPress = AddTwoFingerGesture<LongPressRecognizer>( twoFingerGestures, "OnTwoFingerLongPress" );
        }

        // we're done, remove component (but not game object)
        // Destroy( this );
    }

    T AddFingerEventDetector<T>( GameObject node ) where T : FingerEventDetector
    {
        T detector = node.AddComponent<T>();
        detector.Raycaster = screenRaycaster;
        detector.MessageTarget = MessageTarget;
        return detector;
    }

    T AddGesture<T>( GameObject node ) where T : GestureRecognizer
    {
        T gesture = node.AddComponent<T>();
        gesture.Raycaster = screenRaycaster;
        gesture.EventMessageTarget = MessageTarget;

        if( gesture.SupportFingerClustering )
            gesture.MaxSimultaneousGestures = MaxSimultaneousGestures;

        return gesture;
    }

    T AddSingleFingerGesture<T>( GameObject node ) where T : GestureRecognizer
    {
        T gesture = AddGesture<T>( node );
        gesture.RequiredFingerCount = 1;
        return gesture;
    }

    T AddTwoFingerGesture<T>( GameObject node ) where T : GestureRecognizer
    {
        T gesture = AddGesture<T>( node );
        gesture.RequiredFingerCount = 2;
        return gesture;
    }

    T AddTwoFingerGesture<T>( GameObject node, string eventName ) where T : GestureRecognizer
    {
        T gesture = AddTwoFingerGesture<T>( node );
        gesture.EventMessageName = eventName;
        return gesture;
    }
}
