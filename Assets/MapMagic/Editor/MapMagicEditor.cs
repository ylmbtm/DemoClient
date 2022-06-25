
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

//using Plugins;

namespace MapMagic 
{
	[CustomEditor(typeof(MapMagic))]
	public class MapMagicEditor : Editor
	{
		MapMagic script; //aka target
		Layout layout;
		
		public int backgroundHeight = 0; //to draw type background
		public int oldSelected = 0; //to repaint gui with new background if new type was selected
		public enum SelectionMode { none, nailing, locking }
		public SelectionMode selectionMode = SelectionMode.none;
		Color nailColor = new Color(0.6f,0.8f,1,1); Color lockColor = new Color(1,0.3f,0.2f,1);
		//public List<Vector3> selectedCoords; use script.selectedcoords
		public enum Pots { _64=64, _128=128, _256=256, _512=512, _1024=1024, _2048=2048, _4096=4096 };

		#region Previewing
		[DrawGizmo(GizmoType.NotInSelectionHierarchy)]
		static void ShowPreview(Transform objectTransform, GizmoType gizmoType)
		{
			if (MapMagic.instance==null || MapMagic.instance.terrains==null) return; 
			foreach (Chunk tw in MapMagic.instance.terrains.Objects()) tw.Preview();
		}
		#endregion

		public void  OnSceneGUI ()
		{	
			if (script == null) script = (MapMagic)target;
			MapMagic.instance = script;
			if (!script.enabled) return;
			script.terrains.CheckEmpty();
			

			#region Drawing Selection

			//drawing frames
			if (Event.current.type == EventType.Repaint)
			foreach(Coord coord in MapMagic.instance.terrains.Coords())
			{
				Handles.color = nailColor*0.8f;
				if (MapMagic.instance.terrains[coord].locked) Handles.color = lockColor*0.8f;
				DrawSelectionFrame(coord, 5f);
			}

			#endregion
		

			#region Selecting terrains
			if (selectionMode==SelectionMode.nailing || selectionMode==SelectionMode.locking)
			{
				//disabling selection
				HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

				//finding aiming ray
				float pixelsPerPoint = 1;
				#if UNITY_5_4_OR_NEWER 
				pixelsPerPoint = EditorGUIUtility.pixelsPerPoint;
				#endif

				SceneView sceneview = UnityEditor.SceneView.lastActiveSceneView;
				if (sceneview==null || sceneview.camera==null) return;
				
				Vector2 mousePos = Event.current.mousePosition;
				mousePos = new Vector2(mousePos.x/sceneview.camera.pixelWidth, 1f/pixelsPerPoint - mousePos.y/sceneview.camera.pixelHeight) * pixelsPerPoint;
				Ray aimRay = sceneview.camera.ViewportPointToRay(mousePos);

				//aiming terrain or empty place
				Vector3 aimPos = Vector3.zero;
				RaycastHit hit;
				if (Physics.Raycast(aimRay, out hit, Mathf.Infinity)) aimPos = hit.point;
				else
				{
					aimRay.direction = aimRay.direction.normalized;
					float aimDist = aimRay.origin.y / (-aimRay.direction.y);
					aimPos = aimRay.origin + aimRay.direction*aimDist;
				}
				aimPos -= MapMagic.instance.transform.position;

				Coord aimCoord = aimPos.FloorToCoord(MapMagic.instance.terrainSize);

				if (selectionMode == SelectionMode.nailing && !Event.current.alt)
				{
					//drawing selection frame
					Handles.color = nailColor;
					DrawSelectionFrame(aimCoord, width:5f);

					//selecting / unselecting
					if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
					{
						if (MapMagic.instance.terrains[aimCoord]==null) //if obj not exists - nail
						{
							Undo.RegisterFullObjectHierarchyUndo(MapMagic.instance, "MapMagic Pin Terrain");
							MapMagic.instance.terrains.Nail(aimCoord); 
							//MapMagic.instance.terrains.maxCount++;
						}
						else 
						{
							Undo.DestroyObjectImmediate(MapMagic.instance.terrains[aimCoord].terrain.gameObject);
							Undo.RecordObject(MapMagic.instance, "MapMagic Unpin Terrain");
							MapMagic.instance.setDirty = !MapMagic.instance.setDirty;

							MapMagic.instance.terrains.Unnail(aimCoord); 
							//MapMagic.instance.terrains.maxCount--;
						}
						//if (MapMagic.instance.terrains.maxCount < MapMagic.instance.terrains.nailedHashes.Count+4) MapMagic.instance.terrains.maxCount = MapMagic.instance.terrains.nailedHashes.Count+4;
						//EditorUtility.SetDirty(MapMagic.instance); //already done via undo
					}
				}

				if (selectionMode == SelectionMode.locking  && !Event.current.alt)
				{
					Chunk aimedTerrain = MapMagic.instance.terrains[aimCoord];
					if (aimedTerrain != null)
					{
						//drawing selection frame
						Handles.color = lockColor;
						DrawSelectionFrame(aimCoord, width:5f);

						//selecting / unselecting
						if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
						{
							Undo.RecordObject(MapMagic.instance, "MapMagic Lock Terrain");
							MapMagic.instance.setDirty = !MapMagic.instance.setDirty;

							aimedTerrain.locked = !aimedTerrain.locked;
							//EditorUtility.SetDirty(MapMagic.instance); //already done via undo
						}
					}
				}
				
				//redrawing scene by moving temp object
				if (script.sceneRedrawObject==null) { script.sceneRedrawObject = new GameObject(); script.sceneRedrawObject.hideFlags = HideFlags.HideInHierarchy; }
				script.sceneRedrawObject.transform.position = aimPos;
			}
			#endregion
		}

