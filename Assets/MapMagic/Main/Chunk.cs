using UnityEngine;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

#if UNITY_5_5
using UnityEngine.Profiling;
#endif

namespace MapMagic 
{
		[System.Serializable]
		public class Chunk
		{
			public Coord coord;
			public Terrain terrain;
			public TerrainCollider terrainCollider;
			public TransformPool[] pools;
			[System.NonSerialized] public Thread thread;
			
			public bool locked;
			public bool start;
			public bool stop; //start and stop at the same time will make the thread restart 
			public bool clear = true; //when generate has not been started
			public bool queuedApply; //enables after threadFn work complete, disbled in apply
			public bool running { get {return thread != null && thread.IsAlive; }}
			public bool complete { get {return (!start && !running && apply.Count==0 && !clear) || locked; }} //when apply complete and no changes to this terrain will be made infuture

			[System.NonSerialized] public HashSet<Generator> ready = new HashSet<Generator>();
			[System.NonSerialized] public Dictionary<Generator.Output, object> results = new Dictionary<Generator.Output, object>(); //saved outputs per-generator during generate
			[System.NonSerialized] public Dictionary<System.Type, object> apply = new Dictionary<System.Type, object>(); //the final sum, created during apply, generally obj-prototypes tuples
			[System.NonSerialized] public Matrix heights; //last heights applied to floor objects
			//[System.NonSerialized] public object previewObject; 

			[System.NonSerialized] public System.Diagnostics.Stopwatch timer = null; //debug timer

			//Neighbors 
			[System.NonSerialized] private Terrain oldNeig_x = null;
			[System.NonSerialized] private Terrain oldNeig_X = null;
			[System.NonSerialized] private Terrain oldNeig_z = null;
			[System.NonSerialized] private Terrain oldNeig_Z = null;
			public void SetNeighbors (bool force=false)
			{
			UnityEngine.Profiling.Profiler.BeginSample("Set Neigs");
				TerrainGrid terrains = MapMagic.instance.terrains;

				Terrain newNeig_x = terrains.GetTerrain(coord.x-1, coord.z, onlyComplete:false);
				Terrain newNeig_Z = terrains.GetTerrain(coord.x, coord.z+1, onlyComplete:false);
				Terrain newNeig_X = terrains.GetTerrain(coord.x+1, coord.z, onlyComplete:false);
				Terrain newNeig_z = terrains.GetTerrain(coord.x, coord.z-1, onlyComplete:false);


				if (oldNeig_x != newNeig_x || oldNeig_Z != newNeig_Z || oldNeig_X != newNeig_X || oldNeig_z != newNeig_z || force)
				{
					terrain.SetNeighbors( newNeig_x, newNeig_Z, newNeig_X, newNeig_z );

					oldNeig_x = newNeig_x;  oldNeig_Z = newNeig_Z;  oldNeig_X = newNeig_X;  oldNeig_z = newNeig_z;
				}
			UnityEngine.Profiling.Profiler.EndSample();
			}

			//defaults
			public CoordRect defaultRect 
			{get{
				int res = MapMagic.instance.resolution; //lod? instance.lodResolution : instance.resolution;
				return new CoordRect(coord.x*res, coord.z*res, res, res) ;
			}}
			public Matrix defaultMatrix 
			{get{
				return new Matrix( defaultRect );
			}}
			public SpatialHash defaultSpatialHash
			{get{
				SpatialHash spatialHash = new SpatialHash(new Vector2(coord.x*MapMagic.instance.resolution,coord.z*MapMagic.instance.resolution), MapMagic.instance.resolution, 16);
				return spatialHash;
			}}

			public object locker = new object();


