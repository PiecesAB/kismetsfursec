using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoWeaponBorder : MonoBehaviour
{
    public GenericBlowMeUp mainObject;
    public Renderer boundRenderer;
    public bool onlyWhenVisible = false;
    public float timesAllowed = 1;

    private static HashSet<NoWeaponBorder> all = new HashSet<NoWeaponBorder>();

    private void Start()
    {
        all.Add(this);
        if (!boundRenderer) { throw new System.Exception("No bound!"); }
        if (timesAllowed < 1) { throw new System.Exception("timesAllowed < 1"); }
        float t = Mathf.Min(timesAllowed, 5);
        GetComponent<SpriteRenderer>().size = boundRenderer.bounds.size + new Vector3(8f*t,8f*t,0f);
    }

    private void OnDestroy()
    {
        all.Remove(this);
    }

    public static void WeaponWasFired(float amount)
    {
        foreach (NoWeaponBorder b in all)
        {
            if (b.onlyWhenVisible && !b.boundRenderer.isVisible) { continue; }
            b.timesAllowed -= amount;
            if (b.timesAllowed <= 0.0001f)
            {
                b.mainObject.BlowMeUp(0.05f); // The very small time frame is so myst can teleport onto it, which is funny
                b.transform.SetParent(b.mainObject.transform.parent);
                Destroy(b);
            }
            else
            {
                float t = Mathf.Min(b.timesAllowed, 5);
                b.GetComponent<SpriteRenderer>().size = b.boundRenderer.bounds.size + new Vector3(8f * t, 8f * t, 0f);
            }
        }
    }
}
