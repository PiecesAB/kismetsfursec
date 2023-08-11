using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    [Header("There are 16 platforms")]
    public float[] samples;
    public GameObject[] platforms;
    public LineRenderer[] lines;
    public AudioSource soundToRender;
    public float mvtScale;
    public float maxMvt;
    public bool autoRenderBgMusic;

    private Vector2[] velocities;

    private int[] scala = new int[17] { 0, 1, 2, 3, 4, 5, 6, 8, 10, 12, 16, 24, 32, 64, 128, 256, 512 };

    void Start()
    {
        samples = new float[512];
        velocities = new Vector2[16];
        if (autoRenderBgMusic)
        {
            soundToRender = FindObjectOfType<BGMController>().GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (soundToRender)
        {
            bool rend = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].isVisible)
                {
                    rend = true;
                    //print("jk");
                    break;
                }
            }

            if (rend)
            {
                soundToRender.GetSpectrumData(samples, 0, FFTWindow.Blackman);
                for (int i = 0; i < 16; i++)
                {
                    float avg = 0f;
                    int count = 0;
                    for (int j = scala[i]; j < scala[i + 1]; j++)
                    {
                        avg += samples[j];
                        count++;
                    }
                    avg /= Mathf.Pow(count, 0.2f);
                    avg *= avg;
                    Transform t = platforms[i].transform;
                    Vector3 vo = t.localPosition;
                    float newY = Mathf.Min(maxMvt, mvtScale * avg);
                    t.localPosition = Vector2.SmoothDamp(vo, new Vector2(vo.x, newY), ref velocities[i], 0.1f, 99f);
                    float newYs = t.localPosition.y;
                    platforms[i].GetComponent<Rigidbody2D>().MovePosition(t.position);
                    Color newCol = Color.HSVToRGB(i / 25f, newYs / maxMvt, 1f);
                    SpriteRenderer sr = platforms[i].GetComponent<SpriteRenderer>();
                    sr.material.color = new Color(newCol.r, newCol.g, newCol.b, sr.color.a);

                    lines[i].SetPosition(1, new Vector3(0f, newYs, 0.01f));
                    lines[i].material.color = sr.material.color;
                    BoxCollider2D lcol = lines[i].GetComponent<BoxCollider2D>();
                    lcol.offset = new Vector2(0f, newYs * 0.5f);
                    lcol.size = new Vector2(lcol.size.x, newYs);
                }
            }
            else
            {
                
            }
        }
    }
}
