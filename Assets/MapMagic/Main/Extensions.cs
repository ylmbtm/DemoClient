using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection; //to copy properties

namespace MapMagic
{
	static public class Extensions
	{
		public static bool InRange (this Rect rect, Vector2 pos) 
		{ 
			return (rect.center - pos).sqrMagnitude < (rect.width/2f)*(rect.width/2f); 
			//return rect.Contains(pos);
		}

		public static Vector3 ToDir (this float angle) { return new Vector3( Mathf.Sin(angle*Mathf.Deg2Rad), 0, Mathf.Cos(angle*Mathf.Deg2Rad) ); }
		public static float ToAngle (this Vector3 dir) { return Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg; }

		public static Vector3 V3 (this Vector2 v2) { return new Vector3(v2.x, 0, v2.y); }
		public static Vector2 V2 (this Vector3 v3) { return new Vector2(v3.x, v3.z); }
		public static Vector3 ToV3 (this float f) { return new Vector3(f,f, f); }

		public static Quaternion EulerToQuat (this Vector3 v) { Quaternion rotation = Quaternion.identity; rotation.eulerAngles = v; return rotation; }
		public static Quaternion EulerToQuat (this float f) { Quaternion rotation = Quaternion.identity; rotation.eulerAngles = new Vector3(0,f,0); return rotation; }

		public static Vector3 Direction (this float angle) { return new Vector3( Mathf.Sin(angle*Mathf.Deg2Rad), 0, Mathf.Cos(angle*Mathf.Deg2Rad) ); }
		public static float Angle (this Vector3 dir) { return Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg; }

		public static Rect Clamp (this Rect r, float p) { return new Rect(r.x, r.y, r.width*p, r.height); }
		public static Rect ClampFromLeft (this Rect r, float p) { return new Rect(r.x+r.width*(1f-p), r.y, r.width*p, r.height); }
		public static Rect Clamp (this Rect r, int p) { return new Rect(r.x, r.y, p, r.height); }
		public static Rect ClampFromLeft (this Rect r, int p) { return new Rect(r.x+(r.width-p), r.y, p, r.height); }

		public static Rect ToRect(this Vector3 center, float range) { return new Rect(center.x-range, center.z-range, range*2, range*2); }

		public static bool Intersects (this Rect r1, Rect r2)
		{
			Vector2 r1min = r1.min; Vector2 r1max = r1.max;
			Vector2 r2min = r2.min; Vector2 r2max = r2.max;
			if (r2max.x < r1min.x || r2min.x > r1max.x || r2max.y < r1min.y || r2min.y > r1max.y) return false;
			else return true;
		}
		public static bool Intersects (this Rect r1, Rect[] rects)
		{
			for (int i=0; i<rects.Length; i++) 
				if (r1.Intersects(rects[i])) return true; //todo: remove fn call, use r1 min max
			return false;
		}

		public static bool Contains (this Rect r1, Rect r2)
		{
			Vector2 r1min = r1.min; Vector2 r1max = r1.max;
			Vector2 r2min = r2.min; Vector2 r2max = r2.max;
			if (r2min.x > r1min.x && r2max.x < r1max.x && r2min.y > r1min.y && r2max.y < r1max.y) return true;
			else return false;
		}

		public static Rect Extend (this Rect r, float f) { return new Rect(r.x-f, r.y-f, r.width+f*2, r.height+f*2); }

		public static float RectangularDistToRect (this Vector3 pos, Vector3 min, float size)
		{
			//finding x distance
			float distPosX = min.x - pos.x;
			float distNegX = pos.x - min.x - size;
			
			float distX;
			if (distPosX >= 0) distX = distPosX;
			else if (distNegX >= 0) distX = distNegX;
			else distX = 0;

			//finding z distance
			float distPosZ = min.z - pos.z;
			float distNegZ = pos.z - min.z - size;
			
			float distZ;
			if (distPosZ >= 0) distZ = distPosZ;
			else if (distNegZ >= 0) distZ = distNegZ;
			else distZ = 0;

			//returning the maximum(!) distance 
			if (distX > distZ) return distX;
			else return distZ;
		}

