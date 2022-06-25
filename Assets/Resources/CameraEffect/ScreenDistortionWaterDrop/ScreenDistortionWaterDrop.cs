////////////////////////////////////////////////////////////////////////////////////
//  CameraFilterPack v2.0 - by VETASOFT 2015 //////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ScreenDistortionWaterDrop : MonoBehaviour {
	#region Variables
	public Shader SCShader;
	private float TimeX = 1.0f;
	private Vector4 ScreenResolution;
	private Material SCMaterial;

	[Range(-1, 1)]
	public float CenterX = 0f;
	[Range(-1, 1)]
	public float CenterY = 0f;
	[Range(0, 10)]
	public float WaveIntensity = 1;
	[Range(0, 20)]
	public int NumberOfWaves = 5;
	
	public static float ChangeCenterX;
	public static float ChangeCenterY;
	public static float ChangeWaveIntensity;
	public static int ChangeNumberOfWaves;

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
		ChangeCenterX = CenterX;
		ChangeCenterY = CenterY;
		ChangeWaveIntensity = WaveIntensity;
		ChangeNumberOfWaves = NumberOfWaves;

		SCShader = Shader.Find("Camera/ScreenDistortionWaterDrop");

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
			TimeX+=Time.deltaTime;
			if (TimeX>100)  TimeX=0;
			material.SetFloat("_TimeX", TimeX);
			material.SetVector("_ScreenResolution",new Vector2(Screen.width,Screen.height));
			material.SetFloat("_CenterX", CenterX);
			material.SetFloat("_CenterY", CenterY);
			material.SetFloat("_WaveIntensity",WaveIntensity);
			material.SetInt("_NumberOfWaves",NumberOfWaves);
			Graphics.Blit(sourceTexture, destTexture, material);
		}
		else
		{
			Graphics.Blit(sourceTexture, destTexture);	
		}
		
		
	}
void OnValidate()
{
		ChangeCenterX=CenterX;
		ChangeCenterY=CenterY;
		ChangeWaveIntensity=WaveIntensity;
		ChangeNumberOfWaves=NumberOfWaves;
		
}
	// Update is called once per frame
	void Update () 
	{
		if (Application.isPlaying)
		{
			CenterX = ChangeCenterX ;
			CenterY = ChangeCenterY;
			WaveIntensity = ChangeWaveIntensity;
			NumberOfWaves = ChangeNumberOfWaves;
		}
		#if UNITY_EDITOR
		if (Application.isPlaying!=true)
		{
			SCShader = Shader.Find("Camera/ScreenDistortionWaterDrop");

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