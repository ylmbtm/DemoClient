using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//using Plugins;

namespace MapMagic
{

	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Test", disengageable = true, disabled = true, priority = 1)]
	public class TestGenerator : Generator
	{
		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Output> Outputs() { yield return output; }
		public int iterations = 100000;
		public float result;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			Matrix matrix = chunk.defaultMatrix;
			if (!enabled) { output.SetObject(chunk, matrix); return; }

			//testing matrix
			for (int x=0; x<matrix.rect.size.x; x++)
				for (int z=0; z<matrix.rect.size.z; z++)
					matrix[x+matrix.rect.offset.x, z+matrix.rect.offset.z] = 0.3f*x/matrix.rect.size.x*5f;// + 0.5f*z/matrix.rect.size.z;
			
			//testing performance

			//for (int i=iterations*1000-1; i>=0; i--) 
			//	{ result = InlineFn(result); result = InlineFn(result); result = InlineFn(result); result = InlineFn(result); result = InlineFn(result); }
				//{ result += 0.01f; result += 0.01f; result += 0.01f; result += 0.01f; result += 0.01f; }

			if (chunk.stop) return; //do not write object is generating is stopped
			output.SetObject(chunk, matrix);
		}

		public float InlineFn (float input)
		{
			return input + 0.01f; 
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); output.DrawIcon(layout, "Output");
			layout.Par(5);
			
			//params
			//output.sharedResolution.guiResolution = layout.ComplexField(output.sharedResolution.guiResolution, "Output Resolution");
			layout.fieldSize = 0.55f;
			layout.Field(ref iterations, "K Iterations");
			layout.Field(ref result, "Result");
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Constant", disengageable = true)]
	public class ConstantGenerator : Generator
	{
		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Output> Outputs() { yield return output; }
		public float level;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			Matrix matrix = chunk.defaultMatrix;
			if (!enabled) { output.SetObject(chunk, matrix); return; }
			matrix.Fill(level);

			if (chunk.stop) return; //do not write object is generating is stopped
			output.SetObject(chunk, matrix);
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); output.DrawIcon(layout, "Output");
			layout.Par(5);
			
			//params
			//output.sharedResolution.guiResolution = layout.ComplexField(output.sharedResolution.guiResolution, "Output Resolution");
			layout.Field(ref level, "Value", max:1);
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Noise", disengageable = true)]
	public class NoiseGenerator1 : Generator
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

			Noise noise = new Noise(size, matrix.rect.size.x, MapMagic.instance.seed + seed*7, MapMagic.instance.seed + seed*3);
			Coord min = matrix.rect.Min; Coord max = matrix.rect.Max;
			for (int x=min.x; x<max.x; x++)
				for (int z=min.z; z<max.z; z++)
			{
				float result = noise.Fractal(x+(int)(offset.x), z+(int)(offset.y), detail);
									
				//apply contrast and bias
				result = result*intensity;
				result -= 0*(1-bias) + (intensity-1)*bias; //0.5f - intensity*bias;

				if (result < 0) result = 0; 
				if (result > 1) result = 1;

				if (mask==null) matrix[x,z] += result;
				else matrix[x,z] += result*mask[x,z];
			}

			//Noise(matrix, size, intensity, bias, detail, offset, seed, mask);
			
			if (chunk.stop) return; //do not write object is generating is stopped
			output.SetObject(chunk, matrix);
		}

