using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//using Plugins;

namespace MapMagic
{
	[System.Serializable]
	[GeneratorMenu (menu="Objects", name ="Scatter", disengageable = true)]
	public class ScatterGenerator1 : Generator
	{
		public Input probability = new Input(InoutType.Map);
		public Output output = new Output(InoutType.Objects);
		public override IEnumerable<Input> Inputs() { yield return probability; }
		public override IEnumerable<Output> Outputs() { yield return output; }

		public int seed = 12345;
		public enum Algorithm { Random, SquareCells, HexCells };
		public Algorithm algorithm = Algorithm.Random;
		public float density = 10;
		public float uniformity = 0.1f;
		public float relax = 0.1f;
		public int safeBorders = 2;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			Matrix probMatrix = (Matrix)probability.GetObject(chunk);
			if (!enabled || chunk.stop) return;
			SpatialHash spatialHash = chunk.defaultSpatialHash;
			
			//initializing random
			InstanceRandom rnd = new InstanceRandom(MapMagic.instance.seed + seed + chunk.coord.x*1000 + chunk.coord.z);

			float square = MapMagic.instance.terrainSize * MapMagic.instance.terrainSize;
			int count = (int)(square*(density/1000000)); //number of items per terrain
			
			if (algorithm==Algorithm.Random) RandomScatter(count, spatialHash, rnd, probMatrix);
			else CellScatter(count, spatialHash, rnd, probMatrix, hex:algorithm==Algorithm.HexCells);

			if (chunk.stop) return;
			output.SetObject(chunk, spatialHash);
		}

		public void RandomScatter (int count, SpatialHash spatialHash, InstanceRandom rnd, Matrix probMatrix)
		{
			int candidatesNum = (int)(uniformity*100);
			if (candidatesNum < 1) candidatesNum = 1;
			
			for (int i=0; i<count; i++)
			{
				Vector2 bestCandidate = Vector3.zero;
				float bestDist = 0;
				
				for (int c=0; c<candidatesNum; c++)
				{
					Vector2 candidate = new Vector2((spatialHash.offset.x+1) + (rnd.Random()*(spatialHash.size-2.01f)), (spatialHash.offset.y+1) + (rnd.Random()*(spatialHash.size-2.01f)));
				
					//checking if candidate available here according to probability map
					//if (probMatrix!=null && probMatrix[candidate] < rnd.Random()+0.0001f) continue;

					//checking if candidate is the furthest one
					float dist = spatialHash.MinDist(candidate);

					//distance to the edge
					float bd = (candidate.x-spatialHash.offset.x)*2; if (bd < dist) dist = bd;
					bd = (candidate.y-spatialHash.offset.y)*2; if (bd < dist) dist = bd;
					bd = (spatialHash.offset.x+spatialHash.size-candidate.x)*2; if (bd < dist) dist = bd;
					bd = (spatialHash.offset.y+spatialHash.size-candidate.y)*2; if (bd < dist) dist = bd;

					if (dist>bestDist) { bestDist=dist; bestCandidate = candidate; }
				}

				if (bestDist>0.001f) 
				{
					spatialHash.Add(bestCandidate, 0, 0, 1); //adding only if some suitable candidate found
				}
			}

			//masking
			for (int c=0; c<spatialHash.cells.Length; c++)
			{
				SpatialHash.Cell cell = spatialHash.cells[c];
				for (int i=cell.objs.Count-1; i>=0; i--)
				{
					Vector2 pos = cell.objs[i].pos;
				
					if (pos.x < spatialHash.offset.x+safeBorders || 
						pos.y < spatialHash.offset.y+safeBorders ||
						pos.x > spatialHash.offset.x+spatialHash.size-safeBorders ||
						pos.y > spatialHash.offset.y+spatialHash.size-safeBorders ) { cell.objs.RemoveAt(i); continue; }

					if (probMatrix!=null && probMatrix[pos] < rnd.Random()+0.0001f) { cell.objs.RemoveAt(i); continue; }
				}
			}
		}

