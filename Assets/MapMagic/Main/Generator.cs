
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_5_5
using UnityEngine.Profiling;
#endif

namespace MapMagic 
{
	public class GeneratorMenuAttribute : System.Attribute
	{
		public string menu { get; set; }
		public string name { get; set; }
		public bool disengageable { get; set; }
		public bool disabled { get; set; }
		public int priority { get; set; }
	}

	[System.Serializable]
	public abstract class Generator
	{
		#region Inout

			public enum InoutType { Map, Objects, Spline }

			public interface IGuiInout
			{
				Rect guiRect { get; set; }
				//string guiName { get; set; }
				//Vector2 guiConnectionPos { get; }

				//void DrawIcon (Layout layout, bool drawLabel);
			}

			public interface IOutput {} //can store result in chunk results dict. Could be either Output or Output Generator

			public class Input : IGuiInout
			{
				public InoutType type;

				//linking
				public Output link; //{get;}
				public Generator linkGen; //{get;}

				//gui
				public Rect guiRect { get; set; }
				//public string guiName { get; set; }
				public Color guiColor 
				{get{
					bool isProSkin = false;
					#if UNITY_EDITOR
					if (UnityEditor.EditorGUIUtility.isProSkin) isProSkin = true;
					#endif

					switch (type)
					{
						case InoutType.Map: return isProSkin? new Color(0.23f, 0.5f, 0.652f) : new Color(0.05f, 0.2f, 0.35f);
						case InoutType.Objects: return isProSkin? new Color(0.15f, 0.6f, 0.15f) : new Color(0.1f, 0.4f, 0.1f);
						default: return Color.black; 
					}
				}}
				public Vector2 guiConnectionPos {get{ return new Vector2(guiRect.xMin, guiRect.center.y); }}

				public void DrawIcon (Layout layout, string label=null, bool mandatory=false, bool setRectOnly=false)
				{ 
					string textureName = "";
					switch (type) 
					{ 
						case InoutType.Map: textureName = "MapMagicMatrix"; break;
						case InoutType.Objects: textureName = "MapMagicScatter"; break;
						case InoutType.Spline: textureName = "MapMagicSpline"; break;
					}

					guiRect = new Rect(layout.field.x-5, layout.cursor.y+layout.field.y, 18,18);
					if (!setRectOnly) layout.Icon(textureName,guiRect);

					if (label != null)
					{
						Rect nameRect = guiRect;
						nameRect.width = 100; nameRect.x += guiRect.width + 2;
						layout.Label(label, nameRect,  fontSize:10);
					}

					if (mandatory && linkGen==null) 
						layout.Icon("MapMagic_Mandatory", new Rect (guiRect.x+10+2, guiRect.y-2, 8,8));
				}


				public Input () {} //default constructor to create with activator
				public Input (InoutType t) { type = t; }
				public Input (string n, InoutType t, bool write=false, bool mandatory=false) { type = t; } //lagacy constructor
			
				public Generator GetGenerator (Generator[] gens) 
				{
					for (int g=0; g<gens.Length; g++) 
						foreach (Input input in gens[g].Inputs())
							if (input == this) return gens[g];
					return null;
				}
				public void Link (Output output, Generator outputGen) { 
				link = output; linkGen = outputGen; }
				public void Unlink () { link = null; linkGen = null; }

				public object GetObject (Chunk tw)
				{ 
					if (link == null) return null;
					if (!tw.results.ContainsKey(link)) return null;
					return tw.results[link];
				}

				public T GetObject<T> (Chunk chunk) where T : class
				{
					if (link == null) return null;
					if (!chunk.results.ContainsKey(link)) return null;
					object obj = chunk.results[link];
					if (obj == null) return null;
					else return (T)obj;
				}

				//serialization
				/*public string Encode (System.Func<object,int> writeClass)
				{
					//return "offsetX=" + offset.x + " offsetZ=" + offset.z + " sizeX=" + size.x + " sizeZ=" + size.z; }
					//public void Decode (string[] lineMembers) { offset.x=(int)lineMembers[2].Parse(typeof(int)); offset.z=(int)lineMembers[3].Parse(typeof(int));
					//size.x=(int)lineMembers[4].Parse(typeof(int)); size.z=(int)lineMembers[5].Parse(typeof(int)); }
					return null;
				}

				public void Decode (string[] lineMembers, System.Func<int,object> readClass) 
				{ 
				
				}*/
			}

			public class Output : IGuiInout, IOutput
			{
				public InoutType type;
				
