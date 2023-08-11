using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BeforeLevelDisplay : MonoBehaviour {

    bool halt = false;
    bool donePanning;
    public Transform panCamera;

    public Vector3[] cameraWaypoints;
    public float[] cameraTimes;

    public GameObject banner;
    public GameObject rating;
    public AudioClip levelBeginChime;

    private double a;
    private int b;
    // Use this for initialization
    void Start () {
        if (!gameObject.activeSelf)
        {
            Destroy(gameObject);
        }

        halt = false;
        donePanning = false;
        GetComponent<Image>().enabled = false;
        banner.SetActive(false);
        rating.SetActive(true);
        if (panCamera.GetComponent<FollowThePlayer>())
        {
            panCamera.GetComponent<FollowThePlayer>().enabled = false;
        }
        b = 0;
        if (cameraTimes.Length >= 1)
        {
            a = DoubleTime.UnscaledTimeRunning;
        }
    }

    void TurnOn()
    {
        donePanning = true;
        GetComponent<Image>().enabled = true;
        banner.SetActive(true);
        rating.SetActive(true);
        if (cameraWaypoints.Length >= 1)
        {
            panCamera.position = cameraWaypoints[cameraWaypoints.Length - 1];
            if (panCamera.GetComponent<FollowThePlayer>() != null)
            {
                panCamera.GetComponent<FollowThePlayer>().enabled = true;
            }
        }
    }


	// Update is called once per frame
	void Update () {
        if (!halt)
        {
            Time.timeScale = 0;
            Utilities.canPauseGame = false;
            Utilities.canUseInventory = false;
            halt = true;
        }
        if (!donePanning)
        {
            if (cameraTimes.Length >= 1)
            {

                if (b >= cameraWaypoints.Length-1 && DoubleTime.UnscaledTimeRunning - a >= cameraTimes[b-1])
                {
                    TurnOn();
                }
                else
                {
                    if (b <= cameraTimes.Length-1 && DoubleTime.UnscaledTimeRunning - a >= cameraTimes[b])
                    {
                        b++;
                        if (b <= cameraTimes.Length-1)
                        {
                            a = DoubleTime.UnscaledTimeRunning;
                        }
                    }
                }
                if (b <= cameraWaypoints.Length - 2)
                {
                   panCamera.position = Vector3.Lerp(cameraWaypoints[b], cameraWaypoints[b+1], (float)(DoubleTime.UnscaledTimeRunning - a) / cameraTimes[b]);
                }
            }
            else
            {
                TurnOn();
            }
        }
        if (halt && (GetComponent<Encontrolmentation>().flags & 16UL) == 16UL && (GetComponent<Encontrolmentation>().currentState & 16UL) == 16UL)
        {
            if (!donePanning)
            {
                TurnOn();
            }
            else
            {
                Time.timeScale = 1;
                Utilities.canPauseGame = true;
                Utilities.canUseInventory = true;
                if (panCamera.GetComponent<FollowThePlayer>() != null)
                {
                    panCamera.GetComponent<FollowThePlayer>().enabled = true;
                }
                AudioSource.PlayClipAtPoint(levelBeginChime, panCamera.position,Mathf.Min(1f,Mathf.Pow(10f,PlayerPrefs.GetFloat("MasterVolume",0f)/100f)));
                Destroy(gameObject);
            }
        }

    }
}
