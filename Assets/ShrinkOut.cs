using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkOut : MonoBehaviour, IChoiceUIResponse
{
    public float shrinkDuration = 1f;
    public bool onlyWhenUnpaused = true;
    public bool scaledTime = true;
    public bool auto = false;
    public PrimExaminableItem[] examinablesToDestroy;
    public bool shrinkOutOnChoiceResponse = true;

    private bool running = false;
    private float t = 0;
    private Vector3 initSize;

    public GameObject ChoiceResponse(string text)
    {
        if (!shrinkOutOnChoiceResponse) { return null; }
        Bye();
        return null;
    }

    public void Bye()
    {
        //yield return new WaitUntil(() => (!onlyWhenUnpaused || Time.timeScale > 0);

        for (int i = 0; i < examinablesToDestroy.Length; ++i)
        {
            Destroy(examinablesToDestroy[i]);
        }

        running = true;
    }

    private void Start()
    {
        initSize = transform.localScale;
        if (auto) { Bye(); }
    }

    private void Update()
    {
        if (running)
        {
            if (t < shrinkDuration)
            {
                transform.localScale = (1f - t / shrinkDuration) * initSize;
                float mult = (scaledTime) ? Time.timeScale : 1f;
                t += 0.016666666f * mult;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
