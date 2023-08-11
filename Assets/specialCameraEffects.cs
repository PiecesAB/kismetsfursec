using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class specialCameraEffects : MonoBehaviour {

    public Material[] effects;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void OnRenderImage (RenderTexture s, RenderTexture d) {

        s.filterMode = FilterMode.Point;
        Graphics.Blit(s, d);
        for (int i = 0; i < effects.Length; i++)
            {
            if (effects[i] != null)
            {
                Graphics.Blit(s, d, effects[i]);
            }
            }
    }
}
