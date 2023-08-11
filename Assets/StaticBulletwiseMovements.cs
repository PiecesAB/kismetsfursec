using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StaticBulletsOnVertices))]
public class StaticBulletwiseMovements : MonoBehaviour
{
    [System.Serializable]
    public class WaveInfo
    {
        public float xScale = 1;
        public float yScale = 1;
        public AnimationCurve waveX;
        public AnimationCurve waveY;
        public Vector2 k; // cos(kx - wt)
        public Vector2 w;
        public Vector2 ti; // initial time
    }

    public bool useResize;
    public WaveInfo resizeWave;

    private float t;

    public Vector2 EvaluateResize(Vector2 pos)
    {
        float pxy = resizeWave.xScale * pos.x + resizeWave.yScale * pos.y;
        return new Vector2(
            resizeWave.waveX.Evaluate(Mathf.Repeat(resizeWave.k.x * pxy - resizeWave.w.x * (t + resizeWave.ti.x), 1f)),
            resizeWave.waveY.Evaluate(Mathf.Repeat(resizeWave.k.y * pxy - resizeWave.w.y * (t + resizeWave.ti.y), 1f))
        );
    }

    private void Start()
    {
        t = (float)(DoubleTime.ScaledTimeSinceLoad % 100000.0);
    }

    private void Update()
    {
        t = (float)(DoubleTime.ScaledTimeSinceLoad % 100000.0);
    }
}
