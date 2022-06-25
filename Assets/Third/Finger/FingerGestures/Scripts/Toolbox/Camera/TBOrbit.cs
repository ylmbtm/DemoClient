using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Adaptation of the standard MouseOrbit script to use the finger drag gesture to rotate the current object using
/// the fingers/mouse around a target object
/// </summary>
[AddComponentMenu( "FingerGestures/Toolbox/Camera/Orbit" )]
public class TBOrbit : MonoBehaviour
{
    public enum PanMode
    {
        Disabled,
        OneFinger,
        TwoFingers
    }

    /// <summary>
    /// The object to orbit around
    /// </summary>
    public Transform target;

    /// <summary>
    /// Initial camera distance to target
    /// </summary>
    public float initialDistance = 10.0f;

    /// <summary>
    /// Minimum distance between camera and target
    /// </summary>
    public float minDistance = 1.0f;

    /// <summary>
    /// Maximum distance between camera and target
    /// </summary>
    public float maxDistance = 20.0f;

    /// <summary>
    /// Affects horizontal rotation speed
    /// </summary>
    public float yawSensitivity = 80.0f;

    /// <summary>
    /// Affects vertical rotation speed
    /// </summary>
    public float pitchSensitivity = 80.0f;

    /// <summary>
    /// Keep yaw angle value between minPitch and maxPitch?
    /// </summary>
    public bool clampYawAngle = false;
    public float minYaw = -75;
    public float maxYaw = 75;

    /// <summary>
    /// Keep pitch angle value between minPitch and maxPitch?
    /// </summary>
    public bool clampPitchAngle = true;
    public float minPitch = -20;
    public float maxPitch = 80;

    /// <summary>
    /// Allow the user to affect the orbit distance using the pinch zoom gesture
    /// </summary>
    public bool allowPinchZoom = true;

    /// <summary>
    /// Affects pinch zoom speed
    /// </summary>
    public float pinchZoomSensitivity = 2.0f;

    /// <summary>
    /// Use smooth camera motions?
    /// </summary>
    public bool smoothMotion = true;
    public float smoothZoomSpeed = 3.0f;
    public float smoothOrbitSpeed = 4.0f;

    /// <summary>
    /// Two-Finger camera panning.
    /// Panning will apply an offset to the pivot/camera target point
    /// </summary>
    public bool allowPanning = false;
    public bool invertPanningDirections = false;
    public float panningSensitivity = 1.0f;
    public Transform panningPlane;  // reference transform used to apply the panning translation (using panningPlane.right and panningPlane.up vectors)
    public bool smoothPanning = true;
    public float smoothPanningSpeed = 8.0f;

    float distance = 10.0f;
    float yaw = 0;
    float pitch = 0;

    float idealDistance = 0;
    float idealYaw = 0;
    float idealPitch = 0;

    Vector3 idealPanOffset = Vector3.zero;
    Vector3 panOffset = Vector3.zero;

    public float Distance
    {
        get { return distance; }
    }

    public float IdealDistance
    {
        get { return idealDistance; }
        set { idealDistance = Mathf.Clamp( value, minDistance, maxDistance ); }
    }

    public float Yaw
    {
        get { return yaw; }
    }

    public float IdealYaw
    {
        get { return idealYaw; }
        set { idealYaw = clampYawAngle ? ClampAngle( value, minYaw, maxYaw ) : value; }
    }

    public float Pitch
    {
        get { return pitch; }
    }

    public float IdealPitch
    {
        get { return idealPitch; }
        set { idealPitch = clampPitchAngle ? ClampAngle( value, minPitch, maxPitch ) : value; }
    }

    public Vector3 IdealPanOffset
    {
        get { return idealPanOffset; }
        set { idealPanOffset = value; }
    }

    public Vector3 PanOffset
    {
        get { return panOffset; }
    }

