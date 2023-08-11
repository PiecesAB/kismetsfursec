using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartBlockDriver : MonoBehaviour
{
    private List<SkinnedMeshRenderer> blocks;

    private float t;
    private float prevT;
    private float mult;

    private float beat;

    void Start()
    {
        blocks = new List<SkinnedMeshRenderer>();

        foreach (Transform c in transform)
        {
            if (c != transform && c.GetComponent<SkinnedMeshRenderer>())
            {
                blocks.Add(c.GetComponent<SkinnedMeshRenderer>());
            }
        }

        mult = 1f;

        beat = 0f;
    }

    void Update()
    {
        float noize = Mathf.PerlinNoise(732, (float)(DoubleTime.UnscaledTimeSinceLoad % 100000.0));
        if (BrainwaveReader.internalValue > 0.95f)
        {
            mult = 2f + 2f*noize;
        }
        else if (BrainwaveReader.internalValue > 0f)
        {
            mult = 0.8f + 1f*BrainwaveReader.internalValue + 0.4f * noize;
        }
        else if (BrainwaveReader.internalValue > -0.99f)
        {
            mult = 0.5f + 0.5f * (BrainwaveReader.internalValue + 1f) - 0.1f * noize;
        }
        else
        {
            mult = 0f;
        }
        t += 0.01666666f * mult;
        t %= 1f;

        beat = Mathf.Max(0f, beat - Mathf.Max(3f, 5f * mult)); 
        if (prevT - t > 0.5f)
        {
            beat = 100f;
        }
        
        for (int i = 0; i < blocks.Count; ++i) { blocks[i].SetBlendShapeWeight(0, beat); }

        prevT = t;
    }
}
