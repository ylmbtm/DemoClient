using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using MapMagic;
namespace MapMagic
{
	public class MapMagicWindow : EditorWindow
	{
		private GUIStyle generatorWindowStyle;
		private GUIStyle groupWindowStyle;

		#region Undo

			void PerformUndo ()
			{
				Repaint(); //just to make curve undo work.
				//modifying curve with ascript.layout writes undo the usual way, and it is displayed in undo stack as "MapMaic Generator Change"
				//but somehow GetCurrentGroupName returns the previous action instead, like "Selection Change"
			
				if (!Undo.GetCurrentGroupName().Contains("MapMagic")) return;

				foreach (Chunk tw in MapMagic.instance.terrains.Objects()) tw.results.Clear();
				if (MapMagic.instance.guiGens != null) MapMagic.instance.guiGens.ChangeGenerator(null);
				MapMagic.instance.gens.ChangeGenerator(null);
				if (MapMagic.instance.instantGenerate) { MapMagic.instance.ForceGenerate(); }

				Repaint();
				
			}
		#endregion

		#region Right-click actions
	
			void CreateGenerator (System.Type type, Vector2 guiPos)
			{
				if (MapMagic.instance.guiGens == null) MapMagic.instance.guiGens = MapMagic.instance.gens;

				Undo.RecordObject (MapMagic.instance.guiGens, "MapMagic Create Generator");
				MapMagic.instance.guiGens.setDirty = !MapMagic.instance.guiGens.setDirty;

				Generator gen = MapMagic.instance.guiGens.CreateGenerator(type, guiPos);
				MapMagic.instance.guiGens.ChangeGenerator(gen);

				repaint=true; Repaint(); 

				EditorUtility.SetDirty(MapMagic.instance.guiGens);
			}

			void DeleteGenerator (Generator gen)
			{
				if (MapMagic.instance.guiGens == null) MapMagic.instance.guiGens = MapMagic.instance.gens;

				Undo.RecordObject (MapMagic.instance.guiGens, "MapMagic Delete Generator"); 
				MapMagic.instance.guiGens.setDirty = !MapMagic.instance.guiGens.setDirty;
				
				MapMagic.instance.guiGens.DeleteGenerator(gen);
				MapMagic.instance.guiGens.ChangeGenerator(null);
				Repaint();

				EditorUtility.SetDirty(MapMagic.instance.guiGens);
			}

			void PreviewOutput (Generator gen, Generator.Output output, bool inWindow)
			{
				MapMagic.instance.previewGenerator = gen;
				MapMagic.instance.previewOutput = output;
				
				if (gen==null || output==null) { MapMagic.instance.previewGenerator=null; MapMagic.instance.previewOutput=null; } //should be defined both - or none
				
				if (gen!=null) MapMagic.instance.guiGens.ChangeGenerator(null);

				if (inWindow)
				{
					//previewOutput.inWindow = true;
					PreviewWindow.ShowWindow();
				}

				SceneView.RepaintAll();
			} 

			void ResetGenerator (Generator gen)
			{
				if (MapMagic.instance.guiGens == null) MapMagic.instance.guiGens = MapMagic.instance.gens;
				
				Undo.RecordObject (MapMagic.instance.guiGens, "MapMagic Reset Generator"); 
				MapMagic.instance.guiGens.setDirty = !MapMagic.instance.guiGens.setDirty;

				gen.ReflectionReset();
				MapMagic.instance.guiGens.ChangeGenerator(gen);

				EditorUtility.SetDirty(MapMagic.instance.guiGens);
			}

			private static Generator[] SmartCopyGenerators (Generator gen)
			{
				//saving all gens if clicked to background
				if (gen == null) return (Generator[])CustomSerialization.DeepCopy(MapMagic.instance.guiGens.list);
				
				//saving group
				else if (gen is Group)
				{
					Group grp = (Group)gen;
					Generator[] gens = MapMagic.instance.guiGens.list;

					//creating a list of group children (started with a group itself)
					List<Generator> gensList = new List<Generator>();
					gensList.Add(gen);
					for (int g=0; g<gens.Length; g++)
						if (grp.guiRect.Contains(gens[g].guiRect)) gensList.Add(gens[g]);

					//copying group children
					Generator[] copyGens =  (Generator[])CustomSerialization.DeepCopy(gensList.ToArray());
					
					//unlinking children from out-of-group generators
					HashSet<Generator> copyGensHash = new HashSet<Generator>();
					for (int g=0; g<copyGens.Length; g++) copyGensHash.Add(copyGens[g]);
					for (int g=0; g<copyGens.Length; g++)
						foreach (Generator.Input input in copyGens[g].Inputs())
						{
							if (input.link == null) continue;
							if (!copyGensHash.Contains(input.linkGen)) input.Unlink();
						}

					return copyGens;
				}

				//single generator
				else 
				{
					Generator copyGen = (Generator)CustomSerialization.DeepCopy(gen);
					foreach (Generator.Input input in copyGen.Inputs()) { if (input != null) input.Unlink(); }
					return new Generator[] { copyGen };
				}
			}