		public void CellScatter (int count, SpatialHash spatialHash, InstanceRandom rnd, Matrix probMatrix, bool hex=true)
		{
			//finding scatter rect
			CoordRect rect = new CoordRect(spatialHash.offset.x+1, spatialHash.offset.y+1, spatialHash.size-2, spatialHash.size-2);
			rect.Contract(safeBorders);

			//positioned scatter
			float sideCount = Mathf.Sqrt(count);
			//float step = 1f * rect.size.x / sideCount;

			//scattering in hexagonal order
			float heightFactor = 1; if (hex) heightFactor = 0.8660254f;
			Matrix2<Vector2> positions = new Matrix2<Vector2>( new CoordRect(0,0,sideCount,(int)(sideCount/heightFactor)) );
			Vector2 cellSize = new Vector2(spatialHash.size/positions.rect.size.x, spatialHash.size/positions.rect.size.z);

			Coord min = positions.rect.Min; Coord max = positions.rect.Max;
			for (int x=min.x; x<max.x; x++)
				for (int z=min.z; z<max.z; z++)
			{
				Vector2 position = new Vector2(x*cellSize.x + spatialHash.offset.x,  z*cellSize.y + spatialHash.offset.y);
				position.x += cellSize.x/2; position.y += cellSize.y/2;
				if (hex && z%2!=0) position.x += cellSize.x/2;
				
				//random
				position += new Vector2( rnd.CoordinateRandom(x,z*2000)-0.5f, rnd.CoordinateRandom(x*3000,z)-0.5f ) * cellSize.x * (1-uniformity); 

				positions[x,z] = position;
			}

			//relaxing
			Matrix2<Vector2> newPositions = new Matrix2<Vector2>(positions.rect);
			Vector2[] closestPositions = new Vector2[8];

			for (int x=min.x; x<max.x; x++)
				for (int z=min.z; z<max.z; z++)
			{
				if (x==min.x || x==max.x-1 || z==min.z || z==max.z-1) { newPositions[x,z] = positions[x,z]; continue; }

				Vector2 position = positions[x,z];
				Vector2 relaxDir = new Vector2();

				//getting closest positions
				closestPositions[0] = positions[x-1,z+1]; closestPositions[1] = positions[x,z+1]; closestPositions[2] = positions[x+1,z+1];
				closestPositions[3] = positions[x-1,z];											  closestPositions[4] = positions[x+1,z];
				closestPositions[5] = positions[x-1,z-1]; closestPositions[6] = positions[x,z-1]; closestPositions[7] = positions[x+1,z-1];

				//relaxing
				for (int i=0; i<8; i++)
				{
					Vector2 deltaVec = (position-closestPositions[i]) / cellSize.x; //in cells
					float deltaDist = deltaVec.magnitude;
					if (deltaDist > 1) deltaDist = 1;

					float relaxFactor = (1-deltaDist)*(1-deltaDist); //1 / deltaVec.magnitude;

					relaxDir += deltaVec.normalized * relaxFactor;
				}

				//float relaxDirMagnitude = relaxDir.magnitude;
				//relaxDir /= relaxDirMagnitude;
				//relaxDirMagnitude = Mathf.Sqrt(relaxDirMagnitude);

				newPositions[x,z] = position + relaxDir*relax*(cellSize.x/2);
			}

			//masking and converting to spatial hash
			for (int x=min.x; x<max.x; x++)
				for (int z=min.z; z<max.z; z++)
			{
				Vector2 pos = newPositions[x,z];

				if (pos.x < spatialHash.offset.x+safeBorders+0.001f || 
					pos.y < spatialHash.offset.y+safeBorders+0.001f ||
					pos.x > spatialHash.offset.x+spatialHash.size-safeBorders-0.001f ||
					pos.y > spatialHash.offset.y+spatialHash.size-safeBorders-0.001f ) continue;

				if (probMatrix!=null && probMatrix[(int)pos.x, (int)pos.y] < rnd.CoordinateRandom(x,z*1000)+0.0001f) continue;

				spatialHash.Add(pos, 0,0,1);
			}
		}


		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); probability.DrawIcon(layout, "Probability"); output.DrawIcon(layout, "Output");
			layout.Par(5);

			//params
			layout.Field(ref algorithm, "Algorithm");
			layout.Field(ref seed, "Seed");
			layout.Field(ref density, "Density");
			layout.Field(ref uniformity, "Uniformity", min:0, max:1);
			layout.Field(ref relax, "Relax", min:0, max:1);
			layout.Field(ref safeBorders, "Safe Borders", min:0);
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Objects", name ="Adjust", disengageable = true)]
	public class AdjustGenerator : Generator
	{
		public Input input = new Input(InoutType.Objects);
		public Input intensity = new Input(InoutType.Map);
		public Output output = new Output(InoutType.Objects);
		public override IEnumerable<Input> Inputs() { yield return input; yield return intensity; }
		public override IEnumerable<Output> Outputs() { yield return output; }

		public int seed = 12345;
		public enum Type { absolute, relative };
		public Type type = Type.relative;
		public Vector2 height = Vector2.zero;
		public Vector2 rotation = Vector2.zero;
		public Vector2 scale = Vector2.one;
		public float sizeFactor = 0;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//getting inputs
			SpatialHash sourceHash = (SpatialHash)input.GetObject(chunk); if (sourceHash==null) return;
			SpatialHash spatialHash = sourceHash.Copy();
			Matrix intensityMatrix = (Matrix)intensity.GetObject(chunk);

			//return on stop/disable/null input
			if (chunk.stop) return; 
			if (!enabled || spatialHash==null) { output.SetObject(chunk, spatialHash); return; }
			
			//preparing output
			spatialHash = spatialHash.Copy();

			InstanceRandom rnd = new InstanceRandom(MapMagic.instance.seed + seed + chunk.coord.x*1000 + chunk.coord.z, lutLength:1000);

			foreach (SpatialObject obj in spatialHash.AllObjs())
			{
				float percent = 1;
				if (intensityMatrix != null) percent = intensityMatrix[obj.pos];

				if (type == Type.relative)
				{
					//scale is not affected by sizeFactor
					obj.size *= rnd.CoordinateRandom(obj.id+2, scale) * percent;

					//everything else does
					percent = percent*(1-sizeFactor) + percent*obj.size*sizeFactor;
					obj.height += rnd.CoordinateRandom(obj.id, height) * percent / MapMagic.instance.terrainHeight;
					obj.rotation += rnd.CoordinateRandom(obj.id+1, rotation) * percent;
				}
				else 
				{
					obj.size = rnd.CoordinateRandom(obj.id+2, scale) * percent;
					
					percent = percent*(1-sizeFactor) + percent*obj.size*sizeFactor;
					obj.height = rnd.CoordinateRandom(obj.id, height) * percent / MapMagic.instance.terrainHeight;
					obj.rotation = rnd.CoordinateRandom(obj.id+1, rotation) * percent;
				}
			}

			//setting output
			if (chunk.stop) return;
			output.SetObject(chunk, spatialHash);
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); input.DrawIcon(layout, "Input", mandatory:true); output.DrawIcon(layout, "Output");
			layout.Par(20); intensity.DrawIcon(layout, "Mask");
			layout.Par(5);
			
			//params
			layout.fieldSize = 0.7f;
