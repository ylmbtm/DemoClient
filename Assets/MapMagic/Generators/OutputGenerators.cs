using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//using Plugins;

namespace MapMagic
{
	

	[System.Serializable]
	[GeneratorMenu (menu="Output", name ="Height", disengageable = true)]
	public class HeightOutput : Generator, Generator.IOutput
	{
		public Input input = new Input(InoutType.Map);
		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Input> Inputs() { yield return input; }
		public override IEnumerable<Output> Outputs() { if (output==null) output = new Output(InoutType.Map);  yield return output; }

		public float layer { get; set; }


		public static void Process (Chunk chunk)
		{
			if (chunk.stop) return;

			//debug timer
			if (MapMagic.instance.guiDebug)
			{
				if (chunk.timer==null) chunk.timer = new System.Diagnostics.Stopwatch(); 
				else chunk.timer.Reset();
				chunk.timer.Start();
			}

			//reading height outputs
			if (chunk.heights==null || chunk.heights.rect.size.x!=MapMagic.instance.resolution) chunk.heights = chunk.defaultMatrix;
			if (chunk.heights.rect != chunk.defaultRect) chunk.heights.Resize(chunk.defaultRect);
			chunk.heights.Clear();
			
			//processing main height
			foreach (HeightOutput gen in MapMagic.instance.gens.GeneratorsOfType<HeightOutput>(onlyEnabled:true, checkBiomes:true))
			{
				if (chunk.stop) return;

				//loading inputs
				Matrix heights = (Matrix)gen.input.GetObject(chunk);
				if (heights == null) continue;

				//loading biome matrix
				Matrix biomeMask = null;
				if (gen.biome != null) 
				{
					object biomeMaskObj = gen.biome.mask.GetObject(chunk);
					if (biomeMaskObj==null) continue; //adding nothing if biome has no mask
					biomeMask = (Matrix)biomeMaskObj;
					if (biomeMask == null) continue; 
					if (biomeMask.IsEmpty()) continue; //optimizing empty biomes
				}

				//adding to final result
				if (gen.biome==null) chunk.heights.Add(heights);
				else if (biomeMask!=null) chunk.heights.Add(heights, biomeMask);
			}

			//creating 2d array //TODO: apply scale here with a blur
			if (chunk.stop) return;
			int scale = MapMagic.instance.heightScale;
			int heightSize = MapMagic.instance.resolution*scale + 1;
			float[,] heights2D = new float[heightSize, heightSize];
			for (int x=0; x<heightSize-1; x++)
				for (int z=0; z<heightSize-1; z++)
			{
				if (scale==1) heights2D[z,x] += chunk.heights[x+chunk.heights.rect.offset.x, z+chunk.heights.rect.offset.z];
				else
				{
					float fx = 1f*x/scale; float fz = 1f*z/scale;
					heights2D[z,x] = chunk.heights.GetInterpolated(fx+chunk.heights.rect.offset.x, fz+chunk.heights.rect.offset.z);
				}
			}

			//processing sides
			for (int x=0; x<heightSize; x++) 
			{
				float prevVal = heights2D[heightSize-3, x]; //size-2
				float currVal = heights2D[heightSize-2, x]; //size-1, point on border
				float nextVal = currVal - (prevVal-currVal);
				heights2D[heightSize-1,x] = nextVal;
			}
			for (int z=0; z<heightSize; z++)
			{
				float prevVal = heights2D[z, heightSize-3]; //size-2
				float currVal = heights2D[z, heightSize-2]; //size-1, point on border
				float nextVal = currVal - (prevVal-currVal);
				heights2D[z,heightSize-1] = nextVal;
			}
			heights2D[heightSize-1,heightSize-1] = heights2D[heightSize-1, heightSize-2];

			//blurring if scale is more than 1
			if (scale>1)
			{
				for (int x=2; x<heightSize-1; x+=2)
					for (int z=2; z<heightSize-1; z+=2)
						heights2D[x,z] = heights2D[x,z]/2 + (heights2D[x+1,z] + heights2D[x,z+1] + heights2D[x-1,z] + heights2D[x,z-1])/8;
			}

			//blurring bounds for better welding
			/*for (int z=1; z<heightSize-1; z++)
			{
				heights2D[z,0] = (heights2D[z-1,0]+heights2D[z+1,0])/2;
				heights2D[z,heightSize-1] = (heights2D[z-1,heightSize-1]+heights2D[z+1,heightSize-1])/2;
			}

			for (int x=1; x<heightSize-1; x++)
			{
				heights2D[0,x] = (heights2D[0,x-1]+heights2D[0,x+1])/2;
				heights2D[heightSize-1,x] = (heights2D[heightSize-1,x-1]+heights2D[heightSize-1,x+1])/2;
			}*/

			//pushing to apply
			if (chunk.stop) return;
			chunk.apply.CheckAdd(typeof(HeightOutput), heights2D, replace: true);
			
			//debug timer
			if (chunk.timer != null) 
			{ 
				chunk.timer.Stop(); 
				MapMagic.instance.guiDebugProcessTimes.CheckAdd(typeof(HeightOutput), chunk.timer.ElapsedMilliseconds, replace:true); 
			}
		}

		public static void Purge (Chunk tw)
		{
			if (tw.locked) return; 
			//if (tw.terrain.terrainData.heightmapResolution == 33) return false; //already cleared
			
			float[,] heights2D = new float[33,33]; //already locked in update
			tw.terrain.terrainData.heightmapResolution = heights2D.GetLength(0);
			tw.terrain.terrainData.SetHeights(0,0,heights2D);
			tw.terrain.terrainData.size = new Vector3(MapMagic.instance.terrainSize, MapMagic.instance.terrainHeight, MapMagic.instance.terrainSize);
			tw.SetNeighbors();

			if (MapMagic.instance.guiDebug) Debug.Log("Heights Purged");
		}

