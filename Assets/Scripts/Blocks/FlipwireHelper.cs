using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipwireHelper : MonoBehaviour
{
    private Rigidbody2D r2;
    private Encontrolmentation control;

    [HideInInspector]
    public Vector2 velocity;
    [HideInInspector]
    public Flipwire source;
    [HideInInspector]
    public GameObject ripple;
    [HideInInspector]
    public AudioSource flipOff;


    private void Start()
    {
        r2 = GetComponent<Rigidbody2D>();
        control = GetComponent<Encontrolmentation>();
        r2.velocity = velocity;
    }

    private int frame = 9;

    private void Update()
    {
        if (Time.timeScale == 0f) { return; }
        Vector2 v = velocity;
        float adjustMag = 0.3f * velocity.magnitude / Time.timeScale;
        if ((control.currentState & 3UL) == 1UL) { v += Vector2.left * adjustMag; }
        if ((control.currentState & 3UL) == 2UL) { v += Vector2.right * adjustMag; }
        if ((control.currentState & 12UL) == 4UL) { v += Vector2.up * adjustMag; }
        if ((control.currentState & 12UL) == 8UL) { v += Vector2.down * adjustMag; }
        r2.velocity = v;
        ++frame;
        if (frame >= 10)
        {
            frame = 0;
            GameObject newRipple = Instantiate(ripple, transform.position - Vector3.forward * 8, Quaternion.identity);
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        int otherLayer = col.gameObject.layer;
        if ((otherLayer == 8 || otherLayer == 9) && Vector2.Dot(col.GetContact(0).normal, velocity) < -0.3f)
        {
            LauncherEnemy.ImmediateRestoreControl(source, gameObject);
            flipOff.Stop(); flipOff.Play();
            Destroy(this);
        }
    }

}
