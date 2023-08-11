using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingTransitionalScreen : MonoBehaviour {

    public enum Mode
    {
        Standard, Animation, TitleCard, None
    }
    public Mode mode = Mode.Standard;
    public Image holder;
    public static string cardTitle = "";
    public static string cardSubtitle = "";
    public Text cardTitleObject;
    public Text cardSubtitleObject;

    void Start () {
        if (mode == Mode.Standard)
        {
            GetComponent<Image>().material.SetFloat("_T", 0f);
            GetComponent<Image>().material.SetFloat("_R", -1f);
            holder.material.SetFloat("_Transition", 0f);
        }
        else if (mode == Mode.TitleCard)
        {
            if (cardTitleObject) { cardTitleObject.text = cardTitle; }
            if (cardSubtitleObject) { cardSubtitleObject.text = cardSubtitle; }
        }
    }

    private bool animatorToggle = false;

    private void WhileNotFinished(LoadingScreenScript l)
    {
        switch (mode)
        {
            case Mode.Animation:
                if (!animatorToggle)
                {
                    Animator a = GetComponentInChildren<Animator>();
                    a.CrossFade("StartLoading", 0f);
                    animatorToggle = true;
                }
                break;
            case Mode.Standard:
                GetComponent<Image>().material.SetFloat("_R", -1f);
                float t = ((float)(DoubleTime.UnscaledTimeRunning - l.realTimeAtStartseq)) / l.beginDelay;
                GetComponent<Image>().material.SetFloat("_T", t);
                holder.material.SetFloat("_Transition", t);
                break;
            default: break;
        }
    }

    private void WhileFinished(LoadingScreenScript l)
    {
        switch (mode)
        {
            case Mode.Animation:
                if (animatorToggle)
                {
                    Animator a = GetComponentInChildren<Animator>();
                    a.CrossFade("FinishLoading", 0f);
                    animatorToggle = false;
                }
                break;
            case Mode.Standard:
                GetComponent<Image>().material.SetFloat("_R", 1f);
                float t = ((float)(DoubleTime.UnscaledTimeRunning - l.realTimeAtFinishload)) / l.finishDelay;
                GetComponent<Image>().material.SetFloat("_T", t);
                holder.material.SetFloat("_Transition", 1f - t);
                break;
            default: break;
        }
    }

	void Update () {
	    if (LoadingScreenScript.all.Count > 0)
        {
            LoadingScreenScript l = LoadingScreenScript.all[0];
            if (!l.finished) { WhileNotFinished(l); }
            else { WhileFinished(l); }
        }
	}
}