		public static IEnumerator Apply (Chunk chunk)
		{
			//debug timer
			if (MapMagic.instance.guiDebug)
			{
				if (chunk.timer==null) chunk.timer = new System.Diagnostics.Stopwatch(); 
				else chunk.timer.Reset();
				chunk.timer.Start();
			}

			float[,] heights2D = (float[,])chunk.apply[typeof(HeightOutput)];

			//quick lod apply
			/*if (chunk.lod)
			{
				//if (chunk.lodTerrain == null) { chunk.lodTerrain = (MapMagic.instance.transform.AddChild("Terrain " + chunk.coord.x + "," + chunk.coord.z + " LOD")).gameObject.AddComponent<Terrain>(); chunk.lodTerrain.terrainData = new TerrainData(); }
				if (chunk.lodTerrain.terrainData==null) chunk.lodTerrain.terrainData = new TerrainData();

				chunk.lodTerrain.Resize(heights2D.GetLength(0), new Vector3(MapMagic.instance.terrainSize, MapMagic.instance.terrainHeight, MapMagic.instance.terrainSize));
				chunk.lodTerrain.terrainData.SetHeightsDelayLOD(0,0,heights2D);
				
				yield break;
			}*/

			//determining data
			//TerrainData data = null;
			//if (chunk.terrain.terrainData.heightmapResolution == heights2D.GetLength(0)) data = chunk.terrain.terrainData;
			//else data = UnityEngine.Object.Instantiate(chunk.terrain.terrainData);
			TerrainData data = chunk.terrain.terrainData;
			
			//resizing terrain
			Vector3 terrainSize = new Vector3(MapMagic.instance.terrainSize, MapMagic.instance.terrainHeight, MapMagic.instance.terrainSize);
			int terrainResolution = heights2D.GetLength(0); //heights2D[0].GetLength(0);
			if ((data.size-terrainSize).sqrMagnitude > 0.01f || data.heightmapResolution != terrainResolution) 
			{
				if (terrainResolution <= 64) //brute force
				{
					data.heightmapResolution = terrainResolution;
					data.size = new Vector3(terrainSize.x, terrainSize.y, terrainSize.z);
				}

				else //setting res 64, re-scaling to 1/64, and then changing res
				{
					data.heightmapResolution = 65;
					chunk.terrain.Flush(); //otherwise unity crushes without an error
					int resFactor = (terrainResolution-1) / 64;
					data.size = new Vector3(terrainSize.x/resFactor, terrainSize.y, terrainSize.z/resFactor);
					data.heightmapResolution = terrainResolution;
				}
			}
			yield return null;

			chunk.terrain.SetNeighbors( null, null, null, null );

			//welding
			TerrainGrid terrains = MapMagic.instance.terrains;
			WeldTerrains.WeldHeights(heights2D, 
				terrains.GetTerrain(chunk.coord.x-1, chunk.coord.z), 
				terrains.GetTerrain(chunk.coord.x, chunk.coord.z+1),
				terrains.GetTerrain(chunk.coord.x+1, chunk.coord.z),
				terrains.GetTerrain(chunk.coord.x, chunk.coord.z-1),
				MapMagic.instance.heightWeldMargins);
			yield return null;

			data.SetHeightsDelayLOD(0,0,heights2D);
			yield return null;
			
			/* sliced
			int sliceCount = heights2D.Length; int sliceSize = heights2D[0].GetLength(1); int heightSize = heights2D[0].GetLength(0);
			for (int c=0; c<sliceCount; c++) 
			{
				wrapper.terrain.terrainData.SetHeightsDelayLOD(sliceSize*c, 0, heights2D[c]);
				yield return null;
			}
			*/

			chunk.terrain.ApplyDelayedHeightmapModification();
			chunk.terrain.Flush();

			//setting neigs - no matter if the terrains were complete or not
			/*chunk.terrain.SetNeighbors( //terrainPrevX, terrainNextZ, terrainNextX, terrainPrevZ);
				terrains.GetTerrain(chunk.coord.x-1, chunk.coord.z, onlyComplete:false),
				terrains.GetTerrain(chunk.coord.x, chunk.coord.z+1, onlyComplete:false),
				terrains.GetTerrain(chunk.coord.x+1, chunk.coord.z, onlyComplete:false),
				terrains.GetTerrain(chunk.coord.x, chunk.coord.z-1, onlyComplete:false) );*/
			
			chunk.SetNeighbors(force:true);
			//if (terrains[chunk.coord.x-1, chunk.coord.z] != null) terrains[chunk.coord.x-1, chunk.coord.z].SetNeighbors();
			//if (terrains[chunk.coord.x, chunk.coord.z+1] != null) terrains[chunk.coord.x, chunk.coord.z+1].SetNeighbors();
			//if (terrains[chunk.coord.x+1, chunk.coord.z] != null) terrains[chunk.coord.x+1, chunk.coord.z].SetNeighbors();
			//if (terrains[chunk.coord.x, chunk.coord.z-1] != null) terrains[chunk.coord.x, chunk.coord.z-1].SetNeighbors();

			//debug timer
			if (chunk.timer != null) 
			{ 
				chunk.timer.Stop(); 
				MapMagic.instance.guiDebugApplyTimes.CheckAdd(typeof(HeightOutput), chunk.timer.ElapsedMilliseconds, replace:true); 
				Debug.Log("Heights Applied");
			}

			yield return null;
		}
		