			//switching state, starting generate and apply (called each frame)
			public void Update (float camDist)
			{
				//removing (unpinning) chunk if it's terrain was removed somehow
				if (terrain == null) 
				{
					MapMagic.instance.terrains.Unnail(coord, remove: true);
					return;
				}

				//enabling/disabling
				if ((camDist<MapMagic.instance.enableRange && complete) || MapMagic.instance.isEditor) 
					{ if (!terrain.gameObject.activeSelf) terrain.gameObject.SetActive(true); }
				else
					{ if (terrain.gameObject.activeSelf) terrain.gameObject.SetActive(false); }

				//setting terrain neighbors (they reset after each serialize)
				SetNeighbors();

				//starting generate
				if ((camDist<MapMagic.instance.generateRange || MapMagic.instance.isEditor) && !running && clear) start = true;

				//starting threads
				if (start)
				{		
					//calling before-gen event
					MapMagic.CallOnGenerateStarted(terrain); //if (MapMagic.OnGenerateStarted != null) MapMagic.OnGenerateStarted(terrain);

					//preparing generators
					if (!MapMagic.instance.generatorsPrepared) MapMagic.instance.PrepareGenerators();
								
					//generating
					if (!MapMagic.instance.multiThreaded) ThreadFn();
					else
					{
						//restarting thread if it is still alive
						if (running) stop=true;

						//if dead and thread limit not reached
						else if (MapMagic.instance.runningThreadsCount < MapMagic.instance.maxThreads)
						{ 
							thread = new Thread(ThreadFn);
							thread.IsBackground = true;
							//thread.Priority = System.Threading.ThreadPriority.BelowNormal;
							thread.Start(MapMagic.instance.gens);
							MapMagic.instance.runningThreadsCount++;
						}
					}
				}

				//sending terrain on apply
				if (!MapMagic.instance.applyRunning && queuedApply  &&  !start && !stop && !running) //if apply not running and got something to apply. Do not apply when terrain stopped, restarted or still producing
				//	queuedApply)
				{
					//iteraing routine manually in editor (in one frame)
					if (MapMagic.instance.isEditor)
					{
						IEnumerator e = ApplyRoutine();
						while (e.MoveNext());
					}

					//starting routine in playmode
					else MapMagic.instance.StartCoroutine(ApplyRoutine());
				}
			}


