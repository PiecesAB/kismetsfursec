using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CutsceneTextHandler : MonoBehaviour
{
    public Text myText;
    public GameObject textDisplay;

    [System.Serializable]
    public class DialogItem
    {
        public string text;
        public AudioClip voice;
        public SkinnedMeshRenderer jawMesh; // for nonhumans
        public HumanMouthSync mouth; // for humans
        public string humanEmotion; // ||
        public float talkingSpeed = 1;
    }

    public AudioClip defaultVoice;

    public List<DialogItem> lines = new List<DialogItem>();

    private GameObject narrator;
    private Quaternion jawNaturalRotation;

    private int iter = 0;
    private bool exiting = false;

    [SerializeField]
    private bool testTrigger = false;

    [SerializeField]
    private int editIndex;
    [SerializeField]
    private bool insertTrigger = false;
    [SerializeField]
    private bool deleteTrigger = false;

    private void Awake()
    {
        Settings.PrepareAll(true);
    }

    private void Start()
    {
        textDisplay.SetActive(false);
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            if (insertTrigger && editIndex >= 0 && editIndex < lines.Count)
            {
                lines.Insert(editIndex, new DialogItem());
            }
            if (deleteTrigger)
            {
                lines.RemoveAt(editIndex);
            }
            insertTrigger = deleteTrigger = false;
            return;
        }
        if (!exiting) { Application.targetFrameRate = 24; QualitySettings.vSyncCount = 0; }
        if (testTrigger) { TextTrigger(); testTrigger = false; }
    }

    private SkinnedMeshRenderer movingJaw = null;
    private int jawCounter = 0;
    private IEnumerator MoveJaw(SkinnedMeshRenderer jaw, Transform voiceT)
    {
        if (!jaw) { yield break; }
        movingJaw = jaw;
        int currJawCounter = ++jawCounter;
        AudioSource consonant = voiceT.Find("Consonant").GetComponent<AudioSource>();
        float[] consonantData = new float[64];
        AudioSource vowel = voiceT.Find("Vowel").GetComponent<AudioSource>();
        float[] vowelData = new float[64];
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        float smoothSumVol = 0f;
        while (jawCounter == currJawCounter)
        {
            // We're attempting a different way of shape key animation for the jaw
            consonant.GetSpectrumData(consonantData, 0, FFTWindow.Hamming);
            vowel.GetSpectrumData(vowelData, 0, FFTWindow.Hamming);
            float sumVol = 0;
            for (int i = 0; i < 64; ++i) { sumVol += consonantData[i]; sumVol += vowelData[i]; }
            sumVol = Mathf.Clamp01(sumVol * 6f);
            smoothSumVol = Mathf.Lerp(smoothSumVol, sumVol, 0.5f);
            jaw.SetBlendShapeWeight(0, 60f * sumVol);
            yield return new WaitForEndOfFrame();
        }
        if (movingJaw != jaw) { jaw.SetBlendShapeWeight(0, 0); }
        
        yield return null;
    }

    private string RemoveTags(string input)
    {
        StringBuilder sb = new StringBuilder();
        bool inTag = false;
        for (int i = 0; i < input.Length; ++i) {
            if (input[i] == '<') { inTag = true; }
            else if (input[i] == '>') { inTag = false; }
            else if (!inTag) { sb.Append(input[i]); }
        }
        return sb.ToString();
    }

    private IEnumerator TriggerTime()
    {
        ++iter;
        int oldIter = iter;

        DialogItem ditem = lines[0];
        if (!narrator) { narrator = Instantiate(Resources.Load<GameObject>("Narrator"), transform); }
        ActualSpeaking speaker = narrator.GetComponent<ActualSpeaking>();
        speaker.emotion = ditem.humanEmotion;
        if (ditem.humanEmotion != "" && ditem.mouth) { ditem.mouth.GetComponent<HumanFacialControl>().UpdateEmotion(ditem.humanEmotion); }
        if (ditem.text != "")
        {
            myText.text = ditem.text;
            if (ditem.voice)
            {
                speaker.SetVoice(ditem.voice, true);
                speaker.speedMultiplier2 = ditem.talkingSpeed == 0 ? 1 : ditem.talkingSpeed;
                speaker.Say(RemoveTags(myText.text));
                StartCoroutine(MoveJaw(ditem.jawMesh, speaker.transform));
                if (ditem.mouth)
                {
                    ditem.mouth.speaker = speaker;
                    ditem.mouth.Activate();
                    speaker.syncedMouths.Add(ditem.mouth);
                }
            }
        }

        lines.RemoveAt(0);

        if (ditem.text != "")
        {
            // turning it off and on resets the text formatting
            textDisplay.SetActive(false);
            textDisplay.SetActive(true);

            float l = myText.text.Length * 0.075f;
            if (ditem.talkingSpeed > 0f && ditem.talkingSpeed < 1f) { l /= ditem.talkingSpeed; }
            yield return new WaitForSecondsRealtime(l + 1f);
            if (oldIter != iter) { yield break; }

            myText.text = "";
            yield return new WaitForEndOfFrame();
            textDisplay.SetActive(false);
        }
    }

    public void TextTrigger()
    {
        if (lines.Count == 0) { throw new System.Exception("No string are left for cutscene text."); }

        StartCoroutine(TriggerTime());
    }

    public void ExitCutscene()
    {
        GetComponentInChildren<primAddScene>().activate = true;
        exiting = true;
        Application.targetFrameRate = 60;
    }
}
