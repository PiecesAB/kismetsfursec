using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HumanMouthSync))]
public class HumanFacialControl : MonoBehaviour
{
    [System.Serializable]
    public class Emotion
    {
        public string name;
        public Sprite leftBrow;
        public Sprite rightBrow;
        public Sprite leftEye;
        public Sprite rightEye;
        public HumanMouthSync.Emotion mouthEmotion;
    }

    public Sprite blinkSprite;

    private HumanMouthSync mouthSync;
    public Emotion[] emotions;
    public Renderer model;
    public int leftBrowIndex;
    public int rightBrowIndex;
    public int leftEyeIndex;
    public int rightEyeIndex;

    private Emotion currEmotion = null;

    private Dictionary<string, Emotion> emotionByName = new Dictionary<string, Emotion>();

    void Start()
    {
        mouthSync = GetComponent<HumanMouthSync>();
        foreach (Emotion e in emotions) { emotionByName[e.name] = e; }
    }

    public void UpdateEmotion(string s)
    {
        if (!emotionByName.ContainsKey(s)) { return; }
        Emotion e = emotionByName[s];
        model.materials[leftBrowIndex].SetTexture("_MainTex", e.leftBrow.texture);
        model.materials[rightBrowIndex].SetTexture("_MainTex", e.rightBrow.texture);
        mouthSync.emotion = e.mouthEmotion;
        currEmotion = e;
        blinkTimer = Fakerand.Int(0, timeBetweenBlinks);
    }

    private int blinkTimer = 0;
    public int timeBetweenBlinks = 128;

    private void Update()
    {
        if (currEmotion == null) { currEmotion = emotions[0]; }
        model.materials[leftEyeIndex].SetTexture("_MainTex", (blinkTimer < 2) ? blinkSprite.texture : currEmotion.leftEye.texture);
        model.materials[rightEyeIndex].SetTexture("_MainTex", (blinkTimer < 2) ? blinkSprite.texture : currEmotion.rightEye.texture);
        blinkTimer = (blinkTimer + 1) % timeBetweenBlinks;
    }
}
