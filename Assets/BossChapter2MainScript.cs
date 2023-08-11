using UnityEngine;
using System.Collections;

public class BossChapter2MainScript : MonoBehaviour {

    public GameObject basicLaser;
    public GameObject playerLaser1;
    public GameObject playerLaserRotating;
    public GameObject followLaser;
    public GameObject finalLaser;
    public GameObject healthPack;
    public DefaultTalkingEngine[] speeches;
    public BossChp2SpikeCrucherScript crusherScript;
    public float numberOfSecondsEachAttack;
    public float minX;
    public float maxX;

    private float oldBossHP;

/*
    public IEnumerator LaserAttack(float delay)
    {
        yield return new WaitForSeconds(delay);
        int attack = GetComponent<BossStats>().currentAttackState;
        float aTime = (float)DoubleTime.ScaledTimeSinceLoad;
        float tempBossHP = GetComponent<BossStats>().HP;

        if (attack == 1)
        {
            while (aTime + numberOfSecondsEachAttack > DoubleTime.ScaledTimeSinceLoad)
            {
                float rand1 = Mathf.Floor(Random.Range(minX, maxX) / 16) * 16;

                Instantiate(basicLaser, new Vector3(rand1,-1120,0), Quaternion.identity);
                yield return new WaitForSeconds(0.4f);
            }
        }
        if (attack == 2)
        {
            while (aTime + numberOfSecondsEachAttack > DoubleTime.ScaledTimeSinceLoad)
            {
                float rand1 = Mathf.Floor(Random.Range(minX, maxX-32) / 16) * 16;

                Instantiate(basicLaser, new Vector3(rand1, -1120, 0), Quaternion.identity);
                Instantiate(basicLaser, new Vector3(rand1+32, -1120, 0), Quaternion.identity);
                yield return new WaitForSeconds(0.25f);
                if (Random.value > 0.75f)
                {
                    rand1 = Mathf.Floor(GameObject.FindGameObjectWithTag("Player").transform.position.x / 16) * 16;
                    Instantiate(playerLaser1, new Vector3(rand1, -1120, 0), Quaternion.identity);
                }
                yield return new WaitForSeconds(0.25f);
            }
        }
        if (attack == 3)
        {
            while (aTime + numberOfSecondsEachAttack > DoubleTime.ScaledTimeSinceLoad)
            {
                float rand1 = Mathf.Floor(Random.Range(minX+32, maxX - 32) / 16) * 16;

                Instantiate(basicLaser, new Vector3(rand1 - 32, -1120, 0), Quaternion.identity);
                Instantiate(basicLaser, new Vector3(rand1, -1120, 0), Quaternion.identity);
                Instantiate(basicLaser, new Vector3(rand1 + 32, -1120, 0), Quaternion.identity);
                yield return new WaitForSeconds(0.3f);
                if (Random.value > 0.5f)
                {
                    rand1 = Mathf.Floor(GameObject.FindGameObjectWithTag("Player").transform.position.x / 16) * 16;
                    Instantiate(playerLaser1, new Vector3(rand1, -1120, 0), Quaternion.identity);
                }
                yield return new WaitForSeconds(0.3f);
            }
        }




        if (attack == 4)
        {
            float tempWTime = 0.25f;
            while (aTime + numberOfSecondsEachAttack > DoubleTime.ScaledTimeSinceLoad)
            {
                float rand1 = Mathf.Floor(Random.Range(minX, maxX) / 16) * 16;

                Instantiate(basicLaser, new Vector3(rand1, -1120, 0), Quaternion.identity);
                yield return new WaitForSeconds(tempWTime);
                tempWTime *= 0.98f;
            }
        }

        if (attack == 5)
        {
            float tempWTime = 1.2f;
            while (aTime + numberOfSecondsEachAttack > DoubleTime.ScaledTimeSinceLoad)
            {
                Instantiate(playerLaserRotating, GameObject.FindGameObjectWithTag("Player").transform.position, Quaternion.AngleAxis(Random.value * 180, Vector3.forward));
                Instantiate(playerLaserRotating, GameObject.FindGameObjectWithTag("Player").transform.position, Quaternion.AngleAxis(Random.value * 180, Vector3.forward));
                Instantiate(playerLaserRotating, GameObject.FindGameObjectWithTag("Player").transform.position, Quaternion.AngleAxis(Random.value * 180, Vector3.forward));
                yield return new WaitForSeconds(tempWTime);
                tempWTime *= 0.93f;

            }
        }

        if (attack == 6)
        {
            while (aTime + numberOfSecondsEachAttack > DoubleTime.ScaledTimeSinceLoad)
            {
                if (Random.value < 0.69f)
                {
                    float rand1 = Mathf.Floor(GameObject.FindGameObjectWithTag("Player").transform.position.x / 16) * 16;
                    Instantiate(playerLaser1, new Vector3(rand1, -1120, 0), Quaternion.identity);
                    yield return new WaitForSeconds(0.25f);
                }
                else
                {
                    Instantiate(playerLaserRotating, GameObject.FindGameObjectWithTag("Player").transform.position, Quaternion.AngleAxis(Random.value * 180, Vector3.forward));
                    Instantiate(playerLaserRotating, GameObject.FindGameObjectWithTag("Player").transform.position, Quaternion.AngleAxis(Random.value * 180, Vector3.forward));
                    Instantiate(playerLaserRotating, GameObject.FindGameObjectWithTag("Player").transform.position, Quaternion.AngleAxis(Random.value * 180, Vector3.forward));
                    yield return new WaitForSeconds(0.65f);
                }

            }
        }


        if (attack == 7)
        {
            float tempWTime = 0.2f;
            while (aTime + numberOfSecondsEachAttack > DoubleTime.ScaledTimeSinceLoad)
            {
                float rand1 = Mathf.Floor(Random.Range(minX, maxX) / 16) * 16;

                Instantiate(basicLaser, new Vector3(rand1, -1120, 0), Quaternion.identity);
                float lolNew = Random.value;
                if (lolNew > 0.75f)
                {
                    if (lolNew > 0.875f)
                    {
                        rand1 = Mathf.Floor(GameObject.FindGameObjectWithTag("Player").transform.position.x / 16) * 16;
                        Instantiate(playerLaser1, new Vector3(rand1, -1120, 0), Quaternion.identity);
                    }
                    else
                    {
                        Instantiate(playerLaserRotating, GameObject.FindGameObjectWithTag("Player").transform.position, Quaternion.AngleAxis(Random.value * 180, Vector3.forward));

                    }
                }
                yield return new WaitForSeconds(tempWTime);
                tempWTime *= 0.989f;

            }
        }

        if (attack == 8)
        {
            while (aTime + numberOfSecondsEachAttack > DoubleTime.ScaledTimeSinceLoad)
            {
                float lolNew = Random.value;

                if (lolNew > 0.5f)
                {
                    Instantiate(finalLaser, GameObject.FindGameObjectWithTag("Player").transform.position, Quaternion.AngleAxis(Random.value * 180, Vector3.forward));
                    yield return new WaitForSeconds(0.8f);
                }
                if (lolNew <= 0.5f)
                {
                    float rand1 = Mathf.Floor(Random.Range(minX, maxX) / 16) * 16;
                    Instantiate(followLaser, new Vector3(rand1, -1120, 0), Quaternion.identity);
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        if (attack == 9)
        {
            float tempWTime = 0.5f;
            while (aTime + numberOfSecondsEachAttack > DoubleTime.ScaledTimeSinceLoad)
            {
                int lolNew = (int)Random.Range(0, 4.99f);

                float rand1 = 0;
                switch (lolNew)
                {
                    case 0:
                        rand1 = Mathf.Floor(Random.Range(minX, maxX) / 16) * 16;

                        Instantiate(basicLaser, new Vector3(rand1, -1120, 0), Quaternion.identity);
                        break;

                    case 1:
                        Instantiate(playerLaserRotating, GameObject.FindGameObjectWithTag("Player").transform.position, Quaternion.AngleAxis(Random.value * 180, Vector3.forward));
                        break;

                    case 2:
                        rand1 = Mathf.Floor(Random.Range(minX, maxX) / 16) * 16;
                        Instantiate(followLaser, new Vector3(rand1, -1120, 0), Quaternion.identity);

                        break;
                    case 3:
                        Instantiate(finalLaser, GameObject.FindGameObjectWithTag("Player").transform.position, Quaternion.AngleAxis(Random.value * 180, Vector3.forward));
                        break;
                    case 4:
                        rand1 = Mathf.Floor(GameObject.FindGameObjectWithTag("Player").transform.position.x / 16) * 16;
                        Instantiate(playerLaser1, new Vector3(rand1, -1120, 0), Quaternion.identity);
                        break;
                    default:
                        rand1 = Mathf.Floor(Random.Range(minX, maxX) / 16) * 16;

                        Instantiate(basicLaser, new Vector3(rand1, -1120, 0), Quaternion.identity);
                        break;
                }
                yield return new WaitForSeconds(tempWTime);
                tempWTime *= 0.98f;
            }

            while (aTime + (numberOfSecondsEachAttack*2.5f) > DoubleTime.ScaledTimeSinceLoad)
            {
                int lolNew = (int)Random.Range(0, 3.99f);

                switch (lolNew)
                {
                    case 0:
                        for (int i = (int)minX; i < maxX -192; i+=16)
                        {
                            Instantiate(basicLaser, new Vector3(i, -1120, 0), Quaternion.identity);
                            yield return new WaitForSeconds(0.035f);
                        }
                        yield return new WaitForSeconds(0.5f);
                        for (int i = (int)maxX; i > minX + 176; i-=16)
                        {
                            Instantiate(basicLaser, new Vector3(i, -1120, 0), Quaternion.identity);
                            yield return new WaitForSeconds(0.035f);
                        }
                        yield return new WaitForSeconds(0.5f);
                        break;

                    case 1:
                        for (int j = 0; j < 2; j++)
                        {
                            for (int i = (int)minX; i < maxX; i += 48)
                            {
                                Instantiate(playerLaser1, new Vector3(i, -1120, 0), Quaternion.identity);
                            }
                            yield return new WaitForSeconds(0.6f);
                            for (int i = (int)minX - 24; i < maxX; i += 48)
                            {
                                Instantiate(playerLaser1, new Vector3(i, -1120, 0), Quaternion.identity);
                            }
                            yield return new WaitForSeconds(0.6f);
                        }

                        Instantiate(playerLaserRotating, GameObject.FindGameObjectWithTag("Player").transform.position, Quaternion.AngleAxis(Random.value * 180, Vector3.forward));
                        break;

                    case 2:
                        for (int j = 0; j < 120; j+=10)
                        {
                            Instantiate(playerLaserRotating, GameObject.FindGameObjectWithTag("Player").transform.position, Quaternion.AngleAxis(j, Vector3.forward));
                            yield return new WaitForSeconds(0.03f);
                        }
                        yield return new WaitForSeconds(0.4f);
                        for (int j = 360; j > 240; j-= 10)
                        {
                            Instantiate(playerLaserRotating, GameObject.FindGameObjectWithTag("Player").transform.position, Quaternion.AngleAxis(j, Vector3.forward));
                            yield return new WaitForSeconds(0.03f);
                        }
                        break;
                    case 3:
                        Instantiate(finalLaser, GameObject.FindGameObjectWithTag("Player").transform.position, Quaternion.AngleAxis(Random.value * 180, Vector3.forward));
                        Instantiate(finalLaser, GameObject.FindGameObjectWithTag("Player").transform.position, Quaternion.AngleAxis(Random.value * 180, Vector3.forward));
                        Instantiate(finalLaser, GameObject.FindGameObjectWithTag("Player").transform.position, Quaternion.AngleAxis(Random.value * 180, Vector3.forward));
                        yield return new WaitForSeconds(0.1f);
                        Instantiate(finalLaser, GameObject.FindGameObjectWithTag("Player").transform.position, Quaternion.AngleAxis(Random.value * 180, Vector3.forward));
                        Instantiate(finalLaser, GameObject.FindGameObjectWithTag("Player").transform.position, Quaternion.AngleAxis(Random.value * 180, Vector3.forward));
                        Instantiate(finalLaser, GameObject.FindGameObjectWithTag("Player").transform.position, Quaternion.AngleAxis(Random.value * 180, Vector3.forward));
                        yield return new WaitForSeconds(1.4f);
                        break;
                    default:
                        break;
                }



                yield return new WaitForSeconds(0.4f);
            }


        }


        yield return new WaitForSeconds(0.5f);
        crusherScript.TurnFFOff();
        crusherScript.DropSpike();

        yield return new WaitForSeconds(9f);
        if (tempBossHP == GetComponent<BossStats>().HP)
        {
            crusherScript.TurnFFOn();
            StartCoroutine(LaserAttack(1f));
        }

    }

    public void OnBossLowerHP()
    {
        GetComponent<BossStats>().currentAttackState += 1;
        if (GetComponent<BossStats>().currentAttackState == 4)
        {
            crusherScript.delay = 1.1f;
            Instantiate(healthPack, new Vector3(408, -1244, 0), Quaternion.identity);
        }
        if (GetComponent<BossStats>().currentAttackState == 7)
        {
            crusherScript.delay = 0.6f;
            Instantiate(healthPack, new Vector3(408-64, -1244, 0), Quaternion.identity);
            Instantiate(healthPack, new Vector3(408+64, -1244, 0), Quaternion.identity);
        }
        if (GetComponent<BossStats>().currentAttackState == 9)
        {
            crusherScript.delay = 3f;
            speeches[1].Trigger();
        }
        crusherScript.TurnFFOn();
        StartCoroutine(LaserAttack(2f));
    }


	// Use this for initialization
	void Start () {
        oldBossHP = GetComponent<BossStats>().HP;
        StartCoroutine(LaserAttack(0f));
	}
	
	// Update is called once per frame
	void Update () {
        if (oldBossHP > GetComponent<BossStats>().HP)
        {
            OnBossLowerHP();
        }
        oldBossHP = GetComponent<BossStats>().HP;
    }
    */
}
