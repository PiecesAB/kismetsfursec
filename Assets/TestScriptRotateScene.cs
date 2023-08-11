using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestScriptRotateScene : MonoBehaviour {

    public List<GameObject> doNotSpinThese;
    public List<GameObject> spinTheseButInPlace;
    public GameObject mainPlayer;
    public float goalRotation;
    public float remainingRotation;
    public bool freeAngle;
    public bool rotateAroundCamera = false;

    private int parentTimer = 0;

	// Use this for initialization
    void Start()
    {
        freeAngle = false;
        goalRotation = transform.eulerAngles.z;
        parentTimer = 60;
    }

    public void RecenterAll(Vector3 offset)
    {
        foreach (Transform t in transform)
        {
            //t.localPosition -= offset;
            Vector3 v0 = t.localPosition;
            if (/*System.Math.Abs((v0.x + v0.y + v0.z) % 1f) > 0.01f*/true)
            {
                Vector3 v1 = new Vector3(Mathf.Round(v0.x), Mathf.Round(v0.y), Mathf.Round(v0.z));
                t.localPosition = v1;
            }
        }
        //transform.position += offset;
    }

	void Update () {
        if (Time.timeScale == 0) { return; }

        /* goalRotation=goalRotation % 360;
         transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z % 360);
         if (Camera.main.GetComponent<FollowThePlayer>())
         {
            //transform.RotateAround(Camera.main.GetComponent<FollowThePlayer>().refPlayer.transform.position,Vector3.up,1);
         }*/

        ++parentTimer;
        if (parentTimer >= 60)
        {
            foreach (GameObject i in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                bool putThisIn = true;
                Vector3 scale = i.transform.localScale;
                if (doNotSpinThese.Contains(i) || spinTheseButInPlace.Contains(i) || mainPlayer == i)
                {
                    putThisIn = false;
                }

                if (putThisIn)
                {
                    i.transform.SetParent(transform);
                    i.transform.localScale = scale;
                }
            }

            parentTimer = 0;
        }

        if (goalRotation != 0f)
        {
            //camera is unlocked...
            //FindObjectOfType<FollowThePlayer>().followCameraBounds = false;
        }

        float myAngle = transform.eulerAngles.z;
        if (!freeAngle) { goalRotation = Mathf.Round(goalRotation / 15f) * 15f; }
        remainingRotation = goalRotation - myAngle;

        if (System.Math.Abs(remainingRotation) > 0.000001f)
        {
            remainingRotation = Mathf.Repeat(remainingRotation + 180f, 360f) - 180f;
            //remainingRotation = (Mathf.Round((myAngle + remainingRotation) / 15f) * 15f) - myAngle;

            Vector3 center = mainPlayer.transform.position;
            if (rotateAroundCamera) { center = Camera.main.transform.position; }

            if (System.Math.Abs(remainingRotation) > 1f)
            {
                transform.RotateAround(center, Vector3.forward, 0.4f * remainingRotation);
                FindObjectOfType<FollowThePlayer>().enabled = false;
                RecenterAll(transform.position);
            }
            else
            {
                transform.RotateAround(center, Vector3.forward, remainingRotation);
                FindObjectOfType<FollowThePlayer>().enabled = true;
                RecenterAll(transform.position);
            }
        }
        else
        {
            freeAngle = false;
        }

        foreach (GameObject g in spinTheseButInPlace)
        {
            g.transform.localEulerAngles = -transform.localEulerAngles;
        }

        /*if (System.Math.Abs((goalRotation) - (transform.eulerAngles.z)) > 1f)
        {
            print(System.Math.Abs((goalRotation % 360) - (transform.eulerAngles.z)));
            float z = transform.eulerAngles.z;
            float num = (goalRotation > z) ? (z + 1) : (z - 1);

            Vector3 newRot = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, num);
            transform.eulerAngles = newRot;
        }
        else
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, goalRotation%360);
        }

        if (transform.eulerAngles.z != oldZRotation)
        {
            float change = transform.eulerAngles.z - oldZRotation;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,oldZRotation);
            transform.RotateAround(mainPlayer.transform.position, Vector3.forward, change);
        }
        oldZRotation = transform.eulerAngles.z; */
    }
	
	// Update is called once per frame
}