		public void DrawSelectionFrame (Coord coord, float width=3f)
		{
			float margins = 3;
			int numSteps = 50;
			float sideSize = MapMagic.instance.terrainSize - margins*2f;
			float stepDist = sideSize / numSteps;
			Vector3 start = coord.ToVector3(MapMagic.instance.terrainSize) + new Vector3(margins,0,margins) + MapMagic.instance.transform.position;

			Chunk terrain = MapMagic.instance.terrains[coord];

			if (terrain==null)
			{
				Handles.DrawAAPolyLine( width, new Vector3[] {
					start, start+new Vector3(sideSize,0,0), start+new Vector3(sideSize,0,sideSize), start+new Vector3(0,0,sideSize), start} );
			}
			else
			{
				//DrawTerrainFrame(terrain.terrain, margins, numSteps:50, width:width);
				DrawLineOnTerrain(terrain.terrain, start, new Vector3(stepDist,0,0), numSteps+1, width);
				DrawLineOnTerrain(terrain.terrain, start+new Vector3(0,0,sideSize), new Vector3(stepDist,0,0), numSteps+1, width);
				DrawLineOnTerrain(terrain.terrain, start, new Vector3(0,0,stepDist), numSteps+1, width);
				DrawLineOnTerrain(terrain.terrain, start+new Vector3(sideSize,0,0), new Vector3(0,0,stepDist), numSteps+1, width);
			}
		}

		public void DrawLineOnTerrain (Terrain terrain, Vector3 start, Vector3 step, int numSteps, float width=3f)
		{
			if (terrain == null || terrain.terrainData == null) return;
			Vector3[] steps = new Vector3[numSteps];
			for (int i=0; i<steps.Length; i++)
			{
				steps[i] = start + step*i;
				steps[i].y = terrain.SampleHeight(steps[i]);
			}
			Handles.DrawAAPolyLine(width, steps);
		}

