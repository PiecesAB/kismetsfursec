using UnityEngine;
using System.Collections;

public class AnimateSkybox : MonoBehaviour {

    public Material[] stuff;
    public float wait;

    // Use this for initialization
    void Start () {
        StartCoroutine(animate());
	}
	
    IEnumerator animate()
    {
        while (true)
        {
            foreach (Material t in stuff)
            {
                RenderSettings.skybox = t;
                yield return new WaitForSeconds(wait);
            }

            yield return 1;
            
        }

    }
	// Update is called once per frame
	void Update () {
        
	}
}
