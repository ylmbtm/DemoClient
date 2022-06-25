using UnityEditor;
using UnityEngine;
using System.Reflection; // for clipboard stuff

public abstract class GestureRecognizerInspector<T> : Editor where T:GestureRecognizer
{
    protected static GUIContent LABEL_ResetMode = new GUIContent( "Reset Mode", "Controls when the gesture should reset its internal state after a successful or failed recognition" );
    protected static GUIContent LABEL_RequiredFingerCount = new GUIContent( "Required Finger Count", "Exact number of fingers necessary to perform this gesture" );
    protected static GUIContent LABEL_MaxSimultaneousGestures = new GUIContent( "Max Simultaneous Gestures", "Maximum number of simultaneous gestures this recognizer can track at once" );
    protected static GUIContent LABEL_Exclusive = new GUIContent( "Exclusive", "Exclusive gestures will only trigger when the current number of touches on the device matches exactly the required finger count specified above" );

    protected static GUIContent LABEL_EventMessageSectionTitle = new GUIContent( "Event Message Broadcasting", "Configure the broadcasting of gesture event via Unity's SendMessage() API" );
    protected static GUIContent LABEL_EventMessageName = new GUIContent( "Message Name", "Name/identifier of the method to invoke on the target object when this gesture event fires" );
    protected static GUIContent LABEL_EventMessageTarget = new GUIContent( "Message Target", "The target game object to send the gesture event message to.\nDefaults to current object if no value is provided." );
    protected static GUIContent LABEL_SendMessageToSelection = new GUIContent( "Send Message To Selection", "Specify the selection object to also send the event message to" );
    
    protected static GUIContent LABEL_Raycaster = new GUIContent( "Raycaster", "ScreenRaycaster component used to pick collider-equipped scene objects.\nDefaults to first ScreenRaycaster component on current object if no value is provided." );
    protected static GUIContent LABEL_ClusterManager = new GUIContent( "Cluster Manager", "FingerClusterManager component responsible for dynamically grouping fingers together" );
    protected static GUIContent LABEL_Delegate = new GUIContent( "Delegate", "(Advanced) GestureRecognizerDelegate component used to override specific gesture recognizer behaviours.\nDefaults to first GestureRecognizerDelegate component on current object if no value is provided." );

    protected static GUIStyle SectionTitleStyle;

    static bool stylesInitialized = false;

    static void InitStyles()
    {
        SectionTitleStyle = new GUIStyle( GUI.skin.label );
        SectionTitleStyle.fontStyle = FontStyle.Bold;
    }

    T gesture;

    protected abstract bool ShowRequiredFingerCount { get; }
    protected virtual void ValidateValues() 
    {
        Gesture.RequiredFingerCount = Mathf.Clamp( Gesture.RequiredFingerCount, 1, 50 );
    }
    
    public T Gesture
    {
        get { return gesture; }
    }

    public override void OnInspectorGUI()
    {
        if( !stylesInitialized )
        {
            InitStyles();
            stylesInitialized = true;
        }

        gesture = (T)target;
        
        EditorGUIUtility.LookLikeInspector();

        GUILayout.Space( 5 );

        OnSettingsUI();
			
        GUILayout.Space( 10 );

        OnMessagingUI();

        GUILayout.Space( 10 );

        OnComponentsUI();

        GUILayout.Space( 10 );

        OnToolbar();            

        GUILayout.Space( 5 );

        if( GUI.changed )
        {
            ValidateValues();
            EditorUtility.SetDirty( target );
        }
                
    }

    protected void UISectionTitle( string title )
    {
        GUILayout.Label( title, SectionTitleStyle );
    }

    protected void UISectionTitle( GUIContent title )
    {
        GUILayout.Label( title, SectionTitleStyle );
    }
        
