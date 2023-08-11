using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class ClickToRuinGravity : GridGunTemplate
{
    public GameObject laserShot;

    protected override float Fire()
    {
        Point endpoint = path[path.Count - 1];
        Physics2D.gravity += new Vector2(gravityChange * endpoint.x, gravityChange * endpoint.y);
        int deplete = path.Count - 1;
        path.Clear();
        ccc.DiscoStart();
        Vector2 dir = new Vector2(endpoint.x, endpoint.y).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        GameObject newLaser = Instantiate(laserShot, transform.position + (Vector3)(32f * dir), Quaternion.AngleAxis(angle, Vector3.forward));
        LaserBullet lb = newLaser.GetComponent<LaserBullet>();
        lb.maxWidth = 16 + 6 * deplete;
        lb.existTime = 0.4f + 0.5f * deplete;
        if (deplete > 3)
        {
            Camera.main.GetComponent<FollowThePlayer>().vibSpeed += 3f;
        }
        return deplete;
    }
}
