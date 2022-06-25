////////////////////////////////////////////////////////////////////////////////////
//  FILTER CAMERA PACK - by VETASOFT 2014 //////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ScreenBlurMovie : MonoBehaviour {
	#region Variables
	public Shader SCShader;
	private float TimeX = 1.0f;
	private Vector4 ScreenResolution;
	private Material SCMaterial;
	[Range(0,1000)]
	public float Radius = 150.0f;
	[Range(0,1000)]
	public float Factor = 200.0f;
	[Range(1,8)]
	public int FastFilter = 2;

	public static float ChangeRadius;
	public static float ChangeFactor;
	public static int ChangeFastFilter;

	#endregion
	
	#region Properties
	Material material
	{
		get
		{
			if(SCMaterial == null)
			{
				SCMaterial = new Material(SCShader);
				SCMaterial.hideFlags = HideFlags.HideAndDontSave;	
			}
			return SCMaterial;
		}
	}
	#endregion
	void Start () 
	{
		ChangeRadius = Radius;
		ChangeFactor = Factor;
		ChangeFastFilter = FastFilter;
		SCShader = Shader.Find("Camera/ScreenBlurMovie");

		if(!SystemInfo.supportsImageEffects)
		{
			enabled = false;
			return;
		}
	}

	void OnRenderImage (RenderTexture sourceTexture, RenderTexture destTexture)
	{
	
		if(SCShader != null)
		{
			int DownScale=FastFilter;
			TimeX+=Time.deltaTime;
			if (TimeX>100)  TimeX=0;
			material.SetFloat("_TimeX", TimeX);
			material.SetFloat("_Radius", Radius/DownScale);
			material.SetFloat("_Factor", Factor);
			material.SetVector("_ScreenResolution",new Vector2(Screen.width/DownScale,Screen.height/DownScale));
			int rtW = sourceTexture.width/DownScale;
			int rtH = sourceTexture.height/DownScale;

			if (FastFilter>1)
			{
			RenderTexture buffer = RenderTexture.GetTemporary(rtW, rtH, 0);
			Graphics.Blit(sourceTexture, buffer, material);
			Graphics.Blit(buffer, destTexture);
			RenderTexture.ReleaseTemporary(buffer);
			}
			else
			{
				Graphics.Blit(sourceTexture, destTexture, material);
			}
		}
		else
		{
			Graphics.Blit(sourceTexture, destTexture);	
		}
	
		
	}
void OnValidate()
{
		ChangeRadius=Radius;
		ChangeFactor=Factor;
		ChangeFastFilter=FastFilter;
}
	// Update is called once per frame
	void Update () 
	{
		if (Application.isPlaying)
		{
			Radius = ChangeRadius;
			Factor = ChangeFactor;
			FastFilter = ChangeFastFilter;
		}
		#if UNITY_EDITOR
		if (Application.isPlaying!=true)
		{
			SCShader = Shader.Find("Camera/ScreenBlurMovie");

		}
		#endif

	}
	
	void OnDisable ()
	{
		if(SCMaterial)
		{
			DestroyImmediate(SCMaterial);	
		}
		
	}
	
	
}