		/*public static Coord ToCoord (this Vector3 pos, float cellSize, bool ceil=false) //to use in object grid
		{
			if (!ceil) return new Coord(
				Mathf.FloorToInt((pos.x) / cellSize),
				Mathf.FloorToInt((pos.z) / cellSize) ); 
			else return new Coord(
				Mathf.CeilToInt((pos.x) / cellSize),
				Mathf.CeilToInt((pos.z) / cellSize) ); 
		}*/

		public static Coord RoundToCoord (this Vector2 pos) //to use in spatial hash (when sphash and matrix sizes are equal)
		{
			int posX = (int)(pos.x + 0.5f); if (pos.x < 0) posX--; //snippet for RoundToInt
			int posZ = (int)(pos.y + 0.5f); if (pos.y < 0) posZ--;
			return new Coord(posX, posZ);
		}

		public static Coord FloorToCoord (this Vector3 pos, float cellSize) { return new Coord( Mathf.FloorToInt(pos.x / cellSize),		Mathf.FloorToInt(pos.z / cellSize)  ); }
		public static Coord CeilToCoord (this Vector3 pos, float cellSize) { return new Coord( Mathf.CeilToInt(pos.x / cellSize),		Mathf.CeilToInt(pos.z / cellSize)  ); }
		public static Coord RoundToCoord (this Vector3 pos, float cellSize) { return new Coord( Mathf.RoundToInt(pos.x / cellSize),		Mathf.RoundToInt(pos.z / cellSize)  ); }
		public static CoordRect ToCoordRect (this Vector3 pos, float range, float cellSize) //this one works with Terrain Sculptor
		{
			Coord min = new Coord( Mathf.FloorToInt((pos.x-range)/cellSize),	Mathf.FloorToInt((pos.z-range)/cellSize)  );
			Coord max = new Coord( Mathf.FloorToInt((pos.x+range)/cellSize),	Mathf.FloorToInt((pos.z+range)/cellSize)  )  +  1;
			return new CoordRect(min, max-min);
		}

		/*public static CoordRect ToCoordRect (this Vector3 pos, float range, float cellSize) //this one works with Terrain Sculptor
		{
			Coord size = (Vector3.one*range*2).CeilToCoord(cellSize) + 1;
			Coord offset = pos.FloorToCoord(cellSize) - size/2;
			return new CoordRect (offset, size);
		}*/

		public static CoordRect GetHeightRect (this Terrain terrain) 
		{
			float pixelSize = terrain.terrainData.size.x / terrain.terrainData.heightmapResolution;

			int posX = (int)(terrain.transform.localPosition.x/pixelSize + 0.5f); if (terrain.transform.localPosition.x < 0) posX--;
			int posZ = (int)(terrain.transform.localPosition.z/pixelSize + 0.5f); if (terrain.transform.localPosition.z < 0) posZ--;

			return new CoordRect(posX, posZ, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);
		}

		public static CoordRect GetSplatRect (this Terrain terrain) 
		{
			float pixelSize = terrain.terrainData.size.x / terrain.terrainData.alphamapResolution;

			int posX = (int)(terrain.transform.localPosition.x/pixelSize + 0.5f); if (terrain.transform.localPosition.x < 0) posX--;
			int posZ = (int)(terrain.transform.localPosition.z/pixelSize + 0.5f); if (terrain.transform.localPosition.z < 0) posZ--;

			return new CoordRect(posX, posZ, terrain.terrainData.alphamapResolution, terrain.terrainData.alphamapResolution);
		}

