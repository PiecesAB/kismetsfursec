using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PianoBall : MonoBehaviour {

    public enum TargetType
    {
        None, Less, Same, More
    }

    public float velocity;
    public float topVelocity;
    public bool stuck = false;
    public Transform ball;
    public AudioSource ballSound;
    public int[] songNotes;
    public int currentNote;
    public int target;
    public TargetType targetType;
    public int wrongNoteA;
    public int wrongNoteB;
    public Transform[] keys;
    public List<Transform> keysMoving = new List<Transform>();
    public AudioSource pianoSound;
    public AudioSource loseSound;
    public Light sceneMainLight;
    public bool autoPilot;
    [Header("The following are for displaying music")]
    public RectTransform clef;
    public GameObject noteSample;
    public Sprite[] notePics;
    public RectTransform endLine;
    public Transform staffObject;
    public float targetStaffX;
    public float oldTargetStaffX;
    private List<Image> notePicObjs = new List<Image>();

    IEnumerator Lose(int loseKey, int winKey, int currentNote)
    {
        loseSound.Play();
        Material keyFun = keys[loseKey].GetComponent<Renderer>().material;
        Material keyFun2 = keys[winKey].GetComponent<Renderer>().material;
        Image staffNote = notePicObjs[currentNote];
        Color co = keyFun.color;
        Color co2 = keyFun2.color;
        Color snco = staffNote.color;
        Color cp = sceneMainLight.color;
        sceneMainLight.color = new Color(1f, 0.7f, 0.7f, 1f);
        while (loseSound.isPlaying)
        {
            keyFun.color = Color.red;
            keyFun2.color = staffNote.color = Color.blue;
            yield return new WaitForSecondsRealtime(0.15f);
            keyFun.color = co;
            keyFun2.color = staffNote.color = Color.cyan;
            yield return new WaitForSecondsRealtime(0.15f);
        }
        keyFun.color = co;
        keyFun2.color = co2;
        staffNote.color = snco;
        sceneMainLight.color = cp;
        yield return new WaitForEndOfFrame();
    }

    Vector3 OffsetFind(int x)
    {
        int[] diatonix = new int[7] { 0, 2, 4, 5, 7, 9, 11 };
        for(int i = 0; i < diatonix.Length; i++)
        {
            if (x%12 == diatonix[i])
            {
                return new Vector3(0, 0.4f, 3f); //white keys
            }
        }
        return new Vector3(0, 0.4f, 1.5f); //black keys
    }

	void Start () {
        topVelocity = 0.01f;
        currentNote = 0;
        target = wrongNoteA = wrongNoteB = songNotes[0];
        targetType = TargetType.None;
        clef.localPosition = new Vector3(-48, 0, 0);
        float posMan = 0;
        for (int i = 0; i < notePics.Length; i++)
        {
            GameObject newNotePic = Instantiate(noteSample);
            newNotePic.transform.SetParent(staffObject, false);
            Image im = newNotePic.GetComponent<Image>();
            im.sprite = notePics[i];
            notePicObjs.Add(im);
            RectTransform rt = newNotePic.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(rt.sizeDelta.x,notePics[i].texture.height);
            rt.localPosition = new Vector3(posMan, 0, 0);
            posMan += notePics[i].texture.width;

        }
        endLine.localPosition = new Vector3(posMan, 0, 0);
        targetStaffX = oldTargetStaffX = 0f;
    }
	
    void Bounce(int wantedTarget)
    {
        //bounce ball
        if (target >= 0)
        {
            ball.localPosition = keys[wantedTarget].localPosition + OffsetFind(wantedTarget);
        }
        velocity = topVelocity = Mathf.Max((-velocity * 0.9f) - 0.014f,0);
        ballSound.Play();

        //move key
        Transform closest = keys[21];
        float closestDist = Mathf.Infinity;
        int index = 21;
        int i = 0;
        foreach (Transform key in keys)
        {
            float thisDist = Fastmath.FastV2Dist(key.localPosition, ball.localPosition);
            if (thisDist < closestDist)
            {
                closest = key;
                closestDist = thisDist;
                index = i;
            }
            i++;
        }
        pianoSound.Stop();
        pianoSound.pitch = Mathf.Pow(2f, (index - 24) / 12f);
        pianoSound.Play();
        closest.localEulerAngles = new Vector3(-84, 0, 0);
        keysMoving.Add(closest);

        //next target (lose if this was wrong)
        if (target != wantedTarget) //lose
        {
            stuck = true;
            StartCoroutine(Lose(wantedTarget, target, currentNote));
            target = -1;
        }

        currentNote++;
        if (currentNote == songNotes.Length)
        {
            target = -1;
            stuck = true;
        }
        else
        {
            target = songNotes[currentNote];
            if (songNotes[currentNote-1] == target)
            {
                targetType = TargetType.Same;
                wrongNoteA = Mathf.Max(target - Fakerand.Int(2, 7),0);
                wrongNoteB = Mathf.Min(target + Fakerand.Int(2, 7), 35);
            }
            else if (songNotes[currentNote - 1] < target)
            {
                targetType = TargetType.More;
                wrongNoteA = Mathf.Max(target - Fakerand.Int(target-songNotes[currentNote - 1]+1, target - songNotes[currentNote - 1] + 5), 0);
                wrongNoteB = songNotes[currentNote - 1];
            }
            else
            {
                targetType = TargetType.Less;
                wrongNoteA = songNotes[currentNote - 1];
                wrongNoteB = Mathf.Min(target + Fakerand.Int(songNotes[currentNote - 1] - target + 1, songNotes[currentNote - 1] - target + 5), 35);
            }
        }

        //move score
        oldTargetStaffX = targetStaffX;
        targetStaffX -= notePics[currentNote-1].texture.width;
    }

	void Update () {
        if (Time.timeScale > 0)
        {
            //this deals with the ball
            if (!stuck)
            {
                if (target >= 0)
                {
                    double whatever = 0f;
                    Encontrolmentation e = GetComponent<Encontrolmentation>();
                    int wantedTarget = target;
                    if (!autoPilot)
                    {
                        if (e.ButtonHeld(1UL, 3UL, 0f, out whatever)) //left
                        {
                            switch (targetType)
                            {
                                case TargetType.None:
                                    break;
                                case TargetType.Less:
                                    break;
                                case TargetType.Same:
                                    wantedTarget = wrongNoteA;
                                    break;
                                case TargetType.More:
                                    wantedTarget = wrongNoteB;
                                    break;
                                default:
                                    break;
                            }
                        }
                        else if (e.ButtonHeld(2UL, 3UL, 0f, out whatever)) //right
                        {
                            switch (targetType)
                            {
                                case TargetType.None:
                                    break;
                                case TargetType.Less:
                                    wantedTarget = wrongNoteA;
                                    break;
                                case TargetType.Same:
                                    wantedTarget = wrongNoteB;
                                    break;
                                case TargetType.More:
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            switch (targetType)
                            {
                                case TargetType.None:
                                    break;
                                case TargetType.Less:
                                    wantedTarget = wrongNoteA;
                                    break;
                                case TargetType.Same:
                                    break;
                                case TargetType.More:
                                    wantedTarget = wrongNoteB;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    ball.localPosition += Vector3.up * velocity;
                    velocity -= 0.01f;
                    if (ball.localPosition.y <= keys[0].localPosition.y + 0.4f)
                    {
                        ball.localPosition = new Vector3(ball.localPosition.x, 0.4f, ball.localPosition.z);
                        Bounce(wantedTarget);
                    }
                    float progress = 1f - Mathf.InverseLerp(-topVelocity, topVelocity, velocity);
                    staffObject.localPosition = new Vector3(Mathf.Lerp(oldTargetStaffX,targetStaffX,progress), -8, 0);
                    float urgency = EasingOfAccess.SineSmooth(Mathf.Clamp01(progress));
                    Vector3 z = Vector3.Lerp(ball.localPosition, keys[wantedTarget].localPosition + OffsetFind(wantedTarget), urgency);
                    ball.localPosition = new Vector3(z.x, ball.localPosition.y, z.z);
                }
                
            }


            //this deals with keys
            List<Transform> toRemove = new List<Transform>();
            foreach (Transform mov in keysMoving)
            {
                mov.localEulerAngles = new Vector3(mov.localEulerAngles.x-0.4f, 0, 0);
                float tx = mov.localEulerAngles.x;
                tx = (tx < 0) ? (tx + 360) : tx; // positives only
                if (mov.localEulerAngles.x <= 270)
                {
                    mov.localEulerAngles = new Vector3(-90, 0, 0);
                    toRemove.Add(mov);
                }
            }

            foreach (var bye in toRemove)
            {
                keysMoving.Remove(bye);
            }
                
        }
	}
}