			//generating
			public void ThreadFn ()
			{	
				if (locked) return;
				
				lock (locker) while (true)
				{
					stop=false;  //in case it was restarted
					start = false; 
					clear = false;
					apply.Clear();

					//clearing debug timers to know what generators were processed
					MapMagic.instance.guiDebugProcessTimes.Clear(); 
					MapMagic.instance.guiDebugApplyTimes.Clear();

					try 
					{
						

						#region Generating Main graph first
							
							//calculating the list of changed outputs
							List<Generator> changedOutputs = new List<Generator>();
							foreach (Generator outGen in MapMagic.instance.gens.OutputGenerators(onlyEnabled:true, checkBiomes:false))
							{
								outGen.CheckClearRecursive(this);
								if (!ready.Contains(outGen)) changedOutputs.Add(outGen);
							}

							//preview (checking it twice - here and in the fn end)
							if (MapMagic.instance.previewGenerator!=null && MapMagic.instance.previewOutput!=null)
								MapMagic.instance.previewGenerator.CheckClearRecursive(this);

							//types of objects that were changed (for process)
							HashSet<System.Type> changedTypes = new HashSet<Type>();
							for (int i=0; i<changedOutputs.Count; i++) 
							{
								changedTypes.Add(changedOutputs[i].GetType());
								
								//adding all of the biome outgens to processing list
								if (changedOutputs[i] is Biome)
								{
									Biome biome = (Biome)changedOutputs[i];
									if (biome.data == null) continue;
									foreach (Generator outGen in biome.data.OutputGenerators(onlyEnabled:true, checkBiomes:false))
										changedTypes.Add(outGen.GetType());
								}
							}

							//generating
							for (int i=0; i<changedOutputs.Count; i++) 
							{
								changedOutputs[i].GenerateWithPriors(this);
								if (stop) return;
							}

						#endregion

						#region Generating Biomes

							//calculating the list of changed outputs
							changedOutputs.Clear();
							foreach (Biome biome in MapMagic.instance.gens.GeneratorsOfType<Biome>(onlyEnabled:true, checkBiomes:false))
							{
								if (biome.data==null) continue;
								if (biome.mask.linkGen==null) continue;

								Matrix biomeMask = (Matrix)biome.mask.GetObject(this);
								if (biomeMask==null || biomeMask.IsEmpty()) continue;

								foreach (Generator outGen in biome.data.OutputGenerators(onlyEnabled:true, checkBiomes:false))
								{
									outGen.CheckClearRecursive(this);
									if (!ready.Contains(outGen)) changedOutputs.Add(outGen);
								}
							}

							//adding changed types
							for (int i=0; i<changedOutputs.Count; i++) 
								changedTypes.Add(changedOutputs[i].GetType());

							//generating
							for (int i=0; i<changedOutputs.Count; i++) 
							{
								changedOutputs[i].GenerateWithPriors(this);
								if (stop) return;
							}

						#endregion

						#region Preview

							if (MapMagic.instance.previewGenerator!=null && MapMagic.instance.previewOutput!=null)
							{
								MapMagic.instance.previewGenerator.CheckClearRecursive(this);
								MapMagic.instance.previewGenerator.GenerateWithPriors(this);
								if (stop) return;

								//if (results.ContainsKey(MapMagic.instance.previewOutput)) previewObject = results[MapMagic.instance.previewOutput];  
								//else previewObject = defaultMatrix;
							}

						#endregion

						
						/*//checking and resetting ready state recursive
						foreach (Generator outGen in MapMagic.instance.gens.OutputGenerators(onlyEnabled:true, checkBiomes:true)) //for outputs (including biomes)
							outGen.CheckClearRecursive(this);
						if (MapMagic.instance.previewOutput != null) MapMagic.instance.previewGenerator.CheckClearRecursive(this); //for preview

						//types of objects that were changed (for process)
						HashSet<System.Type> changedTypes = new HashSet<Type>();
						foreach (Generator outGen in MapMagic.instance.gens.OutputGenerators(onlyEnabled:true, checkBiomes:true)) 
							if (!ready.Contains(outGen)) 
								changedTypes.Add(outGen.GetType());

						//resseting all biome output if it has changed
						foreach (Biome biome in MapMagic.instance.gens.GeneratorsOfType<Biome>(onlyEnabled:true, checkBiomes:true))
							if (!ready.Contains(biome) && biome.data!=null && biome.mask.linkGen!=null)
							{
								foreach (Generator outGen in biome.data.OutputGenerators(onlyEnabled:true, checkBiomes:false))
									changedTypes.Add(outGen.GetType());
							}

						//generating main
						foreach (Generator outGen in MapMagic.instance.gens.OutputGenerators(onlyEnabled:true, checkBiomes:true)) //generating outputs
							if (!ready.Contains(outGen)) 
								outGen.GenerateWithPriors(this);

						if (MapMagic.instance.previewOutput != null) //generating preview
						{
							MapMagic.instance.previewGenerator.GenerateWithPriors(this);
							if (!stop) 
							{
								if (results.ContainsKey(MapMagic.instance.previewOutput)) previewObject = results[MapMagic.instance.previewOutput];
								else previewObject = defaultMatrix;
							}
						}*/

						//resetting objects if height changed (to floor them)
						if (changedTypes.Contains(typeof(HeightOutput))) { changedTypes.Add(typeof(TreesOutput)); changedTypes.Add(typeof(ObjectOutput)); }

						//finalizing (processing)
						if (changedTypes.Contains(typeof(HeightOutput))) HeightOutput.Process(this); //typeof(HeightOutput).GetMethod("Process", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Invoke(null,new object[] {this});
						if (changedTypes.Contains(typeof(SplatOutput))) SplatOutput.Process(this);
						if (changedTypes.Contains(typeof(ObjectOutput))) ObjectOutput.Process(this);
						if (changedTypes.Contains(typeof(TreesOutput))) TreesOutput.Process(this);
						if (changedTypes.Contains(typeof(GrassOutput))) GrassOutput.Process(this);
						if (changedTypes.Contains(typeof(RTPOutput))) RTPOutput.Process(this);
					}

					catch (System.Exception e) { Debug.LogError("Generate Thread Error:\n" + e); }

					//if (!stop) Thread.Sleep(2000);
					//exiting thread - only if it should not be restared
					if (!start) { queuedApply=true; break; }
				}
			}


