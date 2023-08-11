using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class AutoPixelate : MonoBehaviour {

    //public Material autoPixelShader;

	void OnRenderImage (RenderTexture s, RenderTexture d) {
        RenderTexture temp = RenderTexture.GetTemporary(Mathf.RoundToInt(240 * Camera.main.aspect), 240, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Default, 1);
        temp.filterMode = FilterMode.Point;
        s.filterMode = FilterMode.Point;
        Graphics.Blit(s, temp);
        Graphics.Blit(temp, d);

        RenderTexture.ReleaseTemporary(temp);
    }

}
