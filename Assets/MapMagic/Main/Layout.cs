using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MapMagic
{
	public class Layout
	{
		public struct Val 
		{
			public float val;
			public bool ovd;

			public static implicit operator Val(bool b) { return new Val() {val=b? 1 : 0, ovd=true}; }
			public static implicit operator Val(float f) { return new Val() {val=f, ovd=true}; }
			public static implicit operator Val(int i) { return new Val() {val=i, ovd=true}; }

			public static implicit operator bool(Val v) { if (v.val>0.5f) return true; else return false; }
			public static implicit operator float(Val v) { return v.val; }
			public static implicit operator int(Val v) { return (int)v.val; }

			//public static implicit operator Val(NumericType t) { return new Val() {val=(int)t, ovd=true}; }
			//public static implicit operator Val(LabelType t) { return new Val() {val=(int)t, ovd=true}; }
			//public static implicit operator Val(ToggleType t) { return new Val() {val=(int)t, ovd=true}; }

			public void Verify (float def) { if (!ovd) val = def; }
			public void Verify (int def) { if (!ovd) val = def; }
			public void Verify (bool def) { if (!ovd) val = def? 1 : 0; }
		}
		
		
		public Rect field; //background rect of layout. Height increases every new line (or stays fixed, which is max)
		public Rect cursor; //rect with zero width. X and Y are relative to field
		public Rect lastRect;

		public int margin = 10;
		public int rightMargin = 10;
		public int lineHeight = 18;

		public int verticalPadding = 2;
		public int horizontalPadding = 3;

		public UnityEngine.Object undoObject;
		public string undoName = "";

		
		//gets rect to draw in inspector
		public static Rect GetInspectorRect ()
		{
			#if UNITY_EDITOR
			UnityEditor.EditorGUI.indentLevel = 0;
			return UnityEditor.EditorGUILayout.GetControlRect(GUILayout.Height(0));
			//UnityEditor.EditorGUIUtility.currentViewWidth;
			//GUILayoutUtility.GetRect(1, 0);
			#else
			return new Rect();
			#endif
		}

		//sets field to inspector
		static public void SetInspectorRect (Rect rect)
		{
			if (Event.current.type == EventType.Layout) GUILayoutUtility.GetRect(1, rect.height, "TextField");
		}


		public void Par (
			Val height = new Val(),
			Val margin = new Val(),
			Val padding = new Val() )
		{ 
			//current params
			int _height = height.ovd? (int)height.val : this.lineHeight;
			int _margin = margin.ovd? (int)margin.val : this.margin;
			int _padding = padding.ovd? (int)padding.val : this.verticalPadding;

			//setting rects
			cursor = new Rect(field.x+_margin, cursor.y + cursor.height + _padding, 0, _height - _padding);
			field = new Rect(field.x, field.y, field.width, Mathf.Max(field.height, cursor.y+cursor.height));
		}

		public Rect Inset (
			Val width = new Val(), 
			Val margin = new Val(), 
			Val rightMargin = new Val(), 
			Val padding = new Val() )
		{
			//finding current params
			int _margin = margin.ovd? (int)margin.val : this.margin;
			int _rightMargin = rightMargin.ovd? (int)rightMargin.val : this.rightMargin;
			int _padding = padding.ovd? (int)padding.val : this.verticalPadding;
			float _width = width.ovd? width.val : 1; if (_width < 1.0001f) _width *= field.width-_margin-_rightMargin; //width should be in pixels
			
			//setting rects
			cursor.x += _width;
			lastRect = new Rect (cursor.x-_width, cursor.y+field.y, _width-_padding, cursor.height);
			return lastRect;
		}

		public Rect ParInset (
			Val height = new Val(),
			Val width = new Val(), 
			Val margin = new Val(), 
			Val rightMargin = new Val(), 
			Val verticalPadding = new Val(),
			Val horizontalPadding = new Val() )
		{ 
			Par(height, margin, verticalPadding); 
			return Inset(width, margin, rightMargin, horizontalPadding); 
		}


		#region Scroll/zoom

			public Vector2 scroll = new Vector2(0,0);
			public float zoom = 1;

			public float zoomStep = 0.0625f;
			public float minZoom=0.25f;
			public float maxZoom=2f;
			public int scrollButton = 2;
			

			public void Zoom ()
			{
				if (Event.current == null) return;

				//reading control
				#if UNITY_EDITOR_OSX
				bool control = Event.current.command;
				#else
				bool control = Event.current.control;
				#endif

				float delta = 0;
				if (Event.current.type==EventType.ScrollWheel) delta = Event.current.delta.y / 3f;
				else if (Event.current.type==EventType.MouseDrag && Event.current.button==0 && control)
					delta = Event.current.delta.y / 15f;
				//else if (control && Event.current.alt && Event.current.type==EventType.KeyDown && Event.current.keyCode==KeyCode.Equals) delta --;
				//else if (control && Event.current.alt && Event.current.type==EventType.KeyDown && Event.current.keyCode==KeyCode.Minus) delta ++;
				
				if (Mathf.Abs(delta)<0.001f) return;

				float zoomChange = - zoom*zoomStep*delta; //progressive step

				//returning if zoom will be out-of-range
				//if (zoom+zoomChange > maxZoom || zoom+zoomChange < minZoom) return;

				//clamping zoom change so it will never be out of range
				if (zoom+zoomChange > maxZoom) zoomChange = maxZoom-zoom;
				if (zoom+zoomChange < minZoom) zoomChange = minZoom-zoom;
			
				//record mouse position in worldspace
				Vector2 worldMousePos = (Event.current.mousePosition - scroll)/zoom;

				//changing zoom
				zoom += zoomChange;
			
				if (zoom >= minZoom && zoom <= maxZoom) scroll -= worldMousePos*zoomChange;
				//zoom = Mathf.Clamp(zoom, minZoom, maxZoom); //returning on out-of-range instead

				#if UNITY_EDITOR
				if (UnityEditor.EditorWindow.focusedWindow != null) UnityEditor.EditorWindow.focusedWindow.Repaint(); 
				#endif
			}

			public void Scroll ()
			{
				if (Event.current == null || Event.current.type!=EventType.MouseDrag) return;
				if (!(Event.current.button==scrollButton || (Event.current.button==0 && Event.current.alt))) return;			 
				
				scroll += Event.current.delta;
				 
				#if UNITY_EDITOR
				UnityEditor.EditorWindow.focusedWindow.Repaint(); 
				#endif
			}


			public Rect ToDisplay (Rect rect)
				{ return new Rect(rect.x*zoom+scroll.x, rect.y*zoom+scroll.y, rect.width*zoom, rect.height*zoom ); }

			public Rect ToInternal (Rect rect)
				{ return new Rect( (rect.x-scroll.x)/zoom, (rect.y-scroll.y)/zoom, rect.width/zoom, rect.height/zoom ); }

			public Vector2 ToInternal (Vector2 pos) { return (pos-scroll)/zoom; } //return new Vector2( (pos.x-scroll.x)/zoom, (pos.y-scroll.y)/zoom); }
			public Vector2 ToDisplay (Vector2 pos) { return pos*zoom + scroll; }

			public void Focus (Vector2 pos)
			{
				pos *= zoom;
				scroll = -pos;
				scroll += new Vector2(field.width/2f, field.height/2f); //note that field size should be equal to window size (use layout.field = this.position)
			}
				

		#endregion

		#region Styles

			[System.NonSerialized] public GUIStyle labelStyle = null;
			[System.NonSerialized] public GUIStyle boldLabelStyle = null;
			[System.NonSerialized] public GUIStyle foldoutStyle = null;
			[System.NonSerialized] public GUIStyle fieldStyle = null;
			[System.NonSerialized] public GUIStyle buttonStyle = null;
			[System.NonSerialized] public GUIStyle enumZoomStyle = null;
			[System.NonSerialized] public GUIStyle urlStyle = null;
			[System.NonSerialized] public GUIStyle toolbarStyle = null;
			[System.NonSerialized] public GUIStyle toolbarButtonStyle = null;
			[System.NonSerialized] public GUIStyle helpBoxStyle = null;

			public void CheckStyles ()
			{
				#if UNITY_EDITOR
				if (labelStyle == null) 
				{
					labelStyle = new GUIStyle(UnityEditor.EditorStyles.label); labelStyle.active.textColor = Color.black; labelStyle.focused.textColor = labelStyle.active.textColor = labelStyle.normal.textColor;
					boldLabelStyle = new GUIStyle(UnityEditor.EditorStyles.label); boldLabelStyle.fontStyle = FontStyle.Bold; boldLabelStyle.focused.textColor = boldLabelStyle.active.textColor = boldLabelStyle.normal.textColor;
					urlStyle = new GUIStyle(UnityEditor.EditorStyles.label); urlStyle.normal.textColor = new Color(0.3f, 0.5f, 1f); 
					foldoutStyle = new GUIStyle(UnityEditor.EditorStyles.foldout);  foldoutStyle.fontStyle = FontStyle.Bold;
					fieldStyle = new GUIStyle(UnityEditor.EditorStyles.numberField);
					buttonStyle = new GUIStyle("Button"); 
					enumZoomStyle = new GUIStyle(UnityEditor.EditorStyles.miniButton); enumZoomStyle.alignment = TextAnchor.MiddleLeft;
					toolbarStyle = new GUIStyle(UnityEditor.EditorStyles.toolbar);
					toolbarButtonStyle = new GUIStyle(UnityEditor.EditorStyles.toolbarButton);  
					helpBoxStyle = new GUIStyle(UnityEditor.EditorStyles.helpBox);  
				}

				int fontSize = Mathf.RoundToInt(this.fontSize * zoom);
				if (labelStyle.fontSize != fontSize)
				{
					labelStyle.fontSize = fontSize;
					boldLabelStyle.fontSize = fontSize;
					urlStyle.fontSize = fontSize; 
					foldoutStyle.fontSize = fontSize;		
					
					buttonStyle.fontSize = fontSize;
					
					toolbarStyle.fontSize = fontSize;
					toolbarButtonStyle.fontSize = fontSize;
					
				}

				int fieldFontSize = Mathf.RoundToInt(14 * zoom * 0.8f);
				if (fieldStyle.fontSize != fieldFontSize)
				{
					fieldStyle.fontSize = fieldFontSize;
					enumZoomStyle.fontSize = fieldFontSize; 
				}

				int helpBoxFontSize = Mathf.RoundToInt(9*zoom*0.5f*(1+zoom));
				if (helpBoxStyle.fontSize != helpBoxFontSize) helpBoxStyle.fontSize = helpBoxFontSize;
				#endif
			}

		#endregion

		#region Icon

			[System.NonSerialized] Dictionary<string, Texture2D> icons = new Dictionary<string, Texture2D>();

			public Texture2D GetIcon (string textureName)
			{
				string nonProName = textureName;
				#if UNITY_EDITOR
				if (UnityEditor.EditorGUIUtility.isProSkin) textureName += "_pro";
				#endif
				
				Texture2D texture=null;
				if (!icons.ContainsKey(textureName))
				{
					texture = Resources.Load(textureName) as Texture2D;
					if (texture==null) texture = Resources.Load(nonProName) as Texture2D; //trying to load a texture without _pro

					icons.Add(textureName, texture);
				}
				else texture = icons[textureName]; 
				return texture;
			}

			public enum IconAligment { resize, min, max, center }

			public bool Icon (string textureName, Rect rect, IconAligment horizontalAlign=IconAligment.resize, IconAligment verticalAlign=IconAligment.resize, int animationFrames=0, bool tile=false, bool clickable=false)
			{
				//drawing animation frames
				if (animationFrames != 0)
				{
					System.DateTime now = System.DateTime.Now;
					int frameNum = (int)((now.Second*5f + now.Millisecond*5f/1000f) % animationFrames);
					string frameString = (frameNum+1<10? "0" : "") + (frameNum+1).ToString();
					return Icon(textureName + frameString, rect, animationFrames:0);
				}
				
				//drawig texture
				return Icon(GetIcon(textureName), rect, horizontalAlign, verticalAlign, tile, clickable);
			}

			public bool Icon (Texture2D texture, Rect rect, IconAligment horizontalAlign=IconAligment.resize, IconAligment verticalAlign=IconAligment.resize, bool tile=false, bool clickable=false)
			{

				//aligning texture if the rect width or height is more than icon size
				if (rect.width > texture.width) 
				{
					switch (horizontalAlign)
					{
						case IconAligment.min: rect.width = texture.width; break;
						case IconAligment.center: rect.x += rect.width/2; rect.x -= texture.width/2; rect.width = texture.width; break;
						case IconAligment.max: rect.x += rect.width; rect.x -= texture.width; rect.width = texture.width; break;
					}
				}
				if (rect.height > texture.height)
				{
					switch (verticalAlign)
					{
						case IconAligment.min: rect.height = texture.height; break;
						case IconAligment.center: rect.y += rect.height/2; rect.y -= texture.height/2; rect.height = texture.height; break;
						case IconAligment.max: rect.y += rect.height; rect.y -= texture.height; rect.height = texture.height; break;
					}
				}

				//click area
				bool result = false;
				//#if UNITY_EDITOR
				//if (clickable) result = UnityEditor.EditorGUI.Toggle(ToDisplay(rect), false, GUIStyle.none);
				//#endif

				//drawing texture
				if (!tile) GUI.DrawTexture(ToDisplay(rect), texture, ScaleMode.ScaleAndCrop); 
				else
				{
					//Debug.Log(zoom);
					Rect localRect = ToDisplay(rect);
					for (float x=0; x<rect.width; x+=texture.width*zoom)
						for (float y=0; y<rect.height; y+=texture.height*zoom)
							GUI.DrawTexture(new Rect(x+localRect.x, y+localRect.y, texture.width*zoom, texture.height*zoom), texture);
				}

				return result;
			}

		#endregion

		#region Element

			[System.NonSerialized] Dictionary<string, GUIStyle> elementStyles = new Dictionary<string, GUIStyle>();

			public void Element (string textureName, Rect rect, RectOffset borders, RectOffset offset)
			{
				if (Event.current.type != EventType.Repaint) return;

				GUIStyle elementStyle = elementStyles.CheckGet(textureName);
	
				if (elementStyle == null || elementStyle.normal.background == null || elementStyle.hover.background == null)
				{
					elementStyle = new GUIStyle();
					elementStyle.normal.background = GetIcon(textureName); //Resources.Load("MapMagic_Window") as Texture2D;
					elementStyle.hover.background = GetIcon(textureName+"_pro");

					elementStyles.CheckAdd(textureName, elementStyle);
				}

				elementStyle.border = borders;

				Rect paddedRect = ToDisplay(rect);
				paddedRect = new Rect(paddedRect.x-offset.left, paddedRect.y-offset.top, paddedRect.width+offset.left+offset.right, paddedRect.height+offset.top+offset.bottom);
				
				#if UNITY_EDITOR
				elementStyle.Draw(paddedRect, UnityEditor.EditorGUIUtility.isProSkin, false, false, false);
				#endif
			}

		#endregion

		#region Drag Change

		Vector2 sliderClickPos;
			int sliderDraggingId = -20000000;
			float sliderOriginalValue;

			public float DragChangeField (float val, Rect sliderRect, float min = 0, float max = 0, float minStep=0.2f)
			{
				sliderRect = ToDisplay(sliderRect);
				int controlId = GUIUtility.GetControlID(FocusType.Passive);
				#if UNITY_EDITOR
				UnityEditor.EditorGUIUtility.AddCursorRect (sliderRect, UnityEditor.MouseCursor.SlideArrow);
				#endif
				if (Event.current.type == EventType.MouseDown && sliderRect.Contains(Event.current.mousePosition) ) 
				{ 
					sliderClickPos = Event.current.mousePosition; 
					sliderOriginalValue = val;
					sliderDraggingId = controlId; 
				}

				if (sliderDraggingId == controlId) // && Event.current.type == EventType.MouseDrag)
				{
					int steps = (int)((Event.current.mousePosition.x - sliderClickPos.x) / 5);
					
					val = sliderOriginalValue;

					for (int i=0; i<Mathf.Abs(steps); i++)
					{
						float absVal = val>=0? val : -val;

						float step = 0.01f;
						if (absVal > 0.99f) step=0.02f; if (absVal > 1.99f) step=0.1f;   if (absVal > 4.999f) step = 0.2f; if (absVal > 9.999f) step=0.5f;
						if (absVal > 39.999f) step=1f;  if (absVal > 99.999f) step = 2f; if (absVal > 199.999f) step = 5f; if (absVal > 499.999f) step = 10f; 
						if (step < minStep) step = minStep;

						val = steps>0? val+step : val-step;
						val = Mathf.Round(val*10000)/10000f;

						if (Mathf.Abs(min)>0.001f && val<min) val=min;
						if (Mathf.Abs(max)>0.001f && val>max) val=max;
					}

					#if UNITY_EDITOR
					if (UnityEditor.EditorWindow.focusedWindow!=null) UnityEditor.EditorWindow.focusedWindow.Repaint(); 
					UnityEditor.EditorGUI.FocusTextInControl("");
					#endif
				}
				if (Event.current.rawType == EventType.MouseUp) 
				{
					sliderDraggingId = -20000000;

					#if UNITY_EDITOR
					if (UnityEditor.EditorWindow.focusedWindow!=null) UnityEditor.EditorWindow.focusedWindow.Repaint(); 
					//UnityEditor.EditorGUI.FocusTextInControl("");
					#endif
				}
				if (Event.current.isMouse && sliderDraggingId == controlId) Event.current.Use();

				return val;
			}

		#endregion

		#region Fields

			#pragma warning disable 0219,0414 
			//disabling warnings "value never used" for builds skipping Unity_editor

			//Determining Change
			public delegate void ChangeAction();
			public event ChangeAction OnBeforeChange;
			//public event ChangeAction OnAfterChange;

			public bool change = false; //any of the controllers have changed. Once set to true will not reset to false
			public bool lastChange = false; //last controller have changed. Resets to false each new element.

			public void SetChange (bool change)
			{
				if (change)
				{
					this.change = true; 
					this.lastChange = true; 
					if (OnBeforeChange != null) OnBeforeChange();
					#if UNITY_EDITOR
					if (undoObject != null) 
					{
						UnityEditor.Undo.RecordObject (undoObject, undoName);
						UnityEditor.EditorUtility.SetDirty(undoObject);
					}
					#endif
				}
				else this.lastChange = false;

				//this.change = false;
				//this.lastChange = false;
			}


			//Fields
			public float fieldSize = 0.5f;
			public float sliderSize = 0.5f; //in percent to field rect
			public bool monitorChange = true;
			public bool markup = false;
			public bool useEvent = false;
			public bool disabled = false;
			public int fontSize = 11;
			public int iconOffset = 4;
			public bool dragChange = false;
			public bool slider = false;
			public enum HelpboxType { off, empty, info, warning, error };

			public void Field<T> (ref T src, string label=null, Rect rect = new Rect(), float min=-200000000, float max=200000000, Val fieldSize = new Val(), Val sliderSize = new Val(), Val monitorChange = new Val(), Val useEvent = new Val(), Val disabled = new Val(), Val dragChange = new Val(), Val slider = new Val(), Val quadratic = new Val(), Val allowSceneObject = new Val(), GUIStyle style=null, string tooltip=null)
			{ src = Field<T> (src, label, rect, min, max, fieldSize, sliderSize, monitorChange, markup, useEvent, disabled, dragChange, slider, quadratic, allowSceneObject, style, tooltip); }

			public T Field<T> (
				T src, 
				string label=null,
				Rect rect = new Rect(), 
				float min=-200000000, float max=200000000, 
				Val fieldSize = new Val(),
				Val sliderSize = new Val(),
				Val monitorChange = new Val(), 
				Val markup = new Val(), 
				Val useEvent = new Val(),
				Val disabled = new Val(),
				Val dragChange = new Val(),
				Val slider = new Val(),
				Val quadratic = new Val(),
				Val allowSceneObject = new Val(),
				GUIStyle style = null, 
				string tooltip=null)
			{
				//finding current params
				fieldSize.Verify(this.fieldSize); sliderSize.Verify(this.sliderSize);
				useEvent.Verify(this.useEvent); disabled.Verify(this.disabled); markup.Verify(this.markup);
				dragChange.Verify(this.dragChange); slider.Verify(this.slider);

				//loading styles
				CheckStyles();
				
				//if no rect specified - taking all of the next line
				if (rect.width < 0.9f && rect.height < 0.9f) { Par(); rect = Inset(); }

				//exiting on markup
				if (markup) return src;

				//disabling
				disabled.Verify(this.disabled);
				#if UNITY_EDITOR
				if (disabled) UnityEditor.EditorGUI.BeginDisabledGroup(true);
				#endif

				//finding field and label rects
				if (label==null) fieldSize = 1;
				Rect labelRect = rect.Clamp(1f-fieldSize); //new Rect(rect.x, rect.y, (1f-_fieldSize)*rect.width, rect.height);
				Rect fieldRect = rect.ClampFromLeft((float)fieldSize); //new Rect(rect.x+(1f-_fieldSize)*rect.width, rect.y, _fieldSize*rect.width, rect.height);
				
				Rect sliderRect = fieldRect.Clamp((float)sliderSize); sliderRect = sliderRect.Clamp((int)sliderRect.width-4);
				if (slider) fieldRect = fieldRect.ClampFromLeft(1f-sliderSize);

				//prefix label 
				if (label!=null && zoom>0.3f) Label(label, labelRect, tooltip:tooltip); //prefix:true does not have a changable width

				//setting focus name
			//	GUI.SetNextControlName("LayoutField");

				//drawing field
				object srcObj = (object)src;
				object dstObj = default(T);

				#if UNITY_EDITOR
				System.Type type = typeof(T);
				if (type == typeof(float)) 
				{
					float val = (float)srcObj;

					if (slider)
					{
						if (!quadratic) val = GUI.HorizontalSlider(ToDisplay(sliderRect), val, min, max); 
						else val = Mathf.Pow(GUI.HorizontalSlider(ToDisplay(sliderRect), Mathf.Pow(val,0.5f), Mathf.Pow(min,0.5f), Mathf.Pow(max,0.5f)),2); 
					}
					if (dragChange && zoom>0.45f) val = DragChangeField(val, fieldRect.ClampFromLeft((int)22), minStep:0);
					val = UnityEditor.EditorGUI.FloatField(ToDisplay(fieldRect), val, fieldStyle);	

					if (val > max) val = max; if (val < min) val = min;
					dstObj = val;

					if (dragChange && zoom>0.45f) Icon("DPLayoutIcon_Slider", fieldRect, horizontalAlign:IconAligment.max, verticalAlign:IconAligment.center);
				}		
				else if (type == typeof(int)) 
				{
					int val = (int)srcObj;

					if (slider)
					{
						if (!quadratic) val = (int)GUI.HorizontalSlider(ToDisplay(sliderRect), val, (int)min, (int)max); 
						else val = (int)Mathf.Pow(GUI.HorizontalSlider(ToDisplay(sliderRect), Mathf.Pow(val,0.5f), Mathf.Pow(min,0.5f), Mathf.Pow(max,0.5f)),2); 
					}
					if (dragChange && zoom>0.45f) val = (int)DragChangeField(val, fieldRect.ClampFromLeft((int)22), minStep:1);
					val = UnityEditor.EditorGUI.IntField(ToDisplay(fieldRect), val, fieldStyle);

					if (val > max) val = (int)max; if (val < min) val = (int)min;
					dstObj = val;

					if (dragChange && zoom>0.45f) Icon("DPLayoutIcon_Slider", fieldRect, horizontalAlign:IconAligment.max, verticalAlign:IconAligment.center);
				}
				else if (type == typeof(Vector2)) 
				{
					Rect leftRect = fieldRect.Clamp((int)(fieldRect.width/2f-1));
					Rect rightRect = fieldRect.ClampFromLeft((int)(fieldRect.width/2f-1));

					Vector2 val = (Vector2)srcObj;

					val.x = Field(val.x, rect:leftRect, min:min, max:max, monitorChange:false, disabled:disabled, dragChange:dragChange, slider:false, quadratic:quadratic, tooltip:tooltip);
					val.y = Field(val.y, rect:rightRect, min:min, max:max, monitorChange:false, disabled:disabled, dragChange:dragChange, slider:false, quadratic:quadratic, tooltip:tooltip);

					dstObj = val;
				}
				else if (type == typeof(bool)) 
				{
					Rect fRect = fieldRect;//ToDisplay(fieldRect);
					if (zoom > 0.75f) dstObj = UnityEditor.EditorGUI.Toggle(ToDisplay(new Rect(fRect.x,fRect.y,20,fRect.height)), (bool)srcObj);
					else  dstObj = UnityEditor.EditorGUI.Toggle(ToDisplay(new Rect(fRect.x,fRect.y,20,fRect.height)), (bool)srcObj, UnityEditor.EditorStyles.miniButton);
				}
				else if (type == typeof(string)) dstObj = UnityEditor.EditorGUI.TextField(ToDisplay(fieldRect), (string)srcObj, style!=null? style : fieldStyle);
				else if (type == typeof(Color)) dstObj = UnityEditor.EditorGUI.ColorField(ToDisplay(fieldRect), (Color)srcObj);
				else if (type == typeof(Texture2D))
				{
					if (zoom>0.55f) dstObj = UnityEditor.EditorGUI.ObjectField(ToDisplay(fieldRect), (Texture2D)srcObj, typeof(Texture2D), false);
					else 
					{
						dstObj = srcObj;
						//dstObj = UnityEditor.EditorGUI.ObjectField(ToDisplay(fieldRect), (Texture2D)srcObj, typeof(Object), false);
						if (srcObj == null) UnityEditor.EditorGUI.HelpBox(ToDisplay(fieldRect),"",UnityEditor.MessageType.None);
						else UnityEditor.EditorGUI.DrawPreviewTexture(ToDisplay(fieldRect), (Texture2D)srcObj);
						
					}
				}
				else if (type == typeof(Material)) dstObj = UnityEditor.EditorGUI.ObjectField(ToDisplay(fieldRect), (Material)srcObj, typeof(Material), false);
				else if (type == typeof(Transform)) dstObj = UnityEditor.EditorGUI.ObjectField(ToDisplay(fieldRect), (Transform)srcObj, typeof(Transform), false);
				else if (type == typeof(GameObject)) dstObj = UnityEditor.EditorGUI.ObjectField(ToDisplay(fieldRect), (GameObject)srcObj, typeof(GameObject), false);
				else if (type.IsEnum) 
				{
					if (zoom > 0.99f) dstObj = UnityEditor.EditorGUI.EnumPopup(ToDisplay(fieldRect), (System.Enum)srcObj);
					else dstObj = (T)(object)UnityEditor.EditorGUI.EnumPopup(ToDisplay(fieldRect), (System.Enum)srcObj, enumZoomStyle);
				}
				else if (type.IsSubclassOf(typeof(UnityEngine.Object))) dstObj = UnityEditor.EditorGUI.ObjectField(ToDisplay(fieldRect), (UnityEngine.Object)srcObj, type, allowSceneObject);
				#endif

				T dst = (T)dstObj;

				//end disabling
				#if UNITY_EDITOR
				if (disabled) UnityEditor.EditorGUI.EndDisabledGroup();
				#endif

				//monitoring change
				monitorChange.Verify(this.monitorChange);
				if (monitorChange && !EqualityComparer<T>.Default.Equals(src, dst)) SetChange(true);
				else SetChange(false);

				//deselecting when clicking somewhere else
			//	if (Event.current.type == EventType.MouseDown)// && !ToDisplay(fieldRect).Contains(Event.current.mousePosition)) 
			//	{
			//		Debug.Log(GUI.GetNameOfFocusedControl() + " " + Event.current.type);
			//	}
				
				
				//UnityEditor.EditorGUI.FocusTextInControl(""); //stop writing on selection change

				//using event
				//if (useEvent && Event.current.type != EventType.Repaint && ToDisplay(fieldRect).Contains(Event.current.mousePosition)) Event.current.Use();

				//returning
				return dst;
			}

			public void Curve (Curve curve, Rect rect, Color color=new Color(), string tooltip=null)
			{
				//if no rect specified - taking all of the next line
				if (rect.width < 0.9f && rect.height < 0.9f) { Par(); rect = Inset(); }
				if (color.a < 0.001f) color = Color.black;

				#if UNITY_EDITOR
				UnityEditor.EditorGUI.HelpBox(ToDisplay(rect), null, UnityEditor.MessageType.None);
				for (int p=0; p<curve.points.Length-1; p++)
				{
					Curve.Point prev = curve.points[p];
					Curve.Point next = curve.points[p+1];
					
					Vector2 pos1 = new Vector2(prev.time, prev.val);
					Vector2 pos2 = new Vector2(next.time, next.val);

					Vector2 tan1 = new Vector2((next.time-prev.time)/4f, prev.outTangent/4f) + pos1;
					Vector2 tan2 = new Vector2((prev.time-next.time)/4f, -next.inTangent/4f) + pos2;
					
					pos1.x = pos1.x*rect.width + rect.x; pos1.y = -pos1.y*rect.height + rect.y + rect.height; 
					pos2.x = pos2.x*rect.width + rect.x; pos2.y = -pos2.y*rect.height + rect.y + rect.height; 
					tan1.x = tan1.x*rect.width + rect.x; tan1.y = -tan1.y*rect.height + rect.y + rect.height; 
					tan2.x = tan2.x*rect.width + rect.x; tan2.y = -tan2.y*rect.height + rect.y + rect.height; 
					
					UnityEditor.Handles.DrawBezier(ToDisplay(pos1), ToDisplay(pos2), ToDisplay(tan1), ToDisplay(tan2), color, null, 1.5f);
				
				}
				#endif
			}

			System.Type curveWindowType;
			AnimationCurve windowCurveRef = null;
			public void Curve (AnimationCurve src, Rect rect, float min=-200000000, float max=200000000, Color color=new Color(), string tooltip=null)
			{
				//if no rect specified - taking all of the next line
				if (rect.width < 0.9f && rect.height < 0.9f) { Par(); rect = Inset(); }
				if (color.a < 0.001f) color = Color.white;
				lastChange = false; //custom set change

				#if UNITY_EDITOR

				//recording undo on change if the curve editor window is opened (and this current curve is selected)
				try
				{
					if (curveWindowType == null) curveWindowType = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.CurveEditorWindow");
					if (UnityEditor.EditorWindow.focusedWindow != null && UnityEditor.EditorWindow.focusedWindow.GetType() == curveWindowType)
					{
						AnimationCurve windowCurve = curveWindowType.GetProperty("curve").GetValue(UnityEditor.EditorWindow.focusedWindow, null) as AnimationCurve;
						if (windowCurve == src)
						{
							if (windowCurveRef == null) windowCurveRef = windowCurve.Copy();
							if (!windowCurve.IdenticalTo(windowCurveRef))
							{
								
								Keyframe[] tempKeys = windowCurve.keys;
								windowCurve.keys = windowCurveRef.keys;
								SetChange(true);
								SetChange(true);
								
								windowCurve.keys = tempKeys;

								windowCurveRef = windowCurve.Copy();
							}
						}
					}
					else windowCurveRef = null;
				}
				catch {};

				UnityEditor.EditorGUI.CurveField(ToDisplay(rect), src, Color.white, new Rect(0,0,1,1)); 

				#endif
			}


			public void Label (
				string label = null, 
				Rect rect = new Rect(), 
				string url = null,
				bool helpbox = false, 
				int messageType = 0,
				Val fontSize = new Val(),
				Val disabled = new Val(),
				FontStyle fontStyle = FontStyle.Normal,
				TextAnchor textAnchor = TextAnchor.UpperLeft,
				bool prefix = false,
				string tooltip=null )
			{
				//if no rect specified - taking all of the next line
				if (rect.width < 0.9f && rect.height < 0.9f) { Par(); rect = Inset(); }
			
				//setting styles
				CheckStyles();

				GUIStyle style = labelStyle;
				if (url != null) style = urlStyle;
				if (helpbox) style = helpBoxStyle;

				fontSize.Verify(this.fontSize);
				if (style.fontSize != Mathf.RoundToInt(fontSize*zoom)) style.fontSize = Mathf.RoundToInt(fontSize*zoom);
				if (style.alignment != textAnchor) labelStyle.alignment = textAnchor;
				if (style.fontStyle != fontStyle) labelStyle.fontStyle = fontStyle;
				
				//gui content
				GUIContent content = new GUIContent(label, tooltip);

				//drawing label
				#if UNITY_EDITOR
				if (prefix) UnityEditor.EditorGUI.PrefixLabel(ToDisplay(rect), content, labelStyle); 
				else if (helpbox) UnityEditor.EditorGUI.HelpBox(ToDisplay(rect), label, (UnityEditor.MessageType)messageType);
				else if (url != null) 
				{
					if (GUI.Button(ToDisplay(rect), content, urlStyle)) Application.OpenURL(url); 
					UnityEditor.EditorGUIUtility.AddCursorRect (ToDisplay(rect), UnityEditor.MouseCursor.Link);
				}
				else UnityEditor.EditorGUI.LabelField(ToDisplay(rect), content, style);
				#endif
			}


			public bool Button (
				string label = null,
				Rect rect = new Rect(),  
				Val monitorChange = new Val(),
				Val disabled = new Val(),
				string icon = null,
				GUIStyle style = null,
				string tooltip=null )
			{
				CheckStyles();
				if (rect.width < 0.9f && rect.height < 0.9f) { Par(); rect = Inset(); }
				GUIContent content = new GUIContent(label, tooltip);
				
				//disabling
				disabled.Verify(this.disabled);
				#if UNITY_EDITOR
				if (disabled) UnityEditor.EditorGUI.BeginDisabledGroup(true);
				#endif

				bool result = false;
				if (style==null) result = GUI.Button(ToDisplay(rect), content, buttonStyle);
				else result = GUI.Button(ToDisplay(rect), content, style);

				//end disabling
				#if UNITY_EDITOR
				if (disabled) UnityEditor.EditorGUI.EndDisabledGroup();
				#endif

				monitorChange.Verify(this.monitorChange);
				if (monitorChange)
				{
					if (result) SetChange(true);
					else SetChange(false);
				}
					
				if (icon!=null) Icon(icon, new Rect(rect.x+4, rect.y, rect.width-8, rect.height), horizontalAlign:IconAligment.min, verticalAlign:IconAligment.center);

				return result;
			}

			//TODO: group with a button
			public void CheckButton (ref bool src, string label = null, Rect rect = new Rect(), Val monitorChange = new Val(), Val disabled = new Val(), string icon = null, string tooltip=null )
			{ src = CheckButton(src, label, rect, monitorChange, disabled, icon, tooltip);}

			public bool CheckButton (
				bool src,
				string label = null,
				Rect rect = new Rect(),  
				Val monitorChange = new Val(),
				Val disabled = new Val(),
				string icon = null,
				string tooltip=null )
			{
				CheckStyles();
				if (rect.width < 0.9f && rect.height < 0.9f) { Par(); rect = Inset(); }
				GUIContent content = new GUIContent(label, tooltip);
				
				bool dst = GUI.Toggle(ToDisplay(rect), src, content, buttonStyle);

				monitorChange.Verify(this.monitorChange);
				if (monitorChange)
				{
					if (dst != src) SetChange(true);
					else SetChange(false);
				}
					
				if (icon!=null) Icon(icon, new Rect(rect.x+4, rect.y, rect.width-8, rect.height), horizontalAlign:IconAligment.min, verticalAlign:IconAligment.center);

				return dst;
			}

			public void Toggle (ref bool src, string label = null, Rect rect = new Rect(), Val monitorChange = new Val(), Val disabled = new Val(), string onIcon=null, string offIcon=null, string tooltip=null )
			{ src = Toggle(src, label, rect, monitorChange, disabled, onIcon, offIcon, tooltip);}

			public bool Toggle (
				bool src,
				string label = null,
				Rect rect = new Rect(),  
				Val monitorChange = new Val(),
				Val disabled = new Val(),
				string onIcon=null, string offIcon=null, 
				string tooltip=null )
			{
				CheckStyles();
				if (rect.width < 0.9f && rect.height < 0.9f) { Par(); rect = Inset(); }

				Rect fieldRect = new Rect(rect.x, rect.y, 20, rect.height);
				Rect labelRect = new Rect(rect.x+20, rect.y, rect.width-20, rect.height);

				if (label != null) Label(label, labelRect);

				bool dst = src;
				#if UNITY_EDITOR
				if (onIcon!=null && offIcon!=null)
				{
					dst = UnityEditor.EditorGUI.Toggle(ToDisplay(fieldRect), src, GUIStyle.none);
					if (src) Icon(onIcon, fieldRect, Layout.IconAligment.center, Layout.IconAligment.center);
					else Icon(offIcon, fieldRect, Layout.IconAligment.center, Layout.IconAligment.center);
				}
				else if (zoom > 0.75f) dst = UnityEditor.EditorGUI.Toggle(ToDisplay(fieldRect), src);
				else  dst = UnityEditor.EditorGUI.Toggle(ToDisplay(new Rect(fieldRect.x,fieldRect.y,20,fieldRect.height)), src, UnityEditor.EditorStyles.miniButton);
				#endif

				monitorChange.Verify(this.monitorChange);
				if (monitorChange)
				{
					if (dst != src) SetChange(true);
					else SetChange(false);
				}
					
				return dst;
			}

			public void Foldout (ref bool src, string label = null, Rect rect = new Rect(), Val disabled = new Val(), string tooltip=null )
			{ src = Foldout(src, label, rect, disabled, tooltip);}

			public bool Foldout (
				bool src,
				string label = null,
				Rect rect = new Rect(),  
				Val disabled = new Val(),
				string tooltip=null )
			{
				CheckStyles();
				if (rect.width < 0.9f && rect.height < 0.9f) { Par(); rect = Inset(); }
				GUIContent content = new GUIContent(label, tooltip);
				
				#if UNITY_EDITOR
				return UnityEditor.EditorGUI.Foldout(ToDisplay(rect), src, content, true, foldoutStyle);
				#else
				return false;
				#endif
			}

			public void Spline (Vector2 pos1, Vector2 pos2, Color color=new Color(), bool invert=false)
			{
				#if UNITY_EDITOR
				pos1 = ToDisplay(pos1); pos2 = ToDisplay(pos2);
				if (color.a < 0.001f) color = Color.black;
				
				float distance = (pos2-pos1).magnitude;

				if (invert) UnityEditor.Handles.DrawBezier(pos2, pos1, new Vector2(pos2.x + distance/3, pos2.y), new Vector2(pos1.x-distance/3, pos1.y), color, null, 3f*zoom+2f);
				else UnityEditor.Handles.DrawBezier(pos1, pos2, new Vector2(pos1.x + distance/3, pos1.y), new Vector2(pos2.x-distance/3, pos2.y), color, null, 3f*zoom+2f);
				#endif
			}

			#pragma warning restore 0219,0414 
			
		#endregion

		#region DragAndDrop

			//enum MouseState { None, Down, Drag, Up }
			//MouseState mouseState;
			//Vector2 mousePos;

			public enum DragState { Pressed, Drag, Released }
			public DragState dragState;
			public Rect dragRect;
			public Vector2 dragPos;
			public Vector2 dragDelta; //drag pos relative to previous position
		
			public Vector2 dragOffset; //click position relative to field rect
			public int dragId = -2000000000;

			public bool DragDrop (Rect initialRect, int id, System.Action<Vector2,Rect> onDrag=null, System.Action<Vector2,Rect> onPress=null, System.Action<Vector2,Rect> onRelease=null)
			//actions args are mouse pos and dragged rect
			//both actions and if (DragDrop) switch (dragState) could be used
			{
				Vector2 mousePos = ToInternal(Event.current.mousePosition);

				
				//dragging
				if (id==dragId) dragState = DragState.Drag;

				//pressing
				if (Event.current.type==EventType.MouseDown && Event.current.button==0 && initialRect.Contains(mousePos))
				{
					dragOffset = new Vector2(initialRect.x, initialRect.y) - mousePos;
					dragId=id; //dragging = true;
					dragState = DragState.Pressed;
				}

				//releasing
				if (Event.current.rawType == EventType.MouseUp && id==dragId)
				{
					dragState = DragState.Released;
					//setting drag id in the end, we'll need it
				}


				//returning if not dragging
				if (id!=dragId) return false; //does not work on release

				//settin pos and rect
				dragDelta = mousePos-dragPos;
				dragPos = mousePos;
				dragRect = new Rect(mousePos.x+dragOffset.x, mousePos.y+dragOffset.y, initialRect.width, initialRect.height);
			
				//performing actions
				switch (dragState)
				{
					case DragState.Pressed: if (onPress!=null) onPress(dragPos, dragRect); break;
					case DragState.Drag: if (onDrag!=null) onDrag(dragPos, dragRect); break;
					case DragState.Released: if (onRelease!=null) onRelease(dragPos, dragRect); break;
				}

				#if UNITY_EDITOR
				if (Event.current.isMouse) Event.current.Use();
				if (UnityEditor.EditorWindow.focusedWindow != null) UnityEditor.EditorWindow.focusedWindow.Repaint(); 
				#endif

				if (dragState == DragState.Released) dragId=-2000000000; //dragging = false;
				return true;
			}

			public enum DragSide { right, left, top, bottom, rightTop, leftTop, rightBottom, leftBottom };
			public DragSide dragSide = DragSide.right;
			public Rect dragInitialRect = new Rect();

			public Rect ResizeRect (Rect rectBase, int id, int border=6, bool sideResize=true)
			{
				Rect rect = ToDisplay(rectBase);
				
				//bound rects
				Rect rightRect = new Rect(rect.x+rect.width-border/2, rect.y, border, rect.height);
				Rect leftRect = new Rect(rect.x-border/2, rect.y, border, rect.height);
				Rect topRect = new Rect(rect.x, rect.y-border/2, rect.width, border);
				Rect bottomRect = new Rect(rect.x, rect.y+rect.height-border/2, rect.width, border);

				Rect rightTopRect = new Rect(rect.x+rect.width-border, rect.y-border, border*2, border*2);
				Rect leftTopRect = new Rect(rect.x-border, rect.y-border, border*2, border*2);
				Rect rightBottomRect = new Rect(rect.x+rect.width-border, rect.y+rect.height-border, border*2, border*2);
				Rect leftBottomRect = new Rect(rect.x-border, rect.y+rect.height-border, border*2, border*2);
				
				//drawing cursor
				#if UNITY_EDITOR
				UnityEditor.EditorGUIUtility.AddCursorRect (rightTopRect, UnityEditor.MouseCursor.ResizeUpRight);
				UnityEditor.EditorGUIUtility.AddCursorRect (leftTopRect, UnityEditor.MouseCursor.ResizeUpLeft);
				UnityEditor.EditorGUIUtility.AddCursorRect (rightBottomRect, UnityEditor.MouseCursor.ResizeUpLeft);
				UnityEditor.EditorGUIUtility.AddCursorRect (leftBottomRect, UnityEditor.MouseCursor.ResizeUpRight);
				
				if (sideResize)
				{
					UnityEditor.EditorGUIUtility.AddCursorRect (rightRect, UnityEditor.MouseCursor.ResizeHorizontal);
					UnityEditor.EditorGUIUtility.AddCursorRect (leftRect, UnityEditor.MouseCursor.ResizeHorizontal);
					UnityEditor.EditorGUIUtility.AddCursorRect (topRect, UnityEditor.MouseCursor.ResizeVertical);
					UnityEditor.EditorGUIUtility.AddCursorRect (bottomRect, UnityEditor.MouseCursor.ResizeVertical);
				}
				#endif

				//pressing
				Vector2 mp = Event.current.mousePosition;
				bool anyRectsContains = rightTopRect.Contains(mp) || leftTopRect.Contains(mp) || rightBottomRect.Contains(mp) || leftBottomRect.Contains(mp);
				if (sideResize) anyRectsContains = anyRectsContains || rightRect.Contains(mp) || leftRect.Contains(mp) || topRect.Contains(mp) || bottomRect.Contains(mp);

				if (Event.current.type==EventType.MouseDown && anyRectsContains) 
				{ 
					dragId=id; 
					dragPos=Event.current.mousePosition; 
					dragInitialRect = rect;
					
					if (sideResize)
					{
						if (rightRect.Contains(mp)) dragSide=DragSide.right; 
						else if (leftRect.Contains(mp)) dragSide=DragSide.left; 
						else if (topRect.Contains(mp)) dragSide=DragSide.top; 
						else if (bottomRect.Contains(mp)) dragSide=DragSide.bottom; 
					}

					if (rightTopRect.Contains(mp)) dragSide=DragSide.rightTop; 
					if (leftTopRect.Contains(mp)) dragSide=DragSide.leftTop; 
					if (rightBottomRect.Contains(mp)) dragSide=DragSide.rightBottom; 
					if (leftBottomRect.Contains(mp)) dragSide=DragSide.leftBottom; 
				}

				//dragging
				if (id==dragId)
				{
					Vector2 dragDist = Event.current.mousePosition - dragPos;

					if (dragSide==DragSide.right || dragSide==DragSide.rightTop || dragSide==DragSide.rightBottom) rect.width = dragInitialRect.width + dragDist.x;
					if (dragSide==DragSide.left || dragSide==DragSide.leftTop || dragSide==DragSide.leftBottom) { rect.width = dragInitialRect.width - dragDist.x; rect.x = dragInitialRect.x + dragDist.x; }
					if (dragSide==DragSide.top || dragSide==DragSide.leftTop || dragSide==DragSide.rightTop) { rect.height = dragInitialRect.height - dragDist.y; rect.y = dragInitialRect.y + dragDist.y; }
					if (dragSide==DragSide.bottom || dragSide==DragSide.leftBottom || dragSide==DragSide.rightBottom) { rect.height = dragInitialRect.height + dragDist.y; }
				}

				//releasing
				if (Event.current.rawType==EventType.MouseUp && id==dragId) dragId = -2000000000;

				#if UNITY_EDITOR
				if (id==dragId) 
				{
					UnityEditor.EditorWindow.focusedWindow.Repaint();
					if (Event.current.isMouse) Event.current.Use();
				}
				#endif

				if (id==dragId) return ToInternal(rect);
				else return rectBase;
			}

		#endregion

		#region Layered

			public interface ILayered
			{
				int selected { get; set; }
				
				int collapsedHeight { get; set; }
				int extendedHeight {get; set; }

				ILayer[] layers { get; set; }

				//void Add (int num);
				//void Remove (int num);
				//void Switch (int n1, int n2);

				ILayer def { get; }
			}

			public interface ILayer
			{
				bool pinned { get; }
				void OnCollapsedGUI (Layout layout);
				void OnExtendedGUI (Layout layout); 
				void OnAdd (int n);
				void OnRemove (int n); 
				void OnSwitch (int o, int n);
				int guiHeight { get; set; }
			}

			public void DrawArrayAdd<T> (ref T[] layers, ref int selected, Rect rect, System.Action<int> onAdded=null, T def = default(T))
			{
				if (Button(rect:rect, tooltip:"Add new array element")) 
				{ 
					if (OnBeforeChange != null) OnBeforeChange();
					layers = ArrayTools.Add(layers, selected, element:def);
					selected++;
					if (onAdded != null) onAdded(selected);
					selected = Mathf.Clamp(selected, 0, layers.Length-1);
					change = true; lastChange = true;
				}
				Icon("DPLayout_Add", rect, IconAligment.center, IconAligment.center);
			}

			public void DrawArrayRemove<T> (ref T[] layers, ref int selected, Rect rect, int min=0, int max=-1, System.Action<int> onBeforeRemove=null, System.Action<int> onRemoved=null) //where T : ILayer
			{
				if (max<0) max = layers.Length;
				if (Button(rect:rect, tooltip:"Remove element") &&
					selected < max && selected >= min)
				{
					if (OnBeforeChange != null) OnBeforeChange();
					if (onBeforeRemove != null) onBeforeRemove(selected);
					layers = ArrayTools.RemoveAt(layers, selected);
					if (onRemoved != null) onRemoved(selected);
					selected--; 
					selected = Mathf.Max(selected,0); 
					change = true; lastChange = true;
				}
				Icon("DPLayout_Remove", lastRect, IconAligment.center, IconAligment.center);
			}

			public void DrawArrayUp<T> (ref T[] layers, ref int selected, Rect rect, int min=0, int max=-1, bool reverseOrder=true, System.Action<int,int> onSwitch=null) //where T : ILayer
			{
				if (max<0) max = layers.Length;
				if (Button(rect:rect, tooltip:"Move selected " + (reverseOrder? "up" : "down")) && 
					selected < max-1 && selected >= min) 
				{ 
					if (OnBeforeChange != null) OnBeforeChange();
					ArrayTools.Switch(layers, selected, selected+1);
					selected++; 
					if (onSwitch != null) onSwitch(selected-1, selected);
					Debug.Log((selected-1) + " " + selected);
					change = true; lastChange = true;
				}
				Icon(reverseOrder? "DPLayout_Up" : "DPLayout_Down", lastRect, IconAligment.center, IconAligment.center);
			}

			public void DrawArrayDown<T> (ref T[] layers, ref int selected, Rect rect, int min=0, int max=-1, bool reverseOrder=true, System.Action<int,int> onSwitch=null) //where T : ILayer
			{
				if (max<0) max = layers.Length;
				if (Button(rect:rect, tooltip:"Move selected " + (reverseOrder? "down" : "up")) && 
					selected < max && selected > min)
				{  
					if (OnBeforeChange != null) OnBeforeChange();
					ArrayTools.Switch(layers, selected, selected-1); 
					selected--; 
					if (onSwitch != null) onSwitch(selected+1, selected);
					change = true; lastChange = true;
				}
				Icon(reverseOrder? "DPLayout_Down" : "DPLayout_Up", lastRect, IconAligment.center, IconAligment.center);
			}
				 

			public void DrawLayered<T> (T[] layers, ref int selected, int min=0, int max=-1, bool reverseOrder=true, bool selectable=true, System.Action<T,Layout,int,bool> onLayerGUI=null) //where T : ILayer
			{
				Rect backgroundRect = new Rect(0,0,0,0);
				Rect backgroundStartCursor = new Rect(0,0,0,0);
				int newSelected = selected;
				if (max<0) max = layers.Length;

				#region Layers
				for (int i=min; i<max; i++)
				{
					int num = i;
					if (reverseOrder) num = layers.Length-1 - i + min;

					T layer = layers[num];

					Par(2);

					bool mousePressed = Event.current.type==EventType.MouseUp; //saving mouse state before drawing layer - it will be needed to select layers

					Rect startCursor = cursor;
					bool prevMarkup = markup; 
					if (num==selected) markup = true;
					if (onLayerGUI != null) onLayerGUI(layer, this, num, num==selected); 
					markup = prevMarkup;
					Rect endCursor = cursor;

					Par(3);
				
					float rectHeight = endCursor.y-startCursor.y + endCursor.height;
					Rect layerRect = new Rect(startCursor.x-1, startCursor.y+startCursor.height+1, endCursor.x-startCursor.x, rectHeight-1); //-1 is initial padding
					layerRect.y += field.y;
					//Label("", layerRect, helpbox: true);

					if (num==selected) //saving background
					{
						backgroundRect = layerRect;
						backgroundStartCursor = startCursor;
					}
					else //selecting
					{
						#if UNITY_EDITOR
						if (mousePressed && layerRect.Contains(ToInternal(Event.current.mousePosition))) 
						{
							newSelected = num;
							UnityEditor.EditorGUI.FocusTextInControl(""); //stop writing on selection change
						}
						#endif
					}

					
				}
				#endregion

				#region Background and re-drawing selected layer above
				if (backgroundRect.width>0.1f && backgroundRect.height>0.1f && selected>=0)
				{
					backgroundRect = backgroundRect.Extend(5);
					GUI.Box(ToDisplay(backgroundRect), "");

					Rect savedCursor = cursor;
					cursor = backgroundStartCursor;
					onLayerGUI(layers[selected], this, selected, true);
					cursor = savedCursor;
				}
				#endregion

				selected = newSelected;
				if (!selectable) selected = -1;
			}

			public void DrawLayered (ILayered splatOut, string label="", string tooltip="", bool reverseOrder=true, bool selectable=true, bool drawButtons=true)
			{
				ILayer[] layers = splatOut.layers;

				//array buttons
				if (drawButtons)
				{
					Par();
					if (label.Length != 0) Label(label, Inset(0.4f), tooltip:tooltip);

					if (Button(rect:Inset(0.15f), tooltip:"Add new array element")) 
					{ 
						if (OnBeforeChange != null) OnBeforeChange();
						splatOut.layers = ArrayTools.Add(splatOut.layers, splatOut.selected, element:splatOut.def);
						splatOut.selected++;
						splatOut.selected = Mathf.Clamp(splatOut.selected, 0, splatOut.layers.Length-1);
						splatOut.layers[splatOut.selected].OnAdd(splatOut.selected);
						change = true; lastChange = true;
					}
					Icon("DPLayout_Add", lastRect, IconAligment.center, IconAligment.center);


					if (Button(rect:Inset(0.15f), tooltip:"Remove element") &&
						splatOut.selected < layers.Length &&
						!layers[splatOut.selected].pinned)
					{
						if (OnBeforeChange != null) OnBeforeChange();
						splatOut.layers[splatOut.selected].OnRemove(splatOut.selected);
						splatOut.layers = ArrayTools.RemoveAt(splatOut.layers, splatOut.selected);
						splatOut.selected--; 
						splatOut.selected = Mathf.Max(splatOut.selected,0); 
						change = true; lastChange = true;
					}
					Icon("DPLayout_Remove", lastRect, IconAligment.center, IconAligment.center);

					if (Button(rect:Inset(0.15f), tooltip:"Move selected up") && 
						splatOut.selected < layers.Length-1 &&
						!layers[splatOut.selected].pinned && !layers[splatOut.selected+1].pinned) 
					{ 
						if (OnBeforeChange != null) OnBeforeChange();
						ArrayTools.Switch(splatOut.layers, splatOut.selected, splatOut.selected+1);
						splatOut.selected++; 
						change = true; lastChange = true;
					}
					Icon("DPLayout_Up", lastRect, IconAligment.center, IconAligment.center);

					if (Button(rect:Inset(0.15f), tooltip:"Move selected down") && 
						splatOut.selected != 0 &&
						!layers[splatOut.selected].pinned && !layers[splatOut.selected-1].pinned) 
					{  
						if (OnBeforeChange != null) OnBeforeChange();
						ArrayTools.Switch(splatOut.layers, splatOut.selected, splatOut.selected-1); 
						splatOut.selected--; 
						change = true; lastChange = true;
					}
					Icon("DPLayout_Down", lastRect, IconAligment.center, IconAligment.center);
					Par(2);
				}

				//drawing selection background
				if (splatOut.selected<layers.Length && splatOut.selected>=0)
				{
					Rect savedRect = cursor;

					int selectedLine = splatOut.selected;
					if (reverseOrder) selectedLine = layers.Length-1 - splatOut.selected; //num inversed from bottom
					Par( Mathf.Max(1, (selectedLine*(splatOut.collapsedHeight+8)+3)) ); //par 0 will create line 18
				
					Par(splatOut.extendedHeight, margin:3, padding:0);
					margin = 0; rightMargin = 7;
					Rect backgroundRect = Inset(margin:3, rightMargin:3, padding:0); backgroundRect.y-=3; backgroundRect.height += 8;
					GUI.Box(ToDisplay(backgroundRect), "");

					#if UNITY_EDITOR
//					if (UnityEditor.EditorGUIUtility.isProSkin) DrawIcon(ToBase(backgroundRect), "MapMagic_LayerBackground_pro");
					#endif

					cursor = savedRect;
				}

				//drawing layers
				int newSelected = splatOut.selected;
				margin += 4; rightMargin += 1;
				for (int i=0; i<layers.Length; i++)
				{
					int num = i;
					if (reverseOrder) num = layers.Length-1 - i;

					ILayer layer = layers[num];
					bool selected = num==splatOut.selected;

					Par(3);

					if (!selected) 
					{
						//selecting
						#if UNITY_EDITOR
						Rect selectRect = new Rect(margin, cursor.y, field.width-margin-rightMargin, 20);
						selectRect.position += field.position;
						selectRect = ToDisplay(selectRect);
						if (Event.current.type == EventType.MouseDown && selectRect.Contains(Event.current.mousePosition) ) newSelected = num;
						#endif

						Rect beforeGui = cursor; 
						layer.OnCollapsedGUI(this);
						Rect afterGui = cursor;

						splatOut.collapsedHeight = (int)((afterGui.y+afterGui.height)-(beforeGui.y+beforeGui.height)); 
					}

					else
					{
						Rect beforeGui = cursor; 
						layer.OnExtendedGUI(this);
						Rect afterGui = cursor;

						splatOut.extendedHeight = (int)((afterGui.y+afterGui.height)-(beforeGui.y+beforeGui.height)); 
					}

					Par(5);
				}

				//changing selection
				if (selectable && splatOut.selected != newSelected)
				{
					#if UNITY_EDITOR
					UnityEditor.EditorGUI.FocusTextInControl(""); //stop writing on selection change
					#endif
					splatOut.selected = newSelected;
				}
				if (!selectable) splatOut.selected = -1;
			}

		#endregion
	}
}
