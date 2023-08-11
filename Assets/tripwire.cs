using UnityEngine;
using System.Collections;

public class tripwire : MonoBehaviour {

    public Transform anchor0;
    public Transform anchor1;
    public GameObject wire;
    public AudioSource tripwireSound;
    public GameObject markerPrefab;
    public GameObject[] markers;
    public string outputMsgWhenTripped;
    public string output;
    public bool triggerAllStonePlatsInScene;
    public LargeStonePlatform[] stonePlatformOutputs;
    public AudioClip newBGM;
    public bool loopNewBGM;
    public AudioSource tripSound;
    public AudioClip resetBGM;
    public GameObject[] onTripObjects;

    public float startLevelTimerValue;
    public bool startLevelTimer;
    public bool makeBGMFollowTimer = true;
    public bool startRitualScroll = false;

    private bool done = false;

	void Start () {
        wire.GetComponent<BoxCollider2D>().enabled = true;
        WireChange();
	}

    public void Trip()
    {
        if (done) { return; }
        done = true;
        wire.GetComponent<BoxCollider2D>().enabled = false;
        output = outputMsgWhenTripped;
        foreach (var t in (triggerAllStonePlatsInScene) ? FindObjectsOfType<LargeStonePlatform>() : stonePlatformOutputs)
        {
            t.input = outputMsgWhenTripped;
        }

        if (newBGM)
        {
            BGMController triggerBGMChange = BGMController.main;
            if (triggerBGMChange != null)
            {
                if (resetBGM == null) { resetBGM = triggerBGMChange.GetComponent<AudioSource>().clip; }
                triggerBGMChange.InstantMusicChange(newBGM, loopNewBGM);
                triggerBGMChange.mustFollowLevel1MinTimer = makeBGMFollowTimer;
                LevelInfoContainer.onDeathBGM = resetBGM;
            }
        }

        tripSound.Stop();
        tripSound.Play();

        if (startLevelTimer)
        {
            LevelInfoContainer.timer = startLevelTimerValue;
            LevelInfoContainer.timerOn = true;
        }

        if (startRitualScroll && FollowThePlayer.main)
        {
            FollowThePlayer.main.ritualScrollingUnlocked = true;
            FollowThePlayer.main.perScreenScrolling = FollowThePlayer.main.originalScrolling = false;
        }

        foreach (GameObject o in onTripObjects)
        {
            if (o.GetComponent<ITripwire>() != null)
            {
                o.GetComponent<ITripwire>().OnTrip();
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D oth)
    {
        if (oth.GetComponent<Rigidbody2D>() != null && !oth.GetComponent<Rigidbody2D>().isKinematic)
        {
            Trip();
        }
    }
	
    public void WireChange()
    {
        Vector2 a1p = anchor1.localPosition;
        Vector2 a0p = anchor0.localPosition;
        wire.GetComponent<BoxCollider2D>().size = new Vector2(Fastmath.FastV2Dist(a1p, a0p), 1f);
        Vector3 lea = wire.transform.localEulerAngles;
        wire.transform.localPosition = Vector2.Lerp(a1p,a0p,0.5f);
        wire.transform.localEulerAngles = new Vector3(lea.x, lea.y, Mathf.Rad2Deg*Mathf.Atan2(a1p.y - a0p.y, a1p.x - a0p.x));
    }

	void Update () {
        if (wire.GetComponent<BoxCollider2D>().enabled)
        {
            WireChange();
        }
	}
}
