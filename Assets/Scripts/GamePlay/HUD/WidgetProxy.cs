using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HUD
{
	public class UISpriteProxy : UISprite
	{
		public override void MarkAsChanged ()
		{
			mChanged = true;
		}
	}

	public class UILabelProxy : UILabel
	{
		public override void MarkAsChanged ()
		{
			mChanged = true;
		}
	}

	public class WidgetProxy
	{
		public UIWidget m_widget = null;
		static public UIPanel s_ProxyPanel = null;
		static GameObject s_widget = null;
		Matrix4x4 m_Matrix = Matrix4x4.identity;
		bool m_HasTraned = true;

		static public Transform root 
		{
			get 
			{
				createPanel ();
				return s_ProxyPanel.transform;
			}
		}

		static void createPanel ()
		{
			if (s_ProxyPanel == null) 
			{
				GameObject panel = new GameObject ("WidgetProxyPanel");
				s_ProxyPanel = panel.AddComponent<UIPanel> ();
				GameObject.DontDestroyOnLoad (panel);
				s_ProxyPanel.enabled = false;
			}
		}

		static public WidgetProxy Create<T> () where T : UIWidget
		{
			createPanel ();
			if (s_widget == null) 
			{
				s_widget = new GameObject ("WidgetsProxy");
				s_widget.transform.parent = s_ProxyPanel.transform;
			}

			WidgetProxy owner = new WidgetProxy ();
			owner.m_widget = s_widget.AddComponent<T> ();
			owner.m_widget.enabled = false;
			owner.m_widget.panel = s_ProxyPanel;
			return owner;
		}

		public void Release ()
		{
			if (m_widget != null) 
			{
				Object.Destroy (m_widget);
				m_widget = null;
			}
		}
			
		public T widget<T> ()  where T : UIWidget
		{
			return m_widget as T;
		}

		const int maxIndexBufferCache = 10;
		static List<int[]> mCache = new List<int[]> (maxIndexBufferCache);

		public static int[] GenerateCachedIndexBuffer (int vertexCount, int indexCount)
		{
			for (int i = 0, imax = mCache.Count; i < imax; ++i) 
			{
				int[] ids = mCache [i];
				if (ids != null && ids.Length == indexCount) 
				{
					if (i != 0) 
					{
						for (int j = 0; j < i; j++) 
						{
							mCache [j + 1] = mCache [j];
						}
						mCache [0] = ids;
					}
					return ids;
				}
			}

			int[] rv = new int[indexCount];
			int index = 0;

			for (int i = 0; i < vertexCount; i += 4) {
				rv [index++] = i;
				rv [index++] = i + 1;
				rv [index++] = i + 2;

				rv [index++] = i + 2;
				rv [index++] = i + 3;
				rv [index++] = i;
			}

			if (mCache.Count > maxIndexBufferCache)
				mCache.RemoveAt (0);
			mCache.Add (rv);
			return rv;
		}

		public static Matrix4x4 BuildMatrix (Vector3 position, Quaternion rotation, Vector3 scale)
		{
			return Matrix4x4.TRS (position, rotation, scale);
		}

		//public static Matrix4x4 BuildMatrix (Matrix4x4 worldToLocalMatrix, Vector3 position, Quaternion rotation, Vector3 scale)
		//{
		//	return worldToLocalMatrix * Matrix4x4.TRS (position, rotation, scale);
		//}

		public bool UpdateGeometry ()
		{
			bool update = m_widget.UpdateGeometry (-1);
			update |= m_HasTraned;
			if (update || m_Matrix != Matrix4x4.identity) {
				m_widget.geometry.ApplyTransform (m_Matrix);
			}
		
			m_HasTraned = false;
			return update;
		}

		public void FillGeometry (List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
		{
			m_widget.geometry.WriteToBuffers (verts, uvs, cols, null, null, null);
		}
			
		public Matrix4x4 matrix {
			get { return m_Matrix; }
			set {
				m_Matrix = value;
				m_HasTraned = true;
				m_widget.geometry.ApplyTransform (m_Matrix);
			}
		}
	}
}