		public void DrawTerrainFrame (Terrain terrain, float margins=3, int numSteps=50, float width=3f)
		{
			float relativeMargins = margins / terrain.terrainData.size.x;
			float lineLength = terrain.terrainData.size.x - margins*2;
			float step = lineLength / (numSteps-1);

			Vector3[] poses = new Vector3[numSteps];
			float[] heights = new float[numSteps];

			GetTerrainHeightsX(terrain, relativeMargins, ref heights, fromEnd:false);
			for (int i=0; i<poses.Length; i++)
			{
				Vector3 pos = new Vector3(margins + i*step, heights[i], margins);
				pos += terrain.transform.position; //pos = terrain.transform.TransformPoint(pos);
				poses[i] = pos;
			}
			Handles.DrawAAPolyLine(width, poses);
			Vector3 lastPosStart = poses[poses.Length-1];
			Vector3 firstPosStart = poses[0];

			GetTerrainHeightsX(terrain, relativeMargins, ref heights, fromEnd:true);
			for (int i=0; i<poses.Length; i++)
			{
				Vector3 pos = new Vector3(margins + i*step, heights[i], terrain.terrainData.size.x-margins);
				pos += terrain.transform.position; //pos = terrain.transform.TransformPoint(pos);
				poses[i] = pos;
			}
			Handles.DrawAAPolyLine(width, poses);
			Vector3 lastPosEnd = poses[poses.Length-1];
			Vector3 firstPosEnd = poses[0];

			GetTerrainHeightsZ(terrain, relativeMargins, ref heights, fromEnd:false);
			for (int i=0; i<poses.Length; i++)
			{
				Vector3 pos = new Vector3(margins, heights[i], margins + i*step);
				pos += terrain.transform.position; //pos = terrain.transform.TransformPoint(pos);
				poses[i] = pos;
			}
			poses[0] = firstPosStart; poses[poses.Length-1] = firstPosEnd;
			Handles.DrawAAPolyLine(width, poses);

			GetTerrainHeightsZ(terrain, relativeMargins, ref heights, fromEnd:true);
			for (int i=0; i<poses.Length; i++)
			{
				Vector3 pos = new Vector3(terrain.terrainData.size.x-margins, heights[i], margins + i*step);
				pos += terrain.transform.position; //pos = terrain.transform.TransformPoint(pos);
				poses[i] = pos;
			}
			poses[0] = lastPosStart; poses[poses.Length-1] = lastPosEnd;
			Handles.DrawAAPolyLine(width, poses);
		}

		public void GetTerrainHeightsX (Terrain terrain, float relativeMargin, ref float[] heights, bool fromEnd=false)
		{
			int margin = (int)(relativeMargin * terrain.terrainData.heightmapResolution);
			int arrLength = terrain.terrainData.heightmapResolution - margin*2;
			float step = 1f * arrLength / heights.Length;
			
			float[,] array = terrain.terrainData.GetHeights(margin,fromEnd? terrain.terrainData.heightmapResolution-margin-1 : margin, arrLength,1);

			for (int i=0; i<heights.Length; i++)
			{
				int pos = (int)(step*i);
				heights[i] = array[0,pos] * terrain.terrainData.size.y;
			}
		}

		public void GetTerrainHeightsZ (Terrain terrain, float relativeMargin, ref float[] heights, bool fromEnd=false)
		{
			int margin = (int)(relativeMargin * terrain.terrainData.heightmapResolution);
			int arrLength = terrain.terrainData.heightmapResolution - margin*2;
			float step = 1f * arrLength / heights.Length;
			
			float[,] array = terrain.terrainData.GetHeights(fromEnd? terrain.terrainData.heightmapResolution-margin-1 : margin,margin, 1,arrLength);

			for (int i=0; i<heights.Length; i++)
			{
				int pos = (int)(step*i);
				heights[i] = array[pos,0] * terrain.terrainData.size.y;
			}
		}