			private void DuplicateGenerator (Generator gen)
			{
				if (MapMagic.instance.guiGens == null) MapMagic.instance.guiGens = MapMagic.instance.gens;

				Generator[] copyGens = SmartCopyGenerators(gen);
				
				//ignoring already existing outputs
				for (int i=copyGens.Length-1; i>=0; i--)
				{
					Generator copyGen = copyGens[i];

					copyGen.guiRect.position += new Vector2(0, gen.guiRect.height + 10);
				}

				ArrayUtility.AddRange(ref MapMagic.instance.guiGens.list, copyGens);
				MapMagic.instance.guiGens.ChangeGenerator(null);
				repaint=true; Repaint();
			}

			void SaveGenerator (Generator gen, Vector2 pos, string path=null) //@Chuck: use this similarly to ExportToFile. If path is defined it will export generator using it
			{
				if (changeLock) return;
				
				if (path==null) path= UnityEditor.EditorUtility.SaveFilePanel(
						"Export Nodes",
						"",
						"MapMagicExport.nodes", 
						"nodes");
				if (path==null || path.Length==0) return;

				Generator[] saveGens = SmartCopyGenerators(gen);
				if (gen!=null) for (int i=0; i<saveGens.Length; i++) saveGens[i].guiRect.position -= gen.guiRect.position;

				//preparing serialization arrays
				List<string> classes = new List<string>();
				List<UnityEngine.Object> objects = new List<UnityEngine.Object>();
				List<object> references = new List<object>();
				List<float> floats = new List<float>();

				//saving
				CustomSerialization.WriteClass(saveGens, classes, objects, floats, references);
				using (System.IO.StreamWriter writer = new System.IO.StreamWriter(path))
					writer.Write(CustomSerialization.ExportXML(classes, objects, floats));

				//AssetDatabase.CreateAsset(saveGens, path);
				//AssetDatabase.SaveAssets();
			}

			void LoadGenerator (Vector2 pos)
			{
				if (MapMagic.instance.guiGens == null) MapMagic.instance.guiGens = MapMagic.instance.gens;	

				string path= UnityEditor.EditorUtility.OpenFilePanel(
						"Import Nodes",
						"", 
						"nodes");
				if (path==null || path.Length==0) return;

				//preparing serialization arrays
				List<string> classes = new List<string>();
				List<UnityEngine.Object> objects = new List<UnityEngine.Object>();
				List<object> references = new List<object>();
				List<float> floats = new List<float>();

				//loading
				System.IO.StreamReader reader = new System.IO.StreamReader(path);
				CustomSerialization.ImportXML(reader.ReadToEnd(), out classes, out objects, out floats);
				Generator[] loadedGens = (Generator[])CustomSerialization.ReadClass(0, classes, objects, floats, references);

				//offset 
				for (int i=loadedGens.Length-1; i>=0; i--) loadedGens[i].guiRect.position += pos;
				

				//GeneratorsAsset loadedGens = (GeneratorsAsset)AssetDatabase.LoadAssetAtPath(path, typeof(GeneratorsAsset));

				/*for (int i=loadedGens.list.Length-1; i>=0; i--)
				{
					//cloning
					//loadedGens.list[i] = loadedGens.list[i].ReflectionCopy();
					Generator gen = loadedGens.list[i];

					//offset
					gen.guiRect.position += pos;
					
					//ignoring already existing outputs
					if (gen is Generator.IOutput && MapMagic.instance.guiGens.GetGenerator(gen.GetType())!=null)
					{
						Debug.Log ("MapMagic: tried to load Output which already exists (" + gen + "). Skipping.");
						loadedGens.UnlinkGenerator(gen);
						ArrayUtility.RemoveAt(ref loadedGens.list, i);
					}

				}*/

				ArrayUtility.AddRange(ref MapMagic.instance.guiGens.list, loadedGens);
				MapMagic.instance.guiGens.ChangeGenerator(null);
				repaint=true; forceAll=true; Repaint();
			}


