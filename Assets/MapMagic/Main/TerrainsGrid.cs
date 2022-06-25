using UnityEngine;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace MapMagic 
{
		[System.Serializable]
		public class TerrainGrid : ObjectGrid<Chunk>
		{
			public bool start { 
				get { bool allStart = true; foreach(KeyValuePair<int,Chunk> kvp in grid) allStart = allStart && kvp.Value.start; return allStart; }
				set { foreach(KeyValuePair<int,Chunk> kvp in grid) kvp.Value.start = value; } }
			public bool stop { 
				get { bool allStop = true; foreach(KeyValuePair<int,Chunk> kvp in grid) allStop = allStop && kvp.Value.stop; return allStop; }
				set { foreach(KeyValuePair<int,Chunk> kvp in grid) kvp.Value.stop = value; } }
			public bool running { 
				get { foreach(KeyValuePair<int,Chunk> kvp in grid) if (kvp.Value.running) return true; return false; }}
			public bool complete { 
				get { foreach(KeyValuePair<int,Chunk> kvp in grid) if (!kvp.Value.complete) return false; return true; }}

			//public Chunk closestChunk { get{ return MapMagic.instance.terrains.GetClosestObj(MapMagic.instance.camPos.FloorToCoord(MapMagic.instance.terrainSize)); }}

			//guards for checking if rects changed on deploy
			public CoordRect[] prevCreateRects = new CoordRect[0];
			public CoordRect[] prevRemoveRects = new CoordRect[0];
			//public CoordRect[] currRects = new CoordRect[0];
			//public Coord[] currCenters = new Coord[0];
			public Coord[] prevCenters = new Coord[0]; //not a guard, used for reset only


			public override Chunk Construct () { return new Chunk(); }

			public override void OnCreate (Chunk chunk, Coord coord)
			{
				//creating gameobject
				GameObject go = new GameObject();
				go.name = "Terrain " + coord.x + "," + coord.z;
				go.transform.parent = MapMagic.instance.transform;
				go.transform.localPosition = coord.ToVector3(MapMagic.instance.terrainSize);

				//creating terrain
				chunk.terrain = go.AddComponent<Terrain>();
				chunk.terrainCollider = go.AddComponent<TerrainCollider>();

				TerrainData terrainData = new TerrainData();
				chunk.terrain.terrainData = terrainData;
				chunk.terrainCollider.terrainData = terrainData;
				terrainData.size = new Vector3(MapMagic.instance.terrainSize, MapMagic.instance.terrainHeight, MapMagic.instance.terrainSize);

				//chunk settings
				chunk.coord = coord;
				chunk.SetSettings();
				chunk.clear = true;
				//if (!instance.isEditor || instance.instantGenerate) { chunk.start = true; }

				MapMagic.CallRepaintWindow(); //if (MapMagic.instance.isEditor) if (RepaintWindow != null) RepaintWindow();
			}

			public override void OnMove (Chunk chunk, Coord newCoord) 
			{
				//chunk.terrain.gameObject.SetActive(false); //wrapper.terrain.enabled = false; 
				chunk.coord = newCoord;
				chunk.terrain.transform.localPosition = newCoord.ToVector3(MapMagic.instance.terrainSize);

				//stopping all generate and all apply (they will be resumed in processthread)
				chunk.stop = true;
				MapMagic.instance.StopAllCoroutines(); MapMagic.instance.applyRunning = false;

				//resetting results
				chunk.results.Clear();
				chunk.ready.Clear();
				chunk.heights = null;
				chunk.clear = true;

				//for (int i=0; i<generators.Length; i++) 
				//	if (wrapper.generated.Contains(generators[i])) wrapper.generated.Remove(generators[i]); //generators.array[i].SetGenerated(wrapper, false);
				//if (!instance.isEditor || instance.instantGenerate) { chunk.start = true; }
			}

			public override void OnRemove (Chunk chunk) 
			{ 
				//stopping all generate and all apply
				chunk.stop = true;
				MapMagic.instance.StopAllCoroutines(); MapMagic.instance.applyRunning = false;

				if (chunk.terrain != null) //it could be destroyed by undo
					GameObject.DestroyImmediate(chunk.terrain.gameObject);
			}

			public void Deploy (Vector3 pos, bool allowMove=true) { Deploy( new Vector3[] {pos}, allowMove:allowMove); }

			public void Deploy (Vector3[] poses, bool allowMove=true)
			{
				bool reDeploy = false;

				//number of cams changed
				if (prevCreateRects == null || prevCreateRects.Length != poses.Length || prevRemoveRects.Length != poses.Length) 
				{ 
					reDeploy = true; 
					prevCreateRects = new CoordRect[poses.Length];
					prevRemoveRects = new CoordRect[poses.Length];
					prevCenters = new Coord[poses.Length];
				}

				//checking guard arrays
				for (int p=0; p<poses.Length; p++)
				{
					Vector3 pos = poses[p];

					//finding rect and center
					CoordRect currCreateRect = pos.ToCoordRect(MapMagic.instance.generateRange, MapMagic.instance.terrainSize);
					CoordRect currRemoveRect = pos.ToCoordRect(MapMagic.instance.removeRange, MapMagic.instance.terrainSize);
					Coord currCenter = pos.RoundToCoord(MapMagic.instance.terrainSize);

					//checking a need to re-deploy
					if (currCreateRect != prevCreateRects[p]) reDeploy = true;
					//if (currRemoveRect != prevRemoveRects[p]) reDeploy = true;
					
					prevCreateRects[p] = currCreateRect;
					prevRemoveRects[p] = currRemoveRect;
					prevCenters[p] = currCenter;
				}

				//deploy
				if (!reDeploy) return;
				base.Deploy(prevCreateRects, prevRemoveRects, prevCenters, allowMove:allowMove);
			}

			public virtual void Reset () 
			{
				foreach (Chunk chunk in Objects()) 
					if (chunk != null) OnRemove(chunk);
				grid = new Dictionary<int,Chunk>();

				HashSet<int> oldNailedHashes = nailedHashes;
				nailedHashes = new HashSet<int>();

				//if (reDeploy)
				//{
					foreach (int nailedHash in oldNailedHashes) Nail(nailedHash.ToCoord());
					Deploy(prevCreateRects, prevRemoveRects, prevCenters);
				//}
			}

			public void CheckEmpty ()
			{
				foreach (Chunk chunk in Objects()) 
					if (chunk==null || chunk.terrain==null || chunk.terrain.terrainData==null) { Reset(); break; }
			}

			public Terrain GetTerrain (int x, int z, bool onlyComplete=true) //for welding and neighbours
			{
				Terrain terrain = null;
				
				Chunk chunk = MapMagic.instance.terrains[x,z]; 
				if (chunk!=null)
					if (chunk.complete || !onlyComplete) terrain = chunk.terrain;

				return terrain;
			}
		}//class

}//namespace