		public override void  OnInspectorGUI ()
		{
			script = (MapMagic)target;
			if (MapMagic.instance == null) MapMagic.instance = script;
			script.terrains.CheckEmpty();
			
			if (layout == null) layout = new Layout();
			layout.margin = 0;
			layout.field = Layout.GetInspectorRect();
			layout.cursor = new Rect();
			layout.undoObject = script;
			layout.undoName =  "MapMagic settings change";

			layout.Par(20); bool modeNailing = layout.CheckButton(selectionMode == SelectionMode.nailing, "Pin Terrain", rect:layout.Inset(0.5f), icon:"MapMagic_PinIcon");
				if (layout.lastChange && modeNailing) selectionMode = SelectionMode.nailing;
				if (layout.lastChange && !modeNailing) selectionMode = SelectionMode.none;
			bool modeLocking = layout.CheckButton(selectionMode == SelectionMode.locking, "Lock Terrain", rect:layout.Inset(0.5f), icon:"MapMagic_LockIcon");
				if (layout.lastChange && modeLocking) selectionMode = SelectionMode.locking;
				if (layout.lastChange && !modeLocking) selectionMode = SelectionMode.none;

			layout.Par(4);
			layout.Par(24); if (layout.Button("Show Editor", rect:layout.Inset(), icon:"MapMagic_EditorIcon"))
			{
				MapMagicWindow window = (MapMagicWindow)EditorWindow.GetWindow (typeof (MapMagicWindow));
				//SceneMagicWindow window = EditorWindow.GetWindow<VoxelandCreate>();
				//window.script = script;
				window.Show();
//				window.FocusOnOutput();
			}

//			layout.ComplexField(ref MapMagic.instance.seed, "Seed");
//			layout.ComplexSlider(script.terrains.terrainSize, "Terrain Size", max:2048, quadratic:true);
//			layout.ComplexSlider(ref script.terrainHeight, "Terrain Height", max:2048, quadratic:true);

//			layout.Par();

			
//			layout.Par(); if (layout.Button("Generate")) { MapMagic.instance.terrains.start = true; script.ProcessThreads(); }
//			layout.Par(); if (layout.Button("Clear")) MapMagic.instance.generators.ClearGenerators();

//			Undo.RecordObject (script, "MapMagic settings change");
			layout.margin =10;

			layout.fieldSize = 0.4f;
			layout.Par(5); layout.Foldout(ref script.guiSettings, "General Settings");
			if (script.guiSettings)
			{
				MapMagic.instance.resolution = (int)layout.Field((Pots)MapMagic.instance.resolution, "Resolution");
				if (layout.lastChange && MapMagic.instance.instantGenerate) MapMagic.instance.ForceGenerate();
				layout.Field(ref MapMagic.instance.terrainSize, "Terrain Size");
				if (layout.lastChange) MapMagic.instance.terrains.Reset();
				layout.Field(ref MapMagic.instance.terrainHeight, "Terrain Height");
				if (layout.lastChange) MapMagic.instance.terrains.Reset();

				layout.Par(5);
				layout.Field(ref MapMagic.instance.generateInfinite, "Generate Infinite Terrain");
				if (MapMagic.instance.generateInfinite)
				{
					layout.Field(ref MapMagic.instance.generateRange, "Generate Range");
					layout.Field(ref MapMagic.instance.removeRange, "Remove Range", min:MapMagic.instance.generateRange);
					layout.Field(ref MapMagic.instance.enableRange, "Enable Range");
					//layout.Field(ref MapMagic.instance.terrains.enableRange, "Low Detail Range");
					//layout.Field(ref MapMagic.instance.terrains.detailRange, "Full Detail Range");
				}

				layout.Par(5);
				layout.Field(ref script.multiThreaded, "Multithreaded");
				if (script.multiThreaded) layout.Field(ref script.maxThreads, "Max Threads");
				layout.Field(ref script.instantGenerate, "Instant Generate");
				layout.Field(ref script.saveIntermediate, "Save Intermediate Results");

				layout.Par(5);
				layout.Field(ref script.heightWeldMargins, "Height Weld Margins", max:100);
				layout.Field(ref script.splatsWeldMargins, "Splats Weld Margins", max:100);
				//layout.ComplexField(ref script.hideWireframe, "Hide Wireframe");
				
				layout.Par(5);
				layout.Toggle(ref script.hideFarTerrains, "Hide Out-of-Range Terrains");
				//layout.Toggle(ref script.useAllCameras, "Generate around All Cameras");
				layout.Toggle(ref script.copyLayersTags, "Copy Layers and Tags to Terrains");
				layout.Toggle(ref script.copyComponents, "Copy Components to Terrains");

				layout.Par(5);
				layout.Label("Generate Terrain Markers:");
				layout.Field(ref script.genAroundMainCam, "Around Main Camera");
				
				layout.Par(); layout.Field(ref script.genAroundObjsTag, "Around Objects Tagged", rect:layout.Inset());
				int tagFieldWidth = (int)(layout.field.width*layout.fieldSize - 25);
				layout.cursor.x -= tagFieldWidth;
				script.genAroundTag = EditorGUI.TagField(layout.Inset(tagFieldWidth), script.genAroundTag);

				layout.Par(10);
				layout.Par(); layout.Label("Data", layout.Inset(0.2f));
				GeneratorsAsset newGens = layout.Field<GeneratorsAsset>(script.gens, rect:layout.Inset(0.5f));
				if (newGens != script.gens) { script.gens = newGens; script.guiGens = newGens; }
				if (layout.lastChange) script.gens.OnAfterDeserialize();
				if (script.gens == null)
				{
					if (layout.Button("Create", layout.Inset(0.3f)))
					{
						MapMagic.instance.gens = GeneratorsAsset.Default();
						MapMagic.instance.guiGens = MapMagic.instance.gens;
						EditorUtility.SetDirty(MapMagic.instance); 
					}
				}
                else if (!AssetDatabase.Contains(script.gens))
				{
					if (layout.Button("Save", layout.Inset(0.3f))) 
					{
						MapMagic.instance.gens.SaveAsset();
						EditorUtility.SetDirty(MapMagic.instance); 
					}
				}
				else 
				{
					if (layout.Button("Release", layout.Inset(0.3f))) 
					{ 
						MapMagic.instance.gens = MapMagic.instance.gens.ReleaseAsset(); 
						MapMagic.instance.guiGens = MapMagic.instance.gens;
						EditorUtility.SetDirty(MapMagic.instance); 
					}
				}

				layout.Par(5);
				//layout.Field(ref script.guiGeneratorWidth, "Node Width");
				layout.Toggle(ref script.guiDebug, "Debug");
			}

			layout.fieldSize = 0.5f; layout.sliderSize = 0.6f;
			layout.Par(5); layout.Foldout(ref script.guiTerrainSettings, "Terrain Settings");
			if (script.guiTerrainSettings)
			{
				layout.Field(ref script.pixelError, "Pixel Error", min:0, max:200, slider:true);
				layout.Field(ref script.baseMapDist, "Base Map Dist.", min:0, max:2000, slider:true);
				layout.Field(ref script.castShadows, "Cast Shadows");

				layout.Field(ref script.terrainMaterialType, "Material Type", disabled:script.previewGenerator!=null);
				layout.Field(ref script.customTerrainMaterial, "Custom Material", disabled:script.terrainMaterialType!=MapMagic.TerrainMaterialType.Custom);
				if (script.previewGenerator!=null) layout.Label("Terrain Material is disabled in preview mode", helpbox: true);

				layout.Field(ref script.applyColliders, "Apply Terrain Colliders");
			}

			layout.Par(5); layout.Foldout(ref script.guiTreesGrassSettings, "Trees, Details and Grass Settings");
			if (script.guiTreesGrassSettings)
			{
				layout.Field(ref script.detailDraw, "Draw");
				layout.Field(ref script.detailDistance, "Detail Distance", min:0, max:250, slider:true);
				layout.Field(ref script.detailDensity, "Detail Density", min:0, max:1, slider:true);
				layout.Field(ref script.treeDistance, "Tree Distance", min:0, max:5000, slider:true);
				layout.Field(ref script.treeBillboardStart, "Billboard Start", min:0, max:2000, slider:true);
				layout.Field(ref script.treeFadeLength, "Fade Length", min:0, max:200, slider:true);
				layout.Field(ref script.treeFullLod, "Max Full LOD Trees", min:0, max:10000, slider:true);

				layout.Par(5);
				layout.Field(ref script.windSpeed, "Wind Amount", min:0, max:1, slider:true);
				layout.Field(ref script.windSize, "Wind Bending", min:0, max:1, slider:true);
				layout.Field(ref script.windBending, "Wind Speed", min:0, max:1, slider:true); //there's no mistake here. Variable names are swapped in unity
				layout.Field(ref script.grassTint, "Grass Tint");
			}

			if (layout.change) 
				foreach (Chunk tw in MapMagic.instance.terrains.Objects()) tw.SetSettings();


			#region About
			layout.Par(5); layout.Foldout(ref script.guiAbout, "About");
			if (script.guiAbout)
			{
				Rect savedCursor = layout.cursor;
				
				layout.Par(100, padding:0);
				layout.Icon("MapMagicAbout", layout.Inset(100,padding:0));

				layout.cursor = savedCursor;
				layout.margin = 115;

				layout.Label("MapMagic " + (int)(MapMagic.version/10f) + "." + (MapMagic.version - (int)(MapMagic.version/10f)*10));
				layout.Label("by Denis Pahunov");
				
				layout.Par(10);
				layout.Label(" - Online Documentation", url:"https://docs.google.com/document/d/1OX7zYOrPz9qOFNAfO0qawhB7T3M6tLG9VgZSEntRJTA/edit?usp=sharing");
				layout.Label(" - Video Tutorials", url:"https://www.youtube.com/playlist?list=PL8fjbXLqBxvZb5yqXwp_bn4keyzyg5e0R");
				layout.Label(" - Forum Thread", url:"http://forum.unity3d.com/threads/map-magic-a-node-based-procedural-and-infinite-map-generator-for-asset-store.344440/");

				//layout.Par(10);
				//layout.Par(); layout.Label("Review or rating vote on");
				//layout.Par(); layout.Label("Asset Store", url:"--");
				//layout.Par(); layout.Label("would be appreciated.");

				layout.Par(10);
				layout.Label("On any issues related");
				layout.Label("with plugin functioning ");
				layout.Label("you can contact the");
				layout.Label("author by mail:");
				layout.Label("mail@denispahunov.ru", url:"mailto:mail@denispahunov.ru");
			}
			#endregion

			Layout.SetInspectorRect(layout.field);
		}

