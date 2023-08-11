using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebcamSpecial : MonoBehaviour
{
    WebCamTexture wct = null;

    public Renderer rend = null;

    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0) { return; }
        wct = new WebCamTexture(devices[0].name,320,240,12);
        wct.Play();
        if (rend) { rend.material.mainTexture = wct; }
    }

    void OnDestroy()
    {
        if (wct != null) { wct.Stop(); }
    }

    void Update()
    {
        
    }
}