		public static float[,] SafeGetHeights (this TerrainData data, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			if (offsetX<0) { sizeX += offsetX; offsetX=0; } if (offsetZ<0) { sizeZ += offsetZ; offsetZ=0; } //Not Tested!
			int res = data.heightmapResolution;
			if (sizeX+offsetX > res) sizeX = res-offsetX; if (sizeZ+offsetZ > res) sizeZ = res-offsetZ;
			return data.GetHeights(offsetX, offsetZ, sizeX, sizeZ);
		}

		public static float[,,] SafeGetAlphamaps (this TerrainData data, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			if (offsetX<0) { sizeX += offsetX; offsetX=0; } if (offsetZ<0) { sizeZ += offsetZ; offsetZ=0; } //Not Tested!
			int res = data.alphamapResolution;
			if (sizeX+offsetX > res) sizeX = res-offsetX; if (sizeZ+offsetZ > res) sizeZ = res-offsetZ;
			return data.GetAlphamaps(offsetX, offsetZ, sizeX, sizeZ);
		}

		public static List<Type> GetAllChildTypes (this Type type)
		{
			List<Type> result = new List<Type>();
		
			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetAssembly(type);
			Type[] allTypes = assembly.GetTypes();
			for (int i=0; i<allTypes.Length; i++) 
				if (allTypes[i].IsSubclassOf(type)) result.Add(allTypes[i]); //nb: IsAssignableFrom will return derived classes

			return result;
		}

		public static Texture2D ColorTexture (int width, int height, Color color)
		{
			Texture2D result = new Texture2D(width, height);
			Color[] pixels = result.GetPixels(0,0,width,height);
			for (int i=0;i<pixels.Length;i++) pixels[i] = color;
			result.SetPixels(0,0,width,height, pixels);
			result.Apply();
			return result;
		}

		public static bool Equal (Vector3 v1, Vector3 v2)
		{
			return Mathf.Approximately(v1.x, v2.x) && 
					Mathf.Approximately(v1.y, v2.y) && 
					Mathf.Approximately(v1.z, v2.z);
		}
		
		public static bool Equal (Ray r1, Ray r2)
		{
			return Equal(r1.origin, r2.origin) && Equal(r1.direction, r2.direction);
		}

		public static void RemoveChildren (this Transform tfm)
		{
			for (int i=tfm.childCount-1; i>=0; i--)
			{
				Transform child = tfm.GetChild(i);
				GameObject.DestroyImmediate(child.gameObject); 
			}
		}

		public static Transform FindChildRecursive (this Transform tfm, string name)
		{
			int numChildren = tfm.childCount;

			for (int i=0; i<numChildren; i++)
				if (tfm.GetChild(i).name == name) return tfm.GetChild(i);

			for (int i=0; i<numChildren; i++)
			{
				Transform result = tfm.GetChild(i).FindChildRecursive(name);
				if (result != null) return result;
			}

			return null;
		}

		public static void ToggleDisplayWireframe (this Transform tfm, bool show)
		{
			#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetSelectedWireframeHidden(tfm.GetComponent<Renderer>(), !show);
			int childCount = tfm.childCount;
			for (int c=0; c<childCount; c++) tfm.GetChild(c).ToggleDisplayWireframe(show);
			#endif
		}

		public static int ToInt (this Coord coord)
		{
			int absX = coord.x<0? -coord.x : coord.x; 
			int absZ = coord.z<0? -coord.z : coord.z;

			return ((coord.z<0? 1000000000 : 0) + absX*30000 + absZ) * (coord.x<0? -1 : 1);
		}

		public static Coord ToCoord (this int hash)
		{
			int absHash = hash<0? -hash : hash;
			int sign = (absHash/1000000000)*1000000000;

			int absX = (absHash - sign)/30000;
			int absZ = absHash - sign - absX*30000;

			return new Coord(hash<0? -absX : absX, sign==0? absZ : -absZ);
		}

