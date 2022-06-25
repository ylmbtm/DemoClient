using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

//using Plugins;

namespace MapMagic
{
	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Voronoi (Legacy)", disengageable = true, disabled = true)]
	public class VoronoiGenerator : Generator
	{
		public float intensity = 1f;
		public int cellCount = 16;
		public float uniformity = 0;
		public int seed = 12345;
		public enum BlendType { flat, closest, secondClosest, cellular, organic }
		public BlendType blendType = BlendType.cellular;
		
		public Input input = new Input(InoutType.Map);
		public Input maskIn = new Input(InoutType.Map);
		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Input> Inputs() { yield return input; yield return maskIn; }
		public override IEnumerable<Output> Outputs() { yield return output; }


		public override void Generate (Chunk chunk, Biome biome=null)
		{
			Matrix matrix = (Matrix)input.GetObject(chunk); if (matrix != null) matrix = matrix.Copy(null);
			if (matrix == null) matrix = chunk.defaultMatrix;
			Matrix mask = (Matrix)maskIn.GetObject(chunk);
			if (chunk.stop) return;
			if (!enabled || intensity==0 || cellCount==0) { output.SetObject(chunk, matrix); return; } 

			//NoiseGenerator.Noise(matrix,200,0.5f,Vector2.zero);
			//matrix.Multiply(amount);

			InstanceRandom random = new InstanceRandom(MapMagic.instance.seed + seed);
	
			//creating point matrix
			float cellSize = 1f * matrix.rect.size.x / cellCount;
			Matrix2<Vector3> points = new Matrix2<Vector3>( new CoordRect(0,0,cellCount+2,cellCount+2) );
			points.rect.offset = new Coord(-1,-1);

			Coord matrixSpaceOffset = new Coord((int)(matrix.rect.offset.x/cellSize), (int)(matrix.rect.offset.z/cellSize));
		
			//scattering points
			for (int x=-1; x<points.rect.size.x-1; x++)
				for (int z=-1; z<points.rect.size.z-1; z++)
				{
					Vector3 randomPoint = new Vector3(x+random.CoordinateRandom(x+matrixSpaceOffset.x,z+matrixSpaceOffset.z), 0, z+random.NextCoordinateRandom());
					Vector3 centerPoint = new Vector3(x+0.5f,0,z+0.5f);
					Vector3 point = randomPoint*(1-uniformity) + centerPoint*uniformity;
					point = point*cellSize + new Vector3(matrix.rect.offset.x, 0, matrix.rect.offset.z);
					point.y = random.NextCoordinateRandom();
					points[x,z] = point;
				}

			Coord min = matrix.rect.Min; Coord max = matrix.rect.Max; 
			for (int x=min.x; x<max.x; x++)
				for (int z=min.z; z<max.z; z++)
			{
				//finding current cell
				Coord cell = new Coord((int)((x-matrix.rect.offset.x)/cellSize), (int)((z-matrix.rect.offset.z)/cellSize));
		
				//finding min dist
				float minDist = 200000000; float secondMinDist = 200000000;
				float minHeight = 0; //float secondMinHeight = 0;
				for (int ix=-1; ix<=1; ix++)
					for (int iz=-1; iz<=1; iz++)
				{
					Coord nearCell = new Coord(cell.x+ix, cell.z+iz);
					//if (!points.rect.CheckInRange(nearCell)) continue; //no need to perform test as points have 1-cell border around matrix

					Vector3 point = points[nearCell];
					float dist = (x-point.x)*(x-point.x) + (z-point.z)*(z-point.z);
					if (dist<minDist) 
					{ 
						secondMinDist = minDist; minDist = dist; 
						minHeight = point.y;
					}
					else if (dist<secondMinDist) secondMinDist = dist; 
				}

				float val = 0;
				switch (blendType)
				{
					case BlendType.flat: val = minHeight; break;
					case BlendType.closest: val = minDist / (MapMagic.instance.resolution*16); break;
					case BlendType.secondClosest: val = secondMinDist / (MapMagic.instance.resolution*16); break;
					case BlendType.cellular: val = (secondMinDist-minDist) / (MapMagic.instance.resolution*16); break;
					case BlendType.organic: val = (secondMinDist+minDist)/2 / (MapMagic.instance.resolution*16); break;
				}
				if (mask==null) matrix[x,z] += val*intensity;
				else matrix[x,z] += val*intensity*mask[x,z];
			}

			if (chunk.stop) return; //do not write object is generating is stopped
			output.SetObject(chunk, matrix);
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); input.DrawIcon(layout, "Input"); output.DrawIcon(layout, "Output");
			layout.Par(20); maskIn.DrawIcon(layout, "Mask");
			layout.Par(5);
			
