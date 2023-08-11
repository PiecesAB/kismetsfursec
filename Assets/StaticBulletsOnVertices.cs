using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticBulletsOnVertices : MonoBehaviour
{
    public BulletData bulletData;
    public BulletData[] extraBulletTypes = new BulletData[0];
    public Mesh mesh;
    public bool useChildrenInsteadOfMesh = false;
    public bool makeRotationFromCenter = false;
    public bool addSelfRotationZ = false;
    public bool deleteObjectWhenAllBulletsGone = false;
    public Vector3 scale;

    public bool disabledBackside;
    public float backsideSizeVar = 0.1f;
    public float activePlaneZ = 0f;
    public float backPlaneZ = 100f;

    public float renderRadius = 480;

    public float bulletAlternateSpin = 0f;
    public float alternateSpinCosinePeriod = 0f;
    public bool scaleBulletsWithTransform = false;
    [HideInInspector]
    public bool cancelGraphicWhenDestroyed = true;

    public bool updateEveryFrameWithoutTranformChanges = false;

    private primDeleteInTime deleteInTime = null;
    public float fadeOutRatio = Mathf.Infinity;

    private Transform t;
    private Vector3[] vertices;

    private static Transform cameraTransform = null;

    [HideInInspector]
    public List<BulletObject> myBullets = new List<BulletObject>();
    private HashSet<Vector3> usedPositions = new HashSet<Vector3>();

    private bool bulletsAreRegistered = false;

    private float oldFadeTransparency = -1f;

    public List<string> specialTags;

    private Func<Vector3, bool> BackCondition;
    private Func<Vector3, Vector2> BackScale;

    private StaticBulletwiseMovements bulletwiseMvts;

    private void OnDestroy()
    {
        for (int i = 0; i < myBullets.Count; ++i)
        {
            BulletObject b = myBullets[i];
            BulletRegister.MarkToDestroy(b, cancelGraphicWhenDestroyed);
        }
    }

    private BulletData GetBulletData()
    {
        if (extraBulletTypes.Length == 0 && specialTags.Count == 0) { return bulletData; }
        int rand = Fakerand.Int(-1, extraBulletTypes.Length);
        BulletData bd = rand == -1 ? bulletData : extraBulletTypes[rand];
        if (specialTags.Contains("rainbow over time"))
        {
            Color.RGBToHSV(bd.color, out float h, out float s, out float v);
            bd.color = Color.HSVToRGB((float)((h + DoubleTime.ScaledTimeSinceLoad)%1.0), s, v);
        }
        return bd;
    }

    void Start()
    {
        t = transform;
        deleteInTime = GetComponent<primDeleteInTime>();
        bulletwiseMvts = GetComponent<StaticBulletwiseMovements>();
        bulletsAreRegistered = false;
        if (BackCondition == null)
        {
            BackCondition = DefaultBackCondition;
        }
        if (BackScale == null)
        {
            BackScale = DefaultBackScale;
        }

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (useChildrenInsteadOfMesh)
        {
            vertices = new Vector3[t.childCount];
            int vi = 0;
            foreach (Transform child in t)
            {
                if (child == t || child.name.StartsWith("Active")) { continue; }
                vertices[vi] = child.localPosition;
                child.gameObject.SetActive(false);
                ++vi;
            }
        }
        else
        {
            vertices = (Vector3[])mesh.vertices.Clone();
        }

        for (int i = 0; i < vertices.Length; ++i)
        {
            Vector3 v = vertices[i];
            v = new Vector3(scale.x * v.x, scale.y * v.y, scale.z * v.z);
            v = transform.TransformPoint(v);
            
            if (usedPositions.Contains(v))
            {
                myBullets.Add(null);
                continue;
            }

            usedPositions.Add(v);

            bool back = (!disabledBackside)?false:BackCondition(v);

            v = new Vector3(v.x, v.y, back? backPlaneZ :activePlaneZ);

            BulletObject b = new BulletObject();
            BulletData bulletData = GetBulletData();
            b.killRadiusRatio = bulletData.killRadiusRatio;
            b.fadeInTime = bulletData.fadeInTime;
            b.damage = bulletData.damage;
            b.deletTime = Mathf.Infinity;
            b.doesntMoveOnItsOwn = true;
            b.squareHitbox = bulletData.squareHitbox;
            b.color = back?(bulletData.color - new Color(0f,0f,0f,0.5f)):bulletData.color;
            b.destroyOnLeaveScreen = false;
            b.grazeDisabled = bulletData.grazeDisabled;
            b.collisionDisabled = back;
            b.rotateWithMovementAngle = bulletData.rotateSprite;
            b.destroyOnAmbushClear = false;
            Vector2 sv = back ? BackScale(v) : Vector2.one;
            if (makeRotationFromCenter)
            {
                float rotFromCenter = makeRotationFromCenter ? (Mathf.Atan2(vertices[i].y, vertices[i].x) * Mathf.Rad2Deg) + transform.eulerAngles.z : 0;
                b.UpdateTransform(v, rotFromCenter, bulletData.scale * sv);
            }
            else
            {
                b.UpdateTransform(v, ((addSelfRotationZ) ? transform.eulerAngles.z : 0), bulletData.scale * sv);
            }

            if (InRenderDist())
            {
                BinaryBullet.Initialize(b, bulletData.bitToggleOnJump, out Texture binImg);
                BulletRegister.Register(ref b, bulletData.material, binImg ?? bulletData.sprite.texture);
                bulletsAreRegistered = true;
            }
            b.staticBulletParent = this;
            b.staticBulletIndex = myBullets.Count;
            myBullets.Add(b);
        }
    }

    private bool InRenderDist()
    {
        return ((Vector2)cameraTransform.position - (Vector2)t.position).magnitude <= renderRadius;
    }

    public void AcknowledgeBulletWasDeleted(int index)
    {
        myBullets[index] = null;
    }

    public bool DefaultBackCondition(Vector3 vertex)
    {
        return (vertex.z > t.position.z + 0.01f);
    }

    public Vector2 DefaultBackScale(Vector3 v)
    {
        float z = v.z - t.position.z;
        return (1f / (1f + backsideSizeVar * z)) * Vector2.one;
    }

    public void SetBackCondition(Func<Vector3, bool> F)
    {
        BackCondition = F;
    }

    public void SetBackScale(Func<Vector3, Vector2> F)
    {
        BackScale = F;
    }

    void Update()
    {
        if (Time.timeScale == 0 || !enabled) { return; }

        bool inRenderDist = InRenderDist();

        bool justRegistered = false;

        if (!inRenderDist) {
            if (bulletsAreRegistered) // unregister far-away bullets to make it easier on the machine.
            {
                for (int i = 0; i < myBullets.Count; ++i)
                {
                    BulletObject b = myBullets[i];
                    if (b == null) { continue; }
                    b.staticBulletParent = null; // hide me so that the bullets don't get destroyed this way
                    BulletRegister.MarkToDestroy(b, false);
                    b.staticBulletParent = this;
                }
                bulletsAreRegistered = false;
            }
            return;
        }
        else if (!bulletsAreRegistered)
        {
            for (int i = 0; i < myBullets.Count; ++i)
            {
                BulletObject b = myBullets[i];
                if (b == null) { continue; }
                BulletData bulletData = GetBulletData();
                if (bulletData.color != b.color)
                {
                    b.color = bulletData.color;
                    if (b.grazed) { b.color += new Color(0.25f, 0.25f, 0.25f, 0f); }
                    b.UpdateRenderGroup();
                }
                BinaryBullet.Initialize(b, bulletData.bitToggleOnJump, out Texture binImg);
                BulletRegister.Register(ref b, bulletData.material, binImg ?? bulletData.sprite.texture);
            }
            bulletsAreRegistered = justRegistered = true;
        }

        if (!t.hasChanged && !updateEveryFrameWithoutTranformChanges && !justRegistered) { return; }
        if (deleteInTime && fadeOutRatio >= 1f) { return; }

        t.hasChanged = false;

        usedPositions.Clear();


        float fadeTransparency = bulletData.color.a;
        if (deleteInTime)
        {
            fadeTransparency = Mathf.Clamp01(1f - Mathf.InverseLerp(fadeOutRatio, 1f, (float)(deleteInTime.activeC / deleteInTime.t)));
        }

        bool oneRegistered = false;

        for (int i = 0; i < vertices.Length; ++i)
        {
            Vector3 v = vertices[i];
            v = new Vector3(scale.x * v.x, scale.y * v.y, scale.z * v.z);
            v = transform.TransformPoint(v);
                
            if (usedPositions.Contains(v))
            {
                continue;
            }

            if (myBullets.Count <= i) { break; }
            BulletObject b = myBullets[i];
            if (b == null) { continue; }
            bool thisIsRegistered = BulletRegister.IsRegistered(b);

            if (!oneRegistered) { oneRegistered = thisIsRegistered; }

            /*if (!thisIsRegistered) // this is how we delete bullets that have been collided with or otherwise removed.
                // this way they won't reload if we leave and come back from the loading zone.
            {
                myBullets[i] = null;
                continue;
            }*/
            

            bool back = (!disabledBackside) ? false : BackCondition(v);
            Vector2 scal = b.GetScale();
            if (bulletwiseMvts && bulletwiseMvts.useResize)
            {
                scal = bulletData.scale * bulletwiseMvts.EvaluateResize(v);
            }

            if (disabledBackside)
            {
                Vector2 sv = back ? BackScale(v) : Vector2.one;
                scal = bulletData.scale * sv;
                b.color = back ? (new Color(b.color.r, b.color.g, b.color.b, 0.5f)) : new Color(b.color.r, b.color.g, b.color.b, fadeTransparency);
                b.renderGroup = new BulletControllerHelper.RenderGroup(b.materialInternalIdx, b.textureInternalIdx, b.color);
                b.collisionDisabled = back;
            }
            else if (oldFadeTransparency != fadeTransparency)
            {
                b.color = new Color(b.color.r, b.color.g, b.color.b, fadeTransparency);
                b.renderGroup = new BulletControllerHelper.RenderGroup(b.materialInternalIdx, b.textureInternalIdx, b.color);
            }

            if (scaleBulletsWithTransform)
            {
                scal = new Vector2(bulletData.scale.x * transform.lossyScale.x, bulletData.scale.y * transform.lossyScale.y);
            }

            v = new Vector3(v.x, v.y, back ? backPlaneZ : activePlaneZ);
                
            if (b.rotateWithMovementAngle) {
                Vector3 lastPositionDif = b.GetPosition();
                //b.position = v;
                lastPositionDif = v - lastPositionDif;
                b.UpdateTransform(v, Mathf.Atan2(lastPositionDif.y, lastPositionDif.x)*Mathf.Rad2Deg, scal);
                //b.rotationDegrees = Mathf.Atan2(lastPositionDif.y, lastPositionDif.x);
            }
            else
            {
                float cos = (float)System.Math.Cos(DoubleTime.ScaledTimeSinceLoad * alternateSpinCosinePeriod);
                b.UpdateTransform(v, ((addSelfRotationZ) ? transform.eulerAngles.z : b.GetRotationDegrees()) + bulletAlternateSpin * cos, scal);
            }
        }

        if (!oneRegistered) {
            myBullets.Clear();
            if (deleteObjectWhenAllBulletsGone)
            {
                Destroy(gameObject);
            }
            else
            {
                if (specialTags.Contains("destroy object with this"))
                {
                    Destroy(gameObject);
                }
                else
                {
                    Destroy(this);
                }
            }
            
        }

        oldFadeTransparency = fadeTransparency;
    }
}
