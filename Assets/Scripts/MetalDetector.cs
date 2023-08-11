using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalDetector : MonoBehaviour
{
    public enum Part
    {
        Remove, Return
    }

    public Part part;
    // This is the gun that drifts somewhere else once the animal goes through the metal detector
    public GameObject fakeGunItem;
    public GameObject effect;

    private void Start()
    {
        fakeGunItem.SetActive(false);
    }

    private IEnumerator DriftGun(string plrName)
    {
        Collider2D fakeGunCollider = fakeGunItem.GetComponent<Collider2D>();
        Transform fakeGunTransform = fakeGunItem.transform;
        Vector3 thisPosition = new Vector3(transform.position.x, transform.position.y, fakeGunTransform.position.z);
        Vector3 realPosition = fakeGunTransform.position;
        fakeGunTransform.position = thisPosition;
        fakeGunTransform.rotation = Quaternion.identity;
        fakeGunCollider.enabled = false;
        fakeGunItem.SetActive(true);
        foreach (Transform t in fakeGunItem.transform)
        {
            if (t == fakeGunItem.transform) { continue; }
            t.gameObject.SetActive(t.gameObject.name.ToLower() == plrName.ToLower());
        }

        double startTime = DoubleTime.ScaledTimeSinceLoad;
        WaitForEndOfFrame frameWait = new WaitForEndOfFrame();
        while (DoubleTime.ScaledTimeSinceLoad - startTime < 0.5f)
        {
            float progress = Mathf.Clamp01((float)(DoubleTime.ScaledTimeSinceLoad - startTime) / 0.5f);
            fakeGunTransform.position = Vector3.Lerp(thisPosition, realPosition, EasingOfAccess.SineSmooth(progress));
            yield return frameWait;
        }
        fakeGunTransform.position = realPosition;
        fakeGunCollider.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        SpecialGunTemplate gun = col.GetComponent<SpecialGunTemplate>();
        if (gun == null) { return; }
        switch (part)
        {
            case Part.Remove:
                if (gun.enabled)
                {
                    gun.gunHealth = 0f;
                    gun.enabled = false;
                    StartCoroutine(DriftGun(col.GetComponent<PrimPlayableCharacter>()?.myName ?? "Khal"));
                    Instantiate(effect, col.transform.position, Quaternion.identity);
                }
                break;
            case Part.Return:
                if (!gun.enabled)
                {
                    gun.enabled = true;
                    fakeGunItem.SetActive(false);
                    Instantiate(effect, col.transform.position, Quaternion.identity, col.transform);
                }
                break;
        }
    }
}
