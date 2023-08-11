using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpecialGate : MonoBehaviour, IAmbushController, IDialSetter
{

    [System.Serializable]
    public struct Trigger
    {
        public GameObject trigObj;
        public bool antiMatch;
        public string matchStr;
        public float openChange;
        public float openTargetIfChangeZero;
        public bool continuous;
    }

    public GameObject gateLeft;
    public GameObject gateRight;
    public float span;
    public float thiccness;
    public float open;
    public float target;
    public float adjSpeed;
    public float timeInfluence;
    public float gradualPerFrameAdjust;
    public float turnstileEffect;
    public List<Trigger> triggers;
    public AudioClip movementSound;
    public AudioClip closeSound;

    public bool accordionType = false;
    public BoxCollider2D accordionCollider;
    public bool accordionDisallowBlock;
    public float valueOnAmbushWin = 1f;

	void Start () {
        Adjust(open);
        if (!accordionType)
        {
            if (gateRight) gateRight.GetComponent<Renderer>().material.SetFloat("_Rev", 0f);
            if (gateLeft) gateLeft.GetComponent<Renderer>().material.SetFloat("_Rev", 2f);
        }
    }

    public void Adjust(float t)
    {
        if (accordionType)
        {
            GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(0, open * 100f);
            accordionCollider.offset = new Vector2(0f,32f*(open-1f));
            accordionCollider.size = new Vector2(accordionCollider.size.x, 64f*(1f-open));
        }
        else
        {
            float st = span * (1f - t);
            float sti = span * t / 2f;
            if (gateLeft) gateLeft.GetComponent<BoxCollider2D>().offset = Vector2.left * sti;
            if (gateRight) gateRight.GetComponent<BoxCollider2D>().offset = Vector2.right * sti;
            if (gateLeft) gateLeft.GetComponent<BoxCollider2D>().size = new Vector2(st, thiccness);
            if (gateRight) gateRight.GetComponent<BoxCollider2D>().size = new Vector2(st, thiccness);

            if (span <= 48)
            {
                if (gateRight) gateRight.GetComponent<Renderer>().material.SetFloat("_HC", 0.5f);
                if (gateLeft) gateLeft.GetComponent<Renderer>().material.SetFloat("_HC", 0.5f);
            }
            else
            {
                if (gateRight) gateRight.GetComponent<Renderer>().material.SetFloat("_HC", (96f - span) / 96f);
                if (gateLeft) gateLeft.GetComponent<Renderer>().material.SetFloat("_HC", span / 96f);
            }
            if (gateRight) gateRight.GetComponent<Renderer>().material.SetFloat("_Open", open * span / 96f);
            if (gateLeft) gateLeft.GetComponent<Renderer>().material.SetFloat("_Open", open * span / 96f);
        }
    }

    public void OnAmbushBegin()
    {
        target = 0f; //i hope we can change this later.
    }

    public void OnAmbushComplete()
    {
        target = valueOnAmbushWin; //i hope we can change this later.
    }

    void Update () {

        foreach (Trigger t in triggers)
        {
            bool ye = t.antiMatch;
            if (!t.trigObj)
            {
                continue;
            }

            if (t.trigObj.GetComponent<MessageBumpBox>() != null)
            {

                if (t.matchStr == t.trigObj.GetComponent<MessageBumpBox>().output)
                {
                    ye = !ye;
                }

            }

            if (t.trigObj.GetComponent<sequenceBlockGroup>() != null)
            {

                if (t.matchStr == t.trigObj.GetComponent<sequenceBlockGroup>().currAsString)
                {
                    ye = !ye;
                }

            }

            if (t.trigObj.GetComponent<tripwire>() != null)
            {

                if (t.matchStr == t.trigObj.GetComponent<tripwire>().output)
                {
                    ye = !ye;
                }

            }

            if (t.trigObj.GetComponent<TurnstileSwitch>() != null)
            {
                target -= turnstileEffect*t.trigObj.GetComponent<TurnstileSwitch>().dx;
            }

            if (t.trigObj.GetComponent<PrimCollectible>() != null)
            {

                if (t.trigObj.GetComponent<PrimCollectible>().got)
                {
                    ye = !ye;
                }

            }

            if (ye)
            {
                if (t.openChange == 0)
                {
                    target = t.openTargetIfChangeZero;
                }
                else
                {
                    target += t.openChange;
                }

                if (!t.continuous)
                {
                    triggers.Remove(t);
                }
                break;
            }

        }

        float ll = ((1f - timeInfluence) + (timeInfluence * Time.timeScale));
        target = Mathf.Clamp01(target + gradualPerFrameAdjust *ll);
        bool suc = true;
        float oldPos = open;

        if (open == 0)
        {
            suc = false;
        }

        if (open < target)
        {
            open += adjSpeed* ll;
            if (open > target)
            {
                open = target;
            }
            Adjust(open);
        }

        if (open > target)
        {
            if (accordionType && !accordionDisallowBlock)
            {
                //with accordion gate, you can place an object to keep it open
                RaycastHit2D[] rh2 = Physics2D.RaycastAll(transform.position - transform.forward * 0.1f
                    , -transform.forward, Mathf.Max(Mathf.Ceil(4f * (1f - open) * transform.localScale.z)*16f -0.2f,15.8f)
                    , 1573632 /*all blocks, no beams, punch, and player*/);
                //Debug.DrawRay(transform.position - transform.forward * 0.1f, Mathf.Max(Mathf.Ceil(16f * (1f - open) * transform.localScale.y) * 4f -0.2f, 0.1f) * -transform.forward);
                bool hit = false;
                foreach (var r in rh2)
                {
                    if (r.collider.gameObject != accordionCollider.gameObject)
                    {
                        hit = true;
                        break;
                    }
                }

                if (!hit)
                {
                    open -= adjSpeed * ll;
                    if (open < target)
                    {
                        open = target;
                    }
                    Adjust(open);
                }
            }
            else
            {
                open -= adjSpeed * ll;
                if (open < target)
                {
                    open = target;
                }
                Adjust(open);
            }
        }
        AudioSource aso = GetComponent<AudioSource>();
        if (open==0 && suc)
        {
            aso.clip = closeSound;
            aso.pitch = 1f;
            aso.loop = false;
            aso.Stop();
            aso.Play();
        }
        else if (suc)
        {
            aso.clip = movementSound;
            aso.pitch = Mathf.Clamp(System.Math.Abs(open - oldPos) * 200f,0f,1.8f);
            aso.loop = true;
            if (!aso.isPlaying)
            {
                aso.Play();
            }
        }

    }

    public void DialChanged(float val)
    {
        target = turnstileEffect * val;
    }
}