		public static void CheckAdd<TKey,TValue> (this Dictionary<TKey,TValue> dict, TKey key, TValue value, bool replace=true)
		{
			if (dict.ContainsKey(key)) 
				{ if (replace) dict[key] = value; }
			else dict.Add(key, value);
		}
		public static void CheckRemove<TKey,TValue> (this Dictionary<TKey,TValue> dict, TKey key) { if (dict.ContainsKey(key)) dict.Remove(key); }
		public static TValue CheckGet<TKey,TValue> (this Dictionary<TKey,TValue> dict, TKey key)
		{
			if (dict.ContainsKey(key)) return dict[key];
			else return default(TValue);
		}

		public static void CheckAdd<T> (this HashSet<T> set, T obj) { if (!set.Contains(obj)) set.Add(obj); }
		public static void CheckRemove<T> (this HashSet<T> set, T obj) { if (set.Contains(obj)) set.Remove(obj); }
		public static void SetState<T> (this HashSet<T> set, T obj, bool state)
		{
			if (state && !set.Contains(obj)) set.Add(obj);
			if (!state && set.Contains(obj)) set.Remove(obj);
		}

		public static void Normalize (this float[,,] array, int pinnedLayer)
		{
			int maxX = array.GetLength(0); int maxZ = array.GetLength(1); int numLayers = array.GetLength(2);
			for (int x=0; x<maxX; x++)
				for (int z=0; z<maxZ; z++)
			{
				float othersSum = 0;

				for (int i=0; i<numLayers; i++)
				{
					if (i==pinnedLayer) continue;
					othersSum += array[x,z,i];
				}

				float pinnedValue = array[x,z,pinnedLayer];
				if (pinnedValue > 1) { pinnedValue = 1; array[x,z,pinnedLayer] = 1; }
				if (pinnedValue < 0) { pinnedValue = 0; array[x,z,pinnedLayer] = 0; }

				float othersTargetSum = 1 - pinnedValue;
				float factor = othersSum>0? othersTargetSum / othersSum : 0;

				for (int i=0; i<numLayers; i++)
				{
					if (i==pinnedLayer) continue;
					 array[x,z,i] *= factor;
				}
			}

		}

		public static void DrawDebug (this Vector3 pos, float range=1, Color color=new Color())
		{
			if (color.a<0.001f) color = Color.white;
			Debug.DrawLine(pos + new Vector3(-1,0,1)*range, pos + new Vector3(1,0,1)*range, color);
			Debug.DrawLine(pos + new Vector3(1,0,1)*range, pos + new Vector3(1,0,-1)*range, color);
			Debug.DrawLine(pos + new Vector3(1,0,-1)*range, pos + new Vector3(-1,0,-1)*range, color);
			Debug.DrawLine(pos + new Vector3(-1,0,-1)*range, pos + new Vector3(-1,0,1)*range, color);
		}

		public static void DrawDebug (this Rect rect, Color color=new Color())
		{
			if (color.a<0.001f) color = Color.white;
			Debug.DrawLine(	new Vector3(rect.x,0,rect.y),							new Vector3(rect.x+rect.width,0,rect.y),				color);
			Debug.DrawLine(	new Vector3(rect.x+rect.width,0,rect.y),				new Vector3(rect.x+rect.width,0,rect.y+rect.height),	color);
			Debug.DrawLine(	new Vector3(rect.x+rect.width,0,rect.y+rect.height),	new Vector3(rect.x,0,rect.y+rect.height),				color);
			Debug.DrawLine(	new Vector3(rect.x,0,rect.y+rect.height),				new Vector3(rect.x,0,rect.y),							color);
		}

