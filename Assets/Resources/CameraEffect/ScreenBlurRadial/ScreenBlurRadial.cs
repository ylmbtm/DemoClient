///////////////////////////////////////////
//  CameraFilterPack v2.0 - by VETASOFT 2015 ///
///////////////////////////////////////////
using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class ScreenBlurRadial : MonoBehaviour {
#region Variables
public Shader SCShader;
private float TimeX = 1.0f;
private Vector4 ScreenResolution;
private Material SCMaterial;
[Range(-0.5f, 0.5f)]
public float Intensity = 0.125f;
[Range(-2f, 2f)]
public float MovX = 0.5f;
[Range(-2f, 2f)]
public float MovY = 0.5f;
[Range(0f, 10f)]
private float blurWidth  = 1f;
public static float ChangeValue;
public static float ChangeValue2;
public static float ChangeValue3;
public static float ChangeValue4;
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
ChangeValue = Intensity;
ChangeValue2 = MovX;
ChangeValue3 = MovY;
ChangeValue4 = blurWidth ;
SCShader = Shader.Find("Camera/ScreenBlurRadial");
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
material.SetFloat("_Value", Intensity);
material.SetFloat("_Value2", MovX);
material.SetFloat("_Value3", MovY);
material.SetFloat("_Value4", blurWidth );
material.SetVector("_ScreenResolution",new Vector4(sourceTexture.width,sourceTexture.height,0.0f,0.0f));
Graphics.Blit(sourceTexture, destTexture, material);
}
else
{
Graphics.Blit(sourceTexture, destTexture);
}
}


void OnValidate()
{
		ChangeValue=Intensity;
		ChangeValue2=MovX;
		ChangeValue3=MovY;
		ChangeValue4=blurWidth;
}
void Update ()
{
if (Application.isPlaying)
{
Intensity = ChangeValue;
MovX = ChangeValue2;
MovY = ChangeValue3;
blurWidth  = ChangeValue4;
}
#if UNITY_EDITOR
if (Application.isPlaying!=true)
{
SCShader = Shader.Find("Camera/ScreenBlurRadial");
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