			// applying
			public IEnumerator ApplyRoutine ()
			{
				//calling before-apply event
				MapMagic.CallOnGenerateCompleted(terrain); //if (MapMagic.OnGenerateCompleted != null) MapMagic.OnGenerateCompleted(terrain);

				MapMagic.instance.applyRunning = true;

				//apply
				foreach (KeyValuePair<Type,object> kvp in apply)
				{
					//Type output = kvp.Key;
					//if (!(output as Generator).enabled) continue;
					//if (output is SplatOutput && MapMagic.instance.gens.GetGenerator<PreviewOutput>()!=null) continue; //skip splat out if preview exists

					//callback
					MapMagic.CallOnApply(terrain, kvp.Value); //if (OnApply!=null) OnApply(terrain, kvp.Value);

					//selecting apply enumerator (with switch, not reflection)
					IEnumerator e = null;
					System.Type type = kvp.Key;
					if (type == typeof(HeightOutput)) e = HeightOutput.Apply(this);
					else if (type == typeof(SplatOutput)) e = SplatOutput.Apply(this);
					else if (type == typeof(ObjectOutput)) e = ObjectOutput.Apply(this);
					else if (type == typeof(TreesOutput)) e = TreesOutput.Apply(this);
					else if (type == typeof(GrassOutput)) e = GrassOutput.Apply(this);
					else if (type == typeof(RTPOutput)) e = RTPOutput.Apply(this);

					//apply enumerator
					while (e.MoveNext()) 
					{				
						if (terrain==null) yield break; //guard in case max terrains count < actual terrains: terrain destroyed or still processing
						yield return null;
					}
				}

				//purging unused outputs
				HashSet<Type> existingOutputs = MapMagic.instance.gens.GetExistingOutputTypes(onlyEnabled:true, checkBiomes:true);

				if (!existingOutputs.Contains(typeof(HeightOutput)) && terrain.terrainData.heightmapResolution != 33) HeightOutput.Purge(this);
				if (!existingOutputs.Contains(typeof(SplatOutput)) && !existingOutputs.Contains(typeof(RTPOutput)) && terrain.terrainData.alphamapResolution != 16) SplatOutput.Purge(this);
				if (!existingOutputs.Contains(typeof(ObjectOutput)) && terrain.transform.childCount != 0) ObjectOutput.Purge(this);
				if (!existingOutputs.Contains(typeof(TreesOutput)) && terrain.terrainData.treeInstanceCount != 0) TreesOutput.Purge(this);
				if (!existingOutputs.Contains(typeof(GrassOutput)) && terrain.terrainData.detailResolution != 16) GrassOutput.Purge(this);

				//previewing
/*				if (instance.previewOutput != null && previewObject != null) 
				{
					SplatOutput.Preview((Matrix)previewObject, this);
				}*/

				//creating initial texture if splatmap count is 0 - just to look good
				if (terrain.terrainData.splatPrototypes.Length == 0) ClearSplats();

				//clearing intermediate results
				apply.Clear();
				if (!MapMagic.instance.isEditor || !MapMagic.instance.saveIntermediate) { results.Clear(); ready.Clear(); } //this should be done in thread, but thread has no access to isPlaying

				//if (!terrain.gameObject.activeSelf) terrain.gameObject.SetActive(true); //terrain.enabled = true;

				MapMagic.instance.applyRunning = false;

				//copy layer, tag, scripts from mm to terrains
				if (MapMagic.instance.copyLayersTags)
				{
					GameObject go = terrain.gameObject;
					go.layer = MapMagic.instance.gameObject.layer;
					go.isStatic = MapMagic.instance.gameObject.isStatic;
					try { go.tag = MapMagic.instance.gameObject.tag; } catch { Debug.LogError("MapMagic: could not copy object tag"); }
					//#if UNITY_EDITOR
					//UnityEditor.GameObjectUtility.SetStaticEditorFlags(go, UnityEditor.GameObjectUtility.GetStaticEditorFlags(MapMagic.instance.gameObject));
					//#endif
				}
				if (MapMagic.instance.copyComponents)
				{
					GameObject go = terrain.gameObject;
					MonoBehaviour[] components = MapMagic.instance.GetComponents<MonoBehaviour>();
					for (int i=0; i<components.Length; i++)
					{
						if (components[i] is MapMagic || components[i] == null) continue; //if MapMagic itself or script not assigned
						if (terrain.gameObject.GetComponent(components[i].GetType()) == null) Extensions.CopyComponent(components[i], go);
					}
				}

				//calling after-apply event
				MapMagic.CallOnApplyCompleted(terrain); //if (MapMagic.OnApplyCompleted != null) MapMagic.OnApplyCompleted(terrain);
				queuedApply = false;

				//returning preview if it is enabled
				//if (MapMagic.instance.previewOutput != null) Preview(forceRefresh:true);
			}