		[MenuItem ("GameObject/3D Object/Map Magic")]
		static void CreateMapMagic () 
		{
			if (FindObjectOfType<MapMagic>() != null)
			{
				Debug.LogError("Could not create new Map Magic instance, it already exists in scene.");
				return;
			}

			GameObject go = new GameObject();
			go.name = "Map Magic";
			MapMagic.instance = go.AddComponent<MapMagic>();

			//new terrains
			MapMagic.instance.terrains = new TerrainGrid();
			MapMagic.instance.seed=12345; MapMagic.instance.terrainSize=1000; MapMagic.instance.terrainHeight=300; MapMagic.instance.resolution=512;
			MapMagic.instance.terrains.Nail(new Coord(0,0));
			//MapMagic.instance.terrains.maxCount = 5;

			//creating initial generators
			MapMagic.instance.gens = GeneratorsAsset.Default();
			MapMagic.instance.guiGens = MapMagic.instance.gens;

			//registering undo
			MapMagic.instance.gens.OnBeforeSerialize();
			Undo.RegisterCreatedObjectUndo (go, "MapMagic Create");
			EditorUtility.SetDirty(MapMagic.instance);

			/*HeightOutput heightOut =  new HeightOutput();
			heightOut.guiRect = new Rect(43,76,200,20);
			MapMagic.instance.generators.array[1] = heightOut;
			heightOut.input.Link(noiseGen.output, noiseGen);*/
			
		}

	}//class
}//namespace