		#endregion
		
		//repainting gui on generate state changed (or if running to make a animated indicator)
		private void OnInspectorUpdate () 
		{ 	
			if (MapMagic.instance == null) return;
			if (!MapMagic.instance.terrains.complete) Repaint();

			//testing serialization
			/*if (MapMagic.instance.guiDebug)
			{
				Serializer ser = new Serializer();
				ser.Store(MapMagic.instance.gens); 
				if (!ser.Equals(MapMagic.instance.serializer)) Debug.LogError("Serialization Difference");
			}*/ //old way to test serialization
		}

		private void OnDisable ()
		{
			//removing callbacks
			Portal.OnChooseEnter -= DrawPortalSelector;
			//Undo.undoRedoPerformed -= PerformUndo;
		}

		private bool repaint = false;
		private bool forceAll = false;
		private void OnGUI() { DrawWindow(); if (repaint) DrawWindow(); repaint = false; } //drawing window, or doing it twice if repaint is needed
		private void DrawWindow()
		{
			if (MapMagic.instance == null) MapMagic.instance = FindObjectOfType<MapMagic>();
			MapMagic script = MapMagic.instance;
			if (script==null) return;
			if (MapMagic.instance.guiGens == null) MapMagic.instance.guiGens = MapMagic.instance.gens;
			GeneratorsAsset gens = MapMagic.instance.guiGens;
			//if (script.guiGens != null) gens = script.guiGens;
			//if (script.gens==null) script.gens = ScriptableObject.CreateInstance<GeneratorsAsset>();

			//un-selecting field on drag
			if (Event.current.button != 0) UnityEditor.EditorGUI.FocusTextInControl("");

			//startingscript.layout
			
			if (script.layout==null) 
				{ script.layout = new Layout(); script.layout.scroll = script.guiScroll; script.layout.zoom = script.guiZoom; script.layout.maxZoom = 1f; }
			script.layout.Zoom(); script.layout.Scroll(); //scrolling and zooming
			script.layout.field = this.position;
			
			//unity 5.4 beta
			if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.Layout) return; 

			if (script.guiDebug) UnityEngine.Profiling.Profiler.BeginSample("Redraw Window");

			//using middle mouse click events
			if (Event.current.button == 2) Event.current.Use();

			//undo
			Undo.undoRedoPerformed -= PerformUndo;
			Undo.undoRedoPerformed += PerformUndo;

			//setting title content
			titleContent = new GUIContent("Map Magic");
			titleContent.image =script.layout.GetIcon("MapMagic_WindowIcon");

			//drawing background
			Vector2 windowZeroPos =script.layout.ToInternal(Vector2.zero);
			windowZeroPos.x = ((int)(windowZeroPos.x/64f)) * 64; 
			windowZeroPos.y = ((int)(windowZeroPos.y/64f)) * 64; 
			script.layout.Icon( 
				"MapMagic_Background",
				new Rect(windowZeroPos - new Vector2(64,64), 
				position.size + new Vector2(128,128)), 
				tile:true);

			//drawing test center
			//script.layout.Button("Zero", new Rect(-10,-10,20,20));

			//calculating visible area
			Rect visibleArea = script.layout.ToInternal( new Rect(0,0,position.size.x,position.size.y) );
			if (forceAll) { visibleArea = new Rect(-200000,-200000,400000,400000); forceAll = false; }
			//visibleArea = new Rect(visibleArea.x+100, visibleArea.y+100, visibleArea.width-200, visibleArea.height-200);
			//layout.Label("Area", helpBox:true, rect:visibleArea);

			//checking if all generators are loaded, and none of them is null
			for (int i=gens.list.Length-1; i>=0; i--)
			{
				if (gens.list[i] == null) { ArrayTools.RemoveAt(ref gens.list, i); continue; }
				foreach (Generator.Input input in gens.list[i].Inputs()) 
				{
					if (input == null) continue;
					if (input.linkGen == null) input.Link(null, null);
				}
			}

			#region Drawing groups
			if (script.guiDebug) UnityEngine.Profiling.Profiler.BeginSample("Drawing Groups");

