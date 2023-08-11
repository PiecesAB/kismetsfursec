using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class clawGrabber : MonoBehaviour
{

    public enum Mode
    {
        Hover, Descend, Catch, Ascend
    }

    public Mode mode;
    public Transform eyes;
    public SkinnedMeshRenderer mainRendererPart;
    public Renderer clawRendererPart;
    public GameObject clawGameObj;
    public Transform leftClaw;
    public Transform rightClaw;
    public Transform all;
    public int patience;
    public int catchTimer;
    public Vector2 xMvtRange;

    private Vector3 origClawPos;
    //private Vector3 origAllPos;

    private const float speedFall = -2f;
    private int initialPatience;
    private const float nearXDist = 32f;
    private float xVel = 0f;




    private Vector3 origEyePos;

    Transform getClosestPlayerToEyes(out Vector2 dist)
    {
        Transform o = null;
        float d = 1000000f;
        Vector2 t1 = Vector2.zero;
        foreach (GameObject t in LevelInfoContainer.allBoxPhysicsObjects)
        {
            t1 = (t.transform.position - eyes.transform.parent.position);
            float ez = t1.sqrMagnitude;
            if (t.CompareTag("Player") && ez < d*d)
            {
                o = t.transform;
                d = Fastmath.FastSqrt(ez);
            }
        }
        dist = t1;
        return o;
    }

    void Start()
    {
        origEyePos = eyes.localPosition;
        initialPatience = patience;
        origClawPos = clawGameObj.transform.localPosition;
    }

    void Update()
    {
        if (Time.timeScale > 0 && mainRendererPart.isVisible)
        {
            Vector2 closestDir;
            Transform closest = getClosestPlayerToEyes(out closestDir);
            Vector2 closestN = closestDir.normalized;
            eyes.localPosition = origEyePos + new Vector3(2.5f * closestN.x, 0f, 2.5f * closestN.y);

            Transform ct = clawGameObj.transform;

            switch (mode)
            {
                case Mode.Hover:
                    float animAngle = 5f*(float)System.Math.Sin(DoubleTime.ScaledTimeSinceLoad + (30f*patience/initialPatience));
                    float targX = Mathf.Clamp(all.parent.InverseTransformPoint(closest.position).x, xMvtRange.x, xMvtRange.y);
                    float newXPos = Mathf.SmoothDamp(all.localPosition.x, targX, ref xVel, 0.6f);
                    //float newXPos = targX;
                    all.localPosition = new Vector3(newXPos, all.localPosition.y, all.localPosition.z);
                    clawGameObj.transform.localPosition = origClawPos + 2.5f * Vector3.up * (float)System.Math.Cos(DoubleTime.ScaledTimeSinceLoad * 6.28f);

                    if (closestDir.y > -10.5f)
                    {
                        mainRendererPart.SetBlendShapeWeight(1, 100f);
                        leftClaw.localEulerAngles = rightClaw.localEulerAngles = Vector3.zero;
                        clawGameObj.transform.localPosition = origClawPos;
                    }
                    else
                    {
                        leftClaw.localEulerAngles = Vector3.forward * (animAngle+5f);
                        rightClaw.localEulerAngles = Vector3.back * (animAngle+5f);
                        if (System.Math.Abs(closestDir.x) < nearXDist)
                        {
                            if (patience > 60)
                            {
                                patience = 60;
                            }
                            else
                            {
                                patience -= 3;
                            }
                        }
                        else
                        {
                            patience--;
                        }

                        if (patience < 60)
                        {
                            mainRendererPart.SetBlendShapeWeight(0, 100f * (1f - (patience / 60f)));
                        }
                        else
                        {
                            mainRendererPart.SetBlendShapeWeight(0, 0f);
                        }
                        mainRendererPart.SetBlendShapeWeight(1, 0f);
                    }

                    if (patience <= 0)
                    {
                        patience = 0;
                        mode = Mode.Descend;
                    }
                    break;
                case Mode.Descend:
                    leftClaw.localEulerAngles = rightClaw.localEulerAngles = Vector3.zero;
                    ct.localPosition += Vector3.up*speedFall*Time.timeScale;
                    RaycastHit2D rh = Physics2D.Raycast(ct.position, -ct.up, 24f, 768);
                    if (ct.position.y-24f <= closest.position.y || rh.rigidbody != null)
                    {
                        mode = Mode.Catch;
                        catchTimer = 96;
                    }
                    break;
                case Mode.Catch:
                    catchTimer--;
                    if (catchTimer > 80)
                    {
                        float r = (96 - catchTimer) * 5.625f;
                        leftClaw.localEulerAngles = new Vector3(0, 0, r);
                        rightClaw.localEulerAngles = new Vector3(0, 0, -r);
                    }
                    else if (catchTimer >= 32)
                    {
                        leftClaw.localEulerAngles = new Vector3(0, 0, 90);
                        rightClaw.localEulerAngles = new Vector3(0, 0, -90);
                    }
                    else if (catchTimer > 0)
                    {
                        float r = catchTimer * 2.8125f;
                        leftClaw.localEulerAngles = new Vector3(0, 0, r);
                        rightClaw.localEulerAngles = new Vector3(0, 0, -r);
                    }
                    else
                    {
                        leftClaw.localEulerAngles = rightClaw.localEulerAngles = Vector3.zero;
                        mode = Mode.Ascend;
                    }
                    break;
                case Mode.Ascend:
                    mainRendererPart.SetBlendShapeWeight(0, 0f);
                    mainRendererPart.SetBlendShapeWeight(1, 0f);
                    leftClaw.localEulerAngles = rightClaw.localEulerAngles = Vector3.zero;
                    ct.localPosition = Vector3.MoveTowards(ct.localPosition, origClawPos, -speedFall * 0.5f * Time.timeScale);
                    if (ct.localPosition == origClawPos)
                    {
                        mode = Mode.Hover;
                        patience = initialPatience;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