/*		public static void Noise (Matrix matrix, float size, float intensity=1, float bias=0, float detail=0.5f, Vector2 offset=new Vector2(), int seed=12345, Matrix mask=null)
		{
			//int step = (int)(4096f / matrix.rect.size.x);

			int totalSeedX = ((MapMagic.instance.seed + seed*7) % 77777);
			int totalSeedZ = ((MapMagic.instance.seed + seed*3) % 73333);

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
					float curSize = size;
					float curAmount = 1;

					//making x and z resolution independent
					float rx = 1f*(x+(int)offset.x) / matrix.rect.size.x * 512;
					float rz = 1f*(z+(int)offset.y) / matrix.rect.size.z * 512;
				
					//applying noise
					for (int i=0; i<numIterations;i++)
					{
						float curSizeBkcw = 8/(curSize*10+1); //for backwards compatibility. Use /curSize to get rid of extra calcualtions
						
						float perlin = Mathf.PerlinNoise(
							(rx + totalSeedX + 1000*(i+1))*curSizeBkcw, 
							(rz + totalSeedZ + 100*i)*curSizeBkcw );
						perlin = (perlin-0.5f)*curAmount + 0.5f;

						//applying overlay
						if (perlin > 0.5f) result = 1 - 2*(1-result)*(1-perlin);
						else result = 2*perlin*result;

						//result = 2*perlin*result;

						curSize *= 0.5f;
						curAmount *= detail; //detail is 0.5 by default
					}
					
					//result = Mathf.PerlinNoise(rx/1.52f, rz/1.52f);

					//apply contrast and bias
					result = result*intensity;
					result -= 0*(1-bias) + (intensity-1)*bias; //0.5f - intensity*bias;

					if (result < 0) result = 0; 
					if (result > 1) result = 1;

					if (mask==null) matrix[x,z] += result;
					else matrix[x,z] += result*mask[x,z];
				}
			}
		}*/

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
	[GeneratorMenu (menu="Map", name ="Voronoi", disengageable = true)]
	public class VoronoiGenerator1 : Generator
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
			float finalIntensity = intensity * cellCount / matrix.rect.size.x * 26; //backward compatibility factor

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
				if (mask==null) matrix[x,z] += val*finalIntensity;
				else matrix[x,z] += val*finalIntensity*mask[x,z];
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
			layout.Field(ref cellCount, "Cell Count", min:1, max:128); cellCount = Mathf.ClosestPowerOfTwo(cellCount);
			layout.Field(ref uniformity, "Uniformity", min:0, max:1);
			layout.Field(ref seed, "Seed");
		}
	}

	/*[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Texture Input", disengageable = true)]
	public class TextureInput : Generator
	{
		public Output output = new Output("Output", InoutType.Map);
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
			
			if (chunk.stop) return;
			output.SetObject(chunk, matrix);
		}

		public override void OnGUI (Layout layout)
		{
			//inouts
			layout.Par(20); output.DrawIcon(layout);
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
	}*/

	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Simple Form", disengageable = true)]
	public class SimpleForm1 : Generator
	{
		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Output> Outputs() { yield return output; }

		public enum FormType { GradientX, GradientZ, Pyramid, Cone }
		public FormType type = FormType.Cone;
		public float intensity = 1;
		public float scale = 1;
		public Vector2 offset;
		public Matrix.WrapMode wrap = Matrix.WrapMode.Once;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			if (!enabled || chunk.stop) return;
			
			Matrix matrix = chunk.defaultMatrix;
			Coord min = matrix.rect.Min; Coord max = matrix.rect.Max;
			
			float pixelSize = 1f * MapMagic.instance.terrainSize / matrix.rect.size.x;
			float ssize = matrix.rect.size.x; //scaled rect size
			Vector2 center = new Vector2(matrix.rect.size.x/2f, matrix.rect.size.z/2f);
			float radius = matrix.rect.size.x / 2f;

			for (int x=min.x; x<max.x; x++)
				for (int z=min.z; z<max.z; z++)
			{
				float sx = (x - offset.x/pixelSize)/scale;
				float sz = (z - offset.y/pixelSize)/scale;

				if (wrap==Matrix.WrapMode.Once && (sx<0 || sx>=ssize || sz<0 || sz>=ssize)) { matrix[x,z] = 0; continue; }
				else if (wrap==Matrix.WrapMode.Clamp)
				{
					if (sx<0) sx=0; if (sx>=ssize) sx=ssize-1;
					if (sz<0) sz=0; if (sz>=ssize) sz=ssize-1;
				}
				else if (wrap==Matrix.WrapMode.Tile)
				{
					sx = sx % ssize; if (sx<0) sx= ssize + sx;
					sz = sz % ssize; if (sz<0) sz= ssize + sz;
				}
				else if (wrap==Matrix.WrapMode.PingPong)
				{
					sx = sx % (ssize*2); if (sx<0) sx=ssize*2 + sx; if (sx>=ssize) sx = ssize*2 - sx - 1;
					sz = sz % (ssize*2); if (sz<0) sz=ssize*2 + sz; if (sz>=ssize) sz = ssize*2 - sz - 1;
				}
				
				float val = 0;
				switch (type)
				{
					case FormType.GradientX:
						val = sx/ssize;
						break;
					case FormType.GradientZ:
						val = sz/ssize;
						break;
					case FormType.Pyramid:
						float valX = sx/ssize; if (valX > 1-valX) valX = 1-valX;
						float valZ = sz/ssize; if (valZ > 1-valZ) valZ = 1-valZ;
						val = valX<valZ? valX*2 : valZ*2;
						break;
					case FormType.Cone:
						val = 1 - ((center-new Vector2(sx,sz)).magnitude)/radius;
						if (val<0) val = 0;
						break;
				}

				matrix[x,z] = val*intensity;
			}

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
			layout.Field(ref wrap, "Wrap Mode");
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Raw Input", disengageable = true, disabled = false)]
	public class RawInput1 : Generator
	{
		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Output> Outputs() { yield return output; }

		public MatrixAsset refMatrixAsset;
		public float intensity = 1;
		public float scale = 1;
		public Vector2 offset;
		public Matrix.WrapMode wrapMode = Matrix.WrapMode.Once;

		//gui
		[System.NonSerialized] public Texture2D preview; 
		public string texturePath;  
		
		//outdated
		public Matrix textureMatrix; 
		//public Matrix previewMatrix;
		public bool tile = false;

		public void ImportRaw (string path=null)
		{
			#if UNITY_EDITOR
			//importing
			if (path==null) path = UnityEditor.EditorUtility.OpenFilePanel("Import Texture File", "", "raw,r16");
			if (path==null || path.Length==0) return;

			UnityEditor.Undo.RecordObject(MapMagic.instance.gens, "MapMagic Open RAW");
			MapMagic.instance.gens.setDirty = !MapMagic.instance.gens.setDirty;

			//creating ref matrix
			Matrix matrix = new Matrix( new CoordRect(0,0,1,1) );
			matrix.ImportRaw(path);
			texturePath = path;

			//saving asset
			if (refMatrixAsset == null) refMatrixAsset = ScriptableObject.CreateInstance<MatrixAsset>();
			refMatrixAsset.matrix = matrix;

			//generating preview
			CoordRect previewRect = new CoordRect(0,0, 128, 128);
			refMatrixAsset.preview = matrix.Resize(previewRect);
			preview = null;

			UnityEditor.EditorUtility.SetDirty(MapMagic.instance.gens);
			#endif
		}

		public void RefreshPreview ()
		{
			if (refMatrixAsset!=null && refMatrixAsset.preview!=null) preview = refMatrixAsset.preview.SimpleToTexture();
			else preview = Extensions.ColorTexture(2,2,Color.black);
		}

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			if (chunk.stop || !enabled || refMatrixAsset==null || refMatrixAsset.matrix==null) return;

			//loading ref matrix
			if (textureMatrix!=null && refMatrixAsset==null)
			{
				refMatrixAsset = ScriptableObject.CreateInstance<MatrixAsset>();
				refMatrixAsset.matrix = textureMatrix;
				textureMatrix = null;
			}
			Matrix refMatrix = refMatrixAsset.matrix;

			Matrix matrix = chunk.defaultMatrix;
			Coord min = matrix.rect.Min; Coord max = matrix.rect.Max;
			float pixelSize = 1f * MapMagic.instance.terrainSize / matrix.rect.size.x;

			for (int x=min.x; x<max.x; x++)
				for (int z=min.z; z<max.z; z++)
			{
				float sx = (x-offset.x/pixelSize)/scale * refMatrix.rect.size.x/matrix.rect.size.x;
				float sz = (z-offset.y/pixelSize)/scale * refMatrix.rect.size.z/matrix.rect.size.z;

				matrix[x,z] = refMatrix.GetInterpolated(sx, sz, wrapMode) * intensity;
			}

			if (scale >= 2f)
			{
				Matrix cpy = matrix.Copy();
				for (int i=1; i<scale-1; i+=2) matrix.Blur();
				Matrix.SafeBorders(cpy, matrix, Mathf.Max(matrix.rect.size.x/128, 4));
			}
			
			if (chunk.stop) return;
			output.SetObject(chunk, matrix);
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); output.DrawIcon(layout, "Output");
			layout.Par(5); 

			//loading ref matrix
			if (textureMatrix!=null && refMatrixAsset==null)
			{
				refMatrixAsset = ScriptableObject.CreateInstance<MatrixAsset>();
				refMatrixAsset.matrix = textureMatrix;
				textureMatrix = null;
			}
			
			//preview texture
			layout.margin = 4;
			
			#if UNITY_EDITOR

			layout.fieldSize = 0.6f;
			layout.Field(ref refMatrixAsset, "Imported RAW");
			if (layout.lastChange) RefreshPreview();

			//warning
			bool rawSaved = refMatrixAsset==null || UnityEditor.AssetDatabase.Contains(refMatrixAsset);
			bool gensSaved = UnityEditor.AssetDatabase.Contains(MapMagic.instance.guiGens);

			if (!rawSaved)
			{
				if (!gensSaved)
					{ layout.Par(65); layout.Label("It is recommended that the imported .RAW file be saved as a separate .ASSET file.", layout.Inset(), helpbox:true, messageType:2, fontSize:9); }	
				else 
					{ layout.Par(85); layout.Label("Warning: Since Graph is saved as a separate data imported RAW should be saved too, otherwise RAW FILE WILL NOT BE LOADED.", layout.Inset(), helpbox:true, messageType:3, fontSize:9); }
			}

			//save
			if (layout.Button("Save Imported RAW", disabled:refMatrixAsset==null)) 
			{ 
				//releasing data on re-save
				if (UnityEditor.AssetDatabase.Contains(refMatrixAsset))
				{
					MatrixAsset newRefMatrixAsset = ScriptableObject.CreateInstance<MatrixAsset>();
					newRefMatrixAsset.matrix = refMatrixAsset.matrix.Copy();
					newRefMatrixAsset.preview = refMatrixAsset.preview.Copy();
					refMatrixAsset = newRefMatrixAsset;
				}
					

				string path= UnityEditor.EditorUtility.SaveFilePanel(
					"Save Data as Unity Asset",
					"Assets",
					"ImportedRAW.asset", 
					"asset");
				if (path!=null && path.Length!=0)
				{
					path = path.Replace(Application.dataPath, "Assets");
					UnityEditor.Undo.RecordObject(MapMagic.instance, "MapMagic Save Data");
					MapMagic.instance.setDirty = !MapMagic.instance.setDirty;
					UnityEditor.AssetDatabase.CreateAsset(refMatrixAsset, path);
					UnityEditor.EditorUtility.SetDirty(MapMagic.instance);
				}
			}
			layout.Par(10);
							
			//preview
			int previewSize = 70;
			int controlsSize = (int)layout.field.width - previewSize - 10;
			Rect oldCursor = layout.cursor;
			if (preview == null) RefreshPreview();
			layout.Par(previewSize+3); layout.Inset(controlsSize);
			layout.Icon(preview, layout.Inset(previewSize+4));
			layout.cursor = oldCursor;
			
			//load raw
			layout.Par(); if (layout.Button("Browse", rect:layout.Inset(controlsSize))) { ImportRaw(); layout.change = true; }
			layout.Par(); if (layout.Button("Refresh", rect:layout.Inset(controlsSize))) { ImportRaw(texturePath); layout.change = true; }
			layout.Par(40); layout.Label("Square gray 16bit RAW, PC byte order", layout.Inset(controlsSize), helpbox:true, fontSize:9);
			
			layout.Par(5);
			#endif



			//checking if matrix asset loaded
			if (MapMagic.instance.guiDebug)
			{
				if (refMatrixAsset == null) layout.Label("Matrix asset is null");
				else
				{
					if (refMatrixAsset.matrix != null) layout.Label("Matrix loaded: " + refMatrixAsset.matrix.rect.size.x);
					else layout.Label("Matrix NOT loaded");
				}

				if (textureMatrix == null) layout.Label("Texture matrix is null");
				else layout.Label("Texture matrix loaded: " + textureMatrix.rect.size.x);
			}

			layout.fieldSize = 0.62f;
			layout.Field(ref intensity, "Intensity");
			layout.Field(ref scale, "Scale");
			layout.Field(ref offset, "Offset");
			if (tile) wrapMode = Matrix.WrapMode.Tile; tile=false;
			layout.Field(ref wrapMode, "Wrap Mode");
		}
	}

	/*[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Raw Input", disengageable = true, disabled = false)]
	public class RawInput1 : Generator
	{
		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Output> Outputs() { yield return output; }

		public Matrix textureMatrix;
		public Matrix previewMatrix;
		[System.NonSerialized] public Texture2D preview;  
		public string texturePath; 
		public float intensity = 1;
		public float scale = 1;
		public Vector2 offset;
		public bool tile = false; //outdated
		public Matrix.WrapMode wrapMode = Matrix.WrapMode.Once;

		public void ImportRaw (string path=null)
		{
			#if UNITY_EDITOR
			//importing
			if (path==null) path = UnityEditor.EditorUtility.OpenFilePanel("Import Texture File", "", "raw,r16");
			if (path==null || path.Length==0) return;

			UnityEditor.Undo.RecordObject(MapMagic.instance.gens, "MapMagic Open RAW");
			MapMagic.instance.gens.setDirty = !MapMagic.instance.gens.setDirty;

			//if (textureAsset == null) textureAsset = ScriptableObject.CreateInstance<MatrixAsset>();
			if (textureMatrix == null) textureMatrix = new Matrix( new CoordRect(0,0,1,1) );

			textureMatrix.ImportRaw(path);
			texturePath = path;

			//generating preview
			CoordRect previewRect = new CoordRect(0,0, 70, 70);
			previewMatrix = textureMatrix.Resize(previewRect, previewMatrix);
			preview = previewMatrix.SimpleToTexture();
			UnityEditor.EditorUtility.SetDirty(MapMagic.instance.gens);
			#endif
		}

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			if (!enabled || textureMatrix==null) { output.SetObject(chunk, null); return; }
			if (chunk.stop) return;

			Matrix matrix = chunk.defaultMatrix;
			Coord min = matrix.rect.Min; Coord max = matrix.rect.Max;
			float pixelSize = 1f * MapMagic.instance.terrainSize / matrix.rect.size.x;

			for (int x=min.x; x<max.x; x++)
				for (int z=min.z; z<max.z; z++)
			{
				float sx = (x-offset.x/pixelSize)/scale * textureMatrix.rect.size.x/matrix.rect.size.x;
				float sz = (z-offset.y/pixelSize)/scale * textureMatrix.rect.size.z/matrix.rect.size.z;

				matrix[x,z] = textureMatrix.GetInterpolated(sx, sz, wrapMode);
			}

			if (scale >= 2f)
			{
				Matrix cpy = matrix.Copy();
				for (int i=1; i<scale-1; i+=2) matrix.Blur();
				Matrix.SafeBorders(cpy, matrix, Mathf.Max(matrix.rect.size.x/128, 4));
			}
			
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
			if (tile) wrapMode = Matrix.WrapMode.Tile; tile=false;
			layout.Field(ref wrapMode, "Wrap Mode");
		}
	}*/

	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Texture Input", disengageable = true)]
	public class TextureInput : Generator
	{
		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Output> Outputs() { yield return output; }

		public Texture2D texture;
		public bool loadEachGen = false;
		
		//[System.NonSerialized] private object matrixLocker = new object();
		public float intensity = 1;
		public float scale = 1;
		public Vector2 offset;
		public Matrix.WrapMode wrapMode = Matrix.WrapMode.Once;
		
		public bool tile = false; //outdated
		[System.NonSerialized] public Matrix textureMatrix = new Matrix();


		public void CheckLoadTexture (bool force=false)
		{
			if (texture==null) return;
			lock (textureMatrix)
			{
				if (textureMatrix.rect.size.x!=texture.width || textureMatrix.rect.size.z!=texture.height || force)
				{
					textureMatrix.ChangeRect( new CoordRect(0,0, texture.width, texture.height), forceNewArray:true );
					try { textureMatrix.FromTexture(texture); }
					catch (UnityException e) { Debug.LogError(e); }
				}
			}
		}

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			if (chunk.stop || !enabled || textureMatrix==null) return;

			Matrix matrix = chunk.defaultMatrix;
			Coord min = matrix.rect.Min; Coord max = matrix.rect.Max;
			float pixelSize = 1f * MapMagic.instance.terrainSize / matrix.rect.size.x;
			 
			for (int x=min.x; x<max.x; x++)
				for (int z=min.z; z<max.z; z++)
			{
				float sx = (x-offset.x/pixelSize)/scale * textureMatrix.rect.size.x/matrix.rect.size.x;
				float sz = (z-offset.y/pixelSize)/scale * textureMatrix.rect.size.z/matrix.rect.size.z;

				matrix[x,z] = textureMatrix.GetInterpolated(sx, sz, wrapMode) * intensity;
			}

			if (scale >= 2f)
			{
				Matrix cpy = matrix.Copy();
				for (int i=1; i<scale-1; i+=2) matrix.Blur();
				Matrix.SafeBorders(cpy, matrix, Mathf.Max(matrix.rect.size.x/128, 4));
			}

			if (chunk.stop) return;
			output.SetObject(chunk, matrix);
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); output.DrawIcon(layout, "Output");
			layout.Par(5);
			
			//preview texture
			layout.fieldSize = 0.62f;
			layout.Field(ref texture, "Texture");
			if (layout.Button("Reload")) CheckLoadTexture(force:true); //ReloadTexture();
			layout.Toggle(ref loadEachGen, "Reload Each Generate");
			layout.Field(ref intensity, "Intensity");
			layout.Field(ref scale, "Scale");
			layout.Field(ref offset, "Offset");
			if (tile) wrapMode = Matrix.WrapMode.Tile; tile=false;
			layout.Field(ref wrapMode, "Wrap Mode");
		}
	}
	
	
	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Intensity/Bias", disengageable = true)]
	public class IntensityBiasGenerator : Generator
	{
		public float intensity = 1f;
		public float bias = 0.0f;

		public Input input = new Input(InoutType.Map);//, mandatory:true);
		public Input maskIn = new Input(InoutType.Map);
		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Input> Inputs() { yield return input; yield return maskIn; }
		public override IEnumerable<Output> Outputs() { yield return output; }

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//getting input
			Matrix src = (Matrix)input.GetObject(chunk);

			//return on stop/disable/null input
			if (chunk.stop) return;
			if (!enabled || src==null) { output.SetObject(chunk, src); return; }

			//preparing output
			Matrix dst = src.Copy(null);

			for (int i=0; i<dst.count; i++)
			{
				float result = dst.array[i];
				
				//apply contrast and bias
				result = result*intensity;
				result -= 0*(1-bias) + (intensity-1)*bias; //0.5f - intensity*bias;

				if (result < 0) result = 0; 
				if (result > 1) result = 1;

				dst.array[i] = result;
			}
			
			//mask and safe borders
			if (chunk.stop) return;
			Matrix mask = (Matrix)maskIn.GetObject(chunk);
			if (mask != null) Matrix.Mask(src, dst, mask);

			//setting output
			if (chunk.stop) return;
			output.SetObject(chunk, dst);
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); input.DrawIcon(layout, "Input"); output.DrawIcon(layout, "Output");
			layout.Par(20); maskIn.DrawIcon(layout, "Mask");
			layout.Par(5);

			//params
			layout.Field(ref intensity, "Intensity");
			layout.Field(ref bias, "Bias");
		}
	}

	
	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Invert", disengageable = true)]
	public class InvertGenerator : Generator
	{
		//yap, this one is from the tutorial

		public Input input = new Input(InoutType.Map);
		public Output output = new Output(InoutType.Map);
		public Input maskIn = new Input(InoutType.Map);

		public override IEnumerable<Input> Inputs() { yield return input; yield return maskIn; }
		public override IEnumerable<Output> Outputs() { yield return output; }

		public float level = 1;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			Matrix src = (Matrix)input.GetObject(chunk);

			if (chunk.stop) return;
			if (!enabled || src==null) { output.SetObject(chunk, src); return; }

			Matrix dst = new Matrix(src.rect);

			Coord min = src.rect.Min; Coord max = src.rect.Max;
			for (int x=min.x; x<max.x; x++)
			   for (int z=min.z; z<max.z; z++)
			   {
					float val = level - src[x,z];
					dst[x,z] = val>0? val : 0;
				}

			//mask and safe borders
			if (chunk.stop) return;
			Matrix mask = (Matrix)maskIn.GetObject(chunk);
			if (mask != null) Matrix.Mask(src, dst, mask);

			if (chunk.stop) return;
			output.SetObject(chunk, dst);
		}

		public override void OnGUI ()
		{
			layout.Par(20); input.DrawIcon(layout, "Input", mandatory:true); output.DrawIcon(layout, "Output");
			layout.Par(20); maskIn.DrawIcon(layout, "Mask");

			layout.Field(ref level, "Level", min:0, max:1);
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Curve", disengageable = true)]
	public class CurveGenerator : Generator
	{
		//public override Type guiType { get { return Generator.Type.curve; } }
		
		public AnimationCurve curve = new AnimationCurve( new Keyframe[] { new Keyframe(0,0,1,1), new Keyframe(1,1,1,1) } );
		public bool extended = true;
		//public float inputMax = 1;
		//public float outputMax = 1;
		public Vector2 range = new Vector2(0,1);

		public Input input = new Input(InoutType.Map);//, mandatory:true);
		public Input maskIn = new Input(InoutType.Map);
		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Input> Inputs() { yield return input; yield return maskIn; }
		public override IEnumerable<Output> Outputs() { yield return output; }

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//getting input
			Matrix src = (Matrix)input.GetObject(chunk);

			//return on stop/disable/null input
			if (chunk.stop) return;
			if (!enabled || src==null) { output.SetObject(chunk, src); return; }

			//preparing output
			Matrix dst = src.Copy(null);

			//curve
			Curve c = new Curve(curve);
			for (int i=0; i<dst.array.Length; i++) dst.array[i] = c.Evaluate(dst.array[i]);
			
			//mask and safe borders
			if (chunk.stop) return;
			Matrix mask = (Matrix)maskIn.GetObject(chunk);
			if (mask != null) Matrix.Mask(src, dst, mask);

			//setting output
			if (chunk.stop) return;
			output.SetObject(chunk, dst);
		}

		/*public static void Curve (Matrix matrix, AnimationCurve curve)
		{
			//some quick curve access
			int keyCount = curve.keys.Length;
			float[] keyTimes = new float[keyCount]; float[] keyVals = new float[keyCount]; float[] keyInTangents = new float[keyCount]; float[] keyOutTangents = new float[keyCount];
			for (int k=0; k<keyCount; k++) 
			{
				keyTimes[k] = curve.keys[k].time;
				keyVals[k] = curve.keys[k].value;
				keyInTangents[k] = curve.keys[k].inTangent;
				keyOutTangents[k] = curve.keys[k].outTangent;
			}

			Curve c = new Curve(curve);

			for (int i=0; i<matrix.array.Length; i++)
			{
				matrix.array[i] = c.Evaluate(matrix.array[i]);
				//Evaluate does not work in multithread mode

				float time = matrix.array[i];

				if (time <= keyTimes[0]) { matrix.array[i] = keyVals[0]; continue; }
				if (time >= keyTimes[keyCount-1]) { matrix.array[i] = keyVals[keyCount-1]; continue; }

				int keyNum = 0;
				for (int k=0; k<keyCount-1; k++)
				{
					if (keyTimes[keyNum+1] > time) break;
					keyNum++;
				}
			
				float delta = keyTimes[keyNum+1] - keyTimes[keyNum];
				float relativeTime = (time - keyTimes[keyNum]) / delta;

				float timeSq = relativeTime * relativeTime;
				float timeCu = timeSq * relativeTime;
     
				float a = 2*timeCu - 3*timeSq + 1;
				float b = timeCu - 2*timeSq + relativeTime;
				float c = timeCu - timeSq;
				float d = -2*timeCu + 3*timeSq;

				matrix.array[i] = a*keyVals[keyNum] + b*keyOutTangents[keyNum]*delta + c*keyInTangents[keyNum+1]*delta + d*keyVals[keyNum+1];
			}
		}*/

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); input.DrawIcon(layout, "Input"); output.DrawIcon(layout, "Output");
			layout.Par(20); maskIn.DrawIcon(layout, "Mask");
			layout.Par(5);

			//params
			Rect savedCursor = layout.cursor;
			layout.Par(50, padding:0);
			layout.Inset(3);
			layout.Curve(curve, rect:layout.Inset(80, padding:0), min:range.x, max:range.y);
			layout.Par(3);

			layout.margin = 86;
			layout.cursor = savedCursor;
			layout.Label("Range:");
			//layout.Par(); layout.Label("Min:", rect:layout.Inset(0.999f)); layout.Label("Max:", rect:layout.Inset(1f));
			layout.Field(ref range);
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Blend", disengageable = true)]
	public class BlendGenerator2 : Generator
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
			Matrix matrix = chunk.defaultMatrix;

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

					switch (layer.algorithm)
					{
						case Algorithm.mix: matrix.array[i] = a*(1-m) + b*m; break;
						case Algorithm.add: matrix.array[i] = a*(1-m) + (a+b)*m; break;
						case Algorithm.multiply: matrix.array[i] = a*(1-m) + (a*b)*m; break;
						case Algorithm.subtract: matrix.array[i] = a*(1-m) + (a-b)*m; break;
						default: matrix.array[i] = a*(1-m) + algorithmFn(a,b)*m; break;
					}

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

	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Normalize", disengageable = true)]
	public class NormalizeGenerator : Generator
	{
		public enum Algorithm { sum, layers };
		public Algorithm algorithm;

		//layer
		public class Layer : Layout.ILayer
		{
			public Input input = new Input(InoutType.Map);
			public Output output = new Output(InoutType.Map);
			public float opacity = 1;

			public bool pinned { get; set; }
			public int guiHeight { get; set; }

			//outdated
			public void OnExtendedGUI (Layout layout) {}
			public void OnCollapsedGUI (Layout layout) {}

			public void OnAdd (int n) { }
			public void OnRemove (int n) { }
			public void OnSwitch (int o, int n) { }
		}
		public Layer[] baseLayers = new Layer[] { new Layer(){} };

		public int guiSelected;


		//generator
		public override IEnumerable<Input> Inputs() 
		{ 
			if (baseLayers==null) baseLayers = new Layer[0];
			for (int i=0; i<baseLayers.Length; i++) 
				if (baseLayers[i] != null && baseLayers[i].input != null)
					yield return baseLayers[i].input; 
		}
		public override IEnumerable<Output> Outputs() 
		{ 
			if (baseLayers==null) baseLayers = new Layer[0];
			for (int i=0; i<baseLayers.Length; i++) 
				if (baseLayers[i] != null && baseLayers[i].output != null)
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
			//matrices[0].Fill(1);
			
			//populating opacity array
			float[] opacities = new float[matrices.Length];
			for (int i=0; i<baseLayers.Length; i++)
				opacities[i] = baseLayers[i].opacity;
			opacities[0] = 1;

			//blending layers
			if (algorithm==Algorithm.layers) Matrix.BlendLayers(matrices, opacities);
			else Matrix.NormalizeLayers(matrices, opacities);

			//saving changed matrix results
			for (int i=0; i<baseLayers.Length; i++) 
			{
				if (chunk.stop) return; //do not write object is generating is stopped
				baseLayers[i].output.SetObject(chunk, matrices[i]);
			}
		}

		public void OnBeforeRemove (int num)
		{
			Layer layer = baseLayers[num];
			layer.input.Link(null,null); 
			Input connectedInput = layer.output.GetConnectedInput(MapMagic.instance.gens.list);
			if (connectedInput != null) connectedInput.Link(null, null);
		}

		public void OnLayerGUI (Layer layer, Layout layout, int num, bool selected) 
		{
			layout.Par(20);
			
			layer.input.DrawIcon(layout);
			layout.Inset(0.1f);
			layout.Icon("MapMagic_Opacity", rect:layout.Inset(0.1f), horizontalAlign:Layout.IconAligment.center, verticalAlign:Layout.IconAligment.center);
			layout.Field(ref layer.opacity, rect:layout.Inset(0.7f));
			layout.Inset(0.1f);
			layer.output.DrawIcon(layout);
		}

		public override void OnGUI () 
		{
			layout.Field(ref algorithm, "Algorithm");
			//layout.DrawLayered(this, "Layers:", selectable:false, drawButtons:true);

			layout.margin=5;
			layout.Par(16);
			layout.DrawArrayAdd(ref baseLayers, ref guiSelected, layout.Inset(0.15f), def:new Layer()); 
			layout.DrawArrayRemove(ref baseLayers, ref guiSelected, layout.Inset(0.15f), onBeforeRemove:OnBeforeRemove);
			layout.DrawArrayUp(ref baseLayers, ref guiSelected, layout.Inset(0.15f));
			layout.DrawArrayDown(ref baseLayers, ref guiSelected, layout.Inset(0.15f));
			layout.Par(5);
			
			layout.margin = 10; layout.rightMargin = 10; layout.fieldSize = 1f;
			layout.DrawLayered(baseLayers, ref guiSelected, onLayerGUI:OnLayerGUI);
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Blur", disengageable = true)]
	public class BlurGenerator : Generator
	{
		public Input input = new Input(InoutType.Map);
		public Input maskIn = new Input(InoutType.Map);
		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Input> Inputs() { yield return input; yield return maskIn; }
		public override IEnumerable<Output> Outputs() { yield return output; }

		public int iterations = 1;
		public float intensity = 1f;
		public int loss = 1;
		public int safeBorders = 5;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//getting input
			Matrix src = (Matrix)input.GetObject(chunk); 

			//return on stop/disable/null input
			if (chunk.stop) return; 
			if (!enabled || src==null) { output.SetObject(chunk, src); return; }
			
			//preparing output
			Matrix dst = src.Copy(null);

			//blurring beforehead if loss is on
			if (loss!=1) for (int i=0; i<iterations;i++) dst.Blur(intensity:0.666f);

			//blur with loss
			int curLoss = loss;
			while (curLoss>1)
			{
				dst.LossBlur(curLoss);
				curLoss /= 2;
			}
			
			//main blur (after loss)
			for (int i=0; i<iterations;i++) dst.Blur(intensity:1f);

			//mask and safe borders
			if (intensity < 0.9999f) Matrix.Blend(src, dst, intensity);
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
			layout.Par(20); input.DrawIcon(layout, "Input", mandatory:true); output.DrawIcon(layout, "Output");
			layout.Par(20); maskIn.DrawIcon(layout, "Mask"); 
			layout.Par(5);
			
			//params
			layout.Field(ref intensity, "Intensity", max:1);
			layout.Field(ref iterations, "Iterations", min:1);
			layout.Field(ref loss, "Loss", min:1);
			layout.Field(ref safeBorders, "Safe Borders");
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Cavity", disengageable = true)]
	public class CavityGenerator1 : Generator
	{
		public Input input = new Input(InoutType.Map);
		public Input maskIn = new Input(InoutType.Map);
		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Input> Inputs() { yield return input; yield return maskIn; }
		public override IEnumerable<Output> Outputs() { yield return output; }

		public enum CavityType { Convex, Concave }
		public CavityType type = CavityType.Convex;
		public float intensity = 1;
		public float spread = 0.5f;
		public bool normalize = true;
		public int safeBorders = 3;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//getting input
			Matrix src = (Matrix)input.GetObject(chunk);

			//return on stop/disable/null input
			if (chunk.stop) return; 
			if (!enabled || src==null) { output.SetObject(chunk, src); return; }; 

			//preparing outputs
			Matrix dst = new Matrix(src.rect);

			//cavity
			System.Func<float,float,float,float> cavityFn = delegate(float prev, float curr, float next) 
			{
				float c = curr - (next+prev)/2;
				return (c*c*(c>0?1:-1))*intensity*100000;
			};
			dst.Blur(cavityFn, intensity:1, additive:true, reference:src); //intensity is set in func
			if (chunk.stop) return;

			//borders
			dst.RemoveBorders(); 
			if (chunk.stop) return;

			//inverting
			if (type == CavityType.Concave) dst.Invert();
			if (chunk.stop) return;

			//normalizing
			if (!normalize) dst.Clamp01();
			if (chunk.stop) return;

			//spread
			dst.Spread(strength:spread); 
			if (chunk.stop) return;

			dst.Clamp01();
			
			//mask and safe borders
			if (intensity < 0.9999f) Matrix.Blend(src, dst, intensity);
			Matrix mask = (Matrix)maskIn.GetObject(chunk);
			if (mask != null) Matrix.Mask(null, dst, mask);
			if (safeBorders != 0) Matrix.SafeBorders(null, dst, safeBorders);

			//setting outputs
			output.SetObject(chunk, dst);
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); input.DrawIcon(layout, "Input", mandatory:true); output.DrawIcon(layout, "Output");
			layout.Par(20); maskIn.DrawIcon(layout, "Mask");
			layout.Par(5);
			
			//params
			layout.Field(ref type, "Type");
			layout.Field(ref intensity, "Intensity");
			layout.Field(ref spread, "Spread");
			layout.Par(3);
			layout.Toggle(ref normalize, "Normalize");
			layout.Par(15); layout.Inset(20); layout.Label(label:"Convex + Concave", rect:layout.Inset(), textAnchor:TextAnchor.LowerLeft);
			layout.Field(ref safeBorders, "Safe Borders");
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Slope", disengageable = true)]
	public class SlopeGenerator1 : Generator
	{
		public Input input = new Input(InoutType.Map);
		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Input> Inputs() { yield return input; }
		public override IEnumerable<Output> Outputs() { yield return output; }
		
		public Vector2 steepness = new Vector2(45,90);
		public float range = 5f;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//getting input
			Matrix matrix = (Matrix)input.GetObject(chunk);

			//return on stop/disable/null input
			if (chunk.stop) return; 
			if (!enabled || matrix==null) { output.SetObject(chunk, matrix); return; }; 

			//preparing output
			Matrix result = new Matrix(matrix.rect);

			//using the terain-height relative values
			float pixelSize = 1f * MapMagic.instance.terrainSize / MapMagic.instance.resolution;
			
			float min0 = Mathf.Tan((steepness.x-range/2)*Mathf.Deg2Rad) * pixelSize / MapMagic.instance.terrainHeight;
			float min1 = Mathf.Tan((steepness.x+range/2)*Mathf.Deg2Rad) * pixelSize / MapMagic.instance.terrainHeight;
			float max0 = Mathf.Tan((steepness.y-range/2)*Mathf.Deg2Rad) * pixelSize / MapMagic.instance.terrainHeight;
			float max1 = Mathf.Tan((steepness.y+range/2)*Mathf.Deg2Rad) * pixelSize / MapMagic.instance.terrainHeight;

			//dealing with 90-degree
			if (steepness.y-range/2 > 89.9f) max0 = 20000000; if (steepness.y+range/2 > 89.9f) max1 = 20000000;

			//ignoring min if it is zero
			if (steepness.x<0.0001f) { min0=0; min1=0; }

			//delta map
			System.Func<float,float,float,float> inclineFn = delegate(float prev, float curr, float next) 
			{
				float prevDelta = prev-curr; if (prevDelta < 0) prevDelta = -prevDelta;
				float nextDelta = next-curr; if (nextDelta < 0) nextDelta = -nextDelta;
				return prevDelta>nextDelta? prevDelta : nextDelta; 
			};
			result.Blur(inclineFn, intensity:1, takemax:true, reference:matrix); //intensity is set in func

			//slope map
			for (int i=0; i<result.array.Length; i++)
			{
				float delta = result.array[i];
				
				if (steepness.x<0.0001f) result.array[i] = 1-(delta-max0)/(max1-max0);
				else
				{
					float minVal = (delta-min0)/(min1-min0);
					float maxVal = 1-(delta-max0)/(max1-max0);
					float val = minVal>maxVal? maxVal : minVal;
					if (val<0) val=0; if (val>1) val=1;

					result.array[i] = val;
				}
			}

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
			layout.fieldSize = 0.6f;
			layout.Field(ref steepness, "Steepness", min:0, max:90);
			layout.Field(ref range, "Range", min:0.1f);
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Terrace", disengageable = true)]
	public class TerraceGenerator : Generator
	{
		public Input input = new Input(InoutType.Map);
		public Input maskIn = new Input(InoutType.Map);
		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Input> Inputs() { yield return input; yield return maskIn; }
		public override IEnumerable<Output> Outputs() { yield return output; }

		public int seed = 12345;
		public int num = 10;
		public float uniformity = 0.5f;
		public float steepness = 0.5f;
		public float intensity = 1f;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//getting inputs
			Matrix src = (Matrix)input.GetObject(chunk);

			//return on stop/disable/null input
			if (chunk.stop) return; 
			if (!enabled || num <= 1 || src==null) { output.SetObject(chunk, src); return; }
			
			//preparing output
			Matrix dst = src.Copy(null);

			//creating terraces
			float[] terraces = new float[num];
			InstanceRandom random = new InstanceRandom(MapMagic.instance.seed + 12345);
			
			float step = 1f / (num-1);
			for (int t=1; t<num; t++)
				terraces[t] = terraces[t-1] + step;

			for (int i=0; i<10; i++)
				for (int t=1; t<num-1; t++)
				{
					float rndVal = random.Random(terraces[t-1], terraces[t+1]);
					terraces[t] = terraces[t]*uniformity + rndVal*(1-uniformity);
				}

			//adjusting matrix
			if (chunk.stop) return;
			for (int i=0; i<dst.count; i++)
			{
				float val = dst.array[i];
				if (val > 0.999f) continue;	//do nothing with values that are out of range

				int terrNum = 0;		
				for (int t=0; t<num-1; t++)
				{
					if (terraces[terrNum+1] > val || terrNum+1 == num) break;
					terrNum++;
				}

				//kinda curve evaluation
				float delta = terraces[terrNum+1] - terraces[terrNum];
				float relativePos = (val - terraces[terrNum]) / delta;

				float percent = 3*relativePos*relativePos - 2*relativePos*relativePos*relativePos;

				percent = (percent-0.5f)*2;
				bool minus = percent<0; percent = Mathf.Abs(percent);

				percent = Mathf.Pow(percent,1f-steepness);

				if (minus) percent = -percent;
				percent = percent/2 + 0.5f;

				dst.array[i] = (terraces[terrNum]*(1-percent) + terraces[terrNum+1]*percent)*intensity + dst.array[i]*(1-intensity);
				//matrix.array[i] = a*keyVals[keyNum] + b*keyOutTangents[keyNum]*delta + c*keyInTangents[keyNum+1]*delta + d*keyVals[keyNum+1];
			}

			//mask and safe borders
			Matrix mask = (Matrix)maskIn.GetObject(chunk);
			if (mask != null) Matrix.Mask(src, dst, mask);

			//setting output
			if (chunk.stop) return;
			output.SetObject(chunk, dst);
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); input.DrawIcon(layout, "Input", mandatory:true); output.DrawIcon(layout, "Output");
			layout.Par(20); maskIn.DrawIcon(layout, "Mask");
			layout.Par(5);
			
			//params
			layout.Field(ref num, "Treads Num", min:2);
			layout.Field(ref uniformity, "Uniformity", min:0, max:1);
			layout.Field(ref steepness, "Steepness", min:0, max:1);
			layout.Field(ref intensity, "Intensity", min:0, max:1);
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Erosion", disengageable = true)]
	public class ErosionGenerator : Generator
	{
		public Input heightIn = new Input(InoutType.Map);
		public Input maskIn = new Input(InoutType.Map);
		public Output heightOut = new Output(InoutType.Map);
		public Output cliffOut = new Output(InoutType.Map);
		public Output sedimentOut = new Output(InoutType.Map);
		public override IEnumerable<Input> Inputs() { yield return heightIn; yield return maskIn; }
		public override IEnumerable<Output> Outputs() { yield return heightOut; yield return cliffOut; yield return sedimentOut; }

		public int iterations = 5;
		public float terrainDurability=0.9f;
		public float erosionAmount=1f;
		public float sedimentAmount=0.75f;
		public int fluidityIterations=3;
		public float ruffle=0.4f;
		public int safeBorders = 10;
		public float cliffOpacity = 1f;
		public float sedimentOpacity = 1f;


		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//getting inputs
			Matrix src = (Matrix)heightIn.GetObject(chunk);
			
			//return
			if (chunk.stop) return; 
			if (!enabled || iterations <= 0 || src==null) { heightOut.SetObject(chunk, src); return; }

			//creating output arrays
			Matrix dst = new Matrix(src.rect);
			Matrix dstErosion = new Matrix(src.rect);
			Matrix dstSediment = new Matrix(src.rect);

			//crating temporary arrays (with margins)
			int margins = 10;
			Matrix height = new Matrix(src.rect.offset-margins, src.rect.size+margins*2);
			height.Fill(src, removeBorders:true);

			Matrix erosion = new Matrix(height.rect);
			Matrix sediment = new Matrix(height.rect);
			Matrix internalTorrents = new Matrix(height.rect);
			int[] stepsArray = new int[1000001];
			int[] heightsInt = new int[height.count];
			int[] order = new int[height.count];

			//calculate erosion
			for (int i=0; i<iterations; i++) 
			{
				if (chunk.stop) return;

				Erosion.ErosionIteration (height, erosion, sediment, area:height.rect,
							erosionDurability:terrainDurability, erosionAmount:erosionAmount, sedimentAmount:sedimentAmount, erosionFluidityIterations:fluidityIterations, ruffle:ruffle, 
							torrents:internalTorrents, stepsArray:stepsArray, heightsInt:heightsInt, order:order);

				Coord min = dst.rect.Min; Coord max = dst.rect.Max;
				for (int x=min.x; x<max.x; x++)
					for (int z=min.z; z<max.z; z++)
						{ dstErosion[x,z] += erosion[x,z]*cliffOpacity*30f; dstSediment[x,z] += sediment[x,z]*sedimentOpacity; }
			}

			//fill dst
			dst.Fill(height);
			
			//expanding sediment map 1 pixel
			//dstSediment.Spread(strength:1, iterations:1);

			//mask and safe borders
			Matrix mask = (Matrix)maskIn.GetObject(chunk);
			if (mask != null) { Matrix.Mask(src, dst, mask); Matrix.Mask(null, dstErosion, mask); Matrix.Mask(null, dstSediment, mask); }
			if (safeBorders != 0) { Matrix.SafeBorders(src, dst, safeBorders); Matrix.SafeBorders(null, dstErosion, safeBorders); Matrix.SafeBorders(null, dstSediment, safeBorders); }
			
			//finally
			if (chunk.stop) return;
			heightOut.SetObject(chunk, dst);
			cliffOut.SetObject(chunk, dstErosion);
			sedimentOut.SetObject(chunk, dstSediment);
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); heightIn.DrawIcon(layout, "Heights", mandatory:true); heightOut.DrawIcon(layout, "Heights");
			layout.Par(20); maskIn.DrawIcon(layout, "Mask"); cliffOut.DrawIcon(layout, "Cliff");
			layout.Par(20); sedimentOut.DrawIcon(layout, "Sediment");
			layout.Par(5);
			
			//params
			//layout.SmartField(ref downscale, "Downscale", min:1); //downscale = Mathf.NextPowerOfTwo(downscale);
			//layout.ComplexField(ref preserveDetail, "Preserve Detail");
			layout.Par(30);
			layout.Label("Generating erosion takes significant amount of time", rect:layout.Inset(), helpbox:true, fontSize:9);
			layout.Par(2);
			layout.Field(ref iterations, "Iterations");
			layout.Field(ref terrainDurability, "Durability");
			layout.Field(ref erosionAmount, "Erosion", min:0, max:1);
			layout.Field(ref sedimentAmount, "Sediment");
			layout.Field(ref fluidityIterations, "Fluidity");
			layout.Field(ref ruffle, "Ruffle");
			layout.Field(ref safeBorders, "Safe Borders");
			layout.Field(ref cliffOpacity, "Cliff Opacity");
			layout.Field(ref sedimentOpacity, "Sediment Opacity");
		}
	}


	[System.Serializable]
	//[GeneratorMenu (menu="Map", name ="Noise Mask", disengageable = true)]
	public class NoiseMaskGenerator : Generator
	{
		public Input inputIn = new Input(InoutType.Map);
		public override IEnumerable<Input> Inputs() { yield return inputIn; }

		public Output maskedOut = new Output(InoutType.Map);
		public Output invMaskedOut = new Output(InoutType.Map);
		public override IEnumerable<Output> Outputs() { yield return maskedOut; yield return invMaskedOut; }

		public float opacity = 1f;
		public float size = 200;
		public Vector2 offset = new Vector2(0,0);
		public AnimationCurve curve = new AnimationCurve( new Keyframe[] { new Keyframe(0,0,1,1), new Keyframe(1,1,1,1) } );

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//getting inputs
			Matrix input = (Matrix)inputIn.GetObject(chunk);
			Matrix masked = chunk.defaultMatrix;
			Matrix invMasked = chunk.defaultMatrix;

			//return
			if (chunk.stop) return; 
			if (!enabled || input==null) { maskedOut.SetObject(chunk, input); return; }
			
			//generating noise
			NoiseGenerator.Noise(masked, size, 1, 0.5f, offset:offset);
			if (chunk.stop) return;
			
			//adjusting curve
			Curve c = new Curve(curve);
			for (int i=0; i<masked.array.Length; i++) masked.array[i] = c.Evaluate(masked.array[i]);
			if (chunk.stop) return;

			//get inverse mask
			for (int i=0; i<masked.array.Length; i++)
				invMasked.array[i] = 1f - masked.array[i];
			if (chunk.stop) return;
			
			//multiply masks by input
			if (input != null)
			{
				for (int i=0; i<masked.array.Length; i++) masked.array[i] = input.array[i]*masked.array[i]*opacity + input.array[i]*(1f-opacity);
				for (int i=0; i<invMasked.array.Length; i++) invMasked.array[i] = input.array[i]*invMasked.array[i]*opacity + input.array[i]*(1f-opacity);
			}

			if (chunk.stop) return;
			maskedOut.SetObject(chunk, masked);
			invMaskedOut.SetObject(chunk, invMasked);
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); inputIn.DrawIcon(layout, "Input"); maskedOut.DrawIcon(layout, "Masked");
			layout.Par(20); invMaskedOut.DrawIcon(layout, "InvMasked");
			layout.Par(5);
			
			//params
			Rect cursor = layout.cursor; layout.rightMargin = 90; layout.fieldSize = 0.75f;
			layout.Field(ref opacity, "A", max:1);
			layout.Field(ref size, "S", min:1);
			layout.Field(ref offset, "O");
			
			layout.cursor = cursor; layout.rightMargin = layout.margin; layout.margin = (int)layout.field.width - 85 - layout.margin*2;
			layout.Par(53);
			layout.Curve(curve, layout.Inset());
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Shore", disengageable = true)]
	public class ShoreGenerator : Generator
	{
		public Input heightIn = new Input(InoutType.Map);
		public Input maskIn = new Input(InoutType.Map);
		public Input ridgeNoiseIn = new Input(InoutType.Map);
		public override IEnumerable<Input> Inputs() { yield return heightIn; yield return maskIn; yield return ridgeNoiseIn; }

		public Output heightOut = new Output(InoutType.Map);
		public Output sandOut = new Output(InoutType.Map);
		public override IEnumerable<Output> Outputs() { yield return heightOut; yield return sandOut; }

		public float intensity = 1f;
		public float beachLevel = 20f;
		public float beachSize = 10f;
		public float ridgeMinGlobal = 2;
		public float ridgeMaxGlobal = 10;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			Matrix src = (Matrix)heightIn.GetObject(chunk);

			if (chunk.stop) return;
			if (!enabled || src==null) { heightOut.SetObject(chunk, src); return; }

			Matrix dst = new Matrix(src.rect);
			Matrix ridgeNoise = (Matrix)ridgeNoiseIn.GetObject(chunk);

			//preparing sand
			Matrix sands = new Matrix(src.rect);

			//converting ui values to internal
			float beachMin = beachLevel / MapMagic.instance.terrainHeight;
			float beachMax = (beachLevel+beachSize) / MapMagic.instance.terrainHeight;
			float ridgeMin = ridgeMinGlobal / MapMagic.instance.terrainHeight;
			float ridgeMax = ridgeMaxGlobal / MapMagic.instance.terrainHeight;

			Coord min = src.rect.Min; Coord max = src.rect.Max;
			for (int x=min.x; x<max.x; x++)
			   for (int z=min.z; z<max.z; z++)
			{
				float srcHeight = src[x,z];

				//creating beach
				float height = srcHeight;
				if (srcHeight > beachMin && srcHeight < beachMax) height = beachMin;
				
				float sand = 0;
				if (srcHeight <= beachMax) sand = 1;

				//blurring ridge
				float curRidgeDist = 0;
				float noise = 0; if (ridgeNoise != null) noise = ridgeNoise[x,z];
				curRidgeDist = ridgeMin*(1-noise) + ridgeMax*noise;
				
				if (srcHeight >= beachMax && srcHeight <= beachMax+curRidgeDist) 
				{
					float percent = (srcHeight-beachMax) / curRidgeDist;
					percent = Mathf.Sqrt(percent);
					percent = 3*percent*percent - 2*percent*percent*percent;
					
					height = beachMin*(1-percent) + srcHeight*percent;
					
					sand = 1-percent;
				}

				//setting height
				height = height*intensity + srcHeight*(1-intensity);
				dst[x,z] = height;
				sands[x,z] = sand;
			}

			//mask
			Matrix mask = (Matrix)maskIn.GetObject(chunk);
			if (mask != null)  Matrix.Mask(src, dst, mask); // Matrix.Mask(null, sands, mask); }

			if (chunk.stop) return;
			heightOut.SetObject(chunk, dst); 
			sandOut.SetObject(chunk, sands);
		}

		public override void OnGUI ()
		{
			layout.Par(20); heightIn.DrawIcon(layout, "Height"); heightOut.DrawIcon(layout, "Output");
			layout.Par(20); maskIn.DrawIcon(layout, "Mask"); sandOut.DrawIcon(layout, "Sand");
			layout.Par(20); ridgeNoiseIn.DrawIcon(layout, "Ridge Noise"); 

			layout.Field(ref intensity, "Intensity", min:0);
			layout.Field(ref beachLevel, "Water Level", min:0);
			layout.Field(ref beachSize, "Beach Size", min:0.0001f);
			layout.Field(ref ridgeMinGlobal, "Ridge Step Min", min:0);
			layout.Field(ref ridgeMaxGlobal, "Ridge Step Max", min:0);
		}
	}

}