				for(int i=0; i<gens.list.Length; i++)
				{
					if (!(gens.list[i] is Group)) continue;
					Group group = gens.list[i] as Group;

					//checking if this is withinscript.layout field
					if (group.guiRect.x > visibleArea.x+visibleArea.width || group.guiRect.y > visibleArea.y+visibleArea.height ||
						group.guiRect.x+group.guiRect.width < visibleArea.x || group.guiRect.y+group.guiRect.height < visibleArea.y) 
							if (group.guiRect.width > 0.001f &&script.layout.dragState != Layout.DragState.Drag) continue; //if guiRect initialized and not dragging

					//settingscript.layout data
					group.layout.field = group.guiRect;
					group.layout.scroll =script.layout.scroll;
					group.layout.zoom =script.layout.zoom;

					group.OnGUI();

					group.guiRect = group.layout.field;
				}
			
			if (script.guiDebug) UnityEngine.Profiling.Profiler.EndSample();
			#endregion

			#region Drawing connections (before generators to make them display under nodes)
			if (script.guiDebug) UnityEngine.Profiling.Profiler.BeginSample("Drawing Connections");

				foreach(Generator gen in gens.list)
				{
					foreach (Generator.Input input in gen.Inputs())
					{
						if (input==null || input.link == null) continue; //input could be null in layered generators
						if (gen is Portal)
						{ 
							Portal portal = (Portal)gen;
							if (!portal.drawInputConnection) continue;
						}
						script.layout.Spline(input.link.guiConnectionPos, input.guiConnectionPos, color:Generator.CanConnect(input.link,input)? input.guiColor : Color.red);
					}
				}

			if (script.guiDebug) UnityEngine.Profiling.Profiler.EndSample();
			#endregion

			#region creating connections (after generators to make clicking in inout work)
			if (script.guiDebug) UnityEngine.Profiling.Profiler.BeginSample("Creating Connections");

			int dragIdCounter = gens.list.Length+1;
				foreach (Generator gen in gens.list)
					foreach (Generator.IGuiInout inout in gen.Inouts())
				{
					if (inout == null) continue;
					if (script.layout.DragDrop(inout.guiRect, dragIdCounter))
					{
						//finding target
						Generator.IGuiInout target = null;
						foreach (Generator gen2 in gens.list)
							foreach (Generator.IGuiInout inout2 in gen2.Inouts())
								if (inout2.guiRect.Contains(script.layout.dragPos)) target = inout2;

						//converting inout to Input (or Output) and target to Output (or Input)
						Generator.Input input = inout as Generator.Input;		if (input==null) input = target as Generator.Input;
						Generator.Output output = inout as Generator.Output;	if (output==null) output = target as Generator.Output;

						//connection validity test
						bool canConnect = input!=null && output!=null && Generator.CanConnect(output,input);

						//infinite loop test
						if (canConnect)
						{ 
							Generator outputGen = output.GetGenerator(gens.list);
							Generator inputGen = input.GetGenerator(gens.list);
							if (inputGen == outputGen || outputGen.IsDependentFrom(inputGen)) canConnect = false;
						}

						//drag
						//if (script.layout.dragState==Layout.DragState.Drag) //commented out because will not be displayed on repaint otherwise
						//{
							if (input == null)script.layout.Spline(output.guiConnectionPos,script.layout.dragPos, color:Color.red);
							else if (output == null)script.layout.Spline(script.layout.dragPos, input.guiConnectionPos, color:Color.red);
							else script.layout.Spline(output.guiConnectionPos, input.guiConnectionPos, color:canConnect? input.guiColor : Color.red);
						//}

						//release
						if (script.layout.dragState==Layout.DragState.Released && input!=null) //on release. Do nothing if input not defined
						{
							Undo.RecordObject (gens, "MapMagic Connection"); 
							gens.setDirty = !gens.setDirty;

							input.Unlink();
							if (canConnect) input.Link(output, output.GetGenerator(gens.list));
							gens.ChangeGenerator(gen);

							EditorUtility.SetDirty(gens);
						}
					}
					dragIdCounter++;
				}

			if (script.guiDebug) UnityEngine.Profiling.Profiler.EndSample();
			#endregion

			#region Drawing generators
			if (script.guiDebug) UnityEngine.Profiling.Profiler.BeginSample("Drawing Generators");

