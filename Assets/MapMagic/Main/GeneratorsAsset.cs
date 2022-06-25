using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//using Plugins;

namespace MapMagic
{
	[System.Serializable]
	public class GeneratorsAsset : ScriptableObject , ISerializationCallbackReceiver
	{
		public Generator[] list = new Generator[0];
		
		public int oldId = 0;
		public int oldCount = 0;
			

		public IEnumerable<T> GeneratorsOfType<T> (bool onlyEnabled=true, bool checkBiomes=true) where T : Generator
		{
			for (int i=0; i<list.Length; i++)
			{
				Generator gen = list[i];
				if (gen==null) continue; //unknown custom type
				if (onlyEnabled && !gen.enabled) continue;
				if (list[i] is T) 
				{
					gen.biome = null;
					yield return (T)gen;
				}
			}

			if (checkBiomes) for (int i=0; i<list.Length; i++)
			{
				Generator gen = list[i];
				if (gen==null) continue; //unknown custom type
				if (onlyEnabled && !gen.enabled) continue;

				if (gen is Biome) 
				{
					Biome biome = (Biome)gen;
					if (biome.data!=null && biome.mask.linkGen!=null) 
						foreach (T biomeGen in biome.data.GeneratorsOfType<T>(onlyEnabled, true)) 
						{
							biomeGen.biome = biome;
							yield return biomeGen;
						}
				}
			}
		}

		public IEnumerable OutputGenerators (bool onlyEnabled=true, bool checkBiomes=true) //TODO: idea make IOutput child class to Generator
		{
			for (int i=0; i<list.Length; i++)
			{
				Generator gen = list[i];
				if (gen==null) continue;
				if (onlyEnabled && !gen.enabled) continue;
				if (list[i] is Generator.IOutput) yield return (Generator.IOutput)gen;
			}

			if (checkBiomes) for (int i=0; i<list.Length; i++)
			{
				Generator gen = list[i];
				if (gen==null) continue;
				if (onlyEnabled && !gen.enabled) continue;

				if (gen is Biome) 
				{
					Biome biome = (Biome)gen;
					if (biome.data!=null && biome.mask.linkGen!=null) 
						foreach (Generator.IOutput biomeGen in biome.data.OutputGenerators(onlyEnabled, true)) yield return biomeGen;
				}
			}
		}


		public HashSet<Type> GetExistingOutputTypes (bool onlyEnabled=true, bool checkBiomes=true) //used to purge the others
		{
			HashSet<Type> existingOutputs = new HashSet<Type>();
			
			for (int i=0; i<list.Length; i++)
			{
				Generator gen = list[i];
				if (gen==null) continue;
				if (onlyEnabled && !gen.enabled) continue;
				if (list[i] is Generator.IOutput)
				{
					Type type = list[i].GetType();
					if (!existingOutputs.Contains(type)) existingOutputs.Add(type);
				} 
			}

			if (checkBiomes) for (int i=0; i<list.Length; i++)
			{
				Generator gen = list[i];
				if (gen==null) continue;
				if (onlyEnabled && !gen.enabled) continue;

				if (gen is Biome) 
				{
					Biome biome = (Biome)gen;
					if (biome.data!=null && biome.mask.linkGen!=null) 
						existingOutputs.UnionWith( biome.data.GetExistingOutputTypes(onlyEnabled, true) );
				}
			}

			return existingOutputs;
		}






		public void ChangeGenerator (Generator gen)
		{
			//if save intermediate - cleaing this generator only
			if (gen!=null && MapMagic.instance.saveIntermediate)
				foreach (Chunk chunk in MapMagic.instance.terrains.Objects()) //iterating in all terrain wrappers
				{
					chunk.ready.CheckRemove(gen);
					//foreach (Generator.Output output in gen.Outputs())
					//	chunk.results.CheckRemove(output);
				}
			
			//if do not save intermediate - clearing all generators
			if (!MapMagic.instance.saveIntermediate)
				foreach (Chunk chunk in MapMagic.instance.terrains.Objects())
					{ chunk.results.Clear(); chunk.ready.Clear(); } 
			
			//starting rebuild
			if (MapMagic.instance.instantGenerate && MapMagic.instance.enabled) MapMagic.instance.Generate();
		}