			public void SetSettings ()
			{
				MapMagic magic = MapMagic.instance;
				
				terrain.heightmapPixelError = magic.pixelError;
				terrain.basemapDistance = magic.baseMapDist;
				terrain.castShadows = magic.castShadows;
				
				if (terrainCollider==null) terrainCollider = terrain.GetComponent<TerrainCollider>();
				terrainCollider.enabled = MapMagic.instance.applyColliders;

				//material
				if (MapMagic.instance.previewGenerator==null)
				{
					terrain.materialType = MapMagic.ToUnityTerrMatType(MapMagic.instance.terrainMaterialType);

					if (MapMagic.instance.terrainMaterialType == MapMagic.TerrainMaterialType.Custom)
					{
						terrain.materialTemplate = MapMagic.instance.customTerrainMaterial;
					}
					else if (MapMagic.instance.terrainMaterialType == MapMagic.TerrainMaterialType.RTP)
					{
						if (terrain.materialTemplate == null || terrain.materialTemplate.shader.name != "Relief Pack/ReliefTerrain-FirstPass")
						{
							Shader shader = Shader.Find("Relief Pack/ReliefTerrain-FirstPass");
							if (shader != null) terrain.materialTemplate = new Material(shader);
							else Debug.Log ("MapMagic: Could not find Relief Pack/ReliefTerrain-FirstPass shader. Make sure RTP is installed or switch material type to Standard.");
						}
					}
				}

				terrain.drawTreesAndFoliage = magic.detailDraw;
				terrain.detailObjectDistance = magic.detailDistance;
				terrain.detailObjectDensity = magic.detailDensity;
				terrain.treeDistance = magic.treeDistance;
				terrain.treeBillboardDistance = magic.treeBillboardStart;
				terrain.treeCrossFadeLength = magic.treeFadeLength;
				terrain.treeMaximumFullLODCount = magic.treeFullLod;

				terrain.terrainData.wavingGrassSpeed = magic.windSpeed;
				terrain.terrainData.wavingGrassAmount = magic.windSize;
				terrain.terrainData.wavingGrassStrength = magic.windBending;
				terrain.terrainData.wavingGrassTint = magic.grassTint;
			}

			public void ClearSplats () //same as SplatOutput.Clear
			{
				terrain.terrainData.splatPrototypes = new SplatPrototype[] { new SplatPrototype() { texture = Extensions.ColorTexture(2,2,new Color(0.5f, 0.5f, 0.5f, 0f)) } };

				float[,,] emptySplats = new float[16,16,1];
				for (int x=0; x<16; x++)
					for (int z=0; z<16; z++)
						emptySplats[z,x,0] = 1;

				terrain.terrainData.alphamapResolution = 16;
				terrain.terrainData.SetAlphamaps(0,0,emptySplats);

				if (MapMagic.instance.guiDebug) Debug.Log("Splats Cleared");
			}


			//preview
			[System.NonSerialized] private Material previewBackupMaterial; //mainly to store rtp material
			[System.NonSerialized] private Matrix lastPreviewedMatrix;

