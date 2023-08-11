using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class ParticleAnimatorS : MonoBehaviour {

    ParticleSystem.Particle[] cool;

    private void Start()
    {
        cool = new ParticleSystem.Particle[GetComponent<ParticleSystem>().main.maxParticles];
    }

    void Update () {
        /*Stopwatch stopwatchtest = new Stopwatch();
        GetComponent<ParticleSystem>().GetParticles(cool);
        stopwatchtest.Start();
        for (int i = 0; i < cool.Length; i++)
        {
            Vector3 cp = cool[i].position;
            //Vector3 cpv = Camera.main.WorldToViewportPoint(cp);
            //if (cpv.x > -0.1f && cpv.x < 1.1f && cpv.y > -0.1f && cpv.y < 1.1f && cpv.z > 0f)
            {
                float t = (float)(DoubleTime.UnscaledTimeRunning % 40000.0);
                float a = (Fastmath.FastAtan2(cp.z, cp.x) * 0.15915494f);
                cp /= 800f;
                a += 0.25f * Mathf.PerlinNoise(cp.x + t / 6f, cp.z);
                float x = (a + Mathf.Sin(t / 5.0f));
                cool[i].startColor = Color.HSVToRGB(x - Mathf.Floor(x), 1f, 0.2f);
                cool[i].remainingLifetime = 1f;
            }
        }
        stopwatchtest.Stop();
        print(stopwatchtest.ElapsedTicks);
        GetComponent<ParticleSystem>().SetParticles(cool, GetComponent<ParticleSystem>().main.maxParticles);*/
    }
}