    void InstallGestureRecognizers()
    {
        List<GestureRecognizer> recogniers = new List<GestureRecognizer>( GetComponents<GestureRecognizer>() );
        DragRecognizer drag = recogniers.Find( r => r.EventMessageName == "OnDrag" ) as DragRecognizer;
        DragRecognizer twoFingerDrag = recogniers.Find( r => r.EventMessageName == "OnTwoFingerDrag" ) as DragRecognizer;
        PinchRecognizer pinch = recogniers.Find( r => r.EventMessageName == "OnPinch" ) as PinchRecognizer;

        if( !drag )
        {
            drag = gameObject.AddComponent<DragRecognizer>();
            drag.RequiredFingerCount = 1;
            drag.IsExclusive = true;
            drag.MaxSimultaneousGestures = 1;
            drag.SendMessageToSelection = GestureRecognizer.SelectionType.None;
        }

        if( !pinch )
            pinch = gameObject.AddComponent<PinchRecognizer>();
        
        if( !twoFingerDrag )
        {
            twoFingerDrag = gameObject.AddComponent<DragRecognizer>();
            twoFingerDrag.RequiredFingerCount = 2;
            twoFingerDrag.IsExclusive = true;
            twoFingerDrag.MaxSimultaneousGestures = 1;
            twoFingerDrag.ApplySameDirectionConstraint = false;
            twoFingerDrag.EventMessageName = "OnTwoFingerDrag";
        }
    }

    void Start()
    {
        InstallGestureRecognizers();
        
        if( !panningPlane )
            panningPlane = this.transform;

        Vector3 angles = transform.eulerAngles;

        distance = IdealDistance = initialDistance;
        yaw = IdealYaw = angles.y;
        pitch = IdealPitch = angles.x;

        // Make the rigid body not change rotation
        if( GetComponent<Rigidbody>() )
            GetComponent<Rigidbody>().freezeRotation = true;

        Apply();
    }

    #region Gesture Event Messages

    float nextDragTime;

    void OnDrag( DragGesture gesture )
    {
        // dont apply drag rotation if more than one touch is on the screen
        //if( FingerGestures.Touches.Count > 1 )
        //    return;
        
        // wait for drag cooldown timer to wear off
        //  used to avoid dragging right after a pinch or pan, when lifting off one finger but the other one is still on screen
        if( Time.time < nextDragTime )
            return;

        if( target )
        {
            IdealYaw += gesture.DeltaMove.x * yawSensitivity * 0.02f;
            IdealPitch -= gesture.DeltaMove.y * pitchSensitivity * 0.02f;
        }
    }

    void OnPinch( PinchGesture gesture )
    {
        if( allowPinchZoom )
        {
            IdealDistance -= gesture.Delta * pinchZoomSensitivity;
            nextDragTime = Time.time + 0.25f;
        }
    }

    void OnTwoFingerDrag( DragGesture gesture )
    {
        //Debug.Log( "OnTwoFingerDrag " + e.Phase + " @ Frame " + Time.frameCount );

        if( allowPanning )
        {
            Vector3 move = -0.02f * panningSensitivity * 
				( panningPlane.right * gesture.DeltaMove.x + panningPlane.up * gesture.DeltaMove.y );
            
            if( invertPanningDirections )
                IdealPanOffset -= move; 
            else
                IdealPanOffset += move;

            nextDragTime = Time.time + 0.25f;
        }
    }

    #endregion

    void Apply()
    {
        if( smoothMotion )
        {
            distance = Mathf.Lerp( distance, IdealDistance, Time.deltaTime * smoothZoomSpeed );
            yaw = Mathf.Lerp( yaw, IdealYaw, Time.deltaTime * smoothOrbitSpeed );
            pitch = Mathf.LerpAngle( pitch, IdealPitch, Time.deltaTime * smoothOrbitSpeed );
        }
        else
        {
            distance = IdealDistance;
            yaw = IdealYaw;
            pitch = IdealPitch;
        }

        if( smoothPanning )
            panOffset = Vector3.Lerp( panOffset, idealPanOffset, Time.deltaTime * smoothPanningSpeed );
        else
            panOffset = idealPanOffset;

        transform.rotation = Quaternion.Euler( pitch, yaw, 0 );
        transform.position = ( target.position + panOffset ) - distance * transform.forward;
    }

    void LateUpdate()
    {
        Apply();
    }

    static float ClampAngle( float angle, float min, float max )
    {
        if( angle < -360 )
            angle += 360;

        if( angle > 360 )
            angle -= 360;

        return Mathf.Clamp( angle, min, max );
    }

    // recenter the camera
    public void ResetPanning()
    {
        IdealPanOffset = Vector3.zero;
    }
}