				//gui
				public Rect guiRect { get; set; }
				public Vector2 guiConnectionPos {get{ return new Vector2(guiRect.xMax, guiRect.center.y); }}
				
				public void DrawIcon (Layout layout, string label=null, bool setRectOnly=false) 
				{ 
					string textureName = "";
					switch (type) 
					{ 
						case InoutType.Map: textureName = "MapMagicMatrix"; break;
						case InoutType.Objects: textureName = "MapMagicScatter"; break;
						case InoutType.Spline: textureName = "MapMagicSpline"; break;
					}

					guiRect = new Rect(layout.field.x+layout.field.width-18+5, layout.cursor.y+layout.field.y, 18,18);

					if (label!=null)
					{
						Rect nameRect = guiRect;
						nameRect.width = 100; nameRect.x-= 103;
						layout.Label(label, nameRect, textAnchor:TextAnchor.LowerRight, fontSize:10);
					}

					if (!setRectOnly) layout.Icon(textureName, guiRect); //detail:resolution.ToString());

					//drawing obj id
					if (MapMagic.instance.guiDebug)
					{
						Rect idRect = guiRect;
						idRect.width = 100; idRect.x += 25;
						Chunk closest = MapMagic.instance.terrains.GetClosestObj(new Coord(0,0));
						if (closest != null)
						{
							object obj = closest.results.CheckGet(this);
							layout.Label(obj!=null? obj.GetHashCode().ToString() : "null", idRect, textAnchor:TextAnchor.LowerLeft);
						}
					}
				}

				public Output () {} //default constructor to create with activator
				public Output (InoutType t) { type = t; }
				public Output (string n, InoutType t) { type = t; } //legacy constructor

				public Generator GetGenerator (Generator[] gens) 
				{
					for (int g=0; g<gens.Length; g++) 
						foreach (Output output in gens[g].Outputs())
							if (output == this) return gens[g];
					return null;
				}

				public Input GetConnectedInput (Generator[] gens)
				{
					for (int g=0; g<gens.Length; g++) 
						foreach (Input input in gens[g].Inputs())
							if (input.link == this) return input;
					return null;
				}

				public void SetObject (Chunk terrain, object obj) //TODO: maybe better replace with CheckAdd
				{
					if (terrain.results.ContainsKey(this))
					{
						if (obj == null) terrain.results.Remove(this);
						else terrain.results[this] = obj;
					}
					else
					{
						if (obj != null) terrain.results.Add(this, obj);
					}
				}

				public T GetObject<T> (Chunk chunk) where T:class
				{
					if (!chunk.results.ContainsKey(this)) return null;
					object obj = chunk.results[this];
					if (obj == null) return null;
					else return (T)obj;
				}
			}
		#endregion

		public bool enabled = true;

		//gui
		[System.NonSerialized] public Layout layout = new Layout();
		public Rect guiRect; //just for serialization
		public float guiDebugTime;

		[System.NonSerialized] public Biome biome; //assigned automatically in GeneratorsOfType enumerable

		public virtual void Move (Vector2 delta, bool moveChildren=true) 
		{
			layout.field.position += delta;
			guiRect = layout.field;

			//moving inouts to remove lag
			foreach (Generator.IGuiInout inout in Inouts()) 
				inout.guiRect = new Rect(inout.guiRect.position+delta, inout.guiRect.size);
		}

		//inputs and outputs
		public virtual IEnumerable<Output> Outputs() { yield break; }
		public virtual IEnumerable<Input> Inputs() { yield break; }
		public IEnumerable<IGuiInout> Inouts() 
		{ 
			foreach (Output i in Outputs()) yield return i; 
			foreach (Input i in Inputs()) yield return i;
		}

		//connection states
		public static bool CanConnect (Output output, Input input) { return output.type == input.type; } //temporary out of order, before implementing resolutions

		/*public bool ValidateConnectionsRecursive ()
		{
			foreach (Input input in Inputs())
			{
				if (input.link != null)  
				{ 
					if (!CanConnect(input.link, input) || 
						input.linkGen == this ||
						!input.linkGen.ValidateConnectionsRecursive()) return false; 
				}
				else if (input.mandatory) return false;
			}
			return true;
		}*/

		public bool IsDependentFrom (Generator prior)
		{
			foreach (Input input in Inputs())
			{
				if (input==null || input.linkGen==null) continue;
				if (prior == input.linkGen) return true;
				if (input.linkGen.IsDependentFrom(prior)) return true;
			}
			return false;
		}

