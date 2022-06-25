using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( TwistRecognizer ) )]
public class TwistRecognizerInspector : GestureRecognizerInspector<TwistRecognizer>
{
	protected static GUIContent LABEL_MinDOT = new GUIContent( "Minimum DOT", 
		"Rotation DOT product treshold\n" +
		"Controls how tolerant the twist recognizer is to the two fingers moving in opposite directions.\n" +
		"Setting this to -1 means the fingers have to move in exactly opposite directions to each other\n" +
		"This value should be kept between -1 and 0 excluded." );
	
    protected static GUIContent LABEL_MinRotation = new GUIContent( "Minimum Rotation", "Minimum amount of rotation required to start the rotation gesture (in degrees)" );
    
    protected override bool ShowRequiredFingerCount
    {
        get { return false; }
    }

    protected override void OnSettingsUI()
    {
        base.OnSettingsUI();
        Gesture.MinDOT = EditorGUILayout.FloatField( LABEL_MinDOT, Gesture.MinDOT );
        Gesture.MinRotation = EditorGUILayout.FloatField( LABEL_MinRotation, Gesture.MinRotation ); 
    }

    protected override void ValidateValues()
    {
        base.ValidateValues();

        Gesture.MinDOT = Mathf.Clamp( Gesture.MinDOT, -1.0f, 0 );
        Gesture.MinRotation = Mathf.Clamp( Gesture.MinRotation, 0, 360 );
    }
}