				for(int i=0; i<gens.list.Length; i++)
				{
					Generator gen = gens.list[i];
					if (gen is Group) continue; //skipping groups

					//checking if this generator is withinscript.layout field
					if (gen.guiRect.x > visibleArea.x+visibleArea.width || gen.guiRect.y > visibleArea.y+visibleArea.height ||
						gen.guiRect.x+gen.guiRect.width < visibleArea.x || gen.guiRect.y+gen.guiRect.height < visibleArea.y) 
							if (gen.guiRect.width > 0.001f &&script.layout.dragState != Layout.DragState.Drag) continue; //if guiRect initialized and not dragging

					if (gen.layout == null) gen.layout = new Layout();
					gen.layout.field = gen.guiRect;
					gen.layout.field.width = MapMagic.instance.guiGeneratorWidth;
				
					//gen.layout.OnBeforeChange -= RecordGeneratorUndo;
					//gen.layout.OnBeforeChange += RecordGeneratorUndo;
					gen.layout.undoObject = gens;
					gen.layout.undoName = "MapMagic Generators Change"; 
					gen.layout.dragChange = true;
					gen.layout.disabled = changeLock;

					//copyscript.layout params
					gen.layout.scroll =script.layout.scroll;
					gen.layout.zoom =script.layout.zoom;

					//drawing
					if (script.guiDebug) UnityEngine.Profiling.Profiler.BeginSample("Generator GUI");
					gen.OnGUIBase();
					if (script.guiDebug) UnityEngine.Profiling.Profiler.EndSample();

					//instant generate on params change
					if (gen.layout.change) 
					{
						gens.ChangeGenerator(gen);
						repaint=true; Repaint();

						EditorUtility.SetDirty(gens);
						
					}
			
					if (gen.guiRect.width<1 && gen.guiRect.height<1) { repaint=true;  Repaint(); } //repainting if some of the generators rect is 0
					gen.guiRect = gen.layout.field;
				}

			if (script.guiDebug) UnityEngine.Profiling.Profiler.EndSample();
			#endregion

			#region Toolbar
			if (script.guiDebug) UnityEngine.Profiling.Profiler.BeginSample("Toolbar");

				if (script.toolbarLayout==null) script.toolbarLayout = new Layout();
				script.toolbarLayout.margin = 0; script.toolbarLayout.rightMargin = 0;
				script.toolbarLayout.field.width = this.position.width;
				script.toolbarLayout.field.height = 18;
				script.toolbarLayout.cursor = new Rect();
				//script.toolbarLayout.window = this;
				script.toolbarLayout.Par(18, padding:0);

				EditorGUI.LabelField(script.toolbarLayout.field, "", EditorStyles.toolbarButton);

				//drawing state icon
				script.toolbarLayout.Inset(25);
				if (!MapMagic.instance.terrains.complete) { script.toolbarLayout.Icon("MapMagic_Loading", new Rect(5,0,16,16), animationFrames:12); Repaint(); }
				else script.toolbarLayout.Icon("MapMagic_Success", new Rect(5,0,16,16));
				//TODO: changed sign

				//generate buttons
				if (GUI.Button(script.toolbarLayout.Inset(120,padding:0), "Generate Changed", EditorStyles.toolbarButton) && MapMagic.instance.enabled) MapMagic.instance.Generate();
				if (GUI.Button(script.toolbarLayout.Inset(120,padding:0), "Force Generate All", EditorStyles.toolbarButton) && MapMagic.instance.enabled) MapMagic.instance.ForceGenerate();

				//seed field
				script.toolbarLayout.Inset(10);
				Rect seedLabelRect = script.toolbarLayout.Inset(34); seedLabelRect.y+=1; seedLabelRect.height-=4;
				Rect seedFieldRect = script.toolbarLayout.Inset(64); seedFieldRect.y+=2; seedFieldRect.height-=4;
				EditorGUI.LabelField(seedLabelRect, "Seed:", EditorStyles.miniLabel);
				int newSeed = EditorGUI.IntField(seedFieldRect, MapMagic.instance.seed, EditorStyles.toolbarTextField);
				if (newSeed != MapMagic.instance.seed) { MapMagic.instance.seed = newSeed; if (MapMagic.instance.instantGenerate) MapMagic.instance.ForceGenerate(); }

				//right part
				script.toolbarLayout.Inset(script.toolbarLayout.field.width - script.toolbarLayout.cursor.x - 150,padding:0);

