using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToTeleportBomb : GridGunTemplate
{
    private static GameObject shieldPrefab = null;
    private static GameObject ripplePrefab = null;

    private static GameObject currRipple = null;

    public bool isFakeFire = false;
    public GameObject lightningPrefab;

    private Vector2 MulScale(Vector2 v)
    {
        return new Vector2(v.x * transform.localScale.x, v.y * transform.localScale.y);
    }

    private IEnumerator ManageRipple(SpriteRenderer rend)
    {
        if (currRipple) { Destroy(currRipple); }
        currRipple = rend.gameObject;
        double startTime = DoubleTime.ScaledTimeSinceLoad;
        while (DoubleTime.ScaledTimeSinceLoad - startTime < 1.5f && rend && rend.gameObject)
        {
            float elapsed = (float)(DoubleTime.ScaledTimeSinceLoad - startTime);
            float rat = 1f - (elapsed / 1.5f);
            rend.material.SetFloat("_RT", Mathf.Clamp(2f - (8f*elapsed),1f,3f) * rat * rat);
            yield return new WaitForEndOfFrame();
        }
        if (rend && rend.gameObject) { Destroy(rend.gameObject); }
        yield return null;
    }

    private void OnDisable()
    {
        if (currRipple) { Destroy(currRipple); }
    }

    protected override float Fire()
    {
        if (shieldPrefab == null)
        {
            shieldPrefab = Resources.Load<GameObject>("ShieldingCircle");
        }
        if (ripplePrefab == null)
        {
            ripplePrefab = Resources.Load<GameObject>("TeleBombRipple");
        }

        Point endpoint = path[path.Count - 1];
        int deplete = path.Count - 1;

        Vector3 newPos = transform.position + (Vector3)MulScale(endpoint.ToVector2()) * gridSize;

        if (!IsLegalTeleportPosition(newPos)) {
            if (isFakeFire)
            {
                GameObject newShieldC = Instantiate(shieldPrefab, transform.position, Quaternion.identity);
                newShieldC.GetComponent<ShieldingCircle>().radius = 64f;
                newShieldC.GetComponent<ShieldingCircle>().invisible = true;
                Destroy(newShieldC, 1f);
                GameObject newRippleC = Instantiate(ripplePrefab, transform.position, Quaternion.identity);
                newRippleC.transform.localScale = 0.5f * Vector3.one;
                StartCoroutine(ManageRipple(newRippleC.GetComponent<SpriteRenderer>()));
                return 0.5f;
            }
            else
            {
                return 0f;
            }
        }

        //go there
        transform.position = newPos;
        GetComponent<Rigidbody2D>().MovePosition(transform.position);

        //damage all enemies on screen
        Vector2 camPos = Camera.main.transform.position;
        float damageMultiplier = 1f;
        for (int i = 0; i < LevelInfoContainer.allEnemiesInLevel.Count; ++i)
        {
            PrimEnemyHealth e = LevelInfoContainer.allEnemiesInLevel[i];
            if (e == null) { continue; }
            Bounds eBound = e.GetComponent<Collider2D>().bounds;
            float eRadius = 0.5f * (eBound.extents.x + eBound.extents.y);
            Transform eTran = e.transform;
            // on screen? then do damage
            if (Mathf.Abs(camPos.x - eTran.position.x) - eRadius < 160 && Mathf.Abs(camPos.y - eTran.position.y) - eRadius < 108)
            {
                e.DoDamage(deplete * 1.5f * damageMultiplier, newPos);
                //too much crap on screen? you can't kill it all...
                damageMultiplier *= 0.9f;
                // create a lightning
                GameObject newLightning = Instantiate(lightningPrefab, transform.position, Quaternion.identity);
                LineRenderer nlLine = newLightning.GetComponent<LineRenderer>();
                nlLine.useWorldSpace = true;
                Vector3[] pos = new Vector3[Fakerand.Int(3, 6)];
                pos[0] = new Vector3(transform.position.x, transform.position.y, -100);
                pos[pos.Length - 1] = new Vector3(e.mainObject.transform.position.x, e.mainObject.transform.position.y, -100);
                for (int j = 1; j < pos.Length - 1; ++j)
                {
                    float prog = j / (float)(pos.Length - 1);
                    pos[j] = Vector3.Lerp(pos[0], pos[pos.Length - 1], prog);
                    pos[j] += (Vector3)Fakerand.UnitCircle() * 48;
                }
                nlLine.positionCount = pos.Length;
                nlLine.SetPositions(pos);
            }
        }

        float r = 64 + (32 * deplete);

        //place shield at new position

        GameObject newShield = Instantiate(shieldPrefab, transform.position, Quaternion.identity);
        newShield.GetComponent<ShieldingCircle>().radius = r;
        newShield.GetComponent<ShieldingCircle>().invisible = true;
        Destroy(newShield, 1f);

        GameObject newRipple = Instantiate(ripplePrefab, transform.position, Quaternion.identity);
        newRipple.transform.localScale = (r/128f)*Vector3.one;
        StartCoroutine(ManageRipple(newRipple.GetComponent<SpriteRenderer>()));

        path.Clear();
        return deplete;
    }
}
