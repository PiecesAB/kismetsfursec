using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class BulletObject
{
    public Vector3 originPosition;
    public float startingDirection;
    public float startingVelocity;
    public bool isAccelerationMultiplicative;
    public float acceleration;
    public float startingTorque;
    public bool isTorqueSineWave;
    public float changeInTorque;
    public float sineMotionSpeed;
    [Range(0f, Mathf.PI * 2)]
    public float sineMotionOffset;
    //public Transform playerToHit;
    public float atWhatDistanceFromCenterIsAHit; // unused
    public float damage;
    public float killRadiusRatio; // = 1f; // a ratio between scale and hitbox
    public bool destroyOnLeaveScreen; // = true;
    public bool destroyOnScreenScroll = false;
    public bool doesntMoveOnItsOwn; // = false;
    public bool collisionDisabled; // = false;
    public bool grazeDisabled = false;
    public bool squareHitbox; // = false;
    public float simulationSpeedMult; // = 1f;

    public bool destroyOnAmbushClear = true;
    public bool wentOffscreen = false;

    // If something uses this, it will also need to initialize if necessary.
    public enum ExtraDataTag
    {
        PhysVel, PhysAccel
    }

    public Dictionary<ExtraDataTag, object> extraData = null;

    public double timeThisWasMade_ScriptsOnly;
    public double timeWhenDestroyed;
    public float deletTime;
    //public bool isVisible = false;
    //public float maxScalePart = 0f;

    public float fadeInTime; // = 0.5f;

    public struct TRS
    {
        public Vector3 position;
        public float rotationDegrees;
        public Vector2 scale;
        public Matrix4x4 renderMatrix;
    }

    private static Transform mainCameraTransform;

    public Vector3 GetCameraPosition()
    {
        if (!mainCameraTransform) { mainCameraTransform = Camera.main.transform; }
        return GetPosition() - mainCameraTransform.position;
    }

    public TRS trs; // normally, this should not be directly accessed

    public void UpdateTransform(Vector3 pos)
    {
        float rot = trs.rotationDegrees; Vector2 scal = trs.scale;
        trs = new TRS
        {
            position = pos,
            rotationDegrees = rot,
            scale = scal,
            renderMatrix = Matrix4x4.TRS(pos - new Vector3(0, 0, 16), Quaternion.AngleAxis(rot, Vector3.forward), new Vector3(scal.x, scal.y, 1f))
        };
        //maxScalePart = Mathf.Max(scal.x, scal.y);
    }

    public void UpdateTransform(Vector3 pos, float rot)
    {
        Vector2 scal = trs.scale;
        trs = new TRS
        {
            position = pos,
            rotationDegrees = rot,
            scale = scal,
            renderMatrix = Matrix4x4.TRS(pos - new Vector3(0, 0, 16), Quaternion.AngleAxis(rot, Vector3.forward), new Vector3(scal.x, scal.y, 1f))
        };
        //maxScalePart = Mathf.Max(scal.x, scal.y);
    }

    public void UpdateTransform(Vector3 pos, float rot, Vector2 scal)
    {
        trs = new TRS
        {
            position = pos,
            rotationDegrees = rot,
            scale = scal,
            renderMatrix = Matrix4x4.TRS(pos - new Vector3(0, 0, 16), Quaternion.AngleAxis(rot, Vector3.forward), new Vector3(scal.x, scal.y, 1f))
        };
        //maxScalePart = Mathf.Max(scal.x, scal.y);
    }

    public void UpdateTransform(Vector3 pos, Vector2 scal)
    {
        float rot = trs.rotationDegrees;
        trs = new TRS
        {
            position = pos,
            rotationDegrees = rot,
            scale = scal,
            renderMatrix = Matrix4x4.TRS(pos - new Vector3(0, 0, 16), Quaternion.AngleAxis(rot, Vector3.forward), new Vector3(scal.x, scal.y, 1f))
        };
        //maxScalePart = Mathf.Max(scal.x, scal.y);
    }

    public double GetExistTime()
    {
        return DoubleTime.ScaledTimeSinceLoad - timeThisWasMade_ScriptsOnly;
    }

    public void UpdateRenderGroup()
    {
        renderGroup = new BulletControllerHelper.RenderGroup(materialInternalIdx, textureInternalIdx, color);
    }

    public Vector3 GetPosition() { return trs.position; }
    public float GetRotationDegrees() { return trs.rotationDegrees; }
    public Vector2 GetScale() { return trs.scale; }

    public int materialInternalIdx = -1;
    public Color color;

    public int textureInternalIdx = -1;

    public int internalSelfId = -1;

    public bool rotateWithMovementAngle; // = false;

    //public Matrix4x4 renderMatrix;
    public BulletControllerHelper.RenderGroup renderGroup;

    // Tracking data for StaticBulletsOnVertices
    public StaticBulletsOnVertices staticBulletParent = null;
    public int staticBulletIndex = -1;

    //private float initAlpha;

    public bool grazed;

    public BulletObject()
    {
        timeThisWasMade_ScriptsOnly = DoubleTime.ScaledTimeSinceLoad;
    }

    public BulletObject ShallowCopy()
    {
        return (BulletObject)MemberwiseClone();
    }

    public BulletObject(BulletObject other)
    {

    }
}