			public void Preview ()
			{
				#if UNITY_EDITOR 
				if (Event.current == null || Event.current.type != EventType.Repaint) return;
					
				//clearing preview if prevew generator is off (or non-map)
				if (MapMagic.instance.previewOutput==null || MapMagic.instance.previewOutput.type!=Generator.InoutType.Map)
				{
					if (terrain.materialTemplate != null && terrain.materialTemplate.shader.name == "MapMagic/TerrainPreviewFirstPass")
					{
						terrain.materialType = MapMagic.ToUnityTerrMatType(MapMagic.instance.terrainMaterialType);
						terrain.materialTemplate = previewBackupMaterial;

						previewBackupMaterial = null;
						lastPreviewedMatrix = null;

						UnityEditor.SceneView.RepaintAll();
					}
				}
						
				if (MapMagic.instance.previewOutput!=null && MapMagic.instance.previewOutput.type == Generator.InoutType.Map)
				{
					//loading preview material from terrain (or creating new)
					Material previewMat = null;
					if (terrain.materialTemplate!=null && terrain.materialTemplate.shader.name=="MapMagic/TerrainPreviewFirstPass") previewMat = terrain.materialTemplate;
					if (previewMat == null) 
					{
						Shader previewShader = Shader.Find("MapMagic/TerrainPreviewFirstPass");
						previewMat = new Material(previewShader);

						previewBackupMaterial = terrain.materialTemplate;
						terrain.materialTemplate = previewMat;
						terrain.materialType = Terrain.MaterialType.Custom;
						
						UnityEditor.SceneView.RepaintAll();
					}

					//loading matrix
					Matrix matrix = MapMagic.instance.previewOutput.GetObject<Matrix>(this);

					//refreshing preview texture
					if (matrix != lastPreviewedMatrix)
					{
						Texture2D tex = null;
						
						//populate
						if (matrix != null) 
						{
							tex = new Texture2D(matrix.rect.size.x, matrix.rect.size.z);
			 
							Color[] line = new Color[matrix.rect.size.z];
							for (int x=0; x<matrix.rect.size.x; x++)
							{
								for (int z=0; z<matrix.rect.size.z; z++)
								{
									float val = matrix[x+matrix.rect.offset.x, z+matrix.rect.offset.z];
									line[z] = new Color(val, val, val);
								}
								tex.SetPixels(x,0,1,line.Length,line);
							}
						}
						else //in case the output is not generated
						{
							tex = Extensions.ColorTexture(2,2,Color.gray);
						}

						//apply
						tex.Apply();
						previewMat.SetTexture("_Preview", tex);
						if (MapMagic.instance.guiDebug) Debug.Log("Preview Applied");

						lastPreviewedMatrix = matrix;

						UnityEditor.SceneView.RepaintAll();
					}
				}

				//spatial hash
				if (MapMagic.instance.previewOutput!=null && MapMagic.instance.previewOutput.type == Generator.InoutType.Objects)
				{
					float pixelSize = 1f * MapMagic.instance.terrainSize / MapMagic.instance.resolution;
					
					SpatialHash objs = MapMagic.instance.previewOutput.GetObject<SpatialHash>(this);

					if (objs != null)
					{
						int objsCount = objs.Count;
						foreach (SpatialObject obj in objs)
						{
							float terrainHeight = 0;
							if (heights != null) terrainHeight = heights.GetInterpolated(obj.pos.x, obj.pos.y);
							Vector3 pos = new Vector3(obj.pos.x*pixelSize, (obj.height+terrainHeight)*MapMagic.instance.terrainHeight, obj.pos.y*pixelSize);
							pos += MapMagic.instance.transform.position;

							UnityEditor.Handles.color = new Color(1,1,1,1);
							UnityEditor.Handles.DrawLine(pos+new Vector3(obj.size/2f,0,0), pos-new Vector3(obj.size/2f,0,0));
							UnityEditor.Handles.DrawLine(pos+new Vector3(0,0,obj.size/2f), pos-new Vector3(0,0,obj.size/2f));

							if (objsCount < 100)
							{
								Vector3 oldPoint = pos;
								foreach (Vector3 point in pos.CircleAround(obj.size/2f, 12, true))
								{
									UnityEditor.Handles.DrawLine(oldPoint,point);
									oldPoint = point;
								}
							}

							UnityEditor.Handles.color = new Color(1,1,1,0.15f); 
							UnityEditor.Handles.DrawLine(new Vector3(pos.x, 0, pos.z), new Vector3(pos.x, MapMagic.instance.terrainHeight, pos.z));
						}
					}
				}

				#endif
			}
		}//class
}//namespace