using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMeltZone : ColliderZoneTemplate<BulletMeltZone>
{
    public float damagePerFrame = 1f;
    public SpriteRenderer meltSecondPass = null;

    public static BulletMeltZone main = null;

    private static bool pausedLastFrame = false;

    public void ExtraStart()
    {
        if (main == null) { main = this; }
        BoxCollider2D bc = GetComponent<BoxCollider2D>();
        meltSecondPass.size = bc.size = GetComponent<SpriteRenderer>().size;
        pausedLastFrame = false;
    }

    public override void ResetStuff()
    {
    }

    public override void ColliderAdd()
    {
    }

    public override void ColliderRemove(int index)
    {
    }

    public override void ObjectIn(int index, GameObject obj, GameObject other)
    {
        if (Time.timeScale == 0) { pausedLastFrame = true; return; }
        if (pausedLastFrame) { pausedLastFrame = false; return; }

        BoxCollider2D boxMelt = other.GetComponent<BoxCollider2D>();
        Transform plrTransform = obj.transform;

        if (((Vector2)boxMelt.bounds.ClosestPoint(plrTransform.position) - (Vector2)plrTransform.position).magnitude < 0.1f)
        {

            KHealth health = obj.GetComponent<KHealth>();
            BasicMove bm = obj.GetComponent<BasicMove>();
            //SpecialGunTemplate cct = obj.GetComponent<SpecialGunTemplate>();

            //if (cct) { cct.gunHealth = 0f; }
            if (health != null && bm != null && bm.CanCollide && BulletControllerHelper.grazedThisFrame == 0)
            {
                health.ChangeHealth(-damagePerFrame, "bullet melt");
            }
        }
    }
}
