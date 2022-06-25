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
	[SelectionBase]
	[ExecuteInEditMode]
	public class MapMagic : MonoBehaviour
	{
		public static readonly int version = 17; 

		//terrains and generators
		public TerrainGrid terrains = new TerrainGrid();
		public GeneratorsAsset gens;

		[System.NonSerialized] public int runningThreadsCount = 0;
		[System.NonSerialized] public bool applyRunning = false;

		//main parameters
		public int seed = 12345;
		public int terrainSize = 1000; //should be int to avoid terrain start between pixels
		public int terrainHeight = 200;
		public int resolution = 512;
		public int lodResolution = 128;

		public static MapMagic instance = null;

		//private Vector3[] camPoses; //this arrays will be reused and will never be used directly
		//private Coord[] camCoords; 
		public int mouseButton = -1; //mouse button in MapMagicWindow, not scene view. So it is not scene view delegate, but assigned from window script

		public bool generateInfinite = true;
		public int generateRange = 350;
		public int removeRange = 400;
		public int enableRange = 300;

		public bool generatorsPrepared = false; //a flag to prepare generators only once per frame

		//events
		public delegate void ApplyEvent (Terrain terrain, object obj);
		public delegate void ChangeEvent (Terrain terrain);
		public static event ApplyEvent OnApply;		public static void CallOnApply (Terrain terrain, object obj) { if (OnApply!=null) OnApply(terrain, obj); }
		public static event ChangeEvent OnGenerateStarted;	public static void CallOnGenerateStarted (Terrain terrain) { if (OnGenerateStarted != null) OnGenerateStarted(terrain); }
		public static event ChangeEvent OnGenerateCompleted;	public static void CallOnGenerateCompleted (Terrain terrain) { if (OnGenerateCompleted != null) OnGenerateCompleted(terrain); }
		public static event ChangeEvent OnApplyCompleted;	public static void CallOnApplyCompleted (Terrain terrain) { if (OnApplyCompleted != null) OnApplyCompleted(terrain); }

		//preview
		[System.NonSerialized] public Generator previewGenerator = null;
		[System.NonSerialized] public Generator.Output previewOutput = null;

		//settings
		public bool multiThreaded = true;
		public int maxThreads = 2;
		public bool instantGenerate = true;
		public bool saveIntermediate = true;
		public int heightWeldMargins = 5;
		public int splatsWeldMargins = 2;
		public bool hideWireframe = true;
		public bool hideFarTerrains = true;
		//public bool useAllCameras = false;
		public bool copyLayersTags = true;
		public bool copyComponents = true;
		public bool applyColliders = true;
		
		public bool genAroundMainCam = true;
		public bool genAroundObjsTag = false;
		public string genAroundTag = null;

		//generator globals
		public int heightScale = 1;
		public int grassPatchRes = 16;

		//terrain settings
		public int pixelError = 1;
		public int baseMapDist = 1000;
		public bool castShadows = false;

		public enum TerrainMaterialType { BuiltInLegacyDiffuse=1, BuiltInLegacySpecular=2, BuiltInStandard=0, RTP=4, Custom=3 };
		public static Terrain.MaterialType ToUnityTerrMatType (TerrainMaterialType src)
		{
			if (src==TerrainMaterialType.RTP) return Terrain.MaterialType.Custom;
			else return (Terrain.MaterialType)src;
		}
		public TerrainMaterialType terrainMaterialType = TerrainMaterialType.BuiltInStandard;
		public Material customTerrainMaterial = null;

		//details and trees
		public bool detailDraw = true;
		public float detailDistance = 80;
		public float detailDensity = 1;
		public float treeDistance = 1000;
		public float treeBillboardStart = 200;
		public float treeFadeLength = 5;
		public int treeFullLod = 150;

		public float windSpeed = 0.5f;
		public float windSize = 0.5f;
		public float windBending = 0.5f;
		public Color grassTint = Color.gray;

		//gui values
		public int selected=0;
		public GeneratorsAsset guiGens = null;
		public Vector2 guiScroll = new Vector2(0,0);
		public float guiZoom = 1;
		[System.NonSerialized] public Layout layout;
		[System.NonSerialized] public Layout toolbarLayout;
		public bool guiGenerators = true;
		public bool guiSettings = false;
		public bool guiTerrainSettings = false;
		public bool guiTreesGrassSettings = false;
		public bool guiDebug = false;
		public bool guiAbout = false;
		public GameObject sceneRedrawObject;
		public int guiGeneratorWidth = 160;

		public Dictionary<Type,long> guiDebugProcessTimes = new Dictionary<Type, long>();
		public Dictionary<Type,long> guiDebugApplyTimes = new Dictionary<Type, long>();
	
		public delegate void RepaintWindowAction();
		public static event RepaintWindowAction RepaintWindow;
		public static void CallRepaintWindow () { if (MapMagic.instance.isEditor) if (RepaintWindow != null) RepaintWindow(); }

		public bool setDirty; //registering change for undo. Inverting this value if Unity Undo does not see a change, but actually there is one (for example when saving data)




		#region isEditor
		public bool isEditor 
		{get{
			#if UNITY_EDITOR
				return 
					!UnityEditor.EditorApplication.isPlaying; //if not playing
					//(UnityEditor.EditorWindow.focusedWindow != null && UnityEditor.EditorWindow.focusedWindow.GetType() == System.Type.GetType("UnityEditor.GameView,UnityEditor")) //if game view is focused
					//UnityEditor.SceneView.lastActiveSceneView == UnityEditor.EditorWindow.focusedWindow; //if scene view is focused
			#else
				return false;
			#endif
		}}
		#endregion


		public void OnEnable ()
		{
            if (Application.isPlaying && GTData.IsLaunched)
            {
                enabled = false;
                return;
            }

#if UNITY_EDITOR
            //adding delegates
            UnityEditor.EditorApplication.update -= Update;	

			if (isEditor) 
				UnityEditor.EditorApplication.update += Update;	
			#endif

			//finding singleton instance
			instance = FindObjectOfType<MapMagic>();

			//checking terrains consistency
			terrains.CheckEmpty();
		}


		public void OnDisable ()
		{
			//removing delegates
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.update -= Update;	
			#endif
		}


		//reusing update arrays
		private Vector3[] camPoses = null;
		private Chunk[] chunksArray = null;
		private float[] distsArray = null;

		public void Update () 
		{ 
			//checking if instance already exists and disabling if it is another mm
			if (instance != null && instance != this) { Debug.LogError("MapMagic object already present in scene. Disabling duplicate"); this.enabled = false; return; }
		
			//loading old non-asset data
			if (gens == null)
			{
				if (serializer != null && serializer.entities != null && serializer.entities.Count != 0) 
				{	
					Debug.Log("MapMagic: Loading outdated scene format. Please check node consistency and re-save the scene.");
					LoadOldNonAssetData();
					serializer = null;
				}
				else { Debug.Log("MapMagic: Could not find the proper graph data. It the data file was changed externally reload the scene, otherwise create the new one in the General Settings tab."); return; }
			}

			//checking gens asset
			//if (gens == null) gens = ScriptableObject.CreateInstance<GeneratorsAsset>();

			//finding camera positions
			camPoses = GetCamPoses(camPoses); //TODO: reuse these arrays
			if (camPoses.Length==0) return;

			//displaying debug range
			if (guiDebug && !isEditor) 
				for (int c=0; c<camPoses.Length; c++) 
				{
					transform.TransformPoint(camPoses[c]).DrawDebug(generateRange, Color.yellow);
					transform.TransformPoint(camPoses[c]).DrawDebug(enableRange, Color.green);
					transform.TransformPoint(camPoses[c]).DrawDebug(removeRange, Color.red);
				}

			//do nothing if chink size is zero
			if (terrainSize < 0.1f) return;

			//deploying terrain matrix
			if (!isEditor && generateInfinite) terrains.Deploy(camPoses, allowMove:true);

			//calculating number of running threads
			runningThreadsCount = 0;
			foreach (Chunk tw in terrains.Objects())
				if (tw.running) runningThreadsCount++; 

			//finding distance to each of the chunks
			int chunksNum = terrains.Count;
			if (chunksArray==null || chunksArray.Length!=chunksNum) chunksArray = new Chunk[chunksNum];
			if (distsArray==null || distsArray.Length!=chunksNum) distsArray = new float[chunksNum];

			int counter = 0;
			foreach (Chunk chunk in terrains.Objects()) 
			{
				Vector3 chunkPos = new Vector3(chunk.coord.x*MapMagic.instance.terrainSize, 0, chunk.coord.z*MapMagic.instance.terrainSize); // + transform.position; cam pos is relative to transform pos
				float minDist = 200000000;
				for (int c=0; c<camPoses.Length; c++)
				{
					float dist = camPoses[c].RectangularDistToRect(chunkPos, MapMagic.instance.terrainSize);
					if (dist < minDist) minDist = dist;
				}

				//for (int i=0; i<chunksArray.Length; i++) if (chunksArray[i]==chunk) Debug.Log("Chunk is in the list");
				chunksArray[counter] = chunk;
				distsArray[counter] = minDist;
				counter++;
			}

			//sorting chunks according to distance from cameras
			Array.Sort(distsArray, chunksArray);

			//updating chunks - from cameras to the edge
			for (int i=0; i<chunksArray.Length; i++)
				chunksArray[i].Update(distsArray[i]);

			//resetting prepare flag
			generatorsPrepared = false;
		}

		public void PrepareGenerators ()
		{
			if (generatorsPrepared) return;

			foreach (TextureInput tin in gens.GeneratorsOfType<TextureInput>()) tin.CheckLoadTexture();
			//more will follow

			generatorsPrepared = true;
		}

		public Vector3[] GetCamPoses (Vector3[] camPoses=null)
		{
			if (isEditor) 
			{
				#if UNITY_EDITOR
				if (UnityEditor.SceneView.lastActiveSceneView==null || UnityEditor.SceneView.lastActiveSceneView.camera==null) return new Vector3[0];
				if (camPoses==null || camPoses.Length!=1) camPoses = new Vector3[1];
				camPoses[0] = UnityEditor.SceneView.lastActiveSceneView.camera.transform.position;
				#else
				camPoses = new Vector3[1];
				#endif
			}
			else
			{
				//finding objects with tag
				GameObject[] taggedObjects = null;
				if (genAroundObjsTag && genAroundTag!=null && genAroundTag.Length!=0) taggedObjects = GameObject.FindGameObjectsWithTag(genAroundTag);

				//calculating cams array length and rescaling it
				int camPosesLength = 0;
				if (genAroundMainCam) camPosesLength++;
				if (taggedObjects !=null) camPosesLength += taggedObjects.Length;
				
				if (camPosesLength == 0) { Debug.LogError("No Main Camera to deploy MapMagic"); return new Vector3[0]; }
				if (camPoses == null || camPosesLength != camPoses.Length) camPoses = new Vector3[camPosesLength];
				
				//filling cams array
				int counter = 0;
				if (genAroundMainCam) 
				{
					Camera mainCam = Camera.main;
					if (mainCam==null) mainCam = FindObjectOfType<Camera>(); //in case it was destroyed or something
					camPoses[0] = mainCam.transform.position;
					counter++;
				}
				if (taggedObjects != null)
					for (int i=0; i<taggedObjects.Length; i++) camPoses[i+counter] = taggedObjects[i].transform.position;
			}

			//transforming cameras position to local
			for (int c=0; c<camPoses.Length; c++) camPoses[c] = transform.InverseTransformPoint(camPoses[c]);
			
			return camPoses;		
		}


		public void ForceGenerate ()
		{
			foreach (Chunk tw in terrains.Objects()) 
			{
				tw.stop = true;
				tw.results.Clear();
				tw.ready.Clear();
				//tw.applyHeight = false;
			}

			applyRunning = false;
			terrains.start=true; 
			Update(); 
		}

		public void Generate ()
		{
			terrains.start=true; 
			Update(); 
		}


		#region Outdated
		[System.Serializable]
			public class GeneratorsList //one class is easier to serialize than multiple arrays
			{
				public Generator[] list = new Generator[0];
				public Generator[] outputs = new Generator[0];
			}

			public Serializer serializer = null;

			public void LoadOldNonAssetData ()
			{
				serializer.ClearLinks();
				GeneratorsList generators = new GeneratorsList();
				generators = (GeneratorsList)serializer.Retrieve(0);
				serializer.ClearLinks();

				gens = ScriptableObject.CreateInstance<GeneratorsAsset>();
				gens.list = generators.list;
				//gens.outputs = generators.outputs; 
			}
		#endregion

	}//class

}//namespace