    protected virtual void OnSettingsUI() 
    {
        UISectionTitle( "Settings" );

        GUI.enabled = ShowRequiredFingerCount;
        if( ShowRequiredFingerCount )
            Gesture.RequiredFingerCount = EditorGUILayout.IntField( LABEL_RequiredFingerCount, Gesture.RequiredFingerCount );
        else
            EditorGUILayout.IntField( LABEL_RequiredFingerCount, Gesture.RequiredFingerCount );

        Gesture.IsExclusive = EditorGUILayout.Toggle( LABEL_Exclusive, Gesture.IsExclusive );

        GUI.enabled = Gesture.SupportFingerClustering && !Gesture.IsExclusive;
        Gesture.MaxSimultaneousGestures = EditorGUILayout.IntField( LABEL_MaxSimultaneousGestures, Gesture.MaxSimultaneousGestures );
        GUI.enabled = true;

        Gesture.ResetMode = (GestureResetMode)EditorGUILayout.EnumPopup( LABEL_ResetMode, Gesture.ResetMode );

        /*
        GUI.enabled = ShowRequiredFingerCount;
        Gesture.RequiredFingerCount = EditorGUILayout.IntField( LABEL_RequiredFingerCount, Gesture.RequiredFingerCount );
        GUI.enabled = true;*/

    }
		
    protected virtual void OnMessagingUI()
    {
        UISectionTitle( LABEL_EventMessageSectionTitle );
        Gesture.UseSendMessage = EditorGUILayout.Toggle( "Enable Message Broadcast", Gesture.UseSendMessage );
        GUI.enabled = Gesture.UseSendMessage;
        string eventName = string.IsNullOrEmpty( Gesture.EventMessageName ) ? Gesture.GetDefaultEventMessageName() : Gesture.EventMessageName;
        Gesture.EventMessageName = EditorGUILayout.TextField( LABEL_EventMessageName, eventName );
        Gesture.EventMessageTarget = EditorGUILayout.ObjectField( LABEL_EventMessageTarget, Gesture.EventMessageTarget, typeof( GameObject ), true ) as GameObject;
        Gesture.SendMessageToSelection = (GestureRecognizer.SelectionType)EditorGUILayout.EnumPopup( LABEL_SendMessageToSelection, Gesture.SendMessageToSelection );
        GUI.enabled = true;
        //gesture.Mess = EditorGUILayout.TextField( gesture.MessageTarget, "Broadcast Messages" );
    }
    
    protected virtual void OnComponentsUI()
    {
        UISectionTitle( "Components" );
        Gesture.Raycaster = EditorGUILayout.ObjectField( LABEL_Raycaster, Gesture.Raycaster, typeof( ScreenRaycaster ), true ) as ScreenRaycaster;

        GUI.enabled = Gesture.SupportFingerClustering;
        if( Gesture.SupportFingerClustering )
            Gesture.ClusterManager = EditorGUILayout.ObjectField( LABEL_ClusterManager, Gesture.ClusterManager, typeof( FingerClusterManager ), true ) as FingerClusterManager;
        else
            EditorGUILayout.ObjectField( LABEL_ClusterManager, Gesture.ClusterManager, typeof( FingerClusterManager ), true );
        GUI.enabled = true;

        Gesture.Delegate = EditorGUILayout.ObjectField( LABEL_Delegate, Gesture.Delegate, typeof( GestureRecognizerDelegate ), true ) as GestureRecognizerDelegate;
    }

    protected virtual void OnToolbar()
    {
        if( GUILayout.Button( "Copy Event To Clipboard" ) )
        {
            string value = string.Format( "void {0}({1} gesture)", Gesture.EventMessageName, Gesture.GetGestureType().Name );
            value += @" { /* your code here */ }";
            SetClipboard( value );
            Debug.Log( value );
        }
    }
    
    public static void SetClipboard( string value )
    {
        System.Type T = typeof( GUIUtility );

        PropertyInfo systemCopyBufferProperty = T.GetProperty( "systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic );

        if( systemCopyBufferProperty == null )
            throw new System.Exception( "Can't access clipboard object." );

        systemCopyBufferProperty.SetValue( null, value, null );
    }
	
	
}
