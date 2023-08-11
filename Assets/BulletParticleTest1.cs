using UnityEngine;
using System.Collections;

[System.Obsolete("Use this only for compatibility/ transferring data.", false)]
public class BulletParticleTest1 : MonoBehaviour {

    public enum Preset
    {
        None,Circle,SinWave,CircleFan, Wall, AlternatingWall, Crumble,Advanced
    }

    public Preset preset;
    public float delayBeforeStart;
    public float damage;
    public float repeatAttackTime;

    public Transform playerTransform;

    public int bulletNumber;
    public int bulletNumber2;
    public Vector2 shapeSize;
    public float generalVelocity;

    public float holeMaybe;
    public Vector2 range1;
    public bool alignToCenter;

    public float vibrateOnCollide;

    public bool noRandomDir = false;
    public bool faceInMovementDir = true;

    public double stopTime;

    //[HideInInspector]
    public double timeSinceLastShot = 0.0;
    
    private bool mvtStart;

    private double t1;

    // Use this for initialization
    IEnumerator Start () {
        // commented out due to obsolescence
        yield break;
        /*
        mvtStart = false;
        t1 = DoubleTime.ScaledTimeSinceLoad + delayBeforeStart;
        if (stopTime == 0)
        {
            stopTime = double.PositiveInfinity;
        }
        else
        {
            stopTime += t1;
        }
        yield return new WaitForSeconds(delayBeforeStart);
        switch (preset)
         {
             case Preset.SinWave:
                 StartCoroutine(SinWave());
                 StartCoroutine(SinMvt());
                 break;
             case Preset.AlternatingWall:
                 StartCoroutine(AltWall());
                 //StartCoroutine(Accel1());
                 break;
             case Preset.Circle:
                 StartCoroutine(Circle());
                 break;
             case Preset.CircleFan:
                 StartCoroutine(FanCircle());
                 StartCoroutine(FanCircleMvt());
                 break;
         }
        timeSinceLastShot = 0.0;
        */
    }

