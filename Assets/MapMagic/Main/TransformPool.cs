using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MapMagic
{
	[System.Serializable]
	public class TransformPool
	{
		public struct InstanceDraft
		{
			//note taht all coordinates are local to transform pool parent
			public Vector3 pos;
			public Vector3 scale;
			public Quaternion rotation;
		}

		public Transform parent;
		public Transform prefab;
		public List<Transform> transforms = new List<Transform>();
		public bool allowReposition = true;


		public void SetTransforms (InstanceDraft[] drafts)
		{
			IEnumerator e = SetTransformsCoroutine(drafts);
			while (e.MoveNext());
		}

		public IEnumerator SetTransformsCoroutine (InstanceDraft[] drafts)
		{
			//removing all objects if reposition is not allowed
			if (!allowReposition)
			{
				for (int i=transforms.Count-1; i>=0; i--) 
					if (transforms[i] != null) GameObject.DestroyImmediate(transforms[i].gameObject);
				transforms.Clear();
			}
			
			//removing nulls from transforms array
			for (int i=transforms.Count-1; i>=0; i--)
				if (transforms[i] == null) transforms.RemoveAt(i);

			//shrinking transforms array
			if (transforms.Count > drafts.Length) 
			{
				for (int i=transforms.Count-1; i>=drafts.Length; i--)
				{
					if (transforms[i] != null) GameObject.DestroyImmediate(transforms[i].gameObject);
					transforms.RemoveAt(i);
				}	
			}
			
			//re-positioning transforms
			for (int i=transforms.Count-1; i>=0; i--)
			{
				transforms[i].localPosition = drafts[i].pos;
				transforms[i].localRotation = drafts[i].rotation;
				transforms[i].localScale = drafts[i].scale;
			}

			//creating new
			int oldObjsCount = transforms.Count;
			int newObjsCount = drafts.Length - transforms.Count;
			int objsPerFrame = 400; 
			int frames = (int)(1f * newObjsCount / objsPerFrame);

			for (int f=0; f<=frames; f++)
			{
				int numObjs = Mathf.Min(objsPerFrame, newObjsCount-f*objsPerFrame);

//				#if UNITY_5_4
//				//creating hunk
//				GameObject hunk = new GameObject();
//				hunk.name = "Unity54 Pool Hunk";
//				hunk.transform.position = parent.position;
//				#endif

				//instantiating
				for (int i=0; i<numObjs; i++)
				{
					Transform tfm = null;

					#if UNITY_EDITOR
					if (!UnityEditor.EditorApplication.isPlaying) tfm = (Transform)UnityEditor.PrefabUtility.InstantiatePrefab(prefab); 
					else tfm = (Transform)GameObject.Instantiate(prefab);
					#else
					tfm = (Transform)GameObject.Instantiate(prefab, drafts[i].pos, drafts[i].rotation);
					#endif

//					#if UNITY_5_4
//					tfm.parent = hunk.transform;
//					#else
					tfm.parent = parent;
//					#endif
					
					InstanceDraft draft = drafts[oldObjsCount + f*objsPerFrame + i];
					tfm.localPosition = draft.pos;
					tfm.localRotation = draft.rotation;
					if ((draft.scale-Vector3.one).sqrMagnitude > 0.0001f) tfm.localScale = draft.scale;

					transforms.Add(tfm);
				}

//				#if UNITY_5_4
//				hunk.transform.parent = parent;
//				#endif

				yield return null;
			}

			//removing empty hunks
			//#if UNITY_5_4
			for (int i=parent.childCount-1; i>=0; i--)
			{
				Transform child = parent.GetChild(i);
				if (child.name == "Unity54 Pool Hunk" && child.childCount==0) GameObject.DestroyImmediate(child.gameObject);
			}
			//#endif

			yield return null;
		}

		public void Clear ()
		{
			for (int i=transforms.Count-1; i>=0; i--)
				if (transforms[i]!=null && transforms[i].gameObject!=null) GameObject.DestroyImmediate(transforms[i].gameObject);
		}
	}
}
