using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeonPipeDecor : MonoBehaviour
{
    private MeshRenderer myRenderer;
    private Vector2 myPosition;
    [SerializeField]
    private Gradient emission;
    [SerializeField]
    private Gradient color;
    [SerializeField]
    private double speed;
    [SerializeField]
    private double wavelength;

    private void Start()
    {
        myPosition = transform.position;
        myRenderer = GetComponent<MeshRenderer>();
    }

    private Coroutine animate = null;

    private IEnumerator Animate()
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        while (true)
        {
            double t = DoubleTime.ScaledTimeRunning % 100000.0;
            float emissionpos = 0.5f * ((float)System.Math.Sin(t * 2 * 3.141592654 * speed + (myPosition.x - myPosition.y) / wavelength) + 1);
            float colorpos = 0.5f * ((float)System.Math.Cos(t * 2 * 3.141592654 * speed + (-myPosition.x - myPosition.y + 374) / wavelength));
            myRenderer.material.SetColor("_EmissionColor", emission.Evaluate(emissionpos));
            myRenderer.material.SetColor("_Color", color.Evaluate(colorpos));
            yield return wait;
        }
    }

    private void OnBecameInvisible()
    {
        if (animate != null)
        {
            StopCoroutine(animate);
            animate = null;
        }
    }

    private void OnBecameVisible()
    {
        if (animate == null)
        {
            print(gameObject.name);
            animate = StartCoroutine(Animate());
        }
    }

}
