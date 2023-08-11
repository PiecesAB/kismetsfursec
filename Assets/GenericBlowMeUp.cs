using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericBlowMeUp : MonoBehaviour
{
    [Header("Leave this blank for a default explosion")]
    public GameObject explosion;
    public static GameObject defaultExplosion;
    public GameObject spawnCloneOfThisOnDestroy;

    private bool delayExpl;

    public virtual void Awake()
    {
        if (defaultExplosion == null)
        {
            defaultExplosion = Resources.Load<GameObject>("SmallExplo");
        }

        if (explosion == null)
        {
            explosion = defaultExplosion;
        }

        SubclassAwake();
    }

    protected virtual void SubclassAwake()
    {
    }

    private void OnDestroy()
    {
        if (delayExpl)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
        }
        SubclassOnDestroy();
    }

    protected virtual void SubclassOnDestroy()
    {
    }

    private void SpawnCrap()
    {
        if (spawnCloneOfThisOnDestroy == null) { return; }
        GameObject crap = Instantiate(spawnCloneOfThisOnDestroy, transform.position, transform.rotation);
        crap.SetActive(true);
    }

    public void ExplodeEffectOnly()
    {
        Instantiate(explosion, transform.position, Quaternion.identity);
    }

    public virtual void BlowMeUp()
    {
        if (!enabled) { return; }
        delayExpl = false;
        if (!delayExpl)
        {
            GameObject g = Instantiate(explosion, transform.position, Quaternion.identity);
            SpawnCrap();
        }
        Destroy(gameObject);
    }

    public void BlowMeUp(float delay)
    {
        if (!enabled) { return; }
        delayExpl = false;
        if (!delayExpl)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
            SpawnCrap();
        }
        Destroy(gameObject, delay);
    }

    public void BlowMeUp(float delay, bool delayExplosion)
    {
        if (!enabled) { return; }
        delayExpl = delayExplosion;
        if (!delayExpl)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
            SpawnCrap();
        }
        Destroy(gameObject, delay);
    }
}