			//params
			layout.fieldSize = 0.5f;
			layout.Field(ref blendType, "Type");
			layout.Field(ref intensity, "Intensity");
			layout.Field(ref cellCount, "Cell Count"); cellCount = Mathf.ClosestPowerOfTwo(cellCount);
			layout.Field(ref uniformity, "Uniformity", min:0, max:1);
			layout.Field(ref seed, "Seed");
		}
	}
	
	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Noise (Legacy)", disengageable = true, disabled = true)]
	public class NoiseGenerator : Generator
	{
		public int seed = 12345;
		public float intensity = 1f;
		public float bias = 0.0f;
		public float size = 200;
		public float detail = 0.5f;
		public Vector2 offset = new Vector2(0,0);
		//public float contrast = 0f;

		public Input input = new Input(InoutType.Map);
		public Input maskIn = new Input(InoutType.Map);
		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Input> Inputs() { yield return input; yield return maskIn; }
		public override IEnumerable<Output> Outputs() { yield return output; }

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			Matrix matrix = (Matrix)input.GetObject(chunk); if (matrix != null) matrix = matrix.Copy(null);
			if (matrix == null) matrix = chunk.defaultMatrix;
			Matrix mask = (Matrix)maskIn.GetObject(chunk);
			if (!enabled) { output.SetObject(chunk, matrix); return; }
			if (chunk.stop) return;

			Noise(matrix, size, intensity, bias, detail, offset, seed, mask);
			
			if (chunk.stop) return; //do not write object is generating is stopped
			output.SetObject(chunk, matrix);
		}

		public static void Noise (Matrix matrix, float size, float intensity=1, float bias=0, float detail=0.5f, Vector2 offset=new Vector2(), int seed=12345, Matrix mask=null)
		{
			int step = (int)(4096f / matrix.rect.size.x);

			int totalSeedX = ((int)offset.x + MapMagic.instance.seed + seed*7) % 77777;
			int totalSeedZ = ((int)offset.y + MapMagic.instance.seed + seed*3) % 73333;

			//get number of iterations
			int numIterations = 1; //max size iteration included
			float tempSize = size;
			for (int i=0; i<100; i++)
			{
				tempSize = tempSize/2;
				if (tempSize<1) break;
				numIterations++;
			}

			//making some noise
			Coord min = matrix.rect.Min; Coord max = matrix.rect.Max;
			for (int x=min.x; x<max.x; x++)
			{
				for (int z=min.z; z<max.z; z++)
				{
					float result = 0.5f;
					float curSize = size*10;
					float curAmount = 1;
				
					//applying noise
					for (int i=0; i<numIterations;i++)
					{
						float perlin = Mathf.PerlinNoise(
						(x + totalSeedX + 1000*(i+1))*step/(curSize+1), 
						(z + totalSeedZ + 100*i)*step/(curSize+1) );
						perlin = (perlin-0.5f)*curAmount + 0.5f;

						//applying overlay
						if (perlin > 0.5f) result = 1 - 2*(1-result)*(1-perlin);
						else result = 2*perlin*result;

						curSize *= 0.5f;
						curAmount *= detail; //detail is 0.5 by default
					}

					//apply contrast and bias
					result = result*intensity;
					result -= 0*(1-bias) + (intensity-1)*bias; //0.5f - intensity*bias;

					if (result < 0) result = 0; 
					if (result > 1) result = 1;

					if (mask==null) matrix[x,z] += result;
					else matrix[x,z] += result*mask[x,z];
				}
			}
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); input.DrawIcon(layout, "Input"); output.DrawIcon(layout, "Output");
			layout.Par(20); maskIn.DrawIcon(layout, "Mask");
			layout.Par(5);
			
			//params
			layout.fieldSize = 0.6f;
			//output.sharedResolution.guiResolution = layout.ComplexField(output.sharedResolution.guiResolution, "Output Resolution");
			layout.Field(ref seed, "Seed");
			layout.Field(ref intensity, "Intensity");
			layout.Field(ref bias, "Bias");
			layout.Field(ref size, "Size", min:1);
			layout.Field(ref detail, "Detail", max:1);
			layout.Field(ref offset, "Offset");
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Raw Input (Legacy)", disengageable = true, disabled = true)]
	public class RawInput : Generator
	{
		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Output> Outputs() { yield return output; }

		public MatrixAsset textureAsset;
		public Matrix previewMatrix;
		[System.NonSerialized] public Texture2D preview;  
		public string texturePath; 
		public float intensity = 1;
		public float scale = 1;
		public Vector2 offset;
		public bool tile = false;

		public void ImportRaw (string path=null)
		{
			#if UNITY_EDITOR
			//importing
			if (path==null) path = UnityEditor.EditorUtility.OpenFilePanel("Import Texture File", "", "raw,r16");
			if (path==null || path.Length==0) return;
			if (textureAsset == null) textureAsset = ScriptableObject.CreateInstance<MatrixAsset>();
			if (textureAsset.matrix == null) textureAsset.matrix = new Matrix( new CoordRect(0,0,1,1) );

			textureAsset.matrix.ImportRaw(path);
			texturePath = path;

			//generating preview
			CoordRect previewRect = new CoordRect(0,0, 70, 70);
			previewMatrix = textureAsset.matrix.Resize(previewRect, previewMatrix);
			preview = previewMatrix.SimpleToTexture();
			#endif
		}

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			Matrix matrix = chunk.defaultMatrix;
			if (!enabled || textureAsset==null || textureAsset.matrix==null) { output.SetObject(chunk, matrix); return; }
			if (chunk.stop) return;

			//matrix = textureMatrix.Resize(matrix.rect);
			
			CoordRect scaledRect = new CoordRect(
				(int)(offset.x), 
				(int)(offset.y), 
				(int)(matrix.rect.size.x*scale),
				(int)(matrix.rect.size.z*scale) );
			Matrix scaledTexture = textureAsset.matrix.Resize(scaledRect);

			matrix.Replicate(scaledTexture, tile:tile);
			matrix.Multiply(intensity);

			if (scale > 1)
			{
				Matrix cpy = matrix.Copy();
				for (int i=0; i<scale-1; i++) matrix.Blur();
				Matrix.SafeBorders(cpy, matrix, Mathf.Max(matrix.rect.size.x/128, 4));
			}
			
			//if (tile) textureMatrix.FromTextureTiled(texture);
			//else textureMatrix.FromTexture(texture);
			
			//if (!Mathf.Approximately(scale,1)) textureMatrix = textureMatrix.Resize(matrix.rect, result:matrix);*/
			if (chunk.stop) return;
			output.SetObject(chunk, matrix);
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); output.DrawIcon(layout, "Output");
			layout.Par(5);
			
			//preview texture
			layout.margin = 4;
			#if UNITY_EDITOR
			int previewSize = 70;
			int controlsSize = (int)layout.field.width - previewSize - 10;
			Rect oldCursor = layout.cursor;
			if (preview == null) 
			{
				if (previewMatrix != null) preview = previewMatrix.SimpleToTexture();
				else preview = Extensions.ColorTexture(2,2,Color.black);
			}
			layout.Par(previewSize+3); layout.Inset(controlsSize);
			layout.Icon(preview, layout.Inset(previewSize+4));
			layout.cursor = oldCursor;
			
			//preview params
			layout.Par(); if (layout.Button("Browse", rect:layout.Inset(controlsSize))) { ImportRaw(); layout.change = true; }
			layout.Par(); if (layout.Button("Refresh", rect:layout.Inset(controlsSize))) { ImportRaw(texturePath); layout.change = true; }
			layout.Par(40); layout.Label("Square gray 16bit RAW, PC byte order", layout.Inset(controlsSize), helpbox:true, fontSize:9);
			#endif

			layout.fieldSize = 0.62f;
			layout.Field(ref intensity, "Intensity");
			layout.Field(ref scale, "Scale");
			layout.Field(ref offset, "Offset");
			layout.Toggle(ref tile, "Tile");
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Cavity (Legacy)", disengageable = true, disabled = true)]
	public class CavityGenerator : Generator
	{
		public Input input = new Input(InoutType.Map);
		public Output convexOut = new Output(InoutType.Map);
		public Output concaveOut = new Output(InoutType.Map);
		public override IEnumerable<Input> Inputs() { yield return input; }
		public override IEnumerable<Output> Outputs() { yield return convexOut;  yield return concaveOut; }
		public float intensity = 1;
		public float spread = 0.5f;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//getting input
			Matrix matrix = (Matrix)input.GetObject(chunk);

			//return on stop/disable/null input
			if (chunk.stop || !enabled || matrix==null) return; 

			//preparing outputs
			Matrix result = new Matrix(matrix.rect);
			Matrix temp = new Matrix(matrix.rect);

			//cavity
			System.Func<float,float,float,float> cavityFn = delegate(float prev, float curr, float next) 
			{
				float c = curr - (next+prev)/2;
				return (c*c*(c>0?1:-1))*intensity*100000;
			};
			result.Blur(cavityFn, intensity:1, additive:true, reference:matrix); //intensity is set in func
			if (chunk.stop) return;

			//borders
			result.RemoveBorders(); 
			if (chunk.stop) return;

			//spread
			result.Spread(strength:spread, copy:temp); 
			if (chunk.stop) return;

			//clamping and inverting
			for (int i=0; i<result.count; i++) 
			{
				temp.array[i] = 0;
				if (result.array[i]<0) { temp.array[i] = -result.array[i]; result.array[i] = 0; }
			}

			//setting outputs
			if (chunk.stop) return;
			convexOut.SetObject(chunk, result);
			concaveOut.SetObject(chunk, temp);
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); input.DrawIcon(layout, "Input", mandatory:true); convexOut.DrawIcon(layout, "Convex");
			layout.Par(20); concaveOut.DrawIcon(layout, "Concave");
			layout.Par(5);
			
			//params
			layout.Field(ref intensity, "Intensity");
			layout.Field(ref spread, "Spread");
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Slope (Legacy)", disengageable = true, disabled = true)]
	public class SlopeGenerator : Generator
	{
		public Input input = new Input(InoutType.Map);
		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Input> Inputs() { yield return input; }
		public override IEnumerable<Output> Outputs() { yield return output; }
		
		public float steepness = 2.5f;
		public float range = 0.3f;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//getting input
			Matrix matrix = (Matrix)input.GetObject(chunk);

			//return on stop/disable/null input
			if (chunk.stop || !enabled || matrix==null) return; 

			//preparing output
			Matrix result = new Matrix(matrix.rect);

			//using the terain-height relative values
			float dist = range;
			float start = steepness-dist/4; //4, not 2 because blurring is additive

			//transforming to 0-1 range
			start = start/MapMagic.instance.terrainHeight;
			dist = dist/MapMagic.instance.terrainHeight;

			//incline
			System.Func<float,float,float,float> inclineFn = delegate(float prev, float curr, float next) 
			{
				float prevDelta = prev-curr; if (prevDelta < 0) prevDelta = -prevDelta;
				float nextDelta = next-curr; if (nextDelta < 0) nextDelta = -nextDelta;
				float delta = prevDelta>nextDelta? prevDelta : nextDelta; 
				delta *= 1.8f; //for backwards compatibility
				float val = (delta-start)/dist; if (val < 0) val=0; if (val>1) val=1;

				return val;
			};
			result.Blur(inclineFn, intensity:1, additive:true, reference:matrix); //intensity is set in func
			
			//setting output
			if (chunk.stop) return;
			output.SetObject(chunk, result);
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); input.DrawIcon(layout, "Input", mandatory:true); output.DrawIcon(layout, "Output");
			layout.Par(5);
			
			//params
			layout.Field(ref steepness, "Steepness", min:0);
			layout.Field(ref range, "Range", min:0.1f);
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Objects", name ="Stamp (legacy)", disengageable = true, disabled = true)]
	public class StampGenerator : Generator
	{
		public Input objectsIn = new Input(InoutType.Objects);
		public Input canvasIn = new Input(InoutType.Map);
		public Input maskIn = new Input(InoutType.Map);
		public override IEnumerable<Input> Inputs() {  yield return objectsIn; yield return canvasIn; yield return maskIn; }

		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Output> Outputs() { yield return output; }

		public float radius = 10;
		public AnimationCurve curve = new AnimationCurve( new Keyframe[] { new Keyframe(0,0,1,1), new Keyframe(1,1,1,1) } );
		public bool useNoise = false;
		public float noiseAmount = 0.1f;
		public float noiseSize = 100;
		public bool maxHeight = true;
		public float sizeFactor = 0;
		public int safeBorders = 0;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//getting inputs
			SpatialHash objects = (SpatialHash)objectsIn.GetObject(chunk);
			Matrix src = (Matrix)canvasIn.GetObject(chunk);
			
			//return on stop/disable/null input
			if (chunk.stop || objects==null) return; 
			if (!enabled) { output.SetObject(chunk, src); return; }

			//preparing output
			Matrix dst; 
			if (src != null) dst = src.Copy(null); 
			else dst = chunk.defaultMatrix;

			//finding maximum radius
			float maxRadius = radius;
			if (sizeFactor > 0.00001f)
			{
				float maxObjSize = 0;
				foreach (SpatialObject obj in objects.AllObjs())
					if (obj.size > maxObjSize) maxObjSize = obj.size;
				maxObjSize = maxObjSize / MapMagic.instance.terrainSize * MapMagic.instance.resolution; //transforming to map-space
				maxRadius = radius*(1-sizeFactor) + radius*maxObjSize*sizeFactor;
			}

			//preparing procedural matrices
			Matrix noiseMatrix = new Matrix( new CoordRect(0,0,maxRadius*2+2,maxRadius*2+2) );
			Matrix percentMatrix = new Matrix( new CoordRect(0,0,maxRadius*2+2,maxRadius*2+2) );

			foreach (SpatialObject obj in objects.AllObjs())
			{
				//finding current radius
				float curRadius = radius*(1-sizeFactor) + radius*obj.size*sizeFactor;
				curRadius = curRadius / MapMagic.instance.terrainSize * MapMagic.instance.resolution; //transforming to map-space

				//resizing procedural matrices
				CoordRect matrixSize = new CoordRect(0,0,curRadius*2+2,curRadius*2+2);
				noiseMatrix.ChangeRect(matrixSize);
				percentMatrix.ChangeRect(matrixSize);

				//apply stamp
				noiseMatrix.rect.offset = new Coord((int)(obj.pos.x-curRadius-1), (int)(obj.pos.y-curRadius-1));
				percentMatrix.rect.offset = new Coord((int)(obj.pos.x-curRadius-1), (int)(obj.pos.y-curRadius-1));

				CoordRect intersection = CoordRect.Intersect(noiseMatrix.rect, dst.rect);
				Coord min = intersection.Min; Coord max = intersection.Max; 
				for (int x=min.x; x<max.x; x++)
					for (int z=min.z; z<max.z; z++)
				{
					float dist = Mathf.Sqrt((x-obj.pos.x+0.5f)*(x-obj.pos.x+0.5f) + (z-obj.pos.y+0.5f)*(z-obj.pos.y+0.5f));
					float percent = 1f - dist / curRadius; 
					if (percent < 0 || dist > curRadius) percent = 0;

					percentMatrix[x,z] = percent;
				}

				//adjusting value by curve
				Curve c = new Curve(curve);
				for (int i=0; i<percentMatrix.array.Length; i++) percentMatrix.array[i] = c.Evaluate(percentMatrix.array[i]);

				//adding some noise
				if (useNoise) 
				{
					NoiseGenerator.Noise(noiseMatrix, noiseSize, 0.5f, offset:Vector2.zero);

					for (int x=min.x; x<max.x; x++)
						for (int z=min.z; z<max.z; z++)
					{
						float val = percentMatrix[x,z];
						if (val < 0.0001f) continue;

						float noise = noiseMatrix[x,z];
						if (val < 0.5f) noise *= val*2;
						else noise = 1 - (1-noise)*(1-val)*2;

						percentMatrix[x,z] = noise*noiseAmount + val*(1-noiseAmount);
					}
				}

				//applying matrices
				for (int x=min.x; x<max.x; x++)
					for (int z=min.z; z<max.z; z++)
				{
					//float distSq = (x-obj.pos.x)*(x-obj.pos.x) + (z-obj.pos.y)*(z-obj.pos.y);
					//if (distSq > radius*radius) continue;
					
					float percent = percentMatrix[x,z];
					dst[x,z] = (maxHeight? 1:obj.height)*percent + dst[x,z]*(1-percent);
				}
			}

			Matrix mask = (Matrix)maskIn.GetObject(chunk);
			if (mask != null) Matrix.Mask(src, dst, mask);
			if (safeBorders != 0) Matrix.SafeBorders(src, dst, safeBorders);

			//setting output
			if (chunk.stop) return;
			output.SetObject(chunk, dst);
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); objectsIn.DrawIcon(layout, "Objects", mandatory:true); output.DrawIcon(layout, "Output");
			layout.Par(20); canvasIn.DrawIcon(layout, "Canvas");
			layout.Par(20); maskIn.DrawIcon(layout, "Mask");
			layout.Par(5);

			//params
			layout.margin=5;
			layout.Field(ref radius, "Radius");
			layout.Label("Fallof:");
			
			//curve
			Rect cursor = layout.cursor;
			layout.Par(53);
			layout.Curve(curve, rect:layout.Inset(80));

			layout.cursor = cursor; 
			int margin = layout.margin; layout.margin = 86; layout.fieldSize = 0.8f;
			layout.Toggle(ref useNoise, "Noise");
			layout.Field(ref noiseAmount, "A");
			layout.Field(ref noiseSize, "S");
			
			layout.Par(5); layout.margin = margin; layout.fieldSize = 0.5f;
			layout.Field(ref sizeFactor, "Size Factor");
			layout.Field(ref safeBorders, "Safe Borders");
			layout.Toggle(ref maxHeight,"Use Maximum Height");
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Output", name ="Preview", disengageable = true, disabled = true)]
	public class PreviewOutput1 : Generator, Generator.IOutput
	{
		public Input input = new Input(InoutType.Map);
		public override IEnumerable<Input> Inputs() { yield return input; }

		public bool onTerrain = false;
		public bool inWindow = false;
		public Color blacks = new Color(1,0,0,0); public Color oldBlacks;
		public Color whites = new Color(0,1,0,0); public Color oldWhites;

		public delegate void RefreshWindow(object obj);
		//public event RefreshWindow OnObjectChanged;

		//[System.NonSerialized] public SplatPrototype[] prototypes = new SplatPrototype[2]; 
		public SplatPrototype redPrototype = new SplatPrototype();
		public SplatPrototype greenPrototype = new SplatPrototype();
		
		public override void OnGUI ()
		{
			layout.Par(20); input.DrawIcon(layout, "Matrix", mandatory:true);
			layout.Par(5);

			layout.Field(ref onTerrain, "On Terrain");
			layout.Field(ref inWindow, "In Window");
			layout.Field(ref whites, "Whites");
			layout.Field(ref blacks, "Blacks");
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Legacy", name ="Subtract (Legacy)", disengageable = true, disabled = true)]
	public class SubtractGenerator0 : Generator
	{
		public Input minuendIn = new Input(InoutType.Objects);
		public Input subtrahendIn = new Input(InoutType.Objects);
		public Output minuendOut = new Output(InoutType.Objects);
		public override IEnumerable<Input> Inputs() { yield return minuendIn; yield return subtrahendIn; }
		public override IEnumerable<Output> Outputs() { yield return minuendOut; }

		public float distance = 1;
		public float sizeFactor = 0;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//getting inputs
			SpatialHash minuend = (SpatialHash)minuendIn.GetObject(chunk);
			SpatialHash subtrahend = (SpatialHash)subtrahendIn.GetObject(chunk);

			//return on stop/disable/null input
			if (chunk.stop || minuend==null) return;
			if (!enabled || subtrahend==null || subtrahend.Count==0) { minuendOut.SetObject(chunk, minuend); return; }

			//preparing output
			SpatialHash result = new SpatialHash(minuend.offset, minuend.size, minuend.resolution);

			//transforming distance to map-space
			float dist = distance / MapMagic.instance.terrainSize * MapMagic.instance.resolution; 

			//finding maximum seek distance
			float maxObjSize = 0;
			foreach (SpatialObject obj in subtrahend.AllObjs())
				if (obj.size > maxObjSize) maxObjSize = obj.size;
			maxObjSize = maxObjSize / MapMagic.instance.terrainSize * MapMagic.instance.resolution; //transforming to map-space
			float maxDist = dist*(1-sizeFactor) + dist*maxObjSize*sizeFactor;

			foreach (SpatialObject obj in minuend.AllObjs())
			{
				bool inRange = false;

				foreach (SpatialObject closeObj in subtrahend.ObjsInRange(obj.pos, maxDist))
				{
					float minDist = (obj.pos - closeObj.pos).magnitude;
					if (minDist < dist*(1-sizeFactor) + dist*closeObj.size*sizeFactor) { inRange = true; break; }
				}

				if (!inRange) result.Add(obj);

				//SpatialObject closestObj = subtrahend.Closest(obj.pos,false);
				//float minDist = (obj.pos - closestObj.pos).magnitude;

				//if (minDist > dist*(1-sizeFactor) + dist*closestObj.size*sizeFactor) result.Add(obj);
			}

			//setting output
			if (chunk.stop) return;
			minuendOut.SetObject(chunk, result);
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); minuendIn.DrawIcon(layout, "Input"); minuendOut.DrawIcon(layout, "Output");
			layout.Par(20); subtrahendIn.DrawIcon(layout, "Subtrahend");
			layout.Par(5);
			
			//params
			layout.Field(ref distance, "Distance");
			layout.Field(ref sizeFactor, "Size Factor"); 
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Objects", name ="Floor (Outdated)", disengageable = true, disabled = true)]
	public class FloorGenerator : Generator
	{
		public Input objsIn = new Input(InoutType.Objects);
		public Input substrateIn = new Input(InoutType.Map);
		public Output objsOut = new Output(InoutType.Objects);
		public override IEnumerable<Input> Inputs() { yield return objsIn; yield return substrateIn; }
		public override IEnumerable<Output> Outputs() { yield return objsOut; }

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//getting inputs
			SpatialHash objs = (SpatialHash)objsIn.GetObject(chunk);
			/*Matrix substrate = (Matrix)substrateIn.GetObject(chunk);
			
			//return on stop/disable/null input
			if (chunk.stop || objs==null) return;
			if (!enabled || substrate==null) { objsOut.SetObject(chunk, objs); return; }
			
			//preparing output
			objs = objs.Copy();

			for (int c=0; c<objs.cells.Length; c++)
			{
				List<SpatialObject> objList = objs.cells[c].objs;
				int objsCount = objList.Count;
				for (int i=0; i<objsCount; i++)
				{
					SpatialObject obj = objList[i];
					//obj.height = substrate[(int)obj.pos.x, (int)obj.pos.y];
					obj.height = substrate.GetInterpolatedValue(obj.pos);
				}
			}

			//setting output
			if (chunk.stop) return;*/
			objsOut.SetObject(chunk, objs);
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); objsIn.DrawIcon(layout, "Input", mandatory:true); objsOut.DrawIcon(layout, "Output");
			layout.Par(20); substrateIn.DrawIcon(layout, "Height");
			layout.Par(5);
			
			//params
			layout.Par(55);
			layout.Label("Floor Generator is outdated. To floor objects to terrain use \"Relative Height\" toggle in Object Output toggle.", rect:layout.Inset(), helpbox:true, fontSize:9);
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Simple Form (Legacy)", disengageable = true, disabled = true)]
	public class SimpleForm : Generator
	{
		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Output> Outputs() { yield return output; }

		public enum FormType { GradientX, GradientZ, Pyramid, Cone }
		public FormType type = FormType.Cone;
		public float intensity = 1;
		public float scale = 1;
		public Vector2 offset;
		public bool tile = false; //outdated
		public Matrix.WrapMode wrap = Matrix.WrapMode.Once;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			if (!enabled || chunk.stop) return;

			CoordRect scaledRect = new CoordRect(
				(int)(offset.x * MapMagic.instance.resolution / MapMagic.instance.terrainSize), 
				(int)(offset.y * MapMagic.instance.resolution / MapMagic.instance.terrainSize), 
				(int)(MapMagic.instance.resolution*scale),
				(int)(MapMagic.instance.resolution*scale) );
			Matrix stampMatrix = new Matrix(scaledRect);

			float gradientStep = 1f/stampMatrix.rect.size.x;
			Coord center = scaledRect.Center;
			float radius = stampMatrix.rect.size.x / 2f;
			Coord min = stampMatrix.rect.Min; Coord max = stampMatrix.rect.Max;
			
			switch (type)
			{
				case FormType.GradientX:
					for (int x=min.x; x<max.x; x++)
						for (int z=min.z; z<max.z; z++)
							stampMatrix[x,z] = x*gradientStep;
					break;
				case FormType.GradientZ:
					for (int x=min.x; x<max.x; x++)
						for (int z=min.z; z<max.z; z++)
							stampMatrix[x,z] = z*gradientStep;
					break;
				case FormType.Pyramid:
					for (int x=min.x; x<max.x; x++)
						for (int z=min.z; z<max.z; z++)
						{
							float valX = x*gradientStep; if (valX > 1-valX) valX = 1-valX;
							float valZ = z*gradientStep; if (valZ > 1-valZ) valZ = 1-valZ;
							stampMatrix[x,z] = valX<valZ? valX*2 : valZ*2;
						}
					break;
				case FormType.Cone:
					for (int x=min.x; x<max.x; x++)
						for (int z=min.z; z<max.z; z++)
						{
							float val = 1 - (Coord.Distance(new Coord(x,z), center) / radius);
							if (val<0) val = 0;
							stampMatrix[x,z] = val;
						}
					break;
			}

			Matrix matrix = chunk.defaultMatrix;
			matrix.Replicate(stampMatrix, tile:tile);
			matrix.Multiply(intensity);

			
			//if (tile) textureMatrix.FromTextureTiled(texture);
			//else textureMatrix.FromTexture(texture);
			
			//if (!Mathf.Approximately(scale,1)) textureMatrix = textureMatrix.Resize(matrix.rect, result:matrix);
			if (chunk.stop) return;
			output.SetObject(chunk, matrix);
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); output.DrawIcon(layout, "Output");
			layout.Par(5);
			
			layout.fieldSize = 0.62f;
			layout.Field(ref type, "Type");
			layout.Field(ref intensity, "Intensity");
			layout.Field(ref scale, "Scale");
			layout.Field(ref offset, "Offset");
			if (tile) wrap = Matrix.WrapMode.Tile; tile=false;
			layout.Field(ref wrap, "Wrap Mode");
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Objects", name ="Scatter (Legacy)", disengageable = true, disabled = true)]
	public class ScatterGenerator : Generator
	{
		public Input probability = new Input(InoutType.Map);
		public Output output = new Output(InoutType.Objects);
		public override IEnumerable<Input> Inputs() { yield return probability; }
		public override IEnumerable<Output> Outputs() { yield return output; }

		public int seed = 12345;
		public float count = 10;
		public float uniformity = 0.1f; //aka candidatesNum/100

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			Matrix probMatrix = (Matrix)probability.GetObject(chunk);
			SpatialHash spatialHash = chunk.defaultSpatialHash;
			if (!enabled) { output.SetObject(chunk, spatialHash); return; }
			if (chunk.stop) return; 
			
			InstanceRandom rnd = new InstanceRandom(MapMagic.instance.seed + seed + chunk.coord.x*1000 + chunk.coord.z);

			//Rect terrainRect = terrain.coord.ToRect(terrain.size);
			//terrainRect.position += Vector2.one; terrainRect.size-=Vector2.one*2;
			
			//SpatialHash spatialHash = new SpatialHash(terrain.coord.ToVector2(terrain.size), terrain.size, 16);
			
			
			//float square = terrainRect.width * terrainRect.height;
			//float count = square*(density/1000000); //number of items per terrain
			
			//positioned scatter
			/*float sideCount = Mathf.Sqrt(count);
			float step = spatialHash.size / sideCount;

			//int uniformity = 100;
			//Random.seed = 12345;
			for (float x=spatialHash.offset.x+step/2; x<spatialHash.offset.x+spatialHash.size-step/2; x+=step)
				for (float y=spatialHash.offset.y+step/2; y<spatialHash.offset.y+spatialHash.size-step/2; y+=step)
			{
				Vector2 offset = new Vector2(((Random.value*2-1)*uniformity), ((Random.value*2-1)*uniformity));
				Vector2 point = new Vector2(x,y) + offset;
				if (point.x > spatialHash.size) point.x -= spatialHash.size; if (point.x < 0) point.x += spatialHash.size;
				if (point.y > spatialHash.size) point.y -= spatialHash.size; if (point.y < 0) point.y += spatialHash.size;
				spatialHash.Add(point, 0,0,0);
			}*/

			//realRandom algorithm
			int candidatesNum = (int)(uniformity*100);
			
			for (int i=0; i<count; i++)
			{
				Vector2 bestCandidate = Vector3.zero;
				float bestDist = 0;
				
				for (int c=0; c<candidatesNum; c++)
				{
					Vector2 candidate = new Vector2((spatialHash.offset.x+1) + (rnd.Random()*(spatialHash.size-2.01f)), (spatialHash.offset.y+1) + (rnd.Random()*(spatialHash.size-2.01f)));
				
					//checking if candidate available here according to probability map
					if (probMatrix!=null && probMatrix[candidate] < rnd.Random()+0.0001f) continue;

					//checking if candidate is the furthest one
					float dist = spatialHash.MinDist(candidate);
					if (dist>bestDist) { bestDist=dist; bestCandidate = candidate; }
				}

				if (bestDist>0.001f) spatialHash.Add(bestCandidate, 0, 0, 1); //adding only if some suitable candidate found
			}

			if (chunk.stop) return;
			output.SetObject(chunk, spatialHash);
		}


		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); probability.DrawIcon(layout, "Probability"); output.DrawIcon(layout, "Output");
			layout.Par(5);

			//params
			layout.Field(ref seed, "Seed");
			layout.Field(ref count, "Count");
			layout.Field(ref uniformity, "Uniformity", max:1);
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Blend (Legacy)", disengageable = true, disabled = true)]
	public class BlendGenerator : Generator
	{
		public Input baseInput = new Input(InoutType.Map);
		public Input blendInput = new Input(InoutType.Map);
		public Input maskInput = new Input(InoutType.Map);
		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Input> Inputs() { yield return baseInput; yield return blendInput; yield return maskInput; }
		public override IEnumerable<Output> Outputs() { yield return output; }

		public enum Algorithm {mix=0, add=1, subtract=2, multiply=3, divide=4, difference=5, min=6, max=7, overlay=8, hardLight=9, softLight=10} 
		public Algorithm algorithm = Algorithm.mix;
		public float opacity = 1;

		//outdated
		public enum GuiAlgorithm {mix, add, subtract, multiply, divide, difference, min, max, overlay, hardLight, softLight, none} 
		public GuiAlgorithm guiAlgorithm = GuiAlgorithm.mix;

		public static System.Func<float,float,float> GetAlgorithm (Algorithm algorithm)
		{
			switch (algorithm)
			{
				case Algorithm.mix: return delegate (float a, float b) { return b; };
				case Algorithm.add: return delegate (float a, float b) { return a+b; };
				case Algorithm.subtract: return delegate (float a, float b) { return a-b; };
				case Algorithm.multiply: return delegate (float a, float b) { return a*b; };
				case Algorithm.divide: return delegate (float a, float b) { return a/b; };
				case Algorithm.difference: return delegate (float a, float b) { return Mathf.Abs(a-b); };
				case Algorithm.min: return delegate (float a, float b) { return Mathf.Min(a,b); };
				case Algorithm.max: return delegate (float a, float b) { return Mathf.Max(a,b); };
				case Algorithm.overlay: return delegate (float a, float b) 
				{
					if (a > 0.5f) return 1 - 2*(1-a)*(1-b);
					else return 2*a*b; 
				}; 
				case Algorithm.hardLight: return delegate (float a, float b) 
				{
						if (b > 0.5f) return 1 - 2*(1-a)*(1-b);
						else return 2*a*b; 
				};
				case Algorithm.softLight: return delegate (float a, float b) { return (1-2*b)*a*a + 2*b*a; };
				default: return delegate (float a, float b) { return b; };
			}
		}


		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//preparing inputs
			Matrix baseMatrix = (Matrix)baseInput.GetObject(chunk);
			Matrix blendMatrix = (Matrix)blendInput.GetObject(chunk);
			Matrix maskMatrix = (Matrix)maskInput.GetObject(chunk);
			
			//return on stop/disable/null input
			if (chunk.stop) return;
			if (!enabled || blendMatrix==null || baseMatrix==null) { output.SetObject(chunk, baseMatrix); return; }
			
			//preparing output
			baseMatrix = baseMatrix.Copy(null);

			//setting algorithm
			if (guiAlgorithm != GuiAlgorithm.none) { algorithm = (Algorithm)guiAlgorithm; guiAlgorithm = GuiAlgorithm.none; }
			System.Func<float,float,float> algorithmFn = GetAlgorithm(algorithm);


			//special fast cases for mix and add
			/*if (maskMatrix == null && guiAlgorithm == GuiAlgorithm.mix)
				for (int i=0; i<baseMatrix.array.Length; i++)
				{
					float a = baseMatrix.array[i];
					float b = blendMatrix.array[i];
					baseMatrix.array[i] = a*(1-opacity) + b*opacity;
				}
			else if (maskMatrix != null && guiAlgorithm == GuiAlgorithm.mix)
				for (int i=0; i<baseMatrix.array.Length; i++)
				{
					float m = maskMatrix.array[i] * opacity;
					float a = baseMatrix.array[i];
					float b = blendMatrix.array[i];
					baseMatrix.array[i] = a*(1-m) + b*m;
				}
			else if (maskMatrix == null && guiAlgorithm == GuiAlgorithm.add)
				for (int i=0; i<baseMatrix.array.Length; i++)
				{
					float a = baseMatrix.array[i];
					float b = blendMatrix.array[i];
					baseMatrix.array[i] = a + b*opacity;
				}
			else if (maskMatrix != null && guiAlgorithm == GuiAlgorithm.mix)
				for (int i=0; i<baseMatrix.array.Length; i++)
				{
					float m = maskMatrix.array[i] * opacity;
					float a = baseMatrix.array[i];
					float b = blendMatrix.array[i];
					baseMatrix.array[i] = a + b*m;
				}*/

			//generating all other
			//else
				for (int i=0; i<baseMatrix.array.Length; i++)
				{
					float m = (maskMatrix==null ? 1 : maskMatrix.array[i]) * opacity;
					float a = baseMatrix.array[i];
					float b = blendMatrix.array[i];

					baseMatrix.array[i] = a*(1-m) + algorithmFn(a,b)*m;
				}
		
			//setting output
			if (chunk.stop) return;
			output.SetObject(chunk, baseMatrix);
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); baseInput.DrawIcon(layout, "Base", mandatory:true); output.DrawIcon(layout, "Output");
			layout.Par(20); blendInput.DrawIcon(layout, "Blend", mandatory:true);
			layout.Par(20); maskInput.DrawIcon(layout, "Mask");
			layout.Par(5);
			
			//params
			if (guiAlgorithm != GuiAlgorithm.none) { algorithm = (Algorithm)guiAlgorithm; guiAlgorithm = GuiAlgorithm.none; }
			layout.Field(ref algorithm, "Algorithm");
			layout.Field(ref opacity, "Opacity");
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Blend", disengageable = true, disabled = true)]
	public class BlendGenerator1 : Generator
	{
		public enum Algorithm {mix=0, add=1, subtract=2, multiply=3, divide=4, difference=5, min=6, max=7, overlay=8, hardLight=9, softLight=10} 
		
		public class Layer
		{
			public Input input = new Input(InoutType.Map);
			public Algorithm algorithm = Algorithm.add;
			public float opacity = 1;
		}
		public Layer[] layers = new Layer[] { new Layer(), new Layer() };
		public int guiSelected;

		public Input maskInput = new Input(InoutType.Map);
		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Input> Inputs() { for (int i=0; i<layers.Length; i++) yield return layers[i].input; yield return maskInput; }
		public override IEnumerable<Output> Outputs() { yield return output; }

		public int inputsNum = 2;

		public static System.Func<float,float,float> GetAlgorithm (Algorithm algorithm)
		{
			switch (algorithm)
			{
				case Algorithm.mix: return delegate (float a, float b) { return b; };
				case Algorithm.add: return delegate (float a, float b) { return a+b; };
				case Algorithm.subtract: return delegate (float a, float b) { return a-b; };
				case Algorithm.multiply: return delegate (float a, float b) { return a*b; };
				case Algorithm.divide: return delegate (float a, float b) { return a/b; };
				case Algorithm.difference: return delegate (float a, float b) { return Mathf.Abs(a-b); };
				case Algorithm.min: return delegate (float a, float b) { return Mathf.Min(a,b); };
				case Algorithm.max: return delegate (float a, float b) { return Mathf.Max(a,b); };
				case Algorithm.overlay: return delegate (float a, float b) 
				{
					if (a > 0.5f) return 1 - 2*(1-a)*(1-b);
					else return 2*a*b; 
				}; 
				case Algorithm.hardLight: return delegate (float a, float b) 
				{
						if (b > 0.5f) return 1 - 2*(1-a)*(1-b);
						else return 2*a*b; 
				};
				case Algorithm.softLight: return delegate (float a, float b) { return (1-2*b)*a*a + 2*b*a; };
				default: return delegate (float a, float b) { return b; };
			}
		}


		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//return on stop/disable/null input
			if (chunk.stop || layers.Length==0) return;
			Matrix baseMatrix = (Matrix)layers[0].input.GetObject(chunk);
			Matrix maskMatrix = (Matrix)maskInput.GetObject(chunk);
			if (!enabled || layers.Length==1) { output.SetObject(chunk,baseMatrix); return; }

			//preparing output
			Matrix matrix = null;
			if (baseMatrix==null) matrix = chunk.defaultMatrix;
			else matrix = baseMatrix.Copy(null); //7994

			//processing
			for (int l=0; l<layers.Length; l++)
			{
				Layer layer = layers[l];
				Matrix layerMatrix = (Matrix)layer.input.GetObject(chunk);
				if (layerMatrix==null) continue;

				System.Func<float,float,float> algorithmFn = GetAlgorithm(layer.algorithm);

				for (int i=0; i<matrix.array.Length; i++)
				{
					float m = (maskMatrix==null ? 1 : maskMatrix.array[i]) * layer.opacity;
					float a = matrix.array[i];
					float b = layerMatrix.array[i];
					float v = 0;

					switch (layer.algorithm)
					{
						case Algorithm.mix: v = a*(1-m) + b*m; break;
						case Algorithm.add: v = a+b; break;//a*(1-m) + (a+b)*m; break;
						case Algorithm.multiply: v = a*(1-m) + (a*b)*m; break;
						case Algorithm.subtract: v = a*(1-m) + (a-b)*m; break;
						default: v = a*(1-m) + algorithmFn(a,b)*m; break;
					}

					//if (v<0) v=0; if (v>1) v=1;

					matrix.array[i] = v;

					//matrix.array[i] = a*(1-m) + algorithmFn(a,b)*m;
				}
			}

			//special fast cases for mix and add
			/*if (maskMatrix == null && guiAlgorithm == GuiAlgorithm.mix)
				for (int i=0; i<baseMatrix.array.Length; i++)
				{
					float a = baseMatrix.array[i];
					float b = blendMatrix.array[i];
					baseMatrix.array[i] = a*(1-opacity) + b*opacity;
				}
			else if (maskMatrix != null && guiAlgorithm == GuiAlgorithm.mix)
				for (int i=0; i<baseMatrix.array.Length; i++)
				{
					float m = maskMatrix.array[i] * opacity;
					float a = baseMatrix.array[i];
					float b = blendMatrix.array[i];
					baseMatrix.array[i] = a*(1-m) + b*m;
				}
			else if (maskMatrix == null && guiAlgorithm == GuiAlgorithm.add)
				for (int i=0; i<baseMatrix.array.Length; i++)
				{
					float a = baseMatrix.array[i];
					float b = blendMatrix.array[i];
					baseMatrix.array[i] = a + b*opacity;
				}
			else if (maskMatrix != null && guiAlgorithm == GuiAlgorithm.mix)
				for (int i=0; i<baseMatrix.array.Length; i++)
				{
					float m = maskMatrix.array[i] * opacity;
					float a = baseMatrix.array[i];
					float b = blendMatrix.array[i];
					baseMatrix.array[i] = a + b*m;
				}*/
		
			//setting output
			if (chunk.stop) return;
			output.SetObject(chunk, matrix);
		}

		public void OnLayerGUI (Layer layer, Layout layout, int num, bool selected) 
		{
			layout.margin += 10; layout.rightMargin +=5;
			layout.Par(20);
			layer.input.DrawIcon(layout, "", mandatory:false);
			layout.Field(ref layer.algorithm, rect:layout.Inset(0.5f), disabled:num==0);
			layout.Inset(0.05f);
			layout.Icon("MapMagic_Opacity", rect:layout.Inset(0.1f), horizontalAlign:Layout.IconAligment.center, verticalAlign:Layout.IconAligment.center);
			layout.Field(ref layer.opacity, rect:layout.Inset(0.35f), disabled:num==0);
			layout.margin -= 10; layout.rightMargin -=5;
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); maskInput.DrawIcon(layout, "Mask"); output.DrawIcon(layout, "Output");
			layout.Par(5);
			
			//params
			layout.Par(16);
			layout.Label("Layers:", layout.Inset(0.4f));
			layout.DrawArrayAdd(ref layers, ref guiSelected, layout.Inset(0.15f), def:new Layer());
			layout.DrawArrayRemove(ref layers, ref guiSelected, layout.Inset(0.15f));
			layout.DrawArrayUp(ref layers, ref guiSelected, layout.Inset(0.15f));
			layout.DrawArrayDown(ref layers, ref guiSelected, layout.Inset(0.15f));

			layout.margin = 10;
			layout.Par(5);
			layout.DrawLayered(layers, ref guiSelected, min:0, max:layers.Length, reverseOrder:true, onLayerGUI:OnLayerGUI);
		}
	}

}