//			layout.inputSize = 0.5f;
			layout.Field(ref seed, "Seed");
			layout.Field(ref type, "Type");
			layout.Field(ref height, "Height");
			layout.Field(ref rotation, "Rotation", min:-360, max:360);
			layout.Field(ref scale, "Scale");
			layout.Field(ref sizeFactor, "Size Factor");
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Objects", name ="Clean Up", disengageable = true)]
	public class CleanUpGenerator : Generator
	{
		public Input mask = new Input(InoutType.Map);
		public Input input = new Input(InoutType.Objects);
		public Output output = new Output(InoutType.Objects);
		public override IEnumerable<Input> Inputs() { yield return mask; yield return input; }
		public override IEnumerable<Output> Outputs() { yield return output; }

		public int seed = 12345;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			Matrix matrix = (Matrix)mask.GetObject(chunk);
			SpatialHash src = (SpatialHash)input.GetObject(chunk);
			
			if (chunk.stop) return; 
			if (!enabled || matrix==null) { output.SetObject(chunk, src); return; }
			
			//random
			InstanceRandom rnd = new InstanceRandom(MapMagic.instance.seed + seed + chunk.coord.x*1000 + chunk.coord.z);

			//preparing output
			SpatialHash dst = new SpatialHash(src.offset, src.size, src.resolution);

			//populating output
			foreach (SpatialObject obj in src.AllObjs())
			{
				float val = matrix[obj.pos];
				if (val < 0.0001f) continue; //sometimes random is less then 0.0001
				if (matrix[obj.pos] > rnd.Random()) dst.Add(obj);
			}

			if (chunk.stop) return;
			output.SetObject(chunk, dst);
		}


		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); input.DrawIcon(layout, "Input"); output.DrawIcon(layout, "Output");
			layout.Par(20); mask.DrawIcon(layout, "Mask", mandatory:true);
			layout.Par(5);

			layout.Field(ref seed, "Seed");
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Objects", name ="Split", disengageable = true)]
	public class SplitGenerator : Generator, Layout.ILayered
	{
		//layer
		public class Layer : Layout.ILayer
		{
			public string name = "Object Layer";
			public Output output = new Output(InoutType.Objects);

			public Vector2 heightCondition = new Vector2(0,1);
			public Vector2 rotationCondition = new Vector2(0,360);
			public Vector2 scaleCondition = new Vector2(0,100);
			public float chance = 1;

			public bool pinned { get{return false;}}
			public int guiHeight { get; set; }
			
			public void OnCollapsedGUI (Layout layout) 
			{ 
				layout.rightMargin = 20; layout.fieldSize = 1f;
				
				layout.Par(20); 
				layout.Label(name, rect:layout.Inset()); 
				output.DrawIcon(layout);
			}
			public void OnExtendedGUI (Layout layout) 
			{ 
				layout.margin = 7; layout.rightMargin = 20; layout.fieldSize = 1f;
				
				layout.Par(20); 
				name = layout.Field(name, rect:layout.Inset()); 
				output.DrawIcon(layout);

				layout.margin = 5; layout.rightMargin = 5; layout.fieldSize = 0.6f;
				layout.Field(ref heightCondition, "Height");
				layout.Field(ref rotationCondition, "Rotation");
				layout.Field(ref scaleCondition, "Scale");
				layout.Field(ref chance, "Chance");
			}

			public void OnAdd (int n) {  }
			public void OnRemove (int n) 
			{ 
				Input connectedInput = output.GetConnectedInput(MapMagic.instance.gens.list);
				if (connectedInput != null) connectedInput.Link(null, null);
			}
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

		//generator
		public Input input = new Input(InoutType.Objects);
		public override IEnumerable<Input> Inputs() { yield return input; }
		public override IEnumerable<Output> Outputs() 
		{ 
			for (int i=0; i<baseLayers.Length; i++) 
				yield return baseLayers[i].output; 
		}

		//params
		public enum MatchType { layered, random };
		public MatchType matchType = MatchType.layered;


		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//getting input
			SpatialHash src = (SpatialHash)input.GetObject(chunk);

			//return on stop/disable/null input
			if (chunk.stop || baseLayers.Length==0) return;
			if (!enabled || src==null) 
			{
				for (int i=0; i<baseLayers.Length; i++)
					baseLayers[i].output.SetObject(chunk, null); 
				return; 
			}

			//creating dst
			SpatialHash[] dst = new SpatialHash[baseLayers.Length];
			for (int i=0; i<dst.Length; i++)
				dst[i] = new SpatialHash(src.offset, src.size, src.resolution);
			
			//random
			InstanceRandom rnd = new InstanceRandom(MapMagic.instance.seed + 12345 + chunk.coord.x*1000 + chunk.coord.z);
			
			//procedural array
			bool[] match = new bool[baseLayers.Length];

			//for each object
			foreach (SpatialObject obj in src.AllObjs())
			{
				//finding suitable objects (and sum of chances btw. And last object for non-random)
				int matchesNum = 0; //how many layers have a suitable obj
				float chanceSum = 0;
				int lastLayerNum = 0;

				for (int i=0; i<baseLayers.Length; i++)
				{
					Layer layer = baseLayers[i];
					if (obj.height >= layer.heightCondition.x && obj.height <= layer.heightCondition.y &&
						obj.rotation % 360 >= layer.rotationCondition.x && obj.rotation % 360 <= layer.rotationCondition.y &&
						obj.size >= layer.scaleCondition.x && obj.size <= layer.scaleCondition.y)
						{
							match[i] = true;

							matchesNum ++;
							chanceSum += layer.chance;
							lastLayerNum = i;
						}
					else match[i] = false;
				}

				//if no matches detected - continue withous assigning obj
				if (matchesNum == 0) continue;

				//if one match - assigning last obj
				else if (matchesNum == 1 || matchType == MatchType.layered) dst[lastLayerNum].Add(obj);

				//selecting layer at random
				else if (matchesNum > 1 && matchType == MatchType.random)
				{
					float randomVal = rnd.CoordinateRandom(obj.id);
					randomVal *= chanceSum;
					chanceSum = 0;

					for (int i=0; i<baseLayers.Length; i++)
					{
						if (!match[i]) continue;
						
						Layer layer = baseLayers[i];
						if (randomVal > chanceSum  &&  randomVal < chanceSum + layer.chance) { dst[i].Add(obj); break; }
						chanceSum += layer.chance;
					}
				}
			}

			for (int i=0; i<baseLayers.Length; i++)
				baseLayers[i].output.SetObject(chunk, dst[i]); 
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); input.DrawIcon(layout, "Input", mandatory:true);
			layout.Par(5);
			
			//params
			layout.Par();
			layout.Label("Match Type", rect:layout.Inset(0.5f));
			layout.Field(ref matchType, rect:layout.Inset(0.5f));

			layout.DrawLayered(this, "Layers:");
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Objects", name ="Subtract", disengageable = true)]
	public class SubtractGenerator : Generator
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
			if (chunk.stop) return;
			if (!enabled || subtrahend==null || subtrahend.Count==0 || minuend==null) { minuendOut.SetObject(chunk, minuend); return; }

			//preparing output
			SpatialHash result = minuend.Copy();

			//transforming distance to map-space
			float pixelSize = 1f * MapMagic.instance.terrainSize / MapMagic.instance.resolution;
			float pixelDist = distance / pixelSize;

			//removing objects in range
			foreach (SpatialObject refObj in subtrahend.AllObjs())
			{
				float range = pixelDist*(1-sizeFactor) + pixelDist*refObj.size*sizeFactor;
				result.RemoveObjsInRange(refObj.pos, range);
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
	[GeneratorMenu (menu="Objects", name ="Combine", disengageable = true)]
	public class CombineGenerator : Generator
	{
		public Input[] inputs = new Input[] { new Input(InoutType.Objects), new Input(InoutType.Objects) };
		public Output output = new Output(InoutType.Objects);
		public override IEnumerable<Input> Inputs() { for (int i=0; i<inputs.Length; i++) yield return inputs[i]; }
		public override IEnumerable<Output> Outputs() { yield return output; }

		public int inputsNum = 2;

		public override void Generate (Chunk chunk, Biome biome=null)
		{	
			//return on stop/disable
			if (chunk.stop) return; 
			if (inputs.Length==0) { output.SetObject(chunk,null); return; }
			if (!enabled)
			{
				if (inputs.Length>1) output.SetObject(chunk, (SpatialHash)inputs[0].GetObject(chunk));
				else output.SetObject(chunk,null);
				return;
			}

			//preparing output
			SpatialHash result = chunk.defaultSpatialHash;

			for (int i=0; i<inputs.Length; i++)
			{
				if (chunk.stop) return;
				SpatialHash inputHash = (SpatialHash)inputs[i].GetObject(chunk);
				if (inputHash == null) continue;

				result.Add(inputHash);
			}
			
			output.SetObject(chunk, result);
		}

		public override void OnGUI ()
		{
			//inouts
			if (inputs.Length >= 1) { layout.Par(20); inputs[0].DrawIcon(layout, "Input"); output.DrawIcon(layout, "Output"); }
			for (int i=1; i<inputs.Length; i++) { layout.Par(20); inputs[i].DrawIcon(layout); }
			layout.Par(5);
			
			//params
			layout.Field(ref inputsNum, "Inputs Count", min:2);
			if (inputsNum < 2) inputsNum = 2;
			if (inputsNum != inputs.Length) 
			{
				if (inputsNum > inputs.Length) 
					for (int i=0; i<inputsNum-inputs.Length; i++)
						ArrayTools.Add(ref inputs, inputsNum, new Input(InoutType.Objects));
				else ArrayTools.Resize(ref inputs, inputsNum);
			}
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Objects", name ="Propagate", disengageable = true)]
	public class PropagateGenerator : Generator
	{
		public Input input = new Input(InoutType.Objects);
		public Output output = new Output(InoutType.Objects);
		public override IEnumerable<Input> Inputs() { yield return input; }
		public override IEnumerable<Output> Outputs() { yield return output; }

		public int seed = 12345;
		public Vector2 growth = new Vector2(1,2);
		public Vector2 distance = new Vector2(1,10);
		public float sizeFactor = 0;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//getting inputs
			SpatialHash src = (SpatialHash)input.GetObject(chunk);

			//return on stop/disable/null input
			if (chunk.stop) return; 
			if (!enabled || src==null) { output.SetObject(chunk, src); return; }

			//preparing output
			SpatialHash dst = chunk.defaultSpatialHash;

			InstanceRandom rnd = new InstanceRandom(MapMagic.instance.seed + seed + chunk.coord.x*1000 + chunk.coord.z);

			foreach (SpatialObject obj in src.AllObjs())
			{
				//calculating number of propagate objects
				float num = growth.x + rnd.CoordinateRandom(obj.id)*(growth.y-growth.x);
				num = num*(1-sizeFactor) + num*obj.size*sizeFactor;
				num = Mathf.Round(num);

				//creating objs
				for (int n=0; n<num; n++)
				{
					float angle = rnd.CoordinateRandom(obj.id, n*2) * Mathf.PI*2; //in radians
					Vector2 direction = new Vector2( Mathf.Sin(angle), Mathf.Cos(angle) );
					float dist = distance.x + rnd.CoordinateRandom(obj.id, n*2+1)*(distance.y-distance.x);
					dist = dist*(1-sizeFactor) + dist*obj.size*sizeFactor;
					dist = dist / MapMagic.instance.terrainSize * MapMagic.instance.resolution; //transforming distance to map-space

					Vector2 pos = obj.pos + direction*dist;
					if (pos.x <= dst.offset.x+1.01f) pos.x = dst.offset.x+1.01f; if (pos.y <= dst.offset.y+1.01f) pos.y = dst.offset.y+1.01f;
					if (pos.x >= dst.offset.x+dst.size-1.01f) pos.x = dst.offset.x+dst.size-1.01f; if (pos.y >= dst.offset.y+dst.size-1.01f) pos.y = dst.offset.y+dst.size-1.01f;

					dst.Add(pos, obj.height, obj.rotation, obj.size, id:obj.id+n);
				}
			}

			//setting output
			if (chunk.stop) return;
			output.SetObject(chunk, dst);
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); input.DrawIcon(layout, "Input"); output.DrawIcon(layout, "Output");
			layout.Par(5);
			
			//params
			layout.fieldSize = 0.65f;
			layout.Field(ref seed, "Seed");
			layout.Field(ref growth, "Growth");
			layout.Field(ref distance, "Distance"); //range could not be less then unit to avoid pool intersections
			layout.Field(ref sizeFactor, "Size Factor");
		}
	}
	
	[System.Serializable]
	[GeneratorMenu (menu="Objects", name ="Stamp", disengageable = true)]
	public class StampGenerator1 : Generator
	{
		public Input stampIn = new Input(InoutType.Map);
		public Input canvasIn = new Input(InoutType.Map);
		public Input positionsIn = new Input(InoutType.Objects);
		public Input maskIn = new Input(InoutType.Map);
		public override IEnumerable<Input> Inputs() {  yield return positionsIn; yield return canvasIn; yield return stampIn; yield return maskIn; }

		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Output> Outputs() { yield return output; }

		public BlendGenerator.Algorithm guiAlgorithm;
		public float radius = 1;
		public float sizeFactor = 1;
		public int safeBorders = 0;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//getting inputs
			Matrix stamp = (Matrix)stampIn.GetObject(chunk);
			Matrix src = (Matrix)canvasIn.GetObject(chunk);
			SpatialHash objs = (SpatialHash)positionsIn.GetObject(chunk);
			
			//return on stop/disable/null input
			if (chunk.stop) return; 
			if (!enabled || stamp==null || objs==null) { output.SetObject(chunk,src); return; }

			//preparing output
			Matrix dst = null;
			if (src==null) dst = chunk.defaultMatrix; 
			else dst = src.Copy(null);

			//algorithm
			System.Func<float,float,float> algorithm = BlendGenerator.GetAlgorithm(guiAlgorithm);

			foreach (SpatialObject obj in objs.AllObjs())
			{
				//finding current radius
				float curRadius = radius*(1-sizeFactor) + radius*obj.size*sizeFactor;
				curRadius = curRadius / MapMagic.instance.terrainSize * MapMagic.instance.resolution; //transforming to map-space

				//stamp coordinates
				float scale = curRadius*2 / stamp.rect.size.x;
				Vector2 stampMin = obj.pos - new Vector2(curRadius, curRadius);
				Vector2 stampMax = obj.pos + new Vector2(curRadius, curRadius);
				Vector2 stampSize = new Vector2(curRadius*2, curRadius*2);

				//calculating rects 
				CoordRect stampRect = new CoordRect(stampMin.x, stampMin.y, stampSize.x, stampSize.y);
				CoordRect intersection = CoordRect.Intersect(stampRect, dst.rect);
				Coord min = intersection.Min; Coord max = intersection.Max; 

				for (int x=min.x; x<max.x; x++)
					for (int z=min.z; z<max.z; z++)
				{
					//float dist = Mathf.Sqrt((x-obj.pos.x+0.5f)*(x-obj.pos.x+0.5f) + (z-obj.pos.y+0.5f)*(z-obj.pos.y+0.5f));
					//float percent = 1f - dist / curRadius; 
					//if (percent < 0 || dist > curRadius) percent = 0;

					Vector2 relativePos = new Vector2(1f*(x-stampMin.x)/(stampMax.x-stampMin.x), 1f*(z-stampMin.y)/(stampMax.y-stampMin.y));
					float val = stamp.GetInterpolated(relativePos.x*stamp.rect.size.x + stamp.rect.offset.x, relativePos.y*stamp.rect.size.z + stamp.rect.offset.z, Matrix.WrapMode.Clamp);
					//float val = stamp.CheckGet((int)(relativePos.x*stamp.rect.size.x + stamp.rect.offset.x), (int)(relativePos.y*stamp.rect.size.z + stamp.rect.offset.z)); //TODO use bilenear filtering

					//matrix[x,z] = matrix[x,z]+val*scale;
					//matrix[x,z] = Mathf.Max(matrix[x,z],val*scale);
					dst[x,z] = algorithm(dst[x,z],val*scale);
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
			layout.Par(20); positionsIn.DrawIcon(layout, "Positions", mandatory:true); output.DrawIcon(layout, "Output");
			layout.Par(20); canvasIn.DrawIcon(layout, "Canvas");
			layout.Par(20); stampIn.DrawIcon(layout, "Stamp", mandatory:true);
			layout.Par(20); maskIn.DrawIcon(layout, "Mask");
			layout.Par(5);

			//params
			layout.Par(5); layout.fieldSize = 0.5f;
			layout.Field(ref guiAlgorithm, "Algorithm");
			layout.Field(ref radius, "Radius");
			layout.Field(ref sizeFactor, "Size Factor");
			layout.Field(ref safeBorders, "Safe Borders");
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Objects", name ="Blob", disengageable = true)]
	public class BlobGenerator : Generator
	{
		public Input objectsIn = new Input(InoutType.Objects);
		public Input canvasIn = new Input(InoutType.Map);
		public Input maskIn = new Input(InoutType.Map);
		public override IEnumerable<Input> Inputs() {  yield return objectsIn; yield return canvasIn; yield return maskIn; }

		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Output> Outputs() { yield return output; }

		public float intensity = 1f;
		public float radius = 10;
		public float sizeFactor = 0;
		public AnimationCurve fallof = new AnimationCurve( new Keyframe[] { new Keyframe(0,0,1,1), new Keyframe(1,1,1,1) } );
		public float noiseAmount = 0.1f;
		public float noiseSize = 100;
		public int safeBorders = 0;

		public static void DrawBlob (Matrix canvas, Vector2 pos, float val, float radius, AnimationCurve fallof, float noiseAmount=0, float noiseSize=20)
		{
			CoordRect blobRect = new CoordRect(
				(int)(pos.x-radius-1), (int)(pos.y-radius-1),
				radius*2+2, radius*2+2 );

			Curve curve = new Curve(fallof);
			Noise noise = new Noise(noiseSize, MapMagic.instance.resolution, MapMagic.instance.seed*7, MapMagic.instance.seed*3);

			CoordRect intersection = CoordRect.Intersect(canvas.rect, blobRect);
			Coord center = blobRect.Center;
			Coord min = intersection.Min; Coord max = intersection.Max; 
			for (int x=min.x; x<max.x; x++)
				for (int z=min.z; z<max.z; z++)
			{
				float dist = Coord.Distance(center, x,z);
				float percent = curve.Evaluate(1f-dist/radius);
				float result = percent;

				if (noiseAmount > 0.001f)
				{
					float maxNoise = percent; if (percent > 0.5f) maxNoise = 1-percent;
					result += (noise.Fractal(x,z)*2 - 1) * maxNoise * noiseAmount;
				}

				//canvas[x,z] = Mathf.Max(result*val, canvas[x,z]);
				canvas[x,z] = val*result + canvas[x,z]*(1-result);
			}
		}

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//getting inputs
			SpatialHash objects = (SpatialHash)objectsIn.GetObject(chunk);
			Matrix src = (Matrix)canvasIn.GetObject(chunk);
			
			//return on stop/disable/null input
			if (chunk.stop) return; 
			if (!enabled || objects==null) { output.SetObject(chunk, src); return; }

			//preparing output
			Matrix dst; 
			if (src != null) dst = src.Copy(null); 
			else dst = chunk.defaultMatrix;

			foreach (SpatialObject obj in objects.AllObjs())
			{
				//finding current radius
				float curRadius = radius*(1-sizeFactor) + radius*obj.size*sizeFactor;
				curRadius = curRadius / MapMagic.instance.terrainSize * MapMagic.instance.resolution; //transforming to map-space

				DrawBlob(dst, obj.pos, intensity, curRadius, fallof, noiseAmount, noiseSize);
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
			layout.Field(ref intensity, "Intensity");
			layout.Field(ref radius, "Radius");
			layout.Field(ref sizeFactor, "Size Factor");
			layout.Field(ref safeBorders, "Safe Borders");

			layout.Label("Fallof:");
			
			//curve
			Rect cursor = layout.cursor;
			layout.Par(53);
			layout.Curve(fallof, rect:layout.Inset(80));

			//noise
			layout.cursor = cursor; 
			layout.margin = 86; layout.fieldSize = 0.8f;
			layout.Label("Noise");
			layout.Field(ref noiseAmount, "A");
			layout.Field(ref noiseSize, "S");
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Objects", name ="Flatten", disengageable = true)]
	public class FlattenGenerator : Generator
	{
		public Input objectsIn = new Input(InoutType.Objects);
		public Input canvasIn = new Input(InoutType.Map);
		public Input maskIn = new Input(InoutType.Map);
		public override IEnumerable<Input> Inputs() {  yield return objectsIn; yield return canvasIn; yield return maskIn; }

		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Output> Outputs() { yield return output; }

		public float radius = 10;
		public float sizeFactor = 0;
		public AnimationCurve fallof = new AnimationCurve( new Keyframe[] { new Keyframe(0,0,1,1), new Keyframe(1,1,1,1) } );
		public float noiseAmount = 0.1f;
		public float noiseSize = 100;
		public int safeBorders = 0;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//getting inputs
			SpatialHash objects = (SpatialHash)objectsIn.GetObject(chunk);
			Matrix src = (Matrix)canvasIn.GetObject(chunk);
			
			//return on stop/disable/null input
			if (chunk.stop) return; 
			if (!enabled || objects==null) { output.SetObject(chunk, src); return; }

			//preparing output
			Matrix dst; 
			if (src != null) dst = src.Copy(null); 
			else dst = chunk.defaultMatrix;

			foreach (SpatialObject obj in objects.AllObjs())
			{
				//finding current radius
				float curRadius = radius*(1-sizeFactor) + radius*obj.size*sizeFactor;
				curRadius = curRadius / MapMagic.instance.terrainSize * MapMagic.instance.resolution; //transforming to map-space

				float objHeight = src.GetInterpolated(obj.pos.x, obj.pos.y);
				BlobGenerator.DrawBlob(dst, obj.pos, objHeight, curRadius, fallof, noiseAmount, noiseSize);
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
			layout.Field(ref sizeFactor, "Size Factor");
			layout.Field(ref safeBorders, "Safe Borders");

			layout.Label("Fallof:");
			
			//curve
			Rect cursor = layout.cursor;
			layout.Par(53);
			layout.Curve(fallof, rect:layout.Inset(80));

			//noise
			layout.cursor = cursor; 
			layout.margin = 86; layout.fieldSize = 0.8f;
			layout.Label("Noise");
			layout.Field(ref noiseAmount, "A");
			layout.Field(ref noiseSize, "S");
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Objects", name ="Forest", disengageable = true)]
	public class ForestGenerator1 : Generator
	{
		public Input seedlingsIn = new Input(InoutType.Objects);
		public Input otherTreesIn = new Input(InoutType.Objects);
		public Input soilIn = new Input(InoutType.Map);
		public Output treesOut = new Output(InoutType.Objects);
		public override IEnumerable<Input> Inputs() { yield return seedlingsIn; yield return otherTreesIn; yield return soilIn; }
		public override IEnumerable<Output> Outputs() { yield return treesOut; }

		public int years = 50;
		public float density = 3f; //max trees per 10*10m
		public float fecundity = 0.5f;
		public float seedDist = 10;
		public float reproductiveAge = 10;
		public float survivalRate = 0.5f;
		public float lifeAge = 100;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//getting inputs
			SpatialHash seedlings = (SpatialHash)seedlingsIn.GetObject(chunk);
			SpatialHash otherTrees = (SpatialHash)otherTreesIn.GetObject(chunk);
			Matrix soil = (Matrix)soilIn.GetObject(chunk);

			//return on stop/disable/null input
			if (chunk.stop) return; 
			if (!enabled || seedlings==null) { treesOut.SetObject(chunk, seedlings); return; }

			//initializing random
			InstanceRandom rnd = new InstanceRandom(MapMagic.instance.seed + 12345 + chunk.coord.x*1000 + chunk.coord.z);

			//creating forest map
			int resolution = (int)Mathf.Sqrt(density*10000f);
			float pixelSize = seedlings.size / resolution;
			float forestSoilFactor = 1f * MapMagic.instance.resolution / resolution;

			Matrix forest = new Matrix( new CoordRect(0,0,resolution,resolution) );
			Matrix otherForest = new Matrix( new CoordRect(0,0,resolution,resolution) );
			if (otherTrees != null)
				foreach (SpatialObject tree in otherTrees.AllObjs()) 
					otherForest[(int)((tree.pos.x-seedlings.offset.x)/pixelSize+0.01f), (int)((tree.pos.y-seedlings.offset.y)/pixelSize+0.01f)] = 1;

			for (int y=0; y<years; y++)
			{
				//filling seedlings - each iteration to make them persistent
				foreach (SpatialObject tree in seedlings)
				{
					int sx = (int)((tree.pos.x-seedlings.offset.x)/pixelSize+0.01f);
					int sz = (int)((tree.pos.y-seedlings.offset.y)/pixelSize+0.01f);
					if (otherForest[sx,sz] > 0.01f) continue;
					forest[sx,sz] = reproductiveAge+1;
				}

				//generating
				for (int x=0; x<resolution; x++)
					for (int z=0; z<resolution; z++)
				{
					float tree = forest[x,z];

					if (tree < 0.5f) continue;

					//growing tree
					forest[x,z] = ++tree;

					//killing the tree
					float curSurvivalRate = survivalRate;
					if (soil != null) 
					{ 
						int flooredX = (int)(x*forestSoilFactor); if (flooredX<0) flooredX--; flooredX += soil.rect.offset.x;
						int flooredZ = (int)(z*forestSoilFactor); if (flooredZ<0) flooredZ--; flooredZ += soil.rect.offset.z;
						curSurvivalRate *= soil[flooredX, flooredZ]; 
					}
					if (tree > lifeAge || rnd.CoordinateRandom(x,z) > curSurvivalRate) forest[x,z] = 0;

					//breeding the tree
					if (tree > reproductiveAge && rnd.Random() < fecundity)
					{
						int nx = (int)((rnd.Random()*2-1)*seedDist/pixelSize) + x;
						int nz = (int)((rnd.Random()*2-1)*seedDist/pixelSize) + z;
						if (forest.rect.CheckInRange(nx, nz) && forest[nx,nz]<0.5f && otherForest[nx,nz]<0.01f) forest[nx,nz] = 1;
					}
				}
			}

			//preparing outputs
			SpatialHash trees = chunk.defaultSpatialHash;
			for (int x=0; x<resolution; x++)
				for (int z=0; z<resolution; z++)
			{
				Vector2 pos = new Vector2(x*pixelSize + trees.offset.x, z*pixelSize + trees.offset.y);

				//position randomness
				pos += new Vector2(rnd.CoordinateRandom(x,z)*pixelSize, rnd.CoordinateRandom(z, x)*pixelSize);

				//not adding tree if the distance to the closest one is lesser than quarter of the cell size
				if (trees.IsAnyObjInRange(pos,pixelSize/2f)) continue;
				//if (otherTrees != null && otherTrees.IsAnyObjInRange(pos,pixelSize/2f)) continue;



				//out of range
				if (pos.x < trees.offset.x+1.001f || pos.y < trees.offset.y+1.001f || pos.x > trees.offset.x+trees.size-1.001f || pos.y > trees.offset.y+trees.size-1.001f) continue; 

				//poor soil
				//if (soil != null && soil[pos] < 0.6f) continue;
			
				if (forest[x,z] > 0.5f) trees.Add(pos, 0, 0, forest[x,z]);
			}

			//testing
			/*if (test)
			{
				trees.Clear();
				for (int x=0; x<resolution; x++)
				for (int z=0; z<resolution; z++)
				{
					Vector2 pos = new Vector2(x*pixelSize + trees.offset.x, z*pixelSize + trees.offset.y);
					//if (otherForest[x,z] < 0.01f) trees.Add(pos, 0, 0, forest[x,z]);
					if (soil[(int)(x*forestSoilFactor + soil.rect.offset.x), (int)(z*forestSoilFactor + soil.rect.offset.z)] > 0.5f) trees.Add(pos, 0, 0, 10);
				}
			}*/

			//setting outputs
			if (chunk.stop) return;
			treesOut.SetObject(chunk, trees);
			//touchwoodOut.SetObject(chunk, touchwood);
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); seedlingsIn.DrawIcon(layout, "Seedlings", mandatory:true); treesOut.DrawIcon(layout, "Trees");
			layout.Par(20); otherTreesIn.DrawIcon(layout, "Other Trees");
			layout.Par(20); soilIn.DrawIcon(layout, "Soil"); 
			layout.Par(5);
			
			//params
			layout.Field(ref years, "Years");
			layout.Field(ref density, "Density");
			layout.Field(ref fecundity, "Fecundity");
			layout.Field(ref seedDist, "Seed Dist");
			layout.Field(ref reproductiveAge, "Reproductive Age", max:lifeAge);
			layout.Field(ref survivalRate, "Survival Rate");
			layout.Field(ref lifeAge, "Max Age");
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Objects", name ="Slide", disengageable = true)]
	public class SlideGenerator : Generator
	{
		public Input input = new Input(InoutType.Objects);
		public Input stratumIn = new Input(InoutType.Map);
		public Output output = new Output(InoutType.Objects);
		public override IEnumerable<Input> Inputs() { yield return input; yield return stratumIn; }
		public override IEnumerable<Output> Outputs() { yield return output; }

		public int smooth = 0;
		public int iterations = 10;
		public float moveFactor = 3;
		public float stopSlope = 15;

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			//getting inputs
			SpatialHash inputHash = (SpatialHash)input.GetObject(chunk);
			SpatialHash outputHash = chunk.defaultSpatialHash;
			Matrix stratum = (Matrix) stratumIn.GetObject(chunk);

			//return on stop/disable/null input
			if (chunk.stop) return; 
			if (!enabled || stratum==null || inputHash==null) { output.SetObject(chunk, inputHash); return; }

			//preparing output
			inputHash = inputHash.Copy();

			//really, smooth does not make a sense
			/*Matrix blurred = null;
			if (smooth == 0) blurred = stratum;
			else 
			{
				blurred = stratum.Clone(null);
				for (int i=0; i<smooth; i++) stratum.Blur(intensity:1);
			}*/

			//finding stop slope (in 0-1 height difference, same as slope gen)
			float pixelSize = 1f * MapMagic.instance.terrainSize / MapMagic.instance.resolution;
			float stopDelta = Mathf.Tan(stopSlope*Mathf.Deg2Rad) * pixelSize / MapMagic.instance.terrainHeight;

			for (int c=0; c<inputHash.cells.Length; c++)
			{
				SpatialHash.Cell cell = inputHash.cells[c];

				for (int n=cell.objs.Count-1; n>=0; n--)
				{
					SpatialObject obj = cell.objs[n];
					if (chunk.stop) return;

					Vector2 pos = obj.pos;
					bool inRange = true;

					for (int i=0; i<iterations; i++)
					{
						//flooring coordiantes
						int posX = (int)(pos.x); if (pos.x < 0) posX--;
						int posZ = (int)(pos.y); if (pos.y < 0) posZ--;

						float heightMXMZ = stratum[posX, posZ];
						float heightPXMZ = stratum[posX+1, posZ];
						float heightMXPZ = stratum[posX, posZ+1];
						float heightPXPZ = stratum[posX+1, posZ+1];

						float xNormal1 = heightMXPZ-heightPXPZ; //Mathf.Atan(heightPXPZ-heightMXPZ) / halfPi;
						float xNormal2 = heightMXMZ-heightPXMZ; //Mathf.Atan(heightPXMZ-heightMXMZ) / halfPi;
						float zNormal1 = heightPXMZ-heightPXPZ; //Mathf.Atan(heightPXPZ-heightPXMZ) / halfPi;
						float zNormal2 = heightMXMZ-heightMXPZ; //Mathf.Atan(heightMXPZ-heightMXMZ) / halfPi;

						//finding incline tha same way as the slope generator
						float xDelta1 = xNormal1>0? xNormal1 : -xNormal1; float xDelta2 = xNormal2>0? xNormal2 : -xNormal2; float xDelta = xDelta1>xDelta2? xDelta1 : xDelta2;
						float zDelta1 = zNormal1>0? zNormal1 : -zNormal1; float zDelta2 = zNormal2>0? zNormal2 : -zNormal2; float zDelta = zDelta1>zDelta2? zDelta1 : zDelta2;
						float delta = xDelta>zDelta? xDelta : zDelta; //because slope generator uses additive blend

						if (delta < stopDelta) continue;

						Vector2 normal = new Vector2( (xNormal1+xNormal2)/2f, (zNormal1+zNormal2)/2f );

						pos += normal*(MapMagic.instance.terrainHeight*moveFactor); 
						inRange = pos.x > inputHash.offset.x+1 && pos.x < inputHash.offset.x+inputHash.size-1.01f && 
								  pos.y > inputHash.offset.y+1 && pos.y < inputHash.offset.y+inputHash.size-1.01f;

						if (!inRange) break;
					}
				
					if (inRange) 
					{
						obj.pos = pos;
						outputHash.Add(obj);
					}
				}
			}

			//setting output
			if (chunk.stop) return;
			output.SetObject(chunk, outputHash);
		}

		public override void OnGUI ()
		{
			//inouts
			layout.Par(20); input.DrawIcon(layout, "Input", mandatory:true); output.DrawIcon(layout, "Output");
			layout.Par(20); stratumIn.DrawIcon(layout, "Height", mandatory:true);
			layout.Par(5);  
			
			//params
			layout.Field(ref iterations, "Iterations");
			layout.Field(ref moveFactor, "Move Factor");
			layout.Field(ref stopSlope, "Stop Slope");
		}
	}


}
