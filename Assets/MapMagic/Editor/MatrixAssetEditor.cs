using UnityEngine;
using UnityEditor;
using System.Collections;

namespace MapMagic 
{
	[CustomEditor(typeof(MatrixAsset))]
	public class MatrixAssetEditor : Editor
	{
		Layout layout;
		Texture2D texture;

	void RecordUndo () 
	{ 
	//	if (mouseWasPressed) return;
		Undo.RecordObject ((MatrixAsset)target, "Map Asset Change"); 
	}

	public override void OnInspectorGUI ()
	{
		//DrawDefaultInspector();
		
		MatrixAsset script = (MatrixAsset)target;
		if (layout == null) layout = new Layout();

		layout.field = Layout.GetInspectorRect();
		layout.cursor = new Rect(layout.field.x, layout.field.y, 0,0);

		layout.OnBeforeChange -= RecordUndo;
		layout.OnBeforeChange += RecordUndo;

		if (script.matrix == null) { layout.Label("Matrix is not defined"); return; }
		
		layout.Label("Size: " + script.matrix.rect.size.x + "," + script.matrix.rect.size.z);
		layout.Label("Offset: " + script.matrix.rect.offset.x + "," + script.matrix.rect.offset.z);

		texture = script.preview.SimpleToTexture(texture);
		layout.Par(texture.height, padding:0);
		layout.Icon(texture, layout.Inset(texture.width, padding:0));
		
		Layout.SetInspectorRect(layout.field);
	}
	}
}