		public static void Resize (this Terrain terrain, int resolution, Vector3 size)
		{
			//setting resolution and THEN terrain size is too laggy
			//so making this trick to resize terrain or change res
			if ((terrain.terrainData.size-size).sqrMagnitude > 0.01f || terrain.terrainData.heightmapResolution != resolution) 
			{
				if (resolution <= 64) //brute force
				{
					terrain.terrainData.heightmapResolution = resolution;
					terrain.terrainData.size = new Vector3(size.x, size.y, size.z);
				}

				else //setting res 64, re-scaling to 1/64, and then changing res
				{
					terrain.terrainData.heightmapResolution = 65;
					terrain.Flush(); //otherwise unity crushes without an error
					int resFactor = (resolution-1) / 64;
					terrain.terrainData.size = new Vector3(size.x/resFactor, size.y, size.z/resFactor);
					terrain.terrainData.heightmapResolution = resolution;
				}
			}
		}

		public static Transform AddChild (this Transform tfm, string name="", Vector3 offset=new Vector3())
		{
			GameObject go = new GameObject();
			go.name = name;
			go.transform.parent = tfm;
			go.transform.localPosition = offset;

			return go.transform;
		}

		public static IEnumerable<Vector3> CircleAround (this Vector3 center, float radius, int numPoints, bool endWhereStart=false)
		{
			float radianStep = 2*Mathf.PI / numPoints;
			if (endWhereStart) numPoints++;
			for (int i=0; i<numPoints; i++)
			{
				float angle = i*radianStep;
				Vector3 dir = new Vector3( Mathf.Sin(angle), 0, Mathf.Cos(angle) );
				yield return center + dir*radius;
			}
		}

		public static float EvaluateMultithreaded (this AnimationCurve curve, float time)
		{
			int keyCount = curve.keys.Length;
			
			if (time <= curve.keys[0].time) return curve.keys[0].value;
			if (time >= curve.keys[keyCount-1].time) return curve.keys[keyCount-1].value; 

			int keyNum = 0;
			for (int k=0; k<keyCount-1; k++)
			{
				if (curve.keys[keyNum+1].time > time) break;
				keyNum++;
			}
			
			float delta = curve.keys[keyNum+1].time - curve.keys[keyNum].time;
			float relativeTime = (time - curve.keys[keyNum].time) / delta;

			float timeSq = relativeTime * relativeTime;
			float timeCu = timeSq * relativeTime;
     
			float a = 2*timeCu - 3*timeSq + 1;
			float b = timeCu - 2*timeSq + relativeTime;
			float c = timeCu - timeSq;
			float d = -2*timeCu + 3*timeSq;

			return a*curve.keys[keyNum].value + b*curve.keys[keyNum].outTangent*delta + c*curve.keys[keyNum+1].inTangent*delta + d*curve.keys[keyNum+1].value;
		}

		public static bool IdenticalTo (this AnimationCurve c1, AnimationCurve c2)
		{
			if (c1==null || c2==null) return false;
			if (c1.keys.Length != c2.keys.Length) return false;
			
			int numKeys = c1.keys.Length;
			for (int k=0; k<numKeys; k++)
			{
				if (c1.keys[k].time != c2.keys[k].time ||
					c1.keys[k].value != c2.keys[k].value ||
					c1.keys[k].inTangent != c2.keys[k].inTangent ||
					c1.keys[k].outTangent != c2.keys[k].outTangent)
						return false;
			}

			return true;
		}

		public static Keyframe[] Copy (this Keyframe[] src)
		{
			Keyframe[] dst = new Keyframe[src.Length];
			for (int k=0; k<src.Length; k++) 
			{
				dst[k].value = src[k].value;
				dst[k].time = src[k].time;
				dst[k].inTangent = src[k].inTangent;
				dst[k].outTangent = src[k].outTangent;
			}
			return dst;
		}

		public static AnimationCurve Copy (this AnimationCurve src)
		{
			AnimationCurve dst = new AnimationCurve();
			dst.keys = src.keys.Copy();
			return dst;
		}


