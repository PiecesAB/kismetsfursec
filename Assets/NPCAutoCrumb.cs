using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAutoCrumb : MonoBehaviour
{
    // Be careful when spawning objects in OnDestroy:
    // When quitting the game (in edit mode), the spawned objects persist into the editor!!!

    public Texture2D crumbTex;
    public bool activateOnDestroy = false;

    private bool quitting = false;

    public void Start()
    {
        Application.quitting += NowQuitting;
    }

    public void NowQuitting()
    {
        quitting = true;
    }

    public void Activate()
    {
        KHealth.Chunkify(crumbTex, transform);
    }

    // Warning: only works if there's at least one KHealth in this scene
    void OnDestroy()
    {
        Application.quitting -= NowQuitting;
        if (activateOnDestroy && !quitting)
        {
            Activate();
        }
    }
}