				//drawing exit biome button
				Rect biomeRect = script.toolbarLayout.Inset(80, padding:0);
				if (MapMagic.instance.guiGens != null && MapMagic.instance.guiGens != MapMagic.instance.gens) 
				{
					if (script.toolbarLayout.Button("", biomeRect, icon:"MapMagic_ExitBiome", style:EditorStyles.toolbarButton)) MapMagic.instance.guiGens = null;
					script.toolbarLayout.Label("Exit Biome", new Rect(script.toolbarLayout.cursor.x-60, script.toolbarLayout.cursor.y+3, 60, script.toolbarLayout.cursor.height), fontSize:9);
				}

				//focus button
				
			//	if (GUI.Button(script.toolbarLayout.Inset(100,padding:0), "Focus", EditorStyles.toolbarButton)) FocusOnGenerators();
				if (script.toolbarLayout.Button("", script.toolbarLayout.Inset(23,padding:0), icon:"MapMagic_Focus", style:EditorStyles.toolbarButton)) FocusOnGenerators();
				if (script.toolbarLayout.Button("", script.toolbarLayout.Inset(60,padding:0), icon:"MapMagic_Zoom", style:EditorStyles.toolbarButton))script.layout.zoom=1;
				script.toolbarLayout.Label((int)(script.layout.zoom*100)+"%", new Rect(script.toolbarLayout.cursor.x-42, script.toolbarLayout.cursor.y+3, 42, script.toolbarLayout.cursor.height), fontSize:8);

			if (script.guiDebug) UnityEngine.Profiling.Profiler.EndSample();
			#endregion

			#region Draging
			if (script.guiDebug) UnityEngine.Profiling.Profiler.BeginSample("Dragging");

				//dragging generators
				for(int i=gens.list.Length-1; i>=0; i--)
				{
					Generator gen = gens.list[i];
					if (gen is Group) continue;
					gen.layout.field = gen.guiRect;

					//dragging
					if (script.layout.DragDrop(gen.layout.field, i)) 
					{
						if (script.layout.dragState == Layout.DragState.Pressed) 
						{
							Undo.RecordObject (gens, "MapMagic Generators Drag");
							gens.setDirty = !gens.setDirty;
						}
						if (script.layout.dragState == Layout.DragState.Drag ||script.layout.dragState == Layout.DragState.Released) 
						{ 
							//moving inout rects to remove lag
							//foreach (Generator.IGuiInout inout in gen.Inouts())
							//	inout.guiRect = new Rect(inout.guiRect.position+layout.dragDelta, inout.guiRect.size);
							gen.Move(script.layout.dragDelta,true);
							repaint=true; Repaint(); 

							EditorUtility.SetDirty(gens);
						}
					}

					//saving all generator rects
					gen.guiRect = gen.layout.field;
				}

				//dragging groups
				for (int i=gens.list.Length-1; i>=0; i--)
				{
					//Generator gen = gens.list[i];
					Group group = gens.list[i] as Group;
					if (group == null) continue;
					group.layout.field = group.guiRect;

					//resizing
					group.layout.field =script.layout.ResizeRect(group.layout.field, i+20000);

					//dragging
					if (script.layout.DragDrop(group.layout.field, i)) 
					{
						if (script.layout.dragState == Layout.DragState.Pressed) 
						{
							Undo.RecordObject (gens, "MapMagic Group Drag");
							gens.setDirty = !gens.setDirty;
							group.Populate(gens);
						}
						if (script.layout.dragState == Layout.DragState.Drag ||script.layout.dragState == Layout.DragState.Released) 
						{ 
							group.Move(script.layout.dragDelta,true);
							repaint=true; Repaint(); 

							EditorUtility.SetDirty(gens);
						}
						if (script.layout.dragState == Layout.DragState.Released && group != null) gens.SortGroups();
					}

					//saving all group rects
					group.guiRect = group.layout.field;
				}

			if (script.guiDebug) UnityEngine.Profiling.Profiler.EndSample();
			#endregion

			//right-click menus
			if (Event.current.type == EventType.ContextClick || (Event.current.type == EventType.MouseDown && Event.current.control)) DrawPopup();

			//debug center
			//EditorGUI.HelpBox(script.layout.ToLocal(new Rect(-25,-10,50,20)), "Zero", MessageType.None);

			//assigning portal popup action
			Portal.OnChooseEnter -= DrawPortalSelector; Portal.OnChooseEnter += DrawPortalSelector;