		public void CheckClearRecursive (Chunk tw) //checks if prior generators were clearied, and if they were - clearing this one
		{
			//if (!tw.ready.Contains(this)) //TODO: optimize here
			foreach (Input input in Inputs())
			{
				if (input.linkGen==null) continue;

				//recursive first
				input.linkGen.CheckClearRecursive(tw);

				//checking if clear
				if (!tw.ready.Contains(input.linkGen))
				{
					if (tw.ready.Contains(this)) tw.ready.Remove(this);
					//break; do not break, go on checking in case of branching-then-connecting
				}
			}
		}

		public void GenerateWithPriors (Chunk tw, Biome biome=null)
		{
			//generating input generators
			foreach (Generator.Input input in Inputs())
			{
				if (input.linkGen==null) continue;
				if (tw.stop) return; //before entry stop
				input.linkGen.GenerateWithPriors(tw, biome);
			}

			if (tw.stop) return; //before generate stop for time economy

			//generating this
			if (!tw.ready.Contains(this))
			{
				//starting timer
				if (MapMagic.instance.guiDebug)
				{
					if (tw.timer==null) tw.timer = new System.Diagnostics.Stopwatch(); 
					else tw.timer.Reset();
					tw.timer.Start();
				}

				Generate(tw, biome);
				if (!tw.stop) tw.ready.Add(this);

				//stopping timer
				if (tw.timer != null) 
				{ 
					tw.timer.Stop(); 
					guiDebugTime = tw.timer.ElapsedMilliseconds; 
				}
			}
		}

		public virtual void Generate (Chunk chunk, Biome biome=null) {}

		//public static virtual void Process (Chunk chunk)

		//gui
		public abstract void OnGUI ();
		public void OnGUIBase()
		{
			//drawing background
			layout.Element("MapMagic_Window", layout.field, new RectOffset(34,34,34,34), new RectOffset(33,33,33,33));

			//resetting layout
			layout.field.height = 0;
			layout.field.width =160;
			layout.cursor = new Rect();
			layout.change = false;
			layout.margin = 1; layout.rightMargin = 1;
			layout.fieldSize = 0.4f;              

			//drawing window header
			if (MapMagic.instance.guiDebug) UnityEngine.Profiling.Profiler.BeginSample("Header");
			layout.Icon("MapMagic_Window_Header", new Rect(layout.field.x, layout.field.y, layout.field.width, 16));

			//drawing eye icon
			layout.Par(14); layout.Inset(2);
			Rect eyeRect = layout.Inset(18);
			GeneratorMenuAttribute attribute = System.Attribute.GetCustomAttribute(GetType(), typeof(GeneratorMenuAttribute)) as GeneratorMenuAttribute;

			if (attribute != null && attribute.disengageable) 
				layout.Toggle(ref enabled, rect:eyeRect, onIcon:"MapMagic_GeneratorEnabled", offIcon:"MapMagic_GeneratorDisabled");
			else layout.Icon("MapMagic_GeneratorAlwaysOn", eyeRect, Layout.IconAligment.center, Layout.IconAligment.center);

			if (layout.lastChange && !enabled && this is IOutput) //hack to refresh on output disable
			{
//				MapMagic.instance.ForceGenerate();

				foreach (Generator sameOut in MapMagic.instance.gens.OutputGenerators(onlyEnabled:false, checkBiomes:true))
					if (sameOut.GetType() == this.GetType()) 
						foreach (Chunk chunk in MapMagic.instance.terrains.Objects()) chunk.ready.CheckRemove(sameOut); //MapMagic.instance.gens.ChangeGenerator(sameOut);
				//MapMagic.instance.Generate(); //re-generate starts itself because generator changed
			}
			

			//drawing label
			string genName = attribute==null? "Unknown" : attribute.name;

			if (MapMagic.instance.guiDebug)
			{
				bool generated = true;
				foreach (Chunk tw in MapMagic.instance.terrains.Objects())
					if (!tw.ready.Contains(this)) generated = false;
				if (!generated) genName+="*";
			}
			
			Rect labelRect = layout.Inset(); labelRect.height = 25; labelRect.y -= (1f-layout.zoom)*6 + 2;
			layout.Label(genName, labelRect, fontStyle:FontStyle.Bold, fontSize:19-layout.zoom*8);

			layout.Par(1);
			
			if (MapMagic.instance.guiDebug) UnityEngine.Profiling.Profiler.EndSample();

			//gen params
			if (MapMagic.instance.guiDebug) UnityEngine.Profiling.Profiler.BeginSample("Gen Params");
			layout.Par(3);
			if (!MapMagic.instance.guiDebug)
			{
				try {OnGUI();}
				catch (System.Exception e) 
					{if (e is System.ArgumentOutOfRangeException || e is System.NullReferenceException) Debug.LogError("Error drawing generator " + GetType() + "\n" + e);}
			}
			else OnGUI();
			layout.Par(3);
			if (MapMagic.instance.guiDebug) UnityEngine.Profiling.Profiler.EndSample();

			//drawing debug generate time
			if (MapMagic.instance.guiDebug)
			{
				Rect timerRect = new Rect(layout.field.x, layout.field.y+layout.field.height, 200, 20);
				string timeLabel = "g:" + guiDebugTime + "ms ";
				if (this is IOutput)
				{
					if (MapMagic.instance.guiDebugProcessTimes.ContainsKey(this.GetType())) timeLabel += " p:" + MapMagic.instance.guiDebugProcessTimes[this.GetType()] + "ms ";
					if (MapMagic.instance.guiDebugApplyTimes.ContainsKey(this.GetType())) timeLabel += " a:" + MapMagic.instance.guiDebugApplyTimes[this.GetType()] + "ms ";
				}
				layout.Label(timeLabel, timerRect);
				//EditorGUI.LabelField(gen.layout.ToLocal(timerRect), gen.timer.ElapsedMilliseconds + "ms");
			}
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="", name ="Portal", disengageable = true, priority = 1)]
	public class Portal : Generator
	{
		public Input input = new Input(InoutType.Map);
		public Output output = new Output(InoutType.Map);
		public override IEnumerable<Input> Inputs() { yield return input; }
		public override IEnumerable<Output> Outputs() { yield return output; }
		