		public Generator CreateGenerator (System.Type type, Vector2 guiPos=new Vector2())
		{
			Generator gen = (Generator)System.Activator.CreateInstance(type);
 
			gen.guiRect.x = guiPos.x - gen.guiRect.width/2;
			gen.guiRect.y = guiPos.y - 10;
			if (gen is Group)
			{
				gen.guiRect.width = 300;
				gen.guiRect.height = 200;
			}

			//adding to outputs
//			if (gen is IOutput && GetGenerator(type) != null) 
//				{ Debug.LogError("MapMagic: Trying to add Output Generator while it already present in generators list"); return null; }
					
			//adding to list
			ArrayTools.Add(ref list, gen);

			return gen;
		}


		public void DeleteGenerator (Generator gen)
		{
			//removing generator from 'ready' and 'results' arrays 
			ChangeGenerator(gen);
	
			//manually resetting all dependent generators ready stae
			for (int g=0; g<list.Length; g++)
				if (list[g].IsDependentFrom(gen)) ChangeGenerator(list[g]);
				
			//clearing if it is output gen
			//if (gen is IOutput) 
			//	foreach (Chunk chunk in MapMagic.instance.terrains.Objects())
			//		(gen as IOutput).Clear(chunk);
			
			//removing group members if it is group
			#if UNITY_EDITOR
			if (gen is Group)
			{
				int dialogResult = UnityEditor.EditorUtility.DisplayDialogComplex("Remove Containing Generators", "Do you want to remove a contaning generators as well?", "Remove Generators", "Remove Group Only", "Cancel");
				if (dialogResult==2) return; //cancel
				if (dialogResult==0) //generators
				{
					Group group = gen as Group;
					group.Populate(this);
					for (int g=group.generators.Count-1; g>=0; g--) DeleteGenerator(group.generators[g]);
				}
			}
			#endif

			//unlinking and removing it's reference in inputs and outputs
			//for (int g=0; g<list.Length; g++)
			//	foreach (Generator.Input input in list[g].Inputs())
			//		if (input.linkGen == gen) input.Unlink();
			UnlinkGenerator(gen);

			//removing from output generators list
			//if (gen is IOutput) 
			//	Extensions.ArrayRemove(ref outputs, gen);

			//removing from array
			ArrayTools.Remove(ref list, gen);

			//removing from preview
			if (MapMagic.instance.previewGenerator == gen)
				{ MapMagic.instance.previewGenerator = null; MapMagic.instance.previewOutput = null; }

			//force regenerate if it was an output
			if (gen is Generator.IOutput) 
				MapMagic.instance.ForceGenerate();

		}

		public void ClearGenerators ()
		{
			list = new Generator[0];
			//outputs = new Generator[0];
		}

		public void UnlinkGenerator (Generator gen, bool unlinkGroup=false)
		{
			//unlinking
			foreach (Generator.Input input in gen.Inputs()) { if (input != null) input.Unlink(); }

			//removing it's reference in inputs and outputs
			for (int g=0; g<list.Length; g++)
				foreach (Generator.Input input in list[g].Inputs())
					if (input != null && input.linkGen == gen) input.Unlink();
			
			//unlinking group
			Group grp = gen as Group;
			if (grp != null && unlinkGroup)
			{
				for (int g=0; g<list.Length; g++)
				{
					//if generator in group - unlinking it from non-group gens
					if (grp.guiRect.Contains(list[g].guiRect)) 
						foreach (Generator.Input input in list[g].Inputs())
							if (!grp.guiRect.Contains(input.linkGen.guiRect)) input.Unlink();

					//if generator not in group - unlinking it from group gens
					if (!grp.guiRect.Contains(list[g].guiRect)) 
						foreach (Generator.Input input in list[g].Inputs())
							if (grp.guiRect.Contains(input.linkGen.guiRect)) input.Unlink();
				} 
			}
		}

		public void SortGroups ()
		{
			for (int i=list.Length-1; i>=0; i--)
			{
				Generator grp = list[i];
				if (!(grp is Group)) continue;

				for (int g=0; g<list.Length; g++)
				{
					Generator grp2 = list[g];
					if (!(grp2 is Group)) continue;

					if (grp2.layout.field.Contains(grp.layout.field)) ArrayTools.Switch(list, grp, grp2);
				}
			}
		}


