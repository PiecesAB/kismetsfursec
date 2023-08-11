using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class BulletCanceller : MonoBehaviour
{
    private static ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    public static void CreateImage(Vector3 pos, float size, Color32 color)
    {
        if (!ps) { return; }
        ParticleSystem.EmitParams ep = new ParticleSystem.EmitParams();
        ep.position = pos;
        ep.startSize = size;
        ep.startColor = color;
        ps.Emit(ep, 1);
    }
}