    // commented due to obsolescence
    /*void OnParticleCollision(GameObject other)
    {
        if (vibrateOnCollide > 0f)
        {
            FindObjectOfType<FollowThePlayer>().vibSpeed += vibrateOnCollide;
        }

        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<KHealth>())
            {
                other.GetComponent<KHealth>().ChangeHealth(-(damage/7f)*other.GetComponent<BasicMove>().Damage,"energy bullet");
                //other.GetComponent<BasicMove>().AddBlood(other.transform.position, Quaternion.AngleAxis(Fakerand.Single() * 360f, Vector3.forward));
                other.GetComponent<BasicMove>().AddBurn(other.transform.position+new Vector3(Fakerand.Int(-7, 8), Fakerand.Int(-15, 16)), transform.eulerAngles.z+90);
            }
        }
    }

    public IEnumerator SinMvt()
    {
        while (true)
        {
            if (mvtStart)
            {
                ParticleSystem.Particle[] p = new ParticleSystem.Particle[GetComponent<ParticleSystem>().main.maxParticles];
                int ab = GetComponent<ParticleSystem>().GetParticles(p);

                for (int i = 0; i < ab; i++)
                {
                    p[i].velocity = new Vector3(p[i].velocity.x, 100f * (float)System.Math.Cos(p[i].position.x * Mathf.PI * 0.01f + DoubleTime.ScaledTimeSinceLoad * Mathf.PI) + generalVelocity);
                }

                GetComponent<ParticleSystem>().SetParticles(p, GetComponent<ParticleSystem>().main.maxParticles);
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator Circle()
    {
        while (DoubleTime.ScaledTimeSinceLoad < stopTime)
        {
            float rand = Fakerand.Single(0f, Mathf.PI * 2f);
            if (noRandomDir)
            {
                rand = transform.eulerAngles.z * Mathf.Deg2Rad;
            }
            for (float i = 0; i < bulletNumber; i++)
            {
                float z = ((i / ((float)bulletNumber))*2f*Mathf.PI + rand)%(Mathf.PI*2f);
                ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
                p.position = transform.position;
                p.velocity = new Vector3(Mathf.Cos(z)*shapeSize.x, Mathf.Sin(z) *shapeSize.y);
                
                if (noRandomDir)
                {
                    p.rotation = -transform.eulerAngles.z;
                }
                if (faceInMovementDir)
                {
                    p.rotation = -z * Mathf.Rad2Deg;
                }

                GetComponent<ParticleSystem>().Emit(p, 1);
            }
            timeSinceLastShot = 0.0;
            t1 += repeatAttackTime;
            yield return new WaitUntil(() => (DoubleTime.ScaledTimeSinceLoad >= t1));
        }
    }

    public IEnumerator SinWave()
    {
        while (true)
        {
            float rand = Fakerand.Single(-10f, 10f);
            for (float i = 0; i <= bulletNumber; i++)
            {
                ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
                p.position = transform.position + new Vector3(shapeSize.x * (i - (bulletNumber / 2f)) / (bulletNumber * 2f) + rand, 0, 0);
                p.velocity = new Vector3(0f, generalVelocity);

                GetComponent<ParticleSystem>().Emit(p, 1);
            }
            mvtStart = true;
            t1 += repeatAttackTime;
            yield return new WaitUntil(() => (DoubleTime.ScaledTimeSinceLoad >= t1));
        }
    }

    public IEnumerator FanCircleMvt()
    {
        while (true)
        {
            if (mvtStart)
            {
                ParticleSystem.Particle[] p = new ParticleSystem.Particle[GetComponent<ParticleSystem>().main.maxParticles];
                int ab = GetComponent<ParticleSystem>().GetParticles(p);

                for (int i = 0; i < ab; i ++)
                {
                    int k = i % bulletNumber2;

                        float j = ((Mathf.Floor(i/bulletNumber2)*bulletNumber2)/bulletNumber)* 2f * Mathf.PI + 0.1f;

                        Vector2 pos = p[i].position - transform.position;

                        float nowAngle = (Mathf.Atan2(-pos.y, -pos.x) + Mathf.PI)%(Mathf.PI*2f);
                        float lerpAngle = Mathf.Lerp(nowAngle % (Mathf.PI * 2f), j, 0.07f);

                        float s = (((float)k) / ((float)bulletNumber2));
                        p[i].position = transform.position + new Vector3(s * shapeSize.x * Mathf.Cos(lerpAngle), s * shapeSize.y * Mathf.Sin(lerpAngle));

                    p[i].position += (0.5f / (p[i].remainingLifetime + 0.04f)) * Fakerand.UnitSphere();


                }

                GetComponent<ParticleSystem>().SetParticles(p, GetComponent<ParticleSystem>().main.maxParticles);
                yield return null;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator FanCircle()
    {
        while (true)
        {
            for (float i = 0; i <= bulletNumber; i++)
            {
                float j = i % bulletNumber2;
                ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
                p.position = transform.position + new Vector3((j / ((float)bulletNumber2)) * shapeSize.x, 0f, 0);
                p.velocity = new Vector3(0f, 0f);

                GetComponent<ParticleSystem>().Emit(p, 1);
            }
            mvtStart = true;
            t1 += repeatAttackTime;
            yield return new WaitUntil(() => (DoubleTime.ScaledTimeSinceLoad >= t1));
        }
        yield return new WaitForEndOfFrame();
    }

    public IEnumerator AltWall()
    {
        float neg = 1f;
        while (true)
        {
            float rand = Fakerand.Single(range1.x, range1.y);
            for (float i = 0; i <= bulletNumber; i++)
            {
                float zz = shapeSize.y * (i - (bulletNumber / 2f)) / (bulletNumber * 2f);
                if (System.Math.Abs(zz - rand) >= holeMaybe)
                {
                    ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
                    p.position = transform.position + new Vector3(-neg*shapeSize.x / 2f, shapeSize.y * (i - (bulletNumber / 2f)) / (bulletNumber * 2f), 0);
                    p.velocity = new Vector3(neg*generalVelocity, 0f);

                    GetComponent<ParticleSystem>().Emit(p, 1);
                }
            }
            t1 += repeatAttackTime;
            yield return new WaitUntil(() => (DoubleTime.ScaledTimeSinceLoad >= t1));
            neg *= -1f;
        }
    }

    public IEnumerator lol()
    {
        float hi = 0;
        while (true)
        {
            for (float i = 0; i < 6; i++)
            {
                float j = (i / 2) * Mathf.PI + hi;
                ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
                p.position = transform.position+new Vector3(Fakerand.Single(-320f,320f),0);
                p.startLifetime = 5f;
                p.startSize = 90f;

                GetComponent<ParticleSystem>().Emit(p, 1);
            }
            t1 += 0.1f;
            yield return new WaitUntil(() => (DoubleTime.ScaledTimeSinceLoad >= t1));
            hi = (hi + 0.1f)%Mathf.PI;
        }
    }

    void Update () {
        if (alignToCenter && GetComponent<ParticleSystem>() != null)
        {
            ParticleSystem.Particle[] p = new ParticleSystem.Particle[GetComponent<ParticleSystem>().main.maxParticles];
            int ab = GetComponent<ParticleSystem>().GetParticles(p);
            
            for (int i=0; i < p.Length; i++)
            {
                Vector2 dir = p[i].position;
                p[i].rotation = Mathf.Atan2(dir.y, -dir.x)*Mathf.Rad2Deg;
            }
            GetComponent<ParticleSystem>().SetParticles(p, GetComponent<ParticleSystem>().main.maxParticles);
        }

        if (Time.timeScale > 0)
        {
            timeSinceLastShot += Time.deltaTime;
        }
	}*/
}
