using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemys1 : GenericBlowMeUp { //shooter type enemies

    public enum Type
    {
        MissileLauncher,
    }

    public Type type;
    public bool rotateToTarget = true;
    [Header("edit this for testing only")]
    public Transform target;
    public LineRenderer targetLine;
    public GameObject missile0;
    public Vector3 myPositionOffset;
    public float optimalHitTime = 0.9f;
    public int framesReloadTime = 54;
    public float maxPower;
    public bool autoTargetClosestPhysObj;
    public float gravMultiplier;
    public bool predictTargetMovement;
    public bool playAudioSource;
    public Transform bowDrawbackBone;
    public Vector3 drawbackStart;
    public Vector3 drawbackStop;
    [Range(0.0f, 1.0f)]
    public float lerpVal = 0.25f;
    public float inaccuracy;
    private Vector3 velocityOffset;
    private List<Vector3> targlinevec = new List<Vector3>() { };
    private Vector3 oldTargetPos;
    private int frameReloadTimeMax;

	void Start () {
        float grav = gravMultiplier * Physics2D.gravity.y;
        targetLine.sortingLayerName = "UI";
        targetLine.sortingOrder = 0;
        frameReloadTimeMax = framesReloadTime;
    }
	
	void Update () {

        if (type == Type.MissileLauncher && Time.timeScale > 0)
        {
            float dist = Mathf.Infinity;
            target = null;
            if (GetComponent<Renderer>().isVisible)
            {
                foreach (GameObject man in LevelInfoContainer.allBoxPhysicsObjects)
                {
                    if (man != null && man.GetComponent<Encontrolmentation>())
                    {
                        float ndist = Fastmath.FastV2Dist(man.transform.position, transform.position + myPositionOffset);
                        if (ndist < dist)
                        {
                            dist = ndist;
                            target = man.transform;
                        }
                    }
                }
            }

            if (GetComponent<Renderer>().isVisible && target != null)
            {
                if (predictTargetMovement && target.GetComponent<Rigidbody2D>() != null)
                {
                    velocityOffset = target.GetComponent<Rigidbody2D>().velocity * optimalHitTime;
                }
                else
                {
                    velocityOffset = Vector2.zero;
                }
                float dvert = oldTargetPos.y + velocityOffset.y - transform.position.y - myPositionOffset.y;
                float grav = gravMultiplier * Physics2D.gravity.y;

                //the initial velocity needed to hit in optimal time, simple kinematics
                float vvert = (dvert / optimalHitTime) - (0.5f * grav * optimalHitTime);
                float vhoriz = (oldTargetPos.x + velocityOffset.x - transform.position.x - myPositionOffset.x) / optimalHitTime;
                Vector2 newPower = new Vector2(vhoriz, vvert);
                if (newPower.magnitude > maxPower)
                {
                    newPower = newPower.normalized * maxPower;
                    vhoriz = newPower.x;
                    vvert = newPower.y;
                }
                float newAngle = Mathf.Atan2(newPower.y, newPower.x) * Mathf.Rad2Deg;
                if (rotateToTarget)
                {
                    transform.eulerAngles = new Vector3(0f, 0f, newAngle);
                }

                //test
                targlinevec.Clear();
                for (float i = 0; i < optimalHitTime; i += optimalHitTime / 7f)
                {
                    Vector3 p0 = new Vector3(transform.position.x + vhoriz * i, transform.position.y + (vvert + 0.5f * grav * i) * i, transform.position.z-1f);
                    targlinevec.Add(p0);
                }
                targetLine.positionCount = targlinevec.Count;
                targetLine.SetPositions(targlinevec.ToArray());
                oldTargetPos = Vector3.Lerp(oldTargetPos, target.transform.position, lerpVal);

                framesReloadTime--;
                if (bowDrawbackBone != null)
                {
                    bowDrawbackBone.localPosition = Vector3.Lerp(drawbackStart, drawbackStop,Mathf.Clamp01((float) (frameReloadTimeMax-framesReloadTime) / (frameReloadTimeMax*0.75f)));
                }
                if (framesReloadTime == 0)
                {
                    framesReloadTime = frameReloadTimeMax;
                    bowDrawbackBone.localPosition = drawbackStart;
                    if (playAudioSource)
                    {
                        AudioSource aso = GetComponent<AudioSource>();
                        aso.Stop();
                        aso.Play();
                    }
                    GameObject newMissile = (GameObject)Instantiate(missile0, transform.position+myPositionOffset, Quaternion.Euler(0f, 0f, newAngle));
                    if (newMissile.GetComponent<Rigidbody2D>() != null)
                    {
                        newMissile.GetComponent<Rigidbody2D>().velocity = newPower;
                    }
                    if (newMissile.GetComponent<Bullets1>() != null)
                    {
                        newMissile.GetComponent<Bullets1>().initVel = newPower; //like this even matters
                    }
                }
            }
            else
            {
                targetLine.positionCount = 0;
                oldTargetPos = transform.position+myPositionOffset;
                framesReloadTime = frameReloadTimeMax;
            }

           
        }

	}
}