		public override void OnGUI ()
		{
			layout.Par(20); input.DrawIcon(layout, "Height");
			layout.Par(5);

			if (output == null) output = new Output(InoutType.Map);

			layout.Field(ref MapMagic.instance.heightScale, "Scale", min:1, max:4f); 
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Output", name ="Textures", disengageable = true)]
	public class SplatOutput : Generator, Generator.IOutput, Layout.ILayered
	{
		//layer
		public class Layer : Layout.ILayer
		{
			public Input input = new Input(InoutType.Map);
			public Output output = new Output(InoutType.Map);
			public string name = "Layer";
			public float opacity = 1;
			public SplatPrototype splat = new SplatPrototype();

			public bool pinned { get; set; }
			public int guiHeight { get; set; }

			public void OnCollapsedGUI (Layout layout) 
			{
				layout.margin = 20; layout.rightMargin = 20; layout.fieldSize = 1f;
				layout.Par(20); 
				if (!pinned) input.DrawIcon(layout);
				layout.Label(name, rect:layout.Inset());
				output.DrawIcon(layout);
			}

			public void OnExtendedGUI (Layout layout) 
			{
				layout.margin = 20; layout.rightMargin = 20;
				layout.Par(20); 

				if (!pinned) input.DrawIcon(layout);
				layout.Field(ref name, rect:layout.Inset());
				output.DrawIcon(layout);

				layout.Par(2);
				layout.Par(60); //not 65
				splat.texture = layout.Field(splat.texture, rect:layout.Inset(60)); 
				splat.normalMap = layout.Field(splat.normalMap, rect:layout.Inset(60));
				layout.Par(2);

				layout.margin = 5; layout.rightMargin = 5; layout.fieldSize = 0.6f;
				//layout.SmartField(ref downscale, "Downscale", min:1, max:8); downscale = Mathf.ClosestPowerOfTwo(downscale);
				opacity = layout.Field(opacity, "Opacity", min:0);
				splat.tileSize = layout.Field(splat.tileSize, "Size");
				splat.tileOffset = layout.Field(splat.tileOffset, "Offset");
				splat.specular = layout.Field(splat.specular, "Specular");
				splat.smoothness = layout.Field(splat.smoothness, "Smooth", max:1);
				splat.metallic = layout.Field(splat.metallic, "Metallic", max:1);
			}

			public void OnAdd (int n) { splat = new SplatPrototype() { texture=defaultTex }; }
			public void OnRemove (int n) 
			{ 
				input.Link(null,null); 
				Input connectedInput = output.GetConnectedInput(MapMagic.instance.gens.list);
				if (connectedInput != null) connectedInput.Link(null, null);
			}
			public void OnSwitch (int o, int n) { }
		}
		public Layer[] baseLayers = new Layer[] { new Layer(){pinned=true, name="Background"} };
		public virtual Layout.ILayer[] layers 
		{ 
			get {return baseLayers;} 
			set {baseLayers=ArrayTools.Convert<Layer,Layout.ILayer>(value);} 
		}

		public int selected { get; set; }
		public int collapsedHeight { get; set; }
		public int extendedHeight { get; set; }
		public Layout.ILayer def {get{ return new Layer() {splat=new SplatPrototype() { texture=defaultTex} };	}}
		
		public static Texture2D _defaultTex;
		public static Texture2D defaultTex {get{ if (_defaultTex==null) _defaultTex=Extensions.ColorTexture(2,2,new Color(0.5f, 0.5f, 0.5f, 0f)); return _defaultTex; }}

		public class SplatsTuple { public float[,,] array; public SplatPrototype[] prototypes; }

		//generator
		public override IEnumerable<Input> Inputs() 
		{ 
			if (baseLayers==null) baseLayers = new Layer[0];
			for (int i=0; i<baseLayers.Length; i++) 
				if (baseLayers[i].input != null)
					yield return baseLayers[i].input; 
		}
		public override IEnumerable<Output> Outputs() 
		{ 
			if (baseLayers==null) baseLayers = new Layer[0];
			for (int i=0; i<baseLayers.Length; i++) 
				if (baseLayers[i].output != null)
					yield return baseLayers[i].output; 
		}

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			if (chunk.stop || !enabled) return;
			
			//loading inputs
			Matrix[] matrices = new Matrix[baseLayers.Length];
			for (int i=0; i<baseLayers.Length; i++)
			{
				if (baseLayers[i].input != null) 
				{
					matrices[i] = (Matrix)baseLayers[i].input.GetObject(chunk);
					if (matrices[i] != null) matrices[i] = matrices[i].Copy(null);
				}
				if (matrices[i] == null) matrices[i] = chunk.defaultMatrix;
			}

			//background matrix
			//matrices[0] = terrain.defaultMatrix; //already created
			matrices[0].Fill(1);
			
			//populating opacity array
			float[] opacities = new float[matrices.Length];
			for (int i=0; i<baseLayers.Length; i++)
				opacities[i] = baseLayers[i].opacity;
			opacities[0] = 1;

			//multiplying on biomes masks
			/*if (biome != null)
			{
				Matrix biomeMask = (Matrix)biome.mask.GetObject(chunk);
				if (biomeMask != null) 
					for (int i=0; i<matrices.Length; i++) matrices[i].Multiply(biomeMask);
			}*/

			//blending layers
			Matrix.BlendLayers(matrices, opacities);

			//saving changed matrix results
			for (int i=0; i<baseLayers.Length; i++) 
			{
				if (chunk.stop) return; //do not write object is generating is stopped
				baseLayers[i].output.SetObject(chunk, matrices[i]);
			}
		}

		public static void Process (Chunk chunk)
		{
			if (chunk.stop) return;

			//debug timer
			if (MapMagic.instance.guiDebug)
			{
				if (chunk.timer==null) chunk.timer = new System.Diagnostics.Stopwatch(); 
				else chunk.timer.Reset();
				chunk.timer.Start();
			}
			
			//gathering prototypes and matrices lists
			List<SplatPrototype> prototypesList = new List<SplatPrototype>();
			List<float> opacities = new List<float>();
			List<Matrix> matrices = new List<Matrix>();
			List<Matrix> biomeMasks = new List<Matrix>();
			
			foreach (SplatOutput gen in MapMagic.instance.gens.GeneratorsOfType<SplatOutput>(onlyEnabled:true, checkBiomes:true))
			{
				//loading biome matrix
				Matrix biomeMask = null;
				if (gen.biome != null) 
				{
					object biomeMaskObj = gen.biome.mask.GetObject(chunk);
					if (biomeMaskObj==null) continue; //adding nothing if biome has no mask
					biomeMask = (Matrix)biomeMaskObj;
					if (biomeMask == null) continue; 
					if (biomeMask.IsEmpty()) continue; //optimizing empty biomes
				}

				for (int i=0; i<gen.baseLayers.Length; i++)
				{
					//reading output directly
					Output output = gen.baseLayers[i].output;
					if (chunk.stop) return; //checking stop before reading output
					if (!chunk.results.ContainsKey(output)) continue;
					Matrix matrix =  (Matrix)chunk.results[output];
					
					//adding to lists
					matrices.Add(matrix);
					biomeMasks.Add(gen.biome==null? null:biomeMask);
					prototypesList.Add(gen.baseLayers[i].splat);
					opacities.Add(gen.baseLayers[i].opacity);
				}
			}

			//optimizing matrices list if they are not used
			for (int i=matrices.Count-1; i>=0; i--)
				if (opacities[i]<0.001f || matrices[i].IsEmpty() || (biomeMasks[i]!=null && biomeMasks[i].IsEmpty())) 
					{ prototypesList.RemoveAt(i); opacities.RemoveAt(i); matrices.RemoveAt(i); biomeMasks.RemoveAt(i); }

			//creating array
			float[,,] splats3D = new float[MapMagic.instance.resolution, MapMagic.instance.resolution, prototypesList.Count];
			if (matrices.Count==0) { chunk.apply.CheckAdd(typeof(SplatOutput), new SplatsTuple() {array=splats3D, prototypes=new SplatPrototype[0]}, replace:true); return; }

			//filling array
			if (chunk.stop) return;
			
			int numLayers = matrices.Count; 
			int maxX = splats3D.GetLength(0); int maxZ = splats3D.GetLength(1); //MapMagic.instance.resolution should not be used because of possible lods
			CoordRect rect =  matrices[0].rect;

			float[] values = new float[numLayers]; //row, to avoid reading/writing 3d array (it is too slow)
			
			for (int x=0; x<maxX; x++)
				for (int z=0; z<maxZ; z++)
			{
				int pos = rect.GetPos(x+rect.offset.x, z+rect.offset.z);
				float sum = 0;

				//getting values
				for (int i=0; i<numLayers; i++)
				{
					float val = matrices[i].array[pos];
					if (biomeMasks[i] != null) val *= biomeMasks[i].array[pos];
					sum += val; //normalizing: calculating sum
					values[i] = val;
				}

				//setting color
				for (int i=0; i<numLayers; i++) splats3D[z,x,i] = values[i] / sum;
			}

			//pushing to apply
			if (chunk.stop) return;
			SplatsTuple splatsTuple = new SplatsTuple() { array=splats3D, prototypes=prototypesList.ToArray() };
			chunk.apply.CheckAdd(typeof(SplatOutput), splatsTuple, replace:true);

			//debug timer
			if (chunk.timer != null) 
			{ 
				chunk.timer.Stop(); 
				MapMagic.instance.guiDebugProcessTimes.CheckAdd(typeof(SplatOutput), chunk.timer.ElapsedMilliseconds, replace:true); 
			}
		}

		public static IEnumerator Apply (Chunk chunk)
		{
			//debug timer
			if (MapMagic.instance.guiDebug)
			{
				if (chunk.timer==null) chunk.timer = new System.Diagnostics.Stopwatch(); 
				else chunk.timer.Reset();
				chunk.timer.Start();
			}

			SplatsTuple splatsTuple = (SplatsTuple)chunk.apply[typeof(SplatOutput)];
			float[,,] splats3D = splatsTuple.array;
			SplatPrototype[] prototypes = splatsTuple.prototypes;

			if (splats3D.GetLength(2)==0) { chunk.ClearSplats(); yield break; }

			TerrainData data = chunk.terrain.terrainData;

			//setting resolution
			int size = splats3D.GetLength(0);
			if (data.alphamapResolution != size) data.alphamapResolution = size;

			//checking prototypes texture
			for (int i=0; i<prototypes.Length; i++)
				if (prototypes[i].texture == null) prototypes[i].texture = defaultTex;

			//welding
			Chunk chunkPrevX = MapMagic.instance.terrains[chunk.coord.x-1, chunk.coord.z]; 
			Chunk chunkNextX = MapMagic.instance.terrains[chunk.coord.x+1, chunk.coord.z]; 
			Chunk chunkPrevZ = MapMagic.instance.terrains[chunk.coord.x, chunk.coord.z-1]; 
			Chunk chunkNextZ = MapMagic.instance.terrains[chunk.coord.x, chunk.coord.z+1]; 

			Terrain terrainPrevX = (chunkPrevX!=null && chunkPrevX.complete)? chunkPrevX.terrain : null;
			Terrain	terrainNextX = (chunkNextX!=null && chunkNextX.complete)? chunkNextX.terrain : null;
			Terrain terrainPrevZ = (chunkPrevZ!=null && chunkPrevZ.complete)? chunkPrevZ.terrain : null;
			Terrain terrainNextZ = (chunkNextZ!=null && chunkNextZ.complete)? chunkNextZ.terrain : null;

			WeldTerrains.WeldSplats(splats3D, terrainPrevX, terrainNextZ, terrainNextX, terrainPrevZ, MapMagic.instance.splatsWeldMargins);

			//setting
			data.splatPrototypes = prototypes;
			data.SetAlphamaps(0,0,splats3D);

			//debug timer
			if (chunk.timer != null) 
			{ 
				chunk.timer.Stop(); 
				MapMagic.instance.guiDebugApplyTimes.CheckAdd(typeof(SplatOutput), chunk.timer.ElapsedMilliseconds, replace:true); 
			}
			if (MapMagic.instance.guiDebug) Debug.Log("Splats Applied");

			yield return null;
		}

		public static void Purge (Chunk chunk)
		{
			if (chunk.locked) return;

			SplatPrototype[] prototypes = new SplatPrototype[1];
			if (prototypes[0]==null) prototypes[0] = new SplatPrototype();
			if (prototypes[0].texture==null) prototypes[0].texture = defaultTex;
			chunk.terrain.terrainData.splatPrototypes = prototypes;
		
			float[,,] emptySplats = new float[16,16,1];
			for (int x=0; x<16; x++)
				for (int z=0; z<16; z++)
					emptySplats[z,x,0] = 1;

			chunk.terrain.terrainData.alphamapResolution = 16;
			chunk.terrain.terrainData.SetAlphamaps(0,0,emptySplats);

			if (MapMagic.instance.guiDebug) Debug.Log("Splats Purged");
		}

		public override void OnGUI () 
		{
			layout.DrawLayered(this, "Layers:");
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Output", name ="Objects", disengageable = true)]
	public class ObjectOutput : Generator, Generator.IOutput, Layout.ILayered
	{
		//layer
		public class Layer : Layout.ILayer
		{
			public Input input = new Input(InoutType.Objects);
			
			public Transform prefab;
			public bool relativeHeight = true;
			public bool rotate = true;
			public bool scale = true;
			public bool scaleY;
			public bool usePool = true;
			public bool parentToRoot = false;

			//public bool processChildren = false;
			//public bool floorChildren;
			//public Vector2 rotateChildren;
			//public Vector2 scaleChildren;
			//public float removeChildren = 0;
			
			public bool pinned { get; set; }
			public int guiHeight { get; set; }

			public void OnCollapsedGUI (Layout layout) 
			{
				layout.margin = 20; layout.rightMargin = 5; layout.fieldSize = 1f;
				layout.Par(20); 
				input.DrawIcon(layout);
				layout.Field(ref prefab, rect:layout.Inset());
			}

			public void OnExtendedGUI (Layout layout) 
			{
				layout.margin = 20; layout.rightMargin = 5;
				layout.Par(20); 

				input.DrawIcon(layout);
				layout.Field(ref prefab, rect:layout.Inset());

				layout.Par(); layout.Toggle(ref relativeHeight, rect:layout.Inset(20)); layout.Label("Relative Height", rect:layout.Inset(100)); 
				layout.Par(); layout.Toggle(ref rotate, rect:layout.Inset(20)); layout.Label("Rotate", rect:layout.Inset(45)); 
				layout.Par(); layout.Toggle(ref scale, rect:layout.Inset(20)); layout.Label("Scale", rect:layout.Inset(40)); //if (layout.lastChange) scaleY = false;
				layout.disabled = !scale;
				layout.Toggle(ref scaleY, rect:layout.Inset(18)); layout.Label("Y only", rect:layout.Inset(45)); //if (layout.lastChange) scaleU = false;
				layout.disabled = false;
				layout.Toggle(ref usePool, "Use Object Pool");

				//layout.Par(); layout.Field(ref processChildren, width:20); layout.Label("Process Children"); 
				//if (processChildren)
				//{
				//	layout.margin += 10;
				//	layout.ComplexField(ref floorChildren, "Floor");
				//	layout.SmartField(ref rotateChildren, "Rotate", min:-360, max:360);
				//	layout.SmartField(ref scaleChildren, "Scale", min:-2, max:2);
				//	layout.SmartField(ref removeChildren, "Delete", max:1);
				//	layout.margin -= 10;
				//	layout.Par(3);
				//}
			}

			public void OnAdd (int n) {  }
			public void OnRemove (int n) { input.Link(null,null); }
			public void OnSwitch (int o, int n) { }
		}
		public Layer[] baseLayers = new Layer[0];
		public Layout.ILayer[] layers 
		{ 
			get {return baseLayers;} 
			set {baseLayers=ArrayTools.Convert<Layer,Layout.ILayer>(value);} 
		}

		public int selected { get; set; }
		public int collapsedHeight { get; set; }
		public int extendedHeight { get; set; }
		public Layout.ILayer def {get{ return new Layer(); }}

		public class ObjectsTuple { public List<TransformPool.InstanceDraft[]> instances; public List<Layer> layers; }


		//generator
		public override IEnumerable<Input> Inputs() 
		{ 
			if (baseLayers==null) baseLayers = new Layer[0];
			for (int i=0; i<baseLayers.Length; i++) 
				if (baseLayers[i].input != null)
					yield return baseLayers[i].input; 
		}

		public static void Process (Chunk chunk)
		{
			if (chunk.stop) return;

			//debug timer
			if (MapMagic.instance.guiDebug)
			{
				if (chunk.timer==null) chunk.timer = new System.Diagnostics.Stopwatch(); 
				else chunk.timer.Reset();
				chunk.timer.Start();
			}

			//preparing output
			List<TransformPool.InstanceDraft[]> instancesList = new List<TransformPool.InstanceDraft[]>();
			List<Layer> layersList = new List<Layer>();

			//InstanceRandom rnd = new InstanceRandom(MapMagic.instance.seed + 12345 + chunk.coord.x*1000 + chunk.coord.z); //to disable objects according biome masks

			//filling array
			foreach (ObjectOutput gen in MapMagic.instance.gens.GeneratorsOfType<ObjectOutput>(onlyEnabled:true, checkBiomes:true))
			{
				//loading biome matrix
				Matrix biomeMask = null;
				if (gen.biome != null) 
				{
					object biomeMaskObj = gen.biome.mask.GetObject(chunk);
					if (biomeMaskObj==null) continue; //adding nothing if biome has no mask
					biomeMask = (Matrix)biomeMaskObj;
					if (biomeMask == null) continue; 
					if (biomeMask.IsEmpty()) continue; //optimizing empty biomes
				}
				
				for (int i=0; i<gen.baseLayers.Length; i++)
				{
					if (chunk.stop) return; //checking stop before reading output
					Layer layer = gen.baseLayers[i];
					if (layer.prefab == null) continue;
					
					//loading objects from input
					SpatialHash hash = (SpatialHash)gen.baseLayers[i].input.GetObject(chunk);
					if (hash == null) continue;

					//adding prototype
					layersList.Add(layer);

					//creating instances
					//TransformPool.InstanceDraft[] instances = new TransformPool.InstanceDraft[hash.Count];
					List<TransformPool.InstanceDraft> instances = new List<TransformPool.InstanceDraft>();
					
					//filling instances
					foreach (SpatialObject obj in hash.AllObjs())
					{
						//disabling object using biome mask
						if (gen.biome != null)
						{
							if (biomeMask == null || biomeMask[obj.pos] < 0.5f) continue;
							//if (biomeMask[obj.pos] < rnd.CoordinateRandom((int)obj.pos.x, (int)obj.pos.y)) continue;
						}

						float terrainHeight = 0;
						if (layer.relativeHeight && chunk.heights!=null) //if checbox enabled and heights exist (at least one height generator is in the graph)
							terrainHeight = chunk.heights.GetInterpolated(obj.pos.x, obj.pos.y);
						if (terrainHeight > 1) terrainHeight = 1;

						Vector3 pos = new Vector3(obj.pos.x, 0, obj.pos.y);
						pos = pos / hash.size * MapMagic.instance.terrainSize;
						pos.y = (obj.height + terrainHeight) * MapMagic.instance.terrainHeight;
						if (!layer.parentToRoot) pos -= chunk.coord.vector3*MapMagic.instance.terrainSize; //local to parent terrain

						Quaternion rot = layer.rotate? (obj.rotation%360).EulerToQuat() : Quaternion.identity;
						Vector3 scl = layer.scale? new Vector3(layer.scaleY?1:obj.size, obj.size, layer.scaleY?1:obj.size) : Vector3.one;

						//instances[objCounter] = new TransformPool.InstanceDraft() { pos=pos, rotation=rot, scale=scl };
						//objCounter++;
						instances.Add( new TransformPool.InstanceDraft() { pos=pos, rotation=rot, scale=scl } );
					}

					instancesList.Add(instances.ToArray());
				}
			}
			
			//queue apply
			if (chunk.stop) return;
			ObjectsTuple objectsTuple = new ObjectsTuple() { instances=instancesList, layers=layersList };
			chunk.apply.CheckAdd(typeof(ObjectOutput), objectsTuple, replace: true);

			//debug timer
			if (chunk.timer != null) 
			{ 
				chunk.timer.Stop(); 
				MapMagic.instance.guiDebugProcessTimes.CheckAdd(typeof(ObjectOutput), chunk.timer.ElapsedMilliseconds, replace:true); 
			}
		}

		public static IEnumerator Apply (Chunk chunk)
		{
			//debug timer
			if (MapMagic.instance.guiDebug)
			{
				if (chunk.timer==null) chunk.timer = new System.Diagnostics.Stopwatch(); 
				else chunk.timer.Reset();
				chunk.timer.Start();
			}

			//loading instances array
			ObjectsTuple objectsTuple = (ObjectsTuple)chunk.apply[typeof(ObjectOutput)];
			List<TransformPool.InstanceDraft[]> instancesList = objectsTuple.instances;
			List<Layer> layersList = objectsTuple.layers;
			int layersCount = layersList.Count;

			//pools operations
			//creating or re-creating pools
			if (chunk.pools == null) chunk.pools = new TransformPool[layersCount];

			//removing extra pools with objects
			if (chunk.pools.Length > layersCount) //TODO: simplify with array ops
			{
				TransformPool[] newPools = new TransformPool[layersCount];
				for (int i=0; i<layersCount; i++) newPools[i] = chunk.pools[i];
				for (int i=layersCount; i<chunk.pools.Length; i++) chunk.pools[i].Clear();
				chunk.pools = newPools;
			}

			//adding lacking pools
			if (chunk.pools.Length < layersCount)
			{
				TransformPool[] newPools = new TransformPool[layersCount];
				for (int i=0; i<chunk.pools.Length; i++) newPools[i] = chunk.pools[i];
				chunk.pools = newPools;
			}

			//updating pools
			for (int i=0; i<layersCount; i++) 
			{
				//filling nulls with new pools
				if (chunk.pools[i] == null) chunk.pools[i] = new TransformPool() { prefab=layersList[i].prefab, parent=chunk.terrain.transform };

				//comparing pool prefabs with current prefabs
				if (chunk.pools[i].prefab != layersList[i].prefab)
				{
					chunk.pools[i].Clear();
					chunk.pools[i].prefab = layersList[i].prefab;
				}

				//other params
				chunk.pools[i].allowReposition = layersList[i].usePool;
				chunk.pools[i].parent = !layersList[i].parentToRoot? chunk.terrain.transform : null;
			}

			//instantiating
			for (int i=0; i<layersCount; i++)
			{
				if (layersList[i].prefab == null) { chunk.pools[i].Clear(); continue; }

				IEnumerator e = chunk.pools[i].SetTransformsCoroutine(instancesList[i]);
				while (e.MoveNext()) yield return null;
			}

			//debug timer
			if (chunk.timer != null) 
			{ 
				chunk.timer.Stop(); 
				MapMagic.instance.guiDebugApplyTimes.CheckAdd(typeof(ObjectOutput), chunk.timer.ElapsedMilliseconds, replace:true); 
				Debug.Log("Objects Applied");
			}

			yield return null;
		}

		public static void Purge (Chunk chunk)
		{
			if (chunk.locked || chunk.pools==null) return;
			for (int i=0; i<chunk.pools.Length; i++)
				chunk.pools[i].Clear();
			//if (MapMagic.instance.guiDebug) Debug.Log("Objects Cleared");
		}

		public override void OnGUI ()
		{
			layout.DrawLayered(this, "Layers:");
		}

	}
	
	[System.Serializable]
	[GeneratorMenu (menu="Output", name ="Trees", disengageable = true)]
	public class TreesOutput : Generator, Generator.IOutput, Layout.ILayered
	{
		//layer
		public class Layer : Layout.ILayer
		{
			public Input input = new Input(InoutType.Objects);
			public Output output = new Output(InoutType.Objects);
			
			public GameObject prefab;
			public bool relativeHeight = true;
			public bool rotate;
			public bool widthScale;
			public bool heightScale;
			public Color color = Color.white;
			public float bendFactor;

			public bool pinned { get; set; }
			public int guiHeight { get; set; }

			public void OnCollapsedGUI (Layout layout) 
			{
				layout.margin = 20; layout.rightMargin = 5; layout.fieldSize = 1f;
				layout.Par(20); 
				input.DrawIcon(layout);
				layout.Field(ref prefab, rect:layout.Inset());
			}

			public void OnExtendedGUI (Layout layout) 
			{
				layout.margin = 20; layout.rightMargin = 5;
				layout.Par(20); 

				input.DrawIcon(layout);
				layout.Field(ref prefab, rect:layout.Inset());

				layout.Par(); layout.Toggle(ref relativeHeight, rect:layout.Inset(20)); layout.Label("Relative Height", rect:layout.Inset(100)); 
				layout.Par(); layout.Toggle(ref rotate, rect:layout.Inset(20)); layout.Label("Rotate", rect:layout.Inset(45)); 
				layout.Par(); layout.Toggle(ref widthScale, rect:layout.Inset(20)); layout.Label("Width Scale", rect:layout.Inset(100));
				layout.Par(); layout.Toggle(ref heightScale, rect:layout.Inset(20)); layout.Label("Height Scale", rect:layout.Inset(100));
				layout.fieldSize = 0.37f;
				layout.Field(ref color, "Color");
				layout.Field(ref bendFactor, "Bend Factor");
			}

			public void OnAdd (int n) { }
			public void OnRemove (int n) { input.Link(null, null); }
			public void OnSwitch (int o, int n) { }
		}
		public Layer[] baseLayers = new Layer[0];
		public Layout.ILayer[] layers 
		{ 
			get {return baseLayers;} 
			set {baseLayers=ArrayTools.Convert<Layer,Layout.ILayer>(value);} 
		}

		public int selected { get; set; }
		public int collapsedHeight { get; set; }
		public int extendedHeight { get; set; }
		public Layout.ILayer def {get{ return new Layer(); }}

		public class TreesTuple { public TreeInstance[] instances; public TreePrototype[] prototypes; }


		//generator
		public override IEnumerable<Input> Inputs() 
		{ 
			if (baseLayers==null) baseLayers = new Layer[0];
			for (int i=0; i<baseLayers.Length; i++) 
				if (baseLayers[i].input != null)
					yield return baseLayers[i].input; 
		}
		public override IEnumerable<Output> Outputs() 
		{ 
			if (baseLayers==null) baseLayers = new Layer[0];
			for (int i=0; i<baseLayers.Length; i++) 
				if (baseLayers[i].output != null)
					yield return baseLayers[i].output; 
		}


		public static void Process (Chunk chunk)
		{
			if (chunk.stop) return;
			
			//debug timer
			if (MapMagic.instance.guiDebug)
			{
				if (chunk.timer==null) chunk.timer = new System.Diagnostics.Stopwatch(); 
				else chunk.timer.Reset();
				chunk.timer.Start();
			}

			List<TreeInstance> instancesList = new List<TreeInstance>();
			List<TreePrototype> prototypesList = new List<TreePrototype>();

			//InstanceRandom rnd = new InstanceRandom(MapMagic.instance.seed + 12345 + chunk.coord.x*1000 + chunk.coord.z); //to disable objects according biome masks

			foreach (TreesOutput gen in MapMagic.instance.gens.GeneratorsOfType<TreesOutput>(onlyEnabled:true, checkBiomes:true))
			{
				//loading biome matrix
				Matrix biomeMask = null;
				if (gen.biome != null) 
				{
					object biomeMaskObj = gen.biome.mask.GetObject(chunk);
					if (biomeMaskObj==null) continue; //adding nothing if biome has no mask
					biomeMask = (Matrix)biomeMaskObj;
					if (biomeMask == null) continue; 
					if (biomeMask.IsEmpty()) continue; //optimizing empty biomes
				}
				
				for (int i=0; i<gen.baseLayers.Length; i++)
				{
					if (chunk.stop) return; //checking stop before reading output
					Layer layer = gen.baseLayers[i];
					
					//loading objects from input
					SpatialHash hash = (SpatialHash)gen.baseLayers[i].input.GetObject(chunk);
					if (hash == null) continue;

					//adding prototype
					if (layer.prefab == null) continue;
					TreePrototype prototype = new TreePrototype() { prefab=layer.prefab, bendFactor=layer.bendFactor };
					prototypesList.Add(prototype);
					int prototypeNum = prototypesList.Count-1;

					foreach (SpatialObject obj in hash.AllObjs())
					{
						//disabling object using biome mask
						if (gen.biome != null)
						{
							if (biomeMask == null || biomeMask[obj.pos] < 0.5f) continue;
							//if (biomeMask[obj.pos] < rnd.CoordinateRandom((int)obj.pos.x, (int)obj.pos.y)) continue; 
						}

						//flooring
						float terrainHeight = 0;
						if (layer.relativeHeight && chunk.heights!=null) //if checbox enabled and heights exist (at least one height generator is in the graph)
							terrainHeight = chunk.heights.GetInterpolated(obj.pos.x, obj.pos.y);
						if (terrainHeight > 1) terrainHeight = 1;

						TreeInstance tree = new TreeInstance();
						tree.position = new Vector3(
							(obj.pos.x-hash.offset.x)/hash.size,
							obj.height + terrainHeight,
							(obj.pos.y-hash.offset.y)/hash.size );
						tree.rotation = layer.rotate? obj.rotation%360 : 0; 
						tree.widthScale = layer.widthScale? obj.size : 1;
						tree.heightScale = layer.heightScale? obj.size : 1;
						tree.prototypeIndex = prototypeNum;
						tree.color = layer.color;
						tree.lightmapColor = layer.color;

						instancesList.Add(tree);
					}
				}
			}

			//setting output
			if (chunk.stop) return;
			if (instancesList.Count==0 && prototypesList.Count==0) return; //empty, process is caused by height change
			TreesTuple treesTuple = new TreesTuple() { instances=instancesList.ToArray(), prototypes=prototypesList.ToArray() };
			chunk.apply.CheckAdd(typeof(TreesOutput), treesTuple, replace:true);

			//debug timer
			if (chunk.timer != null) 
			{ 
				chunk.timer.Stop(); 
				MapMagic.instance.guiDebugProcessTimes.CheckAdd(typeof(TreesOutput), chunk.timer.ElapsedMilliseconds, replace:true); 
			}
		}

		public static IEnumerator Apply (Chunk chunk)
		{
			//debug timer
			if (MapMagic.instance.guiDebug)
			{
				if (chunk.timer==null) chunk.timer = new System.Diagnostics.Stopwatch(); 
				else chunk.timer.Reset();
				chunk.timer.Start();
			}

			//applying
			chunk.terrain.terrainData.treeInstances = new TreeInstance[0];
			TreesTuple treesTuple = (TreesTuple)chunk.apply[typeof(TreesOutput)];
			chunk.terrain.terrainData.treePrototypes = treesTuple.prototypes;
			chunk.terrain.terrainData.treeInstances = treesTuple.instances;

			//debug timer
			if (chunk.timer != null) 
			{ 
				chunk.timer.Stop(); 
				MapMagic.instance.guiDebugApplyTimes.CheckAdd(typeof(TreesOutput), chunk.timer.ElapsedMilliseconds, replace:true); 
				Debug.Log("Trees Applied");
			}

			yield return null;
		}


		public static void Purge (Chunk chunk)
		{
			if (chunk.locked) return;
			chunk.terrain.terrainData.treeInstances = new TreeInstance[0];
			chunk.terrain.terrainData.treePrototypes = new TreePrototype[0];
		}

	
		public override void OnGUI ()
		{
			layout.DrawLayered(this, "Layers:");
		}

	}
	

	[System.Serializable]
	[GeneratorMenu (menu="Output", name ="Grass", disengageable = true)]
	public class GrassOutput : Generator, Generator.IOutput, Layout.ILayered
	{
		//layer
		public class Layer : Layout.ILayer
		{
			public Input input = new Input(InoutType.Map);
			public Output output = new Output(InoutType.Map);
			
			public DetailPrototype det = new DetailPrototype();
			public string name;
			public float density = 0.5f;
			public enum GrassRenderMode { Grass, GrassBillboard, VertexLit, Object };
			public GrassRenderMode renderMode; 

			public bool pinned { get; set; }
			public int guiHeight { get; set; }

			public void OnCollapsedGUI (Layout layout) 
			{
				layout.margin = 20; layout.rightMargin = 20; layout.fieldSize = 1f;
				layout.Par(20); 
				input.DrawIcon(layout);
				layout.Label(name, rect:layout.Inset());
				if (output == null) output = new Output(InoutType.Map); //backwards compatibility
				output.DrawIcon(layout);
			}

			public void OnExtendedGUI (Layout layout) 
			{
				layout.margin = 20; layout.rightMargin = 20;
				layout.Par(20); 

				input.DrawIcon(layout);
				layout.Field(ref name, rect:layout.Inset());
				if (output == null) output = new Output(InoutType.Map); //backwards compatibility
				output.DrawIcon(layout);

				layout.margin = 5; layout.rightMargin = 10; layout.fieldSize = 0.6f;
				layout.fieldSize = 0.65f;

				//setting render mode
				if (renderMode == GrassRenderMode.Grass && det.renderMode != DetailRenderMode.Grass) //loading outdated
				{
					if (det.renderMode == DetailRenderMode.GrassBillboard) renderMode = GrassRenderMode.GrassBillboard;
					else renderMode = GrassRenderMode.VertexLit;
				}

				renderMode = layout.Field(renderMode, "Mode");

				if (renderMode == GrassRenderMode.Object || renderMode == GrassRenderMode.VertexLit)
				{
					det.prototype = layout.Field(det.prototype, "Object");
					det.prototypeTexture = null; //otherwise this texture will be included to build even if not displayed
					det.usePrototypeMesh = true;
				}
				else
				{
					layout.Par(60); //not 65
					layout.Inset((layout.field.width-60)/2);
					det.prototypeTexture = layout.Field(det.prototypeTexture, rect:layout.Inset(60)); 
					det.prototype = null; //otherwise this object will be included to build even if not displayed
					det.usePrototypeMesh = false;
					layout.Par(2);
				}
				switch (renderMode)
				{
					case GrassRenderMode.Grass: det.renderMode = DetailRenderMode.Grass; break;
					case GrassRenderMode.GrassBillboard: det.renderMode = DetailRenderMode.GrassBillboard; break;
					case GrassRenderMode.VertexLit: det.renderMode = DetailRenderMode.VertexLit; break;
					case GrassRenderMode.Object: det.renderMode = DetailRenderMode.Grass; break;
				}

				density = layout.Field(density, "Density", max:50);
				//det.bendFactor = layout.Field(det.bendFactor, "Bend");
				det.dryColor = layout.Field(det.dryColor, "Dry");
				det.healthyColor = layout.Field(det.healthyColor, "Healthy");

				Vector2 temp = new Vector2(det.minWidth, det.maxWidth);
				layout.Field(ref temp, "Width", max:10);
				det.minWidth = temp.x; det.maxWidth = temp.y;

				temp = new Vector2(det.minHeight, det.maxHeight);
				layout.Field(ref temp, "Height", max:10);
				det.minHeight = temp.x; det.maxHeight = temp.y;

				det.noiseSpread = layout.Field(det.noiseSpread, "Noise", max:1);
			}

			public void OnAdd (int n) { name="Grass"; }
			public void OnRemove (int n) { input.Link(null,null); }
			public void OnSwitch (int o, int n) { }
		}
		public Layer[] baseLayers = new Layer[0];
		public Layout.ILayer[] layers 
		{ 
			get {return baseLayers;} 
			set {baseLayers=ArrayTools.Convert<Layer,Layout.ILayer>(value);} 
		}

		public int selected { get; set; }
		public int collapsedHeight { get; set; }
		public int extendedHeight { get; set; }
		public Layout.ILayer def {get{ return new Layer() { name="Grass" }; }}

		//params
		public Input maskIn = new Input(InoutType.Map);
		//public static int patchResolution = 16; //one value for all generators. TODO consider bringing it to the settings
		public bool obscureLayers = false;

		public class GrassTuple { public int[][,] details; public DetailPrototype[] prototypes; }


		//generator
		public override IEnumerable<Input> Inputs() 
		{ 
			if (maskIn==null) maskIn = new Input(InoutType.Map); //for backwards compatibility, input should not be null
			yield return maskIn;

			if (baseLayers==null) baseLayers = new Layer[0];
			for (int i=0; i<baseLayers.Length; i++) 
				if (baseLayers[i].input != null)
					yield return baseLayers[i].input; 
		}

		public override IEnumerable<Output> Outputs() 
		{ 
			if (baseLayers==null) baseLayers = new Layer[0];
			for (int i=0; i<baseLayers.Length; i++) 
				if (baseLayers[i].output != null)
					yield return baseLayers[i].output; 
		}

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//loading inputs
			Matrix[] matrices = new Matrix[baseLayers.Length];
			for (int i=0; i<baseLayers.Length; i++)
			{
				if (baseLayers[i].input != null) 
				{
					matrices[i] = (Matrix)baseLayers[i].input.GetObject(chunk);
					if (matrices[i] != null) matrices[i] = matrices[i].Copy(null);
				}
				if (matrices[i] == null) matrices[i] = chunk.defaultMatrix;
			}

			//blending layers
			if (obscureLayers) Matrix.BlendLayers(matrices);

			//masking layers
			Matrix mask = (Matrix)maskIn.GetObject(chunk);
			if (mask != null)
				for (int i=0; i<matrices.Length; i++) matrices[i].Multiply(mask);

			//saving outputs
			for (int i=0; i<baseLayers.Length; i++) 
			{
				if (chunk.stop) return; //do not write object is generating is stopped
				if (baseLayers[i].output == null) baseLayers[i].output = new Output(InoutType.Map); //back compatibility
				baseLayers[i].output.SetObject(chunk, matrices[i]);
			}
		}

		public static void Process (Chunk chunk)
		{
			if (chunk.stop) return;

			//debug timer
			if (MapMagic.instance.guiDebug)
			{
				if (chunk.timer==null) chunk.timer = new System.Diagnostics.Stopwatch(); 
				else chunk.timer.Reset();
				chunk.timer.Start();
			}
			
			//values to calculate density
			float pixelSize = 1f * MapMagic.instance.terrainSize / MapMagic.instance.resolution;
			float pixelSquare = pixelSize*pixelSize;

			//a random needed to convert float to int
			InstanceRandom rnd = new InstanceRandom(MapMagic.instance.seed + chunk.coord.x*1000 + chunk.coord.z);

			//calculating the totoal number of prototypes
			int prototypesNum = 0;
			foreach (GrassOutput grassOut in MapMagic.instance.gens.GeneratorsOfType<GrassOutput>())
				prototypesNum += grassOut.baseLayers.Length;

			//preparing results
			List<int[,]> detailsList = new List<int[,]>();
			List<DetailPrototype> prototypesList = new List<DetailPrototype>();

			//filling result
			foreach (GrassOutput gen in MapMagic.instance.gens.GeneratorsOfType<GrassOutput>(onlyEnabled:true, checkBiomes:true))
			{
				//loading biome matrix
				Matrix biomeMask = null;
				if (gen.biome != null) 
				{
					object biomeMaskObj = gen.biome.mask.GetObject(chunk);
					if (biomeMaskObj==null) continue; //adding nothing if biome has no mask
					biomeMask = (Matrix)biomeMaskObj;
					if (biomeMask == null) continue; 
					if (biomeMask.IsEmpty()) continue; //optimizing empty biomes
				}

				for (int i=0; i<gen.baseLayers.Length; i++)
				{
					if (chunk.stop) return;
					
					//loading objects from input
					//Matrix matrix = (Matrix)gen.baseLayers[i].input.GetObject(chunk);
					//if (matrix == null) continue;

					//reading output directly
					Output output = gen.baseLayers[i].output;
					if (chunk.stop) return; //checking stop before reading output
					if (!chunk.results.ContainsKey(output)) continue;
					Matrix matrix = (Matrix)chunk.results[output];

					//filling array
					int[,] detail = new int[matrix.rect.size.x, matrix.rect.size.z];
					for (int x=0; x<matrix.rect.size.x; x++)
						for (int z=0; z<matrix.rect.size.z; z++)
						{
							float val = matrix[x+matrix.rect.offset.x, z+matrix.rect.offset.z];
							float biomeVal = 1;
							if (gen.biome != null) 
							{
								if (biomeMask == null) biomeVal = 0;
								else biomeVal = biomeMask[x+matrix.rect.offset.x, z+matrix.rect.offset.z];
							}
							detail[z,x] = rnd.RandomToInt( val * gen.baseLayers[i].density * pixelSquare * biomeVal );
						}

					//adding to arrays
					detailsList.Add( detail );
					prototypesList.Add( gen.baseLayers[i].det );
				}
			}

			//pushing to apply
			if (chunk.stop) return;
			GrassTuple grassTuple = new GrassTuple() { details=detailsList.ToArray(), prototypes=prototypesList.ToArray() };
			chunk.apply.CheckAdd(typeof(GrassOutput), grassTuple, replace:true);

			//debug timer
			if (chunk.timer != null) 
			{ 
				chunk.timer.Stop(); 
				MapMagic.instance.guiDebugProcessTimes.CheckAdd(typeof(GrassOutput), chunk.timer.ElapsedMilliseconds, replace:true); 
			}
		}


		public static IEnumerator Apply (Chunk chunk)
		{
			//debug timer
			if (MapMagic.instance.guiDebug)
			{
				if (chunk.timer==null) chunk.timer = new System.Diagnostics.Stopwatch(); 
				else chunk.timer.Reset();
				chunk.timer.Start();
			}

			GrassTuple grassTuple = (GrassTuple)chunk.apply[typeof(GrassOutput)];

			//resolution
			chunk.terrain.terrainData.SetDetailResolution(MapMagic.instance.resolution, MapMagic.instance.grassPatchRes);

			//prototypes
			chunk.terrain.terrainData.detailPrototypes = grassTuple.prototypes;

			//instances
			for (int i=0; i<grassTuple.details.Length; i++)
				chunk.terrain.terrainData.SetDetailLayer(0,0,i,grassTuple.details[i]);

			//debug timer
			if (chunk.timer != null) 
			{ 
				chunk.timer.Stop(); 
				MapMagic.instance.guiDebugApplyTimes.CheckAdd(typeof(GrassOutput), chunk.timer.ElapsedMilliseconds, replace:true);
				Debug.Log("Grass Applied"); 
			}

			yield return null;
		}

		public static void Purge (Chunk chunk)
		{
			if (chunk.locked) return;

			DetailPrototype[] prototypes = new DetailPrototype[0];
			chunk.terrain.terrainData.detailPrototypes = prototypes;
			chunk.terrain.terrainData.SetDetailResolution(16, 8);

			//if (MapMagic.instance.guiDebug) Debug.Log("Grass Cleared");
		}

		public override void OnGUI () 
		{
			layout.Par(20); maskIn.DrawIcon(layout, "Mask");

			layout.Field(ref MapMagic.instance.grassPatchRes, "Patch Res", min:4, max:64, fieldSize:0.35f);
			MapMagic.instance.grassPatchRes = Mathf.ClosestPowerOfTwo(MapMagic.instance.grassPatchRes);
			layout.Field(ref obscureLayers, "Obscure Layers", fieldSize:0.35f);
			layout.Par(3);
			layout.DrawLayered(this, "Layers:");

			layout.fieldSize = 0.4f; layout.margin = 10; layout.rightMargin = 10;
			layout.Par(5);
		}

	}

	[System.Serializable]
	public partial class RTPOutput : Generator, Generator.IOutput
	{
		public static void Process (Chunk chunk) { _Process(chunk); }
		static partial void _Process (Chunk chunk);

		public static IEnumerator Apply (Chunk chunk) { _Apply(chunk); yield return null; }
		static partial void _Apply (Chunk chunk);

		public static void Purge (Chunk chunk) { _Purge(chunk); }
		static partial void _Purge (Chunk chunk);

		public override void OnGUI () { _OnGUI(); }
		partial void _OnGUI ();
	}
}