		//public enum PortalType { enter, exit }
		//public PortalType type;

		public string name;
		public InoutType type;
		public enum PortalForm { In, Out }
		public PortalForm form;
		public bool drawConnections;
		
		public delegate void ChooseEnter(Portal sender, InoutType type);
		public static event ChooseEnter OnChooseEnter;

		public bool drawInputConnection
		{get{
			bool result = false;
			if (form==PortalForm.In) result = true;
			else
			{
				if (drawConnections) result = true;
				if (input.linkGen != null && ((Portal)input.linkGen).drawConnections) result = true;
			}
			return result;
		}}

		public override void Generate (Chunk chunk, Biome biome=null)
		{
			object obj = null;
			if (input.link != null && enabled) obj = input.GetObject(chunk);
			else 
			{ 
				if (type == InoutType.Map) obj = chunk.defaultMatrix;
				if (type == InoutType.Objects) obj = chunk.defaultSpatialHash;
			}

			if (chunk.stop) return;
			output.SetObject(chunk, obj); 
		}

		public override void OnGUI ()
		{
			layout.margin = 18; layout.rightMargin = 15;
			layout.Par(17); 
			if (form==PortalForm.In) 
			{
				input.DrawIcon(layout); 
				if (drawConnections) output.DrawIcon(layout); 
				else output.guiRect=new Rect(guiRect.x+guiRect.width, guiRect.y+25,0,0);
			}
			if (form==PortalForm.Out)
			{
				if (drawConnections) input.DrawIcon(layout); 
				else input.guiRect=new Rect(guiRect.x, guiRect.y+25,0,0);
				output.DrawIcon(layout);
			}

			layout.Field(ref type, rect:layout.Inset(0.39f));
			if (type != input.type) { input.Unlink(); input.type = type; output.type = type; }
			if (layout.lastChange)
			{
				foreach (Portal portal in MapMagic.instance.gens.GeneratorsOfType<Portal>())
					if (portal.input.linkGen == this) portal.input.Unlink();
			} 

			layout.Field(ref form,rect:layout.Inset(0.30f));
			layout.CheckButton(ref drawConnections, label: "", rect:layout.Inset(20), monitorChange:false, icon:"MapMagic_ShowConnections", tooltip:"Show portal connections");
			if (layout.Button("", layout.Inset(20), monitorChange:false, icon:"MapMagic_Focus_Small", disabled:form==PortalForm.In, tooltip:"Focus on input portal") && 
				input.linkGen != null)
			{
				MapMagic.instance.layout.Focus(input.linkGen.guiRect.center);
			}

			

			//select input/button
			layout.Par(20);
			if (form == PortalForm.In) name = layout.Field(name, rect:layout.Inset());
			if (form == PortalForm.Out)
			{
				string buttonLabel = "Select";
				if (input.linkGen != null) 
				{
					if (!(input.linkGen is Portal)) input.Link(null, null); //in case connected input portal was changet to output
					else buttonLabel = ((Portal)input.linkGen).name;
				}
				Rect buttonRect = layout.Inset();
				buttonRect.height -= 3;
				if (layout.Button(buttonLabel, rect:buttonRect) && OnChooseEnter!=null) OnChooseEnter(this, type);
			}
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="", name ="Group", priority = 2)]
	public class Group : Generator
	{
		public string name = "Group";
		public string comment = "Drag in generators to group them";
		public bool locked;

