using UnityEngine;
using System.Collections;

public class GuillotineGenerator : MonoBehaviour {

    public float speed;
    public AnimationCurve delay;
    public float startDelay;
    public bool on;
    public bool tooSmallToMakeOne;
    public GameObject guillotineObj;
    private float top;
    private float bottom;
    public Vector3 localPosOffset;
    private Vector3 localPosOrigin;
    public float unitHeight;
    public bool raycastAutoOrigin;
    public bool varRaycastLength;
    public float width;
    public float extraScaleLength = 1f;
    public bool BGMUseRhythm;
    public AudioSource BGMObject;
    public float BGMTempo;
    public int beatsPerMeasure;
    [Header("beat cue starts at 0. like (0,1,2,3) in 4/4 time")]
    public int myBeatCue;
    public int currentBeat = 0;


	void Start () {
        if (!BGMUseRhythm)
        {
            StartCoroutine(Make(0));
        }
        if (raycastAutoOrigin)
        {
            localPosOrigin = transform.localPosition;
        }
        currentBeat = 0;
        BGMObject = BGMController.main?.aso;
	}
	
    public IEnumerator Make(int hi)
    {
        if (hi==0)
        {
            yield return new WaitForSeconds(startDelay);
        }
        if (on && !tooSmallToMakeOne)
        {
            
            GameObject stuff = Instantiate(guillotineObj, transform.position, Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z)) as GameObject;
            stuff.transform.Translate((Vector3.up * ((16 * transform.localScale.y)  +0  )));
            stuff.GetComponent<NormalGuillotine1>().speed = speed;
            stuff.GetComponent<NormalGuillotine1>().Slice(((32 *transform.localScale.y)+0));
        }

        if (!BGMUseRhythm)
        {
            yield return new WaitForSeconds(delay.Evaluate(Fakerand.Single()));
            StartCoroutine(Make(1));
        }
    }

	void Update () {
	    if (varRaycastLength)
        {
            Vector3 worldPoint = (transform.parent!= null)?transform.parent.TransformPoint(localPosOrigin):localPosOrigin;
            Vector3 leftPoint = worldPoint + (-width * 0.45f) * transform.right + (transform.up*-0.5f);
            Vector3 rightPoint = worldPoint + (width * 0.45f) * transform.right + (transform.up * -0.5f);
            RaycastHit2D leftCast = Physics2D.Raycast(leftPoint, -transform.up, 216f, 768);
            RaycastHit2D rightCast = Physics2D.Raycast(rightPoint, -transform.up, 216f, 768);
            float dist = (leftCast.point - (Vector2)leftPoint).magnitude;
            float distRTemp = (rightCast.point - (Vector2)rightPoint).magnitude;
            if (distRTemp > dist)
            {
                dist = distRTemp;
            }
            float newScale = dist / unitHeight;
            if (newScale < 0.5f)
            {
                tooSmallToMakeOne = true;
            }
            else
            {
                tooSmallToMakeOne = false;
            }
            transform.localScale = new Vector3(1f, newScale+extraScaleLength, 1f);
            transform.localPosition = localPosOrigin - ((dist / 2) * Vector3.up);

        }

        if (BGMUseRhythm && Time.timeScale > 0)
        {
            if (!BGMObject) { BGMObject = BGMController.main?.aso; }
            float beatTime = 60f / BGMTempo;
            int newBeat = Mathf.FloorToInt(BGMObject.time / beatTime % beatsPerMeasure);
            if (newBeat != currentBeat && newBeat == myBeatCue)
            {
                StartCoroutine(Make(1));
            }
            currentBeat = newBeat;
        }
	}
}
