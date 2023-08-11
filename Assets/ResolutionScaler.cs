using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ResolutionScaler : MonoBehaviour {

    // Use this for initialization
    public CanvasScaler canvasScalerUI;
    public Camera renderCamera;
    public int defaultCameraOrthoSize = 120;
    private float ratio;

    public void ResolutionChanged()
    {
        float rtest = ((float)Screen.currentResolution.width) / ((float)Screen.currentResolution.height);
        /* if (Application.isEditor)
         {
             rtest = 1.3333333f;
         }*/
        if (rtest < 1.3333333f)
        {
            canvasScalerUI.matchWidthOrHeight = 0f;
            renderCamera.orthographicSize = defaultCameraOrthoSize * (1.3333333f / rtest);
        }
        else
        {
            canvasScalerUI.matchWidthOrHeight = 1f;
            renderCamera.orthographicSize = defaultCameraOrthoSize;
        }
    }

	void Start () {
        ResolutionChanged();
	}

}