		public static void GetPropertiesFrom<T1,T2> (this T1 dst, T2 src) where T1:class where T2:class
		{
			PropertyInfo[] srcProps = src.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
			PropertyInfo[] dstProps = src.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
         
			for (int sp=0; sp<srcProps.Length; sp++)
				for (int dp=0; dp<dstProps.Length; dp++)
			{
				if (srcProps[sp].Name==dstProps[dp].Name && dstProps[dp].CanWrite)
					dstProps[dp].SetValue(dst, srcProps[sp].GetValue(src, null), null);
			}
         }


		public static IEnumerable<FieldInfo> UsableFields (this Type type, bool nonPublic=false)
		{
			BindingFlags flags;
			if (nonPublic) flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance; 
			else flags = BindingFlags.Public | BindingFlags.Instance; 
			
			FieldInfo[] fields = type.GetFields(flags);
			for (int i=0; i<fields.Length; i++)
			{
				FieldInfo field = fields[i];
				if (field.IsLiteral) continue; //leaving constant fields blank
				if (field.FieldType.IsPointer) continue; //skipping pointers (they make unity crash. Maybe require unsafe)
				if (field.IsNotSerialized) continue;

				yield return field;
			}
		}

		public static IEnumerable<PropertyInfo> UsableProperties (this Type type, bool nonPublic=false, bool skipItems=true)
		{
			BindingFlags flags;
			if (nonPublic) flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance; 
			else flags = BindingFlags.Public | BindingFlags.Instance; 

			PropertyInfo[] properties = type.GetProperties(flags);
			for (int i=0;i<properties.Length;i++) 
			{
				PropertyInfo prop = properties[i];
				if (!prop.CanWrite) continue;
				if (skipItems && prop.Name=="Item") continue; //ignoring this[x] 

				yield return prop;
			}
		}

		public static IEnumerable<MemberInfo> UsableMembers (this Type type, bool nonPublic=false, bool skipItems=true)
		{
			BindingFlags flags;
			if (nonPublic) flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance; 
			else flags = BindingFlags.Public | BindingFlags.Instance; 

			FieldInfo[] fields = type.GetFields(flags);
			for (int i=0; i<fields.Length; i++)
			{
				FieldInfo field = fields[i];
				if (field.IsLiteral) continue; //leaving constant fields blank
				if (field.FieldType.IsPointer) continue; //skipping pointers (they make unity crash. Maybe require unsafe)
				if (field.IsNotSerialized) continue;

				yield return field;
			}

			PropertyInfo[] properties = type.GetProperties(flags);
			for (int i=0;i<properties.Length;i++) 
			{
				PropertyInfo prop = properties[i];
				if (!prop.CanWrite) continue;
				if (skipItems && prop.Name=="Item") continue; //ignoring this[x] 

				yield return prop;
			}
		}

		public static Component CopyComponent (Component src, GameObject go)
		{
			System.Type type = src.GetType();
			
			Component dst = go.GetComponent(src.GetType());
			if (dst==null) dst = go.AddComponent(type);

			foreach (FieldInfo field in type.UsableFields(nonPublic:true)) field.SetValue(dst, field.GetValue(src));
			foreach (PropertyInfo prop in type.UsableProperties(nonPublic:true))
			{
				if (prop.Name == "name") continue;
				try {prop.SetValue(dst, prop.GetValue(src, null), null); }
				catch { }
			}

			return dst;
		}

		public static T GetAddComponent<T> (this GameObject go) where T:Component
		{
			T c = go.GetComponent<T>();
			if (c==null) c = go.AddComponent<T>();
			return c;
		}

		public static void ReflectionReset<T> (this T obj) 
		{
			Type type = obj.GetType();
			T empty = (T)Activator.CreateInstance(type);
			
			foreach (FieldInfo field in type.UsableFields(nonPublic:true)) field.SetValue(obj, field.GetValue(empty));
			foreach (PropertyInfo prop in type.UsableProperties(nonPublic:true)) prop.SetValue(obj, prop.GetValue(empty,null), null);
		}

