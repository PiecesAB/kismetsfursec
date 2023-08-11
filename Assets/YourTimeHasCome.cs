using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Threading;

public class YourTimeHasCome : MonoBehaviour {

    public float startTime;
    private Transform par;
    public DefaultTalkingEngine dialogueDone;
    private bool dbDone;
    public RectTransform r;

    public Coroutine WaitForRealSeconds(float time)
    {
        return StartCoroutine(WaitForRealSecondsImpl(time));
    }

    private IEnumerator WaitForRealSecondsImpl(float time)
    {
        float startTime = (float)DoubleTime.UnscaledTimeRunning;
        while (DoubleTime.UnscaledTimeRunning - startTime < time)
            yield return 1;
    }


    public IEnumerator SineMove(float origY,float v)
    {
        while (DoubleTime.ScaledTimeSinceLoad < 0.1f && Time.timeScale == 0)
        {
            r.position = new Vector3(r.position.x, origY + v * (float)System.Math.Sin(DoubleTime.UnscaledTimeRunning * 4f), r.position.z);
            yield return WaitForRealSeconds(0.01666666f);
        }
    }


    // Use this for initialization
    void Start () {
        par = transform.parent;
        dbDone = false;
        r = par.gameObject.GetComponent<RectTransform>();
        StartCoroutine(SineMove(r.position.y,20));
    }
	



	// Update is called once per frame
	void Update () {
            float what = Mathf.Clamp(Mathf.Floor(100 * (startTime - (float)DoubleTime.ScaledTimeSinceLoad)), 0, 9999f);
            string hi = "" + what;
        if (DoubleTime.ScaledTimeSinceLoad < 0.5f && Time.timeScale != 0)
        {
            par.gameObject.GetComponent<Image>().color = Color.Lerp(new Color(1, 0, 0, 1), new Color(79f / 255f, 79f / 255f, 79f / 255f, 1), (float)DoubleTime.ScaledTimeSinceLoad*2);
            r.position = Vector3.Lerp(new Vector3(r.position.x,280,r.position.z), new Vector3(r.position.x, 440, r.position.z),Mathf.Pow((float)DoubleTime.ScaledTimeSinceLoad*2,0.5f));
        }
        if (DoubleTime.ScaledTimeSinceLoad > 0.5f && DoubleTime.ScaledTimeSinceLoad < 1)
        {
            par.gameObject.GetComponent<Image>().color = new Color(79f / 255f, 79f / 255f, 79f / 255f, 1);
            r.position = new Vector3(r.position.x, 440, r.position.z);
        }

            if (what >= 1000)
            {
                GetComponent<Text>().text = hi.Substring(0, 2) + "." + hi.Substring(2);
            }
            if (what <= 999 && what >= 100)
            {
                GetComponent<Text>().text = hi.Substring(0, 1) + "." + hi.Substring(1);
            }
            if (what <= 99 && what >= 10)
            {
                GetComponent<Text>().text = "0." + hi;
            }
            if (what <= 9)
            {
                GetComponent<Text>().text = "0.0" + hi;
            }
            if (what <= 0)
            {
            GameObject.FindObjectOfType<KHealth>().SetHealth(Mathf.Infinity,"");
            }
    }
}