		[System.NonSerialized] public List<Generator> generators = new List<Generator>();


		public override void OnGUI () 
		{
			//initializing layout
			layout.cursor = new Rect();
			layout.change = false;

			//drawing background
			layout.Element("MapMagic_Group", layout.field, new RectOffset(16,16,16,16), new RectOffset(0,0,0,0));

			//lock sign
			/*Rect lockRect = new Rect(guiRect.x+guiRect.width-14-6, field.y+6, 14, 12); 
			layout.Icon(locked? "MapMagic_LockLocked":"MapMagic_LockUnlocked", lockRect, verticalAlign:Layout.IconAligment.center);
			bool wasLocked = locked;
			#if UNITY_EDITOR
			locked = UnityEditor.EditorGUI.Toggle(layout.ToDisplay(lockRect.Extend(3)), locked, GUIStyle.none);
			#endif
			if (locked && !wasLocked) LockContents();
			if (!locked && wasLocked) UnlockContents();*/

			//name and comment
			layout.margin = 5;
			layout.CheckStyles();
			float nameWidth = layout.boldLabelStyle.CalcSize( new GUIContent(name) ).x * 1.1f / layout.zoom + 10f;
			float commentWidth = layout.labelStyle.CalcSize( new GUIContent(comment) ).x / layout.zoom + 10;
			nameWidth = Mathf.Min(nameWidth,guiRect.width-5); commentWidth = Mathf.Min(commentWidth, guiRect.width-5);

			if (!locked)
			{
				layout.fontSize = 13; layout.Par(22); name = layout.Field(name, rect:layout.Inset(nameWidth), useEvent:true, style:layout.boldLabelStyle); 
				layout.fontSize = 11; layout.Par(18); comment = layout.Field(comment, rect:layout.Inset(commentWidth), useEvent:true, style:layout.labelStyle);
			}
			else
			{
				layout.fontSize = 13; layout.Par(22); layout.Label(name, rect:layout.Inset(nameWidth), fontStyle:FontStyle.Bold); 
				layout.fontSize = 11; layout.Par(18); layout.Label(comment, rect:layout.Inset(commentWidth)); 
			}
		}

		public void Populate (GeneratorsAsset gens)
		{
			generators.Clear();

			for (int i=0; i<gens.list.Length; i++)
			{
				Generator gen = gens.list[i];
				if (layout.field.Contains(gen.layout.field)) generators.Add(gen); 
			}
		}

		public override void Move (Vector2 delta, bool moveChildren=true)
		{
			base.Move(delta,true);
			if (moveChildren) for (int g=0; g<generators.Count; g++) generators[g].Move(delta,false);
		}


	}

	[System.Serializable]
	[GeneratorMenu (menu="", name ="Biome", disengageable = true, priority = 3)]
	public class Biome : Generator, Generator.IOutput
	{
		public Input mask = new Input(InoutType.Map);
		public override IEnumerable<Input> Inputs() { yield return mask; }
		
		public GeneratorsAsset data;

		public override void OnGUI ()
		{
			layout.Par(20); mask.DrawIcon(layout, "Mask");
			layout.Par(5);

			layout.fieldSize = 0.7f; layout.margin =3;
			layout.Field(ref data, "Data");

			layout.Par(20);

			if (data == null) 
				{ if (layout.Button("Create", rect:layout.Inset(0.5f))) data = ScriptableObject.CreateInstance<GeneratorsAsset>(); }
			else
				{ if (layout.Button("Edit", rect:layout.Inset(0.5f))) MapMagic.instance.guiGens = data; }

			#if UNITY_EDITOR
			if (data==null || !UnityEditor.AssetDatabase.Contains(data))
				{ if (layout.Button("Save", rect:layout.Inset(0.5f), disabled:data==null)) data.SaveAsset(); }
			else 
				{ if (layout.Button("Release", rect:layout.Inset(0.5f))) data = data.ReleaseAsset(); }
			#endif

		}


	}


}