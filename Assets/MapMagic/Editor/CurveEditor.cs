using UnityEngine;
using UnityEditor;

namespace MapMagic
{
	public class CurveEditor : EditorWindow
	{
		static CurveEditor instance;
		//private Curve curve;
		private Layout layout;
		
		static public void ShowWindow (Curve curve)
		{
			instance = (CurveEditor)GetWindow (typeof (CurveEditor));

			//instance.curve = curve;
			instance.position = new Rect(100, 100, instance.position.width, instance.position.height);
			instance.titleContent = new GUIContent("Curve");
			instance.Show();
		}  

		static public void CloseWindow () { if (instance!=null) instance.Close(); }

		void OnEnable()
		{   
	//		EditorApplication.update -= Update;
	//		EditorApplication.update += Update;
		}
 
		void OnDisable()
		{
	//		EditorApplication.update -= Update;
		}

		void OnGUI()
		{
			if (layout==null) layout = new Layout();
			layout.maxZoom = 8; layout.minZoom = 0.125f; layout.zoomStep = 0.125f;
			layout.Zoom(); layout.Scroll(); //scrolling and zooming

			//Handles.DrawBezier(pos2, pos1, new Vector2(pos2.x + distance/3, pos2.y), new Vector2(pos1.x-distance/3, pos1.y), color, null, 3f*zoom+2f);
		}
	}
}