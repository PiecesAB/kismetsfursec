using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class cameraScrModifierObject : MonoBehaviour {

    public bool horizontalScrollAllowed;
    public bool verticalScrollAllowed;
    public bool changeWrapping = false;
    public bool wrappingHorizontal = false;
    public bool wrappingVertical = false;
    public bool perScreenScrolling;
    public bool originalScrolling;
    public Vector4 camBounds;
    public bool lockedToCamXAxis;
    public bool lockedToCamYAxis;
    public bool setPssPosition;
    public Vector2 pssPositionToSet;
    public Vector2 targetOffset = Vector2.zero;
    public float constantVibrate = 0f;
    private static FollowThePlayer f;
    private static TestScriptRotateScene tsrs;
    private Vector4 origBounds;

    public static HashSet<cameraScrModifierObject> all = new HashSet<cameraScrModifierObject>();

    private static Camera mainCam = null;

    private void Awake()
    {
        all.Add(this);
    }

    private void OnDestroy()
    {
        all.Remove(this);
    }

    private bool CanViewPoint(Vector3 plrpt)
    {
        return plrpt.x >= camBounds.x - 160f && plrpt.x < camBounds.z + 160f
            && plrpt.y >= camBounds.y - 108f && plrpt.y < camBounds.w + 108f;
        // assumes camera isn't rotated! use a simpler camera for rotation-themed levels
    }

    private Vector2 GetPositionToCenterPoint(Vector3 plrpt)
    {
        return new Vector2(Mathf.Clamp(plrpt.x, camBounds.x, camBounds.z), Mathf.Clamp(plrpt.y, camBounds.y, camBounds.w));
    }

    // success code is >= 0 if succeed
    // 1: a marker can view the point
    // 0: no marker can view the point, but we're using the closest one
    // -1: no markers exist at all
    public static Vector3 PSSGetCameraPosForPoint(Vector3 plrpt, out int successCode)
    {
        float closestDist = 1e18f;
        cameraScrModifierObject closest = null;

        successCode = 0;
        foreach (cameraScrModifierObject c in all)
        {
            if (c.CanViewPoint(plrpt))
            {
                closest = c;
                successCode = 1;
                break;
            }
            float sm = (c.transform.position - plrpt).sqrMagnitude;
            if (sm < closestDist)
            {
                closestDist = sm;
                closest = c;
            }
        }
        if (closest == null) { successCode = -1; return Vector3.zero; }
        closest.ForceUpdateNow();
        return closest.GetPositionToCenterPoint(plrpt);
    }

    // Use this for initialization
    void Start () {
        GetComponent<SpriteRenderer>().color = Color.clear;
        if (f == null)
        {
            f = FindObjectOfType<FollowThePlayer>();
        }
        if (tsrs == null)
        {
            tsrs = FindObjectOfType<TestScriptRotateScene>();
        }
        if (tsrs)
        {
            origBounds = camBounds;
        }
    }

    public void ForceUpdateNow()
    {
        InternalUpdate(true);
    }
	
    private void InternalUpdate(bool forced = false)
    {
        if (tsrs)
        {
            Vector3 v = tsrs.transform.position;
            Vector2 bl = tsrs.transform.TransformPoint(new Vector3(origBounds.x, origBounds.y));
            Vector2 br = tsrs.transform.TransformPoint(new Vector3(origBounds.z, origBounds.w));
            if (bl.x > br.x)
            {
                float tempbl = bl.x;
                bl = new Vector2(br.x, bl.y);
                br = new Vector2(tempbl, br.y);
            }
            if (bl.y > br.y)
            {
                float tempbl = bl.y;
                bl = new Vector2(bl.x, br.y);
                br = new Vector2(br.x, tempbl);
            }

            camBounds = new Vector4(bl.x, bl.y, br.x, br.y);

        }

        if (DoubleTime.ScaledTimeSinceLoad > 0)
        {
            transform.position = new Vector3((lockedToCamXAxis) ? f.transform.position.x : transform.position.x, (lockedToCamYAxis) ? f.transform.position.y : transform.position.y, transform.position.z);
            if (mainCam == null) { mainCam = Camera.main; }
            Vector2 dis = mainCam.transform.position - transform.position;
            if (GetComponent<Renderer>().isVisible || forced)
            {
                f.perScreenScrolling = perScreenScrolling;
                f.originalScrolling = originalScrolling;
                if (originalScrolling)
                {
                    if (horizontalScrollAllowed)
                    {
                        f.cameraBounds.x = camBounds.x;
                        f.cameraBounds.z = camBounds.z;
                    }
                    if (verticalScrollAllowed)
                    {
                        f.cameraBounds.y = camBounds.y;
                        f.cameraBounds.w = camBounds.w;
                    }
                }
                f.horizontalPSS = horizontalScrollAllowed;
                f.verticalPSS = verticalScrollAllowed;
                if (setPssPosition)
                {
                    f.perScreenPosition = pssPositionToSet;
                }
                f.helpWithLongFallingView = true; //helpWithLongFallingView;
                f.targetOffset = targetOffset;
                if (constantVibrate > 0f)
                {
                    f.vibSpeed = Mathf.Max(f.vibSpeed, constantVibrate);
                }
            }
        }
    }

	private void Update () {
        InternalUpdate();
	}

    private void OnBecameVisible()
    {
        if (changeWrapping)
        {
            SpecialWrapping sw = f.GetComponent<SpecialWrapping>();
            if (sw && sw.enabled)
            {
                sw.horizontal = wrappingHorizontal;
                sw.vertical = wrappingVertical;
                sw.Initialize();
            }
        }
    }
}