		public static T ReflectionCopy<T> (this T obj)
		{
			Type type = obj.GetType();
			T copy = (T)Activator.CreateInstance(type);

			foreach (FieldInfo field in type.UsableFields(nonPublic:true)) field.SetValue(copy, field.GetValue(obj));
			foreach (PropertyInfo prop in type.UsableProperties(nonPublic:true)) prop.SetValue(copy, prop.GetValue(obj,null), null);

			return copy;
		}

		public static object Parse (this string s, Type t)
		{
			//better than creating xml serializer each time. Reverse to "ToString" function

			if (s.Contains("=")) s = s.Remove(0, s.IndexOf('=')+1); //removing everything before =

			object r = null;
			if (t == typeof(float)) r = float.Parse(s);
			else if (t == typeof(int)) r = int.Parse(s);
			else if (t == typeof(bool)) r = bool.Parse(s);
			else if (t == typeof(string)) r = s;
			else if (t == typeof(byte)) r = byte.Parse(s);
			else if (t == typeof(short)) r = short.Parse(s);
			else if (t == typeof(long)) r = long.Parse(s);
			else if (t == typeof(double)) r = double.Parse(s);
			else if (t == typeof(char)) r = char.Parse(s);
			else if (t == typeof(decimal)) r = decimal.Parse(s);
			else if (t == typeof(sbyte)) r = sbyte.Parse(s);
			else if (t == typeof(uint)) r = uint.Parse(s);
			else if (t == typeof(ulong)) r = ulong.Parse(s);
			else return null;

			return r;
		}

		public static float GetInterpolated (this float[,] array, float x, float z)
		{
			int sizeX = array.GetLength(0);
			int sizeY = array.GetLength(1);
			
			int px = (int)x; 
			int nx = px+1; if (nx >= sizeX) nx = sizeX-1;

			int py = (int)z;
			int ny = py+1; if (ny >= sizeY) ny = sizeY-1;

			float percentX = x-px;
			float percentZ = z-py;

			float val_fy = array[px,py]*(1-percentX) + array[nx,py]*percentX;
			float val_cy = array[px,ny]*(1-percentX) + array[nx,ny]*percentX;
			float val = val_fy*(1-percentZ) + val_cy*percentZ;

			return val;
		}

		public static bool IsEditor ()
		{
			#if UNITY_EDITOR
				return 
					!UnityEditor.EditorApplication.isPlaying; //if not playing
					//(UnityEditor.EditorWindow.focusedWindow != null && UnityEditor.EditorWindow.focusedWindow.GetType() == System.Type.GetType("UnityEditor.GameView,UnityEditor")) //if game view is focused
					//UnityEditor.SceneView.lastActiveSceneView == UnityEditor.EditorWindow.focusedWindow; //if scene view is focused
			#else
				return false;
			#endif
		}

		public static bool IsSelected (Transform transform)
		{
			#if UNITY_EDITOR
				return UnityEditor.Selection.activeTransform == transform;
			#else
				return false;
			#endif
		}

		public static Camera GetMainCamera ()
		{
			if (IsEditor()) 
			{
				#if UNITY_EDITOR
				if (UnityEditor.SceneView.lastActiveSceneView==null) return null;
				else return UnityEditor.SceneView.lastActiveSceneView.camera;
				#else
				return null;
				#endif
			}
			else
			{
				Camera mainCam = Camera.main;
				if (mainCam==null) mainCam = GameObject.FindObjectOfType<Camera>(); //in case it was destroyed or something
				return mainCam;
			}
		}

		public static Vector2 GetMousePosition ()
		{
			if (IsEditor()) 
			{
				#if UNITY_EDITOR
				Vector2 mousePos = Event.current.mousePosition;
				mousePos.y = Screen.height - mousePos.y - 40;
				return mousePos;
				//TODO: Error with that on Mac
				#else
				return Input.mousePosition;
				#endif
			}
			else return Input.mousePosition;
		}


	}//extensions
}//namespace
