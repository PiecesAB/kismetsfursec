using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatObstacle : GenericBlowMeUp
{

    public enum Temperament
    {
        Normal, Nice, Aggressive
    }

    public enum Direction
    {
        Neutral, FlapLeft, FlapStraight, FlapRight
    }

    public Temperament personality;
    public Direction currDir;
    public GameObject meshObj;
    public double nextChangeTime;
    public bool fleeing;
    public AudioSource panicSound;

    private const float speed = 80f;
    private const float friction = 0.95f;

    private SkinnedMeshRenderer smr;
    private Rigidbody2D rg2;

    public static HashSet<BatObstacle> all = new HashSet<BatObstacle>();

    private void Start()
    {
        all.Add(this);
        fleeing = false;
        smr = meshObj.GetComponent<SkinnedMeshRenderer>();
        rg2 = GetComponent<Rigidbody2D>();
        nextChangeTime = DoubleTime.ScaledTimeSinceLoad;
        currDir = Direction.Neutral;
        NewState();
    }

    protected override void SubclassOnDestroy()
    {
        all.Remove(this);
    }

    public SkinnedMeshRenderer GetRenderer()
    {
        return smr;
    }

    public void OnHurt(PrimEnemyHealth.OnHurtInfo ohi)
    {
        nextChangeTime = DoubleTime.ScaledTimeSinceLoad + 0.6f;
        fleeing = true;
        personality = Temperament.Aggressive;
        Vector3 lpos = transform.InverseTransformPoint(ohi.pos);
        if (lpos.x < 0f)
        {
            currDir = Direction.FlapRight;
        }
        else
        {
            currDir = Direction.FlapLeft;
        }
        panicSound.Stop();
        panicSound.Play();
        
    }

    private Vector2 CalcTarget()
    {
        float lowestSqrDist = 100000000000000f;
        Vector2 lowestTarg = (Vector2)transform.position + (Fakerand.UnitCircle()*48f);

        int lower = (personality == Temperament.Normal) ? 5 : (
                    (personality == Temperament.Aggressive) ? 9 : 0);
                    

        if (Fakerand.Int(0, 10) < lower)
        {
            for (int i = 0; i < LevelInfoContainer.allPlayersInLevel.Count; i++)
            {
                if (LevelInfoContainer.allPlayersInLevel[i])
                {
                    Vector3 mp = LevelInfoContainer.allPlayersInLevel[i].transform.position;
                    float heuristic = (mp - transform.position).sqrMagnitude;
                    if (heuristic < lowestSqrDist)
                    {
                        lowestSqrDist = heuristic;
                        lowestTarg = (Vector2)mp + (Fakerand.UnitCircle() * 32f);
                    }
                }
            }
        }
        return lowestTarg;
    }

    private void NewState()
    {
        fleeing = false;
        Vector2 targPos = CalcTarget();
        targPos -= (Vector2)transform.position;
        if (currDir == Direction.Neutral)
        {
            //horizontal positioning
            if (targPos.x > 16f)
            {
                currDir = Direction.FlapRight;
            }
            else if (targPos.x < -16f)
            {
                currDir = Direction.FlapLeft;
            }
            else
            {
                currDir = Direction.FlapStraight;
            }
        }
        else
        {
            currDir = Direction.Neutral;
            targPos.y = -targPos.y;
        }

        //vertical positioning
        float fact = Fakerand.NormalDist(0.5f, 0.2f, 0.15f, 1f);
        if (targPos.y > 32f)
        {
            fact *= Fakerand.Single(1.4f, 2.0f);
        }
        else if (targPos.y < 32f)
        {
            fact *= Fakerand.Single(0.5f, 0.7f);
        }
        nextChangeTime += fact;
    }

    private void FixedUpdate()
    {
        if (Time.timeScale > 0 && rg2)
        {
            if (smr && smr.isVisible)
            {
                float speed2 = speed * (fleeing ? 1.5f : 1f);
                switch (currDir)
                {
                    case Direction.Neutral:
                        rg2.velocity = new Vector2(Mathf.Floor(rg2.velocity.x * friction), -speed2);
                        break;
                    case Direction.FlapStraight:
                        rg2.velocity = new Vector2(Mathf.Floor(rg2.velocity.x * friction), speed2);
                        break;
                    case Direction.FlapLeft:
                        rg2.velocity = new Vector2(-speed2, speed2);
                        break;
                    case Direction.FlapRight:
                        rg2.velocity = new Vector2(speed2, speed2);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                rg2.velocity = Vector2.zero;
            }
        }
    }

    private void Update()
    {
        if (Time.timeScale > 0 && smr && smr.isVisible)
        {
            

            while (DoubleTime.ScaledTimeSinceLoad >= nextChangeTime)
            {
                NewState();
            }

            if (currDir == Direction.Neutral)
            {
                smr.SetBlendShapeWeight(0, Mathf.MoveTowards(smr.GetBlendShapeWeight(0), 0f, 5f));
            }
            else
            {
                smr.SetBlendShapeWeight(0, (float)DoubleTime.DoublePong(DoubleTime.ScaledTimeSinceLoad * 600.0, 100.0));

                Vector3 rot = meshObj.transform.eulerAngles;
                if (currDir == Direction.FlapRight)
                {
                    meshObj.transform.eulerAngles = new Vector3(rot.x, Mathf.MoveTowardsAngle(rot.y, 135f, 5f), rot.z);
                }
                else if (currDir == Direction.FlapLeft)
                {
                    meshObj.transform.eulerAngles = new Vector3(rot.x, Mathf.MoveTowardsAngle(rot.y, -135f, 5f), rot.z);
                }
            } 
        }
    }
}
