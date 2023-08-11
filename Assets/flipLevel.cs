using UnityEngine;
using System.Collections;

public class flipLevel : MonoBehaviour {

    public Coroutine WaitForRealSeconds(float time)
    {
        return StartCoroutine(WaitForRealSecondsImpl(time));
    }

    private IEnumerator WaitForRealSecondsImpl(float time)
    {
        double startTime = DoubleTime.UnscaledTimeRunning;
        while (DoubleTime.UnscaledTimeRunning - startTime < time)
            yield return 1;
    }

    public GameObject rotateSceneTest;
    public GameObject[] players;
    public Camera mainCamera;
    public AudioClip flipsound;
    public AudioClip doneflipsound;

    double time1;
    bool flipping;
    float y;
    float d;
    float t;
    Vector3 newPos;
    Vector3 oldPos;
    bool a;

    Vector3 oldRot;
    bool flipt = false;

	// Use this for initialization
	void Start () {
        flipping = false;
        a = true;
	}
	
    void OnTriggerEnter2D(Collider2D c)
    {
        if (Time.timeScale >= 0 && c.gameObject.CompareTag("Player"))
        {
            time1 = DoubleTime.UnscaledTimeRunning; //add so you can't pause the game
            flipping = true;
            AudioSource asc = GetComponent<AudioSource>();
            asc.clip = flipsound;
            asc.volume = 0.2f;
            asc.Play();
            Destroy(GetComponent<CircleCollider2D>());
            GetComponent<SpriteRenderer>().sprite = null;
            Utilities.canPauseGame = false;
            Utilities.canUseInventory = false;
            t = Time.timeScale;
            Time.timeScale = 0;
            oldRot = rotateSceneTest.transform.localEulerAngles;

            float avg = (mainCamera.GetComponent<FollowThePlayer>().cameraBounds.w + mainCamera.GetComponent<FollowThePlayer>().cameraBounds.y) / 2f;
            oldPos = mainCamera.transform.position;
            float ydif = oldPos.y - avg;
            ydif *= -1f;
            newPos = new Vector3(oldPos.x, avg + ydif, oldPos.z);


            y = mainCamera.gameObject.transform.position.y;
            //mainCamera.gameObject.GetComponent<FollowThePlayer>().followCameraBounds = false;
            d = mainCamera.gameObject.GetComponent<FollowThePlayer>().dampTime;
            mainCamera.gameObject.GetComponent<FollowThePlayer>().dampTime = 0;
            foreach (GameObject g in players)
            {
                g.transform.parent = rotateSceneTest.transform;
            }
            foreach (Collider2D col in FindObjectsOfType<Collider2D>())
            {
                col.enabled = false;
            }
        }
    }

    void End()
    {
        
        

        Vector4 v4 = mainCamera.gameObject.GetComponent<FollowThePlayer>().cameraBounds;
        foreach (GameObject g in players)
        {
            g.transform.parent = rotateSceneTest.transform.parent;
        }
        //mainCamera.gameObject.GetComponent<FollowThePlayer>().cameraBounds = new Vector4(v4.x, -v4.y, v4.z, -v4.w);
        //mainCamera.gameObject.GetComponent<FollowThePlayer>().followCameraBounds = true;
        Time.timeScale = t;
        Utilities.canPauseGame = true;
        Utilities.canUseInventory = true;
       mainCamera.gameObject.GetComponent<FollowThePlayer>().dampTime = d;
        foreach (circleSpinner c in FindObjectsOfType<circleSpinner>())
        {
            c.inverted *= -1;
        }
        foreach (BoostArrow c in FindObjectsOfType<BoostArrow>())
        {
            c.flipped *= -1;
        }
        foreach (Collider2D col in FindObjectsOfType<Collider2D>())
        {
            col.enabled = true;
        }

        Destroy(gameObject,1f);
    }
    

	// Update is called once per frame
	void Update () {

        if (flipping)
        {
            double diff = (DoubleTime.UnscaledTimeRunning - time1) * 1.7f;
            if (diff < 1f)
            {
                diff = -System.Math.Acos((diff - 1f) + Mathf.PI) / (Mathf.PI / 2f);
            }
            Vector3 center = new Vector3((mainCamera.GetComponent<FollowThePlayer>().cameraBounds.x + mainCamera.GetComponent<FollowThePlayer>().cameraBounds.z) / 2f, (mainCamera.GetComponent<FollowThePlayer>().cameraBounds.w + mainCamera.GetComponent<FollowThePlayer>().cameraBounds.y) / 2f, 0);

            if (diff < 2f)
            {
                // Vector3 p = mainCamera.gameObject.transform.position;
                //mainCamera.gameObject.transform.position = new Vector3(p.x, y * (1f - diff), p.z);

                // Vector3 v = rotateSceneTest.transform.localScale;
                rotateSceneTest.transform.position = Vector3.zero;
                rotateSceneTest.transform.localEulerAngles = oldRot;
                rotateSceneTest.transform.RotateAround(center, Vector3.right, (float)(diff * 90f));
                mainCamera.transform.position = Vector3.Lerp(oldPos, newPos, (float)diff / 2f);
            }
            else if (a)
            {


                
                if (diff >= 1f && !flipt)
                {
                    foreach (GameObject g in players)
                    {
                        g.transform.localScale = new Vector3(g.transform.localScale.x, -g.transform.localScale.y, g.transform.localScale.z);
                    }
                    flipt = true;
                }

                rotateSceneTest.transform.position = Vector3.zero;
                rotateSceneTest.transform.localEulerAngles = oldRot;
                rotateSceneTest.transform.RotateAround(center, Vector3.right, 180f);

                rotateSceneTest.GetComponent<TestScriptRotateScene>().RecenterAll(-rotateSceneTest.transform.position);

                mainCamera.transform.position = newPos;

                AudioSource asc = GetComponent<AudioSource>();
                asc.Stop();
                asc.clip = doneflipsound;
                asc.volume *= 2.6f;
                if (!asc.isPlaying)
                asc.Play();
                a = false;
                End();
                //if (diff >= 1f && mainCamera.transform.localScale.y >= 0f)
                //{
                //    mainCamera.transform.localEulerAngles = new Vector3(mainCamera.transform.localEulerAngles.x, mainCamera.transform.localEulerAngles.y, mainCamera.transform.localEulerAngles.z);
                //}
            }
        }
       /* else
        {
            //Vector3 p = mainCamera.gameObject.transform.position;
            //mainCamera.gameObject.transform.position = new Vector3(p.x, -y, p.z);
            //rotateSceneTest.transform.localScale = new Vector3(v.x, -1f, v.z);

            
        }*/
       

        
	}
}
