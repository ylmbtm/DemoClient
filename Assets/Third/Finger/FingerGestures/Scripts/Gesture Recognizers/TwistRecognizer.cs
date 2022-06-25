using UnityEngine;
using System.Collections;

public class TwistGesture : ContinuousGesture
{
    float deltaRotation = 0;
    float totalRotation = 0;

    /// <summary>
    /// Rotation angle change since last move (in degrees)
    /// </summary>
    public float DeltaRotation
    {
        get { return deltaRotation; }
        internal set { deltaRotation = value; }
    }

    /// <summary>
    /// Total rotation angle since gesture started (in degrees)
    /// </summary>
    public float TotalRotation
    {
        get { return totalRotation; }
        internal set { totalRotation = value; }
    }
}

/// <summary>
/// Twist Gesture Recognizer (formerly known as rotation gesture)
///   Two fingers moving around a pivot point in circular/opposite directions
/// </summary>
[AddComponentMenu( "FingerGestures/Gestures/Twist Recognizer" )]
public class TwistRecognizer : ContinuousGestureRecognizer<TwistGesture>
{
    /// <summary>
    /// Rotation DOT product treshold - this controls how tolerant the twist gesture detector is to the two fingers
    /// moving in opposite directions.
    /// Setting this to -1 means the fingers have to move in exactly opposite directions to each other.
    /// this value should be kept between -1 and 0 excluded.
    /// </summary>
    public float MinDOT = -0.7f;

    /// <summary>
    /// Minimum amount of rotation required to start the rotation gesture (in degrees)
    /// </summary>
    public float MinRotation = 1.0f;
    
    public override string GetDefaultEventMessageName()
    {
        return "OnTwist";
    }

    // Only support 2 simultaneous fingers right now
    public override int RequiredFingerCount
    {
        get { return 2; }
        set { Debug.LogWarning( "Not Supported" ); }
    }

    // TEMP: multi-gesture tracking is not supported for the Twist gesture yet
    public override bool SupportFingerClustering
    {
        get { return false; }
    }

    public override GestureResetMode GetDefaultResetMode()
    {
        return GestureResetMode.NextFrame;
    }

    protected override GameObject GetDefaultSelectionForSendMessage( TwistGesture gesture )
    {
        return gesture.StartSelection;
    }

    protected override bool CanBegin( TwistGesture gesture, FingerGestures.IFingerList touches )
    {
        if( !base.CanBegin( gesture, touches ) )
            return false;

        FingerGestures.Finger finger0 = touches[0];
        FingerGestures.Finger finger1 = touches[1];

        if( !FingerGestures.AllFingersMoving( finger0, finger1 ) )
            return false;

        if( !FingersMovedInOppositeDirections( finger0, finger1 ) )
            return false;

        // check if we went past the minimum rotation amount treshold
        float rotation = SignedAngularGap( finger0, finger1, finger0.StartPosition, finger1.StartPosition );
        if( Mathf.Abs( rotation ) < MinRotation )
            return false;

        return true;
    }

    protected override void OnBegin( TwistGesture gesture, FingerGestures.IFingerList touches )
    {
        FingerGestures.Finger finger0 = touches[0];
        FingerGestures.Finger finger1 = touches[1];

        gesture.StartPosition = 0.5f * ( finger0.Position + finger1.Position ); //( finger0.StartPosition + finger1.StartPosition );
        gesture.Position = gesture.StartPosition; //0.5f * ( finger0.Position + finger1.Position );

        //float angle = SignedAngularGap( finger0, finger1, finger0.StartPosition, finger1.StartPosition );
        gesture.TotalRotation = 0; //angle; //Mathf.Sign( angle ) * MinRotation;
        gesture.DeltaRotation = 0; //angle;
    }

    protected override GestureRecognitionState OnRecognize( TwistGesture gesture, FingerGestures.IFingerList touches )
    {
        if( touches.Count != RequiredFingerCount )
        {
            gesture.DeltaRotation = 0;

            // fingers were lifted?
            if( touches.Count < RequiredFingerCount )
                return GestureRecognitionState.Ended;

            // more fingers added, gesture failed
            return GestureRecognitionState.Failed;
        }

        FingerGestures.Finger finger0 = touches[0];
        FingerGestures.Finger finger1 = touches[1];

        gesture.Position = 0.5f * ( finger0.Position + finger1.Position );

        // dont do anything if both fingers arent moving
        if( !FingerGestures.AllFingersMoving( finger0, finger1 ) )
            return GestureRecognitionState.InProgress;

        gesture.DeltaRotation = SignedAngularGap( finger0, finger1, finger0.PreviousPosition, finger1.PreviousPosition );

        // only raise event when the twist angle has changed
        if( Mathf.Abs( gesture.DeltaRotation ) > Mathf.Epsilon )
        {
            gesture.TotalRotation += gesture.DeltaRotation;
            RaiseEvent( gesture );
        }

        return GestureRecognitionState.InProgress;
    }

    #region Utils

    bool FingersMovedInOppositeDirections( FingerGestures.Finger finger0, FingerGestures.Finger finger1 )
    {
        return FingerGestures.FingersMovedInOppositeDirections( finger0, finger1, MinDOT );
    }

    // return signed angle in degrees between current finger position and ref positions
    static float SignedAngularGap( FingerGestures.Finger finger0, FingerGestures.Finger finger1, Vector2 refPos0, Vector2 refPos1 )
    {
        Vector2 curDir = ( finger0.Position - finger1.Position ).normalized;
        Vector2 refDir = ( refPos0 - refPos1 ).normalized;

        // check if we went past the minimum rotation amount treshold
        return Mathf.Rad2Deg * FingerGestures.SignedAngle( refDir, curDir );
    }

    #endregion
}