			//saving scroll and zoom
			script.guiScroll =script.layout.scroll; script.guiZoom =script.layout.zoom;  

			DrawDemoLock();
	
			if (script.guiDebug) UnityEngine.Profiling.Profiler.EndSample();
		}

		public void DrawPopup ()
		{
			if (MapMagic.instance.guiGens == null) MapMagic.instance.guiGens = MapMagic.instance.gens;
			GeneratorsAsset gens = MapMagic.instance.guiGens;
			//if (MapMagic.instance.guiGens != null) gens = MapMagic.instance.guiGens;
			
			Vector2 mousePos = MapMagic.instance.layout.ToInternal(Event.current.mousePosition);
				
			//finding something that was clicked
			Generator clickedGenerator = null;
			Group clickedGroup = null;
			Generator.Output clickedOutput = null;

			for (int i=0; i<gens.list.Length; i++) 
			{
				Generator gen = gens.list[i];
				if (gen.guiRect.Contains(mousePos))
				{
					if (!(gen is Group)) clickedGenerator = gens.list[i];
					else clickedGroup = gens.list[i] as Group;
				}
				
				foreach (Generator.Output output in gens.list[i].Outputs())
					if (output.guiRect.Contains(mousePos)) clickedOutput = output; 
			}
			if (clickedGenerator == null) clickedGenerator = clickedGroup;
			
			//create
			Dictionary<string, PopupMenu.MenuItem> itemsDict = new Dictionary<string, PopupMenu.MenuItem>();
			
			List<System.Type> allGeneratorTypes = typeof(Generator).GetAllChildTypes();
			for (int i=0; i<allGeneratorTypes.Count; i++)
			{
				if (System.Attribute.IsDefined(allGeneratorTypes[i], typeof(GeneratorMenuAttribute)))
				{
					GeneratorMenuAttribute attribute = System.Attribute.GetCustomAttribute(allGeneratorTypes[i], typeof(GeneratorMenuAttribute)) as GeneratorMenuAttribute;
					System.Type genType = allGeneratorTypes[i];

					if (attribute.disabled) continue;

					PopupMenu.MenuItem item = new PopupMenu.MenuItem(attribute.name, delegate () { CreateGenerator(genType, mousePos); });
					item.priority = attribute.priority;

					if (attribute.menu.Length != 0)
					{
						if (!itemsDict.ContainsKey(attribute.menu)) itemsDict.Add(attribute.menu, new PopupMenu.MenuItem(attribute.menu, subs:new PopupMenu.MenuItem[0]));
						ArrayTools.Add(ref itemsDict[attribute.menu].subItems, item);
					}
					else itemsDict.Add(attribute.name, item);
				}
			} 

			itemsDict["Map"].priority = 1;
			itemsDict["Objects"].priority = 2;
			itemsDict["Output"].priority = 3;
			itemsDict["Portal"].priority = 4;
			itemsDict["Group"].priority = 5;
			itemsDict["Biome"].priority = 6;

			PopupMenu.MenuItem[] createItems = new PopupMenu.MenuItem[itemsDict.Count];
			itemsDict.Values.CopyTo(createItems, 0);

			//create group
			//PopupMenu.MenuItem createGroupItem = new PopupMenu.MenuItem("Group",  delegate () { CreateGroup(mousePos); });
			//Extensions.ArrayAdd(ref createItems, createItems.Length-1, createGroupItem);

			//additional name
			/*string additionalName = "All";
			if (clickedGenerator != null) 
			{
				additionalName = "Generator";
				if (clickedGenerator is Group) additionalName = "Group";
			}*/

			//preview
			PopupMenu.MenuItem[] previewSubs = new PopupMenu.MenuItem[]
			{
				new PopupMenu.MenuItem("On Terrain", delegate() {PreviewOutput(clickedGenerator, clickedOutput, false);}, disabled:clickedOutput==null||clickedGenerator==null), 
				new PopupMenu.MenuItem("In Window", delegate() {PreviewOutput(clickedGenerator, clickedOutput, true);}, disabled:clickedOutput==null||clickedGenerator==null),
				new PopupMenu.MenuItem("Clear", delegate() {PreviewOutput(null, null, false);} )//, disabled:MapMagic.instance.previewOutput==null)
			};

			PopupMenu.MenuItem[] popupItems = new PopupMenu.MenuItem[]
			{
				new PopupMenu.MenuItem("Create", createItems),
				new PopupMenu.MenuItem("Export",	delegate () { SaveGenerator(clickedGenerator, mousePos); }),
				new PopupMenu.MenuItem("Import",						delegate () { LoadGenerator(mousePos); }),
				new PopupMenu.MenuItem("Duplicate",					delegate () { DuplicateGenerator(clickedGenerator); }),
				new PopupMenu.MenuItem("Remove",	delegate () { if (clickedGenerator!=null) DeleteGenerator(clickedGenerator); },	disabled:(clickedGenerator==null)),
				new PopupMenu.MenuItem("Reset",						delegate () { if (clickedGenerator!=null) ResetGenerator(clickedGenerator); },	disabled:clickedGenerator==null), 
				new PopupMenu.MenuItem("Preview", previewSubs)
			};

			PopupMenu.DrawPopup(popupItems, Event.current.mousePosition, closeAllOther:true);
		}

