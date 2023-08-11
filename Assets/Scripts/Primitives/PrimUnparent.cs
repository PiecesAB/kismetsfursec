using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimUnparent : MonoBehaviour
{
    public Transform newParent = null;
    public enum UnparentMode
    {
        Normal, IntoCamera
    }
    public UnparentMode unparentMode = UnparentMode.Normal;

    public void Start()
    {
        switch (unparentMode)
        {
            case UnparentMode.Normal:
                transform.SetParent(newParent);
                break;
            case UnparentMode.IntoCamera:
                transform.SetParent(Camera.main.transform, true);
                break;
        }
    }
}
