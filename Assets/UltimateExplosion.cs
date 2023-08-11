using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltimateExplosion : MonoBehaviour
{
    public GameObject whatToDestroy;
    public float timeLeft = 2.3f;
    public float dustTime = 0.15f;
    public float dustRadius = 64;
    public int ultraRippleCount = 8;
    public GameObject dustPrefab;
    public GameObject ultraRipple;
    public GameObject plrExplosionPrefab;
    public bool pushPlayer = true;

    private float dustTimeLeft = 0f;
    private bool boom = false;

    void Update()
    {
        float bit = 0.01666666666f * Time.timeScale;
        dustTimeLeft -= bit;
        if (dustTimeLeft <= 0f && timeLeft > 0f)
        {
            Vector3 dustPos = transform.position + (Vector3)(dustRadius * Fakerand.UnitCircle());
            GameObject newDust = Instantiate(dustPrefab, dustPos, Quaternion.identity, transform.parent);
            dustTimeLeft += dustTime;
        }
        timeLeft -= bit;
        if (timeLeft <= 0f && !boom)
        {
            boom = true;
            Destroy(whatToDestroy, 0.15f);
            GetComponent<AudioSource>().Play();
            for (int i = 0; i < ultraRippleCount; ++i)
            {
                float angle = Mathf.PI * 2 * (0.5f + i) / ultraRippleCount;
                Vector3 rpos = transform.position + new Vector3(dustRadius * Mathf.Cos(angle), dustRadius * Mathf.Sin(angle));
                GameObject newRipple = Instantiate(ultraRipple, rpos, Quaternion.identity, transform.parent);
                GameObject newPlrExpl = Instantiate(plrExplosionPrefab, rpos, Quaternion.identity, transform.parent);
                newRipple.SetActive(true);
                Destroy(newRipple, 2.1f);
            }
            BasicMove bm = LevelInfoContainer.GetActiveControl()?.GetComponent<BasicMove>();
            if (bm && pushPlayer)
            {
                // this doesn't account for rotation, but i don't care.
                bm.fakePhysicsVel = 600 * ((Vector2)(bm.transform.position - transform.position)).normalized;
                bm.grounded = 0;
                if (bm.fakePhysicsVel.y < 0)
                {
                    bm.fakePhysicsVel += Vector2.up * (300 - bm.fakePhysicsVel.y);
                }
            }
            if (FollowThePlayer.main)
            {
                FollowThePlayer.main.vibSpeed += 6f;
            }
        }
    }
}