		public void DrawPortalSelector (Portal exit, Generator.InoutType type)
		{
			if (MapMagic.instance.guiGens == null) MapMagic.instance.guiGens = MapMagic.instance.gens;
			GeneratorsAsset gens = MapMagic.instance.guiGens;
			//if (MapMagic.instance.guiGens != null) gens = MapMagic.instance.guiGens;

			int entersNum = 0;
			for (int g=0; g<gens.list.Length; g++)
			{
				Portal portal = gens.list[g] as Portal;
				if (portal == null) continue;
				if (portal.form == Portal.PortalForm.Out) continue;
				if (portal.type != type) continue;

				entersNum++;
			}
			
			PopupMenu.MenuItem[] popupItems = new PopupMenu.MenuItem[entersNum];
			int counter = 0;
			for (int g=0; g<gens.list.Length; g++)
			{
				Portal enter = gens.list[g] as Portal;
				if (enter == null) continue;
				if (enter.form == Portal.PortalForm.Out) continue;
				if (enter.type != type) continue;

				popupItems[counter] = new PopupMenu.MenuItem( enter.name, delegate () 
					{ 
						if (enter.IsDependentFrom(exit)) { Debug.LogError("MapMagic: Linking portals this way will create dependency loop."); return; }
						exit.input.Link(enter.output, enter); 
						gens.ChangeGenerator(exit); 
					} );
				counter++;
			}

			PopupMenu.DrawPopup(popupItems, Event.current.mousePosition, closeAllOther:true);

		}

		public void FocusOnGenerators ()
		{
			if (MapMagic.instance == null) MapMagic.instance = FindObjectOfType<MapMagic>();
			if (MapMagic.instance.guiGens == null) MapMagic.instance.guiGens = MapMagic.instance.gens;
			
			//finding generators center
			Vector2 min = new Vector2(2000000,2000000); Vector2 max = new Vector2(-2000000,-2000000);
			for (int g=0; g<MapMagic.instance.guiGens.list.Length; g++)
			{
				Generator gen = MapMagic.instance.guiGens.list[g];
				if (gen.guiRect.x<min.x) min.x = gen.guiRect.x;
				if (gen.guiRect.y<min.y) min.y = gen.guiRect.y;
				if (gen.guiRect.max.x>max.x) max.x = gen.guiRect.max.x;
				if (gen.guiRect.max.y>max.y) max.y = gen.guiRect.max.y;
			}
			Vector2 center = (min+max)/2f;

			//focusing
			//center =script.layout.ToDisplay(center);
		//	center *= MapMagic.instance.guiZoom;
		//	MapMagic.instance.layout.scroll = -center;
		//	MapMagic.instance.layout.scroll += new Vector2(this.position.width/2f, this.position.height/2f);
			MapMagic.instance.layout.Focus(center);

			

			/*if (script.layout == null)script.layout = new Layout();
			//center =script.layout.ToDisplay(center);
			layout.scroll = -outputRect.center;
			layout.scroll.y += this.position.height / 2;
			layout.scroll.x += this.position.width - outputRect.width; 

			//saving
			if (script==null) script = FindObjectOfType<MapMagic>();
			script.guiScroll =script.layout.scroll; script.guiZoom =script.layout.zoom; //saving*/
		}

		#region Demo lock
		public bool demoLock {get{ return false; }}
		public bool changeLock {get{ return false; }}
		public void DrawDemoLock () { }
		#endregion

	}

}//namespace