		public void SaveAsset ()
		{
			#if UNITY_EDITOR
			string path= UnityEditor.EditorUtility.SaveFilePanel(
				"Save Data as Unity Asset",
				"Assets",
				"MapMagicData.asset", 
				"asset");
			if (path!=null && path.Length!=0)
			{
				path = path.Replace(Application.dataPath, "Assets");

				UnityEditor.Undo.RecordObject(MapMagic.instance, "MapMagic Save Data");
				MapMagic.instance.setDirty = !MapMagic.instance.setDirty;
					
				UnityEditor.AssetDatabase.CreateAsset(this, path);
				OnBeforeSerialize();
				UnityEditor.AssetDatabase.SaveAssets();

				UnityEditor.EditorUtility.SetDirty(MapMagic.instance);
			}
			#endif
		}

		public GeneratorsAsset ReleaseAsset ()
		{
			#if UNITY_EDITOR
			UnityEditor.Undo.RecordObject(MapMagic.instance, "MapMagic Release Data");
			MapMagic.instance.setDirty = !MapMagic.instance.setDirty;

			GeneratorsAsset newData = ScriptableObject.CreateInstance<GeneratorsAsset>();
			newData.list = (Generator[])CustomSerialization.DeepCopy(list);
			return newData;

			//UnityEditor.EditorUtility.SetDirty(MapMagic.instance);
			#else
			return null;
			#endif
		}

		public static GeneratorsAsset Default ()
		{
			GeneratorsAsset gens = ScriptableObject.CreateInstance<GeneratorsAsset>();

			//creating initial generators
			NoiseGenerator1 noiseGen = (NoiseGenerator1)gens.CreateGenerator(typeof(NoiseGenerator1), new Vector2(50,50));
			noiseGen.intensity = 0.75f;

			CurveGenerator curveGen = (CurveGenerator)gens.CreateGenerator(typeof(CurveGenerator), new Vector2(250,50));
			curveGen.curve = new AnimationCurve( new Keyframe[] { new Keyframe(0,0,0,0), new Keyframe(1,1,2.5f,1) } );

			HeightOutput heightOut = (HeightOutput)gens.CreateGenerator(typeof(HeightOutput), new Vector2(450,50));

			curveGen.input.Link(noiseGen.output, noiseGen);
			heightOut.input.Link(curveGen.output, curveGen);

			return gens;
		}



		#region Serialization


		public int serializedVersion = 0;
			
			public string[] classes = new string[0];
			public UnityEngine.Object[] objects = new UnityEngine.Object[0];
			public float[] floats = new float[0];

			public List<object> references = new List<object>();
			

			public bool setDirty;


			//outdated
			public Serializer serializer = new Serializer();
			public int listNum = 0;


			public void OnBeforeSerialize ()
			{ 
				//System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch(); timer.Start();
			
				serializedVersion = MapMagic.version;

				List<string> classesList = new List<string>();  
				List<UnityEngine.Object> objectsList = new List<UnityEngine.Object>();
				List<float> floatsList = new List<float>();
				references.Clear();

				CustomSerialization.WriteClass(list, classesList, objectsList, floatsList, references);

				classes = classesList.ToArray(); 
				objects = objectsList.ToArray();
				floats = floatsList.ToArray();

				//serializer.Clear();  
				//listNum = serializer.Store(list); 
				
				//timer.Stop(); Debug.Log("Serialize Time: " + timer.ElapsedMilliseconds + "ms");
			}

			public void OnAfterDeserialize ()
			{
				//System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch(); timer.Start(); 

				if (serializedVersion < 10) Debug.LogError("MapMagic: trying to load unknow version scene (v." + serializedVersion/10f + "). " +
					"This may cause errors or drastic drop in performance. " +  
					"Delete this MapMagic object and create the new one from scratch when possible."); 

				//loading old serializer
				if (classes.Length==0 && serializer.entities.Count!=0)
				{
					serializer.ClearLinks();
					list = (Generator[])serializer.Retrieve(listNum);
					serializer.ClearLinks();

					OnBeforeSerialize();
					serializer = null; //will not make it null, just 0-length
				}

				List<string> classesList = new List<string>();  classesList.AddRange(classes);
				List<UnityEngine.Object> objectsList = new List<UnityEngine.Object>();  objectsList.AddRange(objects);
				List<float> floatsList = new List<float>();  floatsList.AddRange(floats);

				references.Clear();
				list = (Generator[])CustomSerialization.ReadClass(0, classesList, objectsList, floatsList, references);
				references.Clear();

				//timer.Stop(); Debug.Log("Deserialize Time: " + timer.ElapsedMilliseconds + "ms");
			}

		#endregion
	}
}
