using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PopUpSpikeScaffold : MonoBehaviour
{
    public int length = 8;
    public float spaceBetweenSpikes = 24f;
    public GameObject prefabSpike;
    public Vector2 inTime;
    public Vector2 warnTime;
    public Vector2 outTime;
    public float startOffset;
    public float consecutiveOffset;
    public float startScale;
    public float consecutiveScale;
    public float frameSpeed = 0.1f;

    void PoorlyNamedFunction()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        if (prefabSpike)
        {
            for (int i = 0; i < length; ++i)
            {
                GameObject newSpike = Instantiate(prefabSpike, transform.position + transform.right * (spaceBetweenSpikes * i), transform.rotation, transform);
                PopUpSpike newPopScript = newSpike.GetComponent<PopUpSpike>();

                newPopScript.inTime = inTime;
                newPopScript.warnTime = warnTime;
                newPopScript.outTime = outTime;

                newPopScript.offset = startOffset + (consecutiveOffset * i);
                newPopScript.scale = startScale + (consecutiveScale * i);
                newPopScript.frameSpeed = frameSpeed;
            }
        }
    }

    void Awake()
    {
        if (Application.isPlaying) { Destroy(this); }
    }

    void Start()
    {
        PoorlyNamedFunction();
    }

    void Update()
    {
        PoorlyNamedFunction();
    }
}
