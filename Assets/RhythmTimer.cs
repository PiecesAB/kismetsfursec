using UnityEngine;
using System.Collections;

public class RhythmTimer : MonoBehaviour {

    [System.Serializable]
    public struct TempoStuf
    {
        public float bpm;
        public float whichBeatToChangeOn;
    }

    public float startSecondsOffset;


    public TempoStuf[] tempoData;

    
    public uint currentBeat;
    public float nextBeatOccurence;

    public float audioTime;

    public int whichTempoOn;

    [SerializeField]
    private bool whenTimerOn = false;

    public static RhythmTimer main;

	// Use this for initialization
	void Awake () {
        currentBeat = 0;
        whichTempoOn = 0;
        nextBeatOccurence = 60/tempoData[whichTempoOn].bpm + startSecondsOffset;
        main = this;
	}
	
	// Update is called once per frame
	void Update () {
        if (Time.timeScale == 0 || (whenTimerOn && !LevelInfoContainer.timerOn)) { return; }
        audioTime = ((float)BGMController.main.aso.timeSamples) / ((float)BGMController.main.aso.clip.frequency);
        if (audioTime >= startSecondsOffset && currentBeat == 0)
        {
            currentBeat = 1;
        }
        while (audioTime > nextBeatOccurence)
        {
            currentBeat += 1;
            foreach (TempoStuf td in tempoData)
            {
                if (currentBeat == td.whichBeatToChangeOn)
                {
                    whichTempoOn += 1;
                }
            }
            nextBeatOccurence = (60 / tempoData[whichTempoOn].bpm) + nextBeatOccurence;
        }
	}
}
