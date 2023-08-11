using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using System.Threading;
using System;
using System.Runtime;
using UnityEngine.Scripting;

public class BulletControllerHelper : MBSingleton<BulletControllerHelper> // this script may allocate a lot of memory as a sacrifice for speed.
{
    protected override void ChildAwake()
    {
        for (int i = 0; i < BulletRegister.idsToDelete.Length; ++i)
        {
            BulletRegister.idsToDelete[i] = -1;
        } 

        quad = new Mesh();
        quad.vertices = new Vector3[4] { new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0), new Vector3(-0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0) };
        quad.triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
        quad.uv = new Vector2[4] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one };
        quad.bounds = new Bounds(Vector3.zero, Vector3.one);

        mpb = new MaterialPropertyBlock();

        for (int i = 0; i < renderMatrices.Length; ++i)
        {
            renderMatrices[i] = new Matrix4x4[1024];
        }

        GameObject bulletCancellerObj = (GameObject)Instantiate(Resources.Load("BulletCancel"));
        bulletCancellerObj.name = "Bullet Cancel Graphics";
        bulletCancellerObj.transform.parent = transform;
        DontDestroyOnLoad(bulletCancellerObj);
    }

    public static Mesh quad;
    public Material testMaterial;
    private Camera mainCam;
    private Transform camTransform;
    private MaterialPropertyBlock mpb;

    private static int damageToDealPlayer = -1;
    private static bool didDamageThisFrame = false;
    private static int gunHealthToGive = 0;
    public static int grazedThisFrame = 0;
    public static float extraShieldDistance = 0;

    public int forceCollisionCheckingFrames = 0;

    public struct RenderGroup
    {
        public int material;
        public int image;
        public float4 color;

        public RenderGroup(int mat, int tex, Color col)
        {
            material = mat; image = tex; color = new float4(col.r, col.g, col.b, col.a);
        }

        public override bool Equals(object obj)
        {
            return Equals((RenderGroup)obj);
        }

        public bool Equals(RenderGroup other)
        {
            return material == other.material && image == other.image && color.Equals(other.color);
        }

        public override int GetHashCode()
        {
            return material.GetHashCode() ^ image.GetHashCode() ^ color.GetHashCode();
        }
    }

    private RenderGroup defaultRenderGroup = new RenderGroup(-1, -1, Color.clear);

    private static float2 WorldToBulletSpace(float2 b, float2 v, float2 bRight, float2 bUp)
    {
        v -= b;
        return new float2(
            (v.x * bRight.x + v.y * bRight.y) / (bRight.x * bRight.x + bRight.y * bRight.y),
            (v.x * bUp.x + v.y * bUp.y) / (bUp.x * bUp.x + bUp.y * bUp.y)
            );
    }

    public struct BulletCollisionStruct
    {
        public float2 position;
        public float2 scale;
        public bool grazable;
        public bool enabled;
        public bool squareHitbox;
        public float rotationDegrees;
        public float killRadiusRatio;
        public float damage;
        public int matIdx;
        public int texIdx;
        public double activeTime;
        public int id;

        public BulletCollisionStruct(float2 position, float2 scale, bool grazable, bool enabled, bool squareHitbox, float rotationDegrees, float killRadiusRatio, float damage, int matIdx, int texIdx, double activeTime, int id)
        {
            this.position = position;
            this.scale = scale;
            this.grazable = grazable;
            this.enabled = enabled;
            this.squareHitbox = squareHitbox;
            this.rotationDegrees = rotationDegrees;
            this.killRadiusRatio = killRadiusRatio;
            this.damage = damage;
            this.matIdx = matIdx;
            this.texIdx = texIdx;
            this.activeTime = activeTime;
            this.id = id;
        }
    }

    NativeArray<BulletCollisionStruct> ccjBulletInfo = new NativeArray<BulletCollisionStruct>(BulletRegister.MAX_BULLETS, Allocator.Persistent);
    NativeArray<CollisionPointer> ccjPointers = new NativeArray<CollisionPointer>(BulletRegister.MAX_BULLETS, Allocator.Persistent);
    //NativeArray<bool> ccjCollided = new NativeArray<bool>(BulletRegister.MAX_BULLETS, Allocator.Persistent);
    //NativeArray<bool> ccjGrazed = new NativeArray<bool>(BulletRegister.MAX_BULLETS, Allocator.Persistent);
    BulletObject[] ccjCorrespondence = new BulletObject[BulletRegister.MAX_BULLETS];

    NativeArray<BulletMovementStruct> mvtBulletInfo = new NativeArray<BulletMovementStruct>(BulletRegister.MAX_BULLETS, Allocator.Persistent);
    //NativeArray<float> mvtRotDegrees = new NativeArray<float>(BulletRegister.MAX_BULLETS, Allocator.Persistent);
    //NativeArray<float3> mvtPositions = new NativeArray<float3>(BulletRegister.MAX_BULLETS, Allocator.Persistent);
    NativeArray<MovementTRSPointer> mvtTRSPointers = new NativeArray<MovementTRSPointer>(BulletRegister.MAX_BULLETS, Allocator.Persistent);
    BulletObject[] mvtCorrespondence = new BulletObject[BulletRegister.MAX_BULLETS];

    int collisionsToCheck = 0;
    int movementsToCheck = 0;

    private void OnDestroy() // only when the game ends 
    {
        ccjBulletInfo.Dispose();
        ccjPointers.Dispose();
        //ccjCollided.Dispose();
        //ccjGrazed.Dispose();

        mvtBulletInfo.Dispose();
        mvtTRSPointers.Dispose();
        //mvtRotDegrees.Dispose();
        //mvtPositions.Dispose();
        BulletRegister.Clear();
        BulletRegister.idsToDelete.Dispose();
    }

    public unsafe struct CollisionPointer
    {
        public bool* grazed;
        public Color* color;
        public RenderGroup* renderGroup;

        public CollisionPointer(bool* grazed, Color* color, RenderGroup* renderGroup)
        {
            this.grazed = grazed;
            this.color = color;
            this.renderGroup = renderGroup;
        }
    }

    [BurstCompile]
    public unsafe struct CollisionCheckingJob : IJobFor
    {
        [ReadOnly]
        public float2 playerPos;
        [ReadOnly]
        public float extraShieldDistance;
        [NativeDisableUnsafePtrRestriction]
        public bool* didDamage;
        [NativeDisableUnsafePtrRestriction]
        public int* damage;
        [NativeDisableUnsafePtrRestriction]
        public int* gunHealth;
        [NativeDisableUnsafePtrRestriction]
        public int* grazedThisFrameI;
        [ReadOnly]
        public NativeArray<BulletCollisionStruct> b;

        public NativeArray<int> idsToDelete;

        public NativeArray<CollisionPointer> cpt;

        /*[WriteOnly]
        public NativeArray<bool> collided;
        [WriteOnly]
        public NativeArray<bool> grazed;*/

        public void Execute(int i)
        {
            if (!b[i].enabled) { return; }
            if (Mathf.Abs(b[i].scale.x) < 0.5f || Mathf.Abs(b[i].scale.y) < 0.5f) { return; } // don't collide if the bullet is too small

            float cos = Mathf.Cos(b[i].rotationDegrees * Mathf.Deg2Rad);
            float sin = Mathf.Sin(b[i].rotationDegrees * Mathf.Deg2Rad);

            Vector2 bRight = new Vector2(cos, sin) * (b[i].scale.x + extraShieldDistance * 2f);
            Vector2 bUp = new Vector2(-sin, cos) * (b[i].scale.y + extraShieldDistance * 2f);
            float2 scaledCoords = WorldToBulletSpace(b[i].position, playerPos, bRight, bUp);

            float2 bRightG = new Vector2(cos, sin) * (b[i].scale.x + 32f);
            float2 bUpG = new Vector2(-sin, cos) * (b[i].scale.y + 32f);
            float2 scaledGCoords = WorldToBulletSpace(b[i].position, playerPos, bRightG, bUpG);

            float colDist;
            bool grazed;

            if (b[i].squareHitbox) // intersection of a point and rectangle
            {
                colDist = Mathf.Max(Mathf.Abs(scaledCoords.x), Mathf.Abs(scaledCoords.y));
                grazed = Mathf.Max(Mathf.Abs(scaledGCoords.x), Mathf.Abs(scaledGCoords.y)) < 0.5f;
            }
            else // intersection of a point and scaled circle
            {
                colDist = Mathf.Sqrt(scaledCoords.x*scaledCoords.x + scaledCoords.y*scaledCoords.y);
                grazed = Mathf.Sqrt(scaledGCoords.x * scaledGCoords.x + scaledGCoords.y * scaledGCoords.y) < 0.5f;
            }

            bool collided = colDist < b[i].killRadiusRatio * 0.5f;

            if (collided && b[i].activeTime >= 0f)
            {
                *didDamage = true; // no race condition; only set to false before all threads
                Interlocked.Add(ref *damage, Mathf.FloorToInt(b[i].damage));
                // integer amount of damage. limitation of interlocked add.

                // this line used to cause seg faults! now it's hopefully fixed.
                //idsToDelete.Add(b[i].id);
                idsToDelete[i] = b[i].id;
            }
            else if (grazed)
            {
                Interlocked.Increment(ref *grazedThisFrameI);
                if (!(*cpt[i].grazed) && b[i].grazable)
                {
                    (*cpt[i].grazed) = true;
                    (*cpt[i].color) = new Color(cpt[i].color->r + 0.25f, cpt[i].color->g + 0.25f, cpt[i].color->b + 0.25f, cpt[i].color->a);
                    (*cpt[i].renderGroup) = new RenderGroup(b[i].matIdx, b[i].texIdx, *cpt[i].color);
                    Interlocked.Increment(ref *gunHealth);
                }
            }
        }
    }

    public struct BulletMovementStruct
    {
        public float startingVelocity;
        public bool isAccelerationMultiplicative;
        public float acceleration;
        public double timeSince;
        public bool isTorqueSineWave;
        public float startingTorque;
        public float startingDirection;
        public float changeInTorque;
        public float sineMotionSpeed;
        public float sineMotionOffset;
        public bool rotateWithMovementAngle;
        public float3 originPosition;
        public float simSpeed;
        public int correspond;

        public BulletMovementStruct(float startingVelocity, bool isAccelerationMultiplicative, float acceleration, double timeSince, bool isTorqueSineWave, float startingTorque, float startingDirection, float changeInTorque, float sineMotionSpeed, float sineMotionOffset, bool rotateWithMovementAngle, float3 originPosition, float simSpeed, int correspond)
        {
            this.startingVelocity = startingVelocity;
            this.isAccelerationMultiplicative = isAccelerationMultiplicative;
            this.acceleration = acceleration;
            this.timeSince = timeSince;
            this.isTorqueSineWave = isTorqueSineWave;
            this.startingTorque = startingTorque;
            this.startingDirection = startingDirection;
            this.changeInTorque = changeInTorque;
            this.sineMotionSpeed = sineMotionSpeed;
            this.sineMotionOffset = sineMotionOffset;
            this.rotateWithMovementAngle = rotateWithMovementAngle;
            this.originPosition = originPosition;
            this.simSpeed = simSpeed;
            this.correspond = correspond;
        }
    };

    public unsafe struct MovementTRSPointer
    {
        public BulletObject.TRS* trs;

        public MovementTRSPointer(BulletObject.TRS* trs)
        {
            this.trs = trs;
        }
    }

    [BurstCompile]
    public unsafe struct MovementJob : IJobFor
    {
        [ReadOnly]
        public NativeArray<BulletMovementStruct> b;
        /*[WriteOnly]
        public NativeArray<float> rotationDegrees;
        [WriteOnly]
        public NativeArray<float3> position;*/
        public NativeArray<MovementTRSPointer> trsRegions;

        public void Execute(int i)
        {
            float currVelocity = 0;
            double t = b[i].timeSince * b[i].simSpeed;

            if (b[i].isAccelerationMultiplicative)
            {
                //currVelocity = b[i].startingVelocity * (Mathf.Pow(b[i].acceleration, (float)b[i].timeSince)) - b[i].acceleration / 2;
                currVelocity = b[i].startingVelocity;
            }
            else
            {
                currVelocity = (float)(b[i].startingVelocity + (b[i].acceleration * t / 2));
            }

            float currTorque = 0;

            float currDirection = 0;
            if (b[i].isTorqueSineWave)
            {
                currTorque = b[i].startingTorque;
                currDirection = (float)(b[i].startingDirection + (currTorque * t) + (b[i].changeInTorque * System.Math.Sin((b[i].sineMotionSpeed * t) + b[i].sineMotionOffset)));
            }
            else
            {
                currTorque = (float)(b[i].startingTorque + (b[i].changeInTorque * t));
                currDirection = (float)(b[i].startingDirection + (currTorque * t));
            }

            float rota = b[i].rotateWithMovementAngle ? currDirection : 0;
            //rotationDegrees[i] = rota;
            float distCoefficient;
            if (b[i].isAccelerationMultiplicative) {
                distCoefficient = currVelocity * Mathf.Lerp((float)t,Mathf.Pow((float)t, b[i].acceleration),(float)t);
            }
            else {
                distCoefficient = currVelocity * (float)t;
            }

            float xChange = (float)(System.Math.Cos(currDirection * Mathf.Deg2Rad) * distCoefficient);
            float yChange = (float)(System.Math.Sin(currDirection * Mathf.Deg2Rad) * distCoefficient);

            float3 posa = b[i].originPosition + new float3(xChange, yChange, 0);
            //position[i] = posa;

            float2 scal = trsRegions[i].trs->scale;

            (*trsRegions[i].trs) = new BulletObject.TRS
            {
                position = posa,
                rotationDegrees = rota,
                scale = scal,
                renderMatrix = Matrix4x4.TRS(posa - new float3(0, 0, 16), Quaternion.AngleAxis(rota, Vector3.forward), new Vector3(scal.x, scal.y, 1f))
            };
        }
    }

    private RenderGroup[] renderComps;
    private Matrix4x4[][] renderMatrices = new Matrix4x4[16][];
    private int[] renderMatricesTops = new int[16] { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 };
    private ShieldingCircle[] shields;
    private LaserBullet[] tetraLasers;

    private JobHandle collisionCheckingJobHandle;
    private JobHandle movementJobHandle;

    private Transform playerToHit;
    private SpecialGunTemplate sgt;

    private void Update()
    {
        if (BulletRegister.allType1.Count == 0) { return; }

        collisionsToCheck = 0;
        movementsToCheck = 0;

        mainCam = Camera.main;
        if (mainCam == null) { return; }
        if (!camTransform) { camTransform = mainCam.transform; }

        renderMatricesTops = new int[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        renderComps = new RenderGroup[16];
        for (int i = 0; i < renderComps.Length; ++i)
        {
            renderComps[i] = defaultRenderGroup;
        }

        Encontrolmentation e = LevelInfoContainer.GetActiveControl();
        if (!e) { return; }
        playerToHit = e.transform;
        sgt = playerToHit.gameObject.GetComponent<SpecialGunTemplate>();
        if (sgt is ClickToShieldAndInfJump)
        {
            extraShieldDistance = (sgt as ClickToShieldAndInfJump).layers * 3f;
        } else { extraShieldDistance = 0f; }
        shields = new ShieldingCircle[ShieldingCircle.all.Count];
        ShieldingCircle.all.CopyTo(shields, 0, ShieldingCircle.all.Count);
        tetraLasers = new LaserBullet[LaserBullet.tetraLasers.Count];
        LaserBullet.tetraLasers.CopyTo(tetraLasers, 0, LaserBullet.tetraLasers.Count);
        grazedThisFrame = 0;

        foreach (BulletObject b in BulletRegister.allType1)
        {
            // b.internalSelfId >= 0 check is in case bullets are ever destroyed between frames,
            // after LateUpdate().
            // It will crash otherwise!
            if (b != null && b.internalSelfId >= 0) { Move(b, playerToHit, sgt); }
        }

        if (e.transform.localScale.magnitude < 0.01f && forceCollisionCheckingFrames <= 0) { goto PastCollisionJob; }

        unsafe
        {
            fixed (bool* dd = &didDamageThisFrame)
            {
                fixed (int* d = &damageToDealPlayer)
                {
                    fixed (int* g = &gunHealthToGive)
                    {
                        fixed (int* gf = &grazedThisFrame)
                        {
                            var collisionCheckingJob = new CollisionCheckingJob
                            {
                                playerPos = (Vector2)playerToHit.position,
                                extraShieldDistance = extraShieldDistance,
                                b = ccjBulletInfo,
                                cpt = ccjPointers,
                                damage = d,
                                didDamage = dd,
                                gunHealth = g,
                                grazedThisFrameI = gf,
                                idsToDelete = BulletRegister.idsToDelete,
                                //collided = ccjCollided,
                                //grazed = ccjGrazed,
                            };

                            collisionCheckingJobHandle = collisionCheckingJob.ScheduleParallel(collisionsToCheck, 4, collisionCheckingJobHandle);
                        }
                    }
                }
            }
        }

        PastCollisionJob:

        var movementJob = new MovementJob
        {
            b = mvtBulletInfo,
            trsRegions = mvtTRSPointers,
            //rotationDegrees = mvtRotDegrees,
            //position = mvtPositions
        };
        
        movementJobHandle = movementJob.ScheduleParallel(movementsToCheck, 4, movementJobHandle);

        for (int i = 0; i < renderMatrices.Length; ++i) { Render(i); }

        // don't move this to late update.
        // why? grazedThisFrame may not be correct if it's there. race condition
        collisionCheckingJobHandle.Complete();
        movementJobHandle.Complete();

        //print(BulletRegister.allType1.Count);
    }

    private void Render(int i)
    {
        if (renderMatricesTops[i] == 0) { return; }

        RenderGroup currRender = renderComps[i];
        //if (currRender.image == null) { return; }

        if (BulletRegister.textures[currRender.image] == null) { return; }
        if (BulletRegister.materials[currRender.material] == null) { return; }

        mpb.SetTexture("_MainTex", BulletRegister.textures[currRender.image]);
        mpb.SetColor("_Color", new Color(currRender.color.x, currRender.color.y, currRender.color.z, currRender.color.w));

        Graphics.DrawMeshInstanced(
            quad, 
            0, 
            BulletRegister.materials[currRender.material], 
            renderMatrices[i], 
            renderMatricesTops[i], 
            mpb, 
            UnityEngine.Rendering.ShadowCastingMode.Off, 
            false, 
            22, 
            mainCam, 
            UnityEngine.Rendering.LightProbeUsage.Off);

        //print(renderMatrices.Count);
    }

    /*private Vector2 WorldToBulletSpace(BulletObject b, Vector2 v, Vector2 bRight, Vector2 bUp)
    {
        v -= (Vector2)b.position;
        return new Vector2(
            Vector2.Dot(v, bRight) / bRight.sqrMagnitude,
            Vector2.Dot(v, bUp) / bUp.sqrMagnitude
            );
    }*/

    

    private float colDist = 0f;

    private void Move(BulletObject b, Transform playerToHit, SpecialGunTemplate sgt)
    {
        double timeSince = DoubleTime.ScaledTimeSinceLoad - b.timeThisWasMade_ScriptsOnly;
        float maxScalePart = Mathf.Max(b.GetScale().x, b.GetScale().y);
        /*bool visible = (Mathf.Abs(camPos.x - b.GetPosition().x) <= 160 + maxScalePart
            && Mathf.Abs(camPos.y - b.GetPosition().y) <= 108 + maxScalePart);*/
        Vector2 iv = camTransform.InverseTransformPoint(b.GetPosition());
        bool visible = Mathf.Abs(iv.x) <= 160 + maxScalePart && Mathf.Abs(iv.y) <= 108 + maxScalePart;
        b.wentOffscreen |= !visible;

        if (Time.timeScale == 0 && !FollowThePlayer.main.scrollingIndicator) { goto renderPart; }

        if (timeSince > b.deletTime 
            || (b.destroyOnLeaveScreen && (!visible || FollowThePlayer.main.scrollingIndicator))  
            || (b.destroyOnScreenScroll && FollowThePlayer.main.scrollingIndicator))
        {
            BulletRegister.MarkToDestroy(b);
            return;
        }

        if (Time.timeScale == 0) { goto renderPart; }

        if (!visible || b.collisionDisabled || KHealth.someoneDied) { goto afterCollision; }

        if (b.grazeDisabled) { goto afterSpecialCollisionChecks; }

        for (int i = 0; i < shields.Length; ++i)
        {
            ShieldingCircle sc = shields[i];
            if (sc.radius + maxScalePart >= ((Vector2)b.GetPosition() - (Vector2)sc.t.position).magnitude)
            {
                //print((b.position - sc.t.position).magnitude);
                BulletRegister.MarkToDestroy(b);
                if (shieldCancel) { shieldCancel.Stop(); shieldCancel.Play(); }
                return;
            }
        }

        for (int i = 0; i < tetraLasers.Length; ++i)
        {
            LaserBullet lb = tetraLasers[i];
            if (lb.TetraLaserBlockCollision(b.GetPosition(), maxScalePart)) {
                BulletRegister.MarkToDestroy(b);
                return;
            }
        }

        afterSpecialCollisionChecks:

        ccjBulletInfo[collisionsToCheck] = new BulletCollisionStruct((Vector2)b.GetPosition(), b.GetScale(), !b.grazeDisabled, !b.collisionDisabled, b.squareHitbox, b.GetRotationDegrees(), b.killRadiusRatio, b.damage, b.materialInternalIdx, b.textureInternalIdx, DoubleTime.ScaledTimeSinceLoad - b.timeThisWasMade_ScriptsOnly - b.fadeInTime, b.internalSelfId);
        unsafe
        {
            fixed (bool* g = &b.grazed) { fixed (Color* c = &b.color) { fixed (RenderGroup* r = &b.renderGroup) {
                        ccjPointers[collisionsToCheck] = new CollisionPointer(g, c, r);
            } } }
        }
        ccjCorrespondence[collisionsToCheck] = b;
        ++collisionsToCheck;

        afterCollision:

        if (b.doesntMoveOnItsOwn) { goto renderPart; }

        /*if (timeSince < fadeInTime)
        {
            spr.color = new Color(spr.color.r, spr.color.g, spr.color.b, initAlpha * (float)(timeSince / fadeInTime));
        }
        else
        {
            spr.color = new Color(spr.color.r, spr.color.g, spr.color.b, initAlpha);
        }*/

        mvtBulletInfo[movementsToCheck] = new BulletMovementStruct(b.startingVelocity, b.isAccelerationMultiplicative, b.acceleration, timeSince, b.isTorqueSineWave, b.startingTorque, b.startingDirection, b.changeInTorque, b.sineMotionSpeed, b.sineMotionOffset, b.rotateWithMovementAngle, b.originPosition, b.simulationSpeedMult, movementsToCheck);
        unsafe
        {
            fixed (BulletObject.TRS* p = &(b.trs))
            {
                mvtTRSPointers[movementsToCheck] = new MovementTRSPointer(p);
            }
        }
        mvtCorrespondence[movementsToCheck] = b;
        ++movementsToCheck;
        //b.renderMatrix = Matrix4x4.TRS(b.position - new Vector3(0, 0, 16), Quaternion.AngleAxis(b.rotationDegrees, Vector3.forward), new Vector3(b.scale.x, b.scale.y, 1f));

        renderPart:

        if (!visible || b.GetExistTime() < 0.02 * Time.timeScale) { return; }

        bool rendAdded = false;

        for (int i = 0; i < renderComps.Length; ++i)
        {
            if (renderComps[i].material == -1) { renderComps[i] = b.renderGroup; }
            if (b.materialInternalIdx == renderComps[i].material && b.textureInternalIdx == renderComps[i].image && new float4(b.color.r, b.color.g, b.color.b, b.color.a).Equals(renderComps[i].color) && renderMatricesTops[i] < 1000)
            {
                renderMatrices[i][renderMatricesTops[i]] = b.trs.renderMatrix;
                /*if (b.fadeInTime > 0.03) {
                    float rat = Mathf.Lerp(3f, 1f, (float)(DoubleTime.ScaledTimeSinceLoad - b.timeThisWasMade_ScriptsOnly) / b.fadeInTime);
                    renderMatrices[i][renderMatricesTops[i]].m00 *= rat;
                    renderMatrices[i][renderMatricesTops[i]].m11 *= rat;
                    renderMatrices[i][renderMatricesTops[i]].m22 *= rat;
                }*/
                ++renderMatricesTops[i];
                rendAdded = true; break;
            }
        }

        if (!rendAdded)
        {
            int lowestIdx = 0;
            int lowestVal = renderMatricesTops[0];
            for (int i = 1; i < renderMatricesTops.Length; ++i)
            {
                if (renderMatricesTops[i] < lowestVal)
                {
                    lowestIdx = i; lowestVal = renderMatricesTops[i];
                }
            }

            Render(lowestIdx); renderMatricesTops[lowestIdx] = 0; renderComps[lowestIdx] = defaultRenderGroup;
            goto renderPart;
        }

    }

    private static GameObject grazeSounds;
    private static AudioSource graze1;
    private static AudioSource graze20;
    private static AudioSource collectSound;
    private static AudioSource shieldCancel;

    private void InstantiateGrazeSounds()
    {
        if (grazeSounds == null)
        {
            grazeSounds = Instantiate(Resources.Load<GameObject>("GrazeSounds"));
            grazeSounds.name = "GrazeSounds";
            DontDestroyOnLoad(grazeSounds);

            graze1 = grazeSounds.transform.Find("Graze1").GetComponent<AudioSource>();
            graze20 = grazeSounds.transform.Find("Graze20").GetComponent<AudioSource>();
            collectSound = grazeSounds.transform.Find("Collect").GetComponent<AudioSource>();
            shieldCancel = grazeSounds.transform.Find("ShieldCancel").GetComponent<AudioSource>();
        }
    }

    public void PlayGrazeSound(float oldVal, float newVal)
    {
        InstantiateGrazeSounds();

        graze1.Stop(); graze1.Play();

        if (Mathf.Floor(oldVal / 20) < Mathf.Floor(newVal / 20))
        {
            graze20.Stop(); graze20.Play();
        }
    }

    private void PlayCollectSound()
    {
        InstantiateGrazeSounds();

        collectSound.Stop(); collectSound.Play();
    }

    private void LateUpdate()
    {
        if (damageToDealPlayer > 0 && playerToHit != null)
        {
            if (!Door1.levelComplete) // still do bullet damage when in subtractive laser
            {
                KHealth kh = playerToHit.gameObject.GetComponent<KHealth>();
                if (kh.justFiredBulletInvincibility == 0 && kh.nontoxic == 0)
                {
                    kh.ChangeHealth(-damageToDealPlayer, "danmaku");
                }
                else if (kh.nontoxic != 0)
                {
                    PlayCollectSound();
                }
            }
        }
        if (damageToDealPlayer == 0 && didDamageThisFrame) // fake damage: collect
        {
            PlayCollectSound();
        }
        damageToDealPlayer = 0;
        didDamageThisFrame = false;

        if (gunHealthToGive > 0 && sgt != null)
        {
            float gunHealthToGive2 = gunHealthToGive * GrazeResistance.GetMultiplier();
            PlayGrazeSound(sgt.gunHealth, sgt.gunHealth + gunHealthToGive2);
            if (sgt.enabled)
            {
                sgt.gunHealth += gunHealthToGive2;
                if (sgt.gunHealth > 100) { sgt.gunHealth = 100; }
            }
        }
        gunHealthToGive = 0;

        for (int i = 0; i < collisionsToCheck; ++i)
        {
            if (BulletRegister.idsToDelete[i] != -1)
            {
                BulletRegister.MarkToDestroy(BulletRegister.idList[BulletRegister.idsToDelete[i]]);
                BulletRegister.idsToDelete[i] = -1;
            }
        }

        foreach (BulletObject b in BulletRegister.toDelete1)
        {
            BulletRegister.allType1.Remove(b);
        }
        BulletRegister.toDelete1.Clear();

        if (forceCollisionCheckingFrames > 0) { --forceCollisionCheckingFrames; }
    }
}
