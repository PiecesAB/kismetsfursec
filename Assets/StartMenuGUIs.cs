using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuGUIs : MonoBehaviour
{
    public enum Mode
    {
        Start, Settings, Test, Gallery
    }

    private Encontrolmentation e;
    public Mode mode;
    public int selection;
    public GameObject[] sampleElements;
    public Material[] sampleMaterials;
    [HideInInspector]
    public List<GameObject> elements = new List<GameObject>();
    public RectTransform scroller;

    public AudioSource changeSound;
    public AudioSource selectSound;

    private bool allowedInputLastFrame;
    private int inputDelay = 4;
    private int currScrollPos;

    void Start()
    {
        e = GetComponent<Encontrolmentation>();
        allowedInputLastFrame = false;
        switch (mode)
        {
            case Mode.Start:
                //incomplete
                int i = 0;
                Utilities.SaveData currData = Utilities.LoadGame(i+1, true);
                GameObject sampleBox = sampleElements[0];
                while (currData != null /* i <= 20 */)
                {
                    GameObject newBox = Instantiate(sampleBox, sampleBox.transform.parent);
                    newBox.GetComponent<RectTransform>().localPosition = new Vector3(
                            -230f + (460f * (i%2)),
                            -100f - (140f * (i/2)),
                            sampleBox.transform.localPosition.z
                        );
                    RawImage ri = newBox.GetComponent<RawImage>();
                    ri.uvRect = new Rect(ri.uvRect.x,(0.25f * (i / 2))%1f + 0.25f,ri.uvRect.width,ri.uvRect.height);
                    elements.Add(newBox);

                    Text nbFileNum = newBox.transform.Find("FileNum").GetComponent<Text>();
                    RawImage nbIcon = newBox.transform.Find("Icon").GetComponent<RawImage>();
                    Text nbName = newBox.transform.Find("Name").GetComponent<Text>();
                    Text nbScore = newBox.transform.Find("Score").GetComponent<Text>();
                    Text nbTime = newBox.transform.Find("Time").GetComponent<Text>();
                    Text nbLevel = newBox.transform.Find("Level").GetComponent<Text>();
                    nbFileNum.text = (i+1).ToString();
                    nbName.text = currData.name;
                    nbScore.text = currData.score.ToString("D9");
                    nbTime.text = Utilities.GetFormattedTimePlayed((long)currData.gameTimePlayed);
                    nbLevel.text = currData.levelName;

                    // set icon
                    try
                    {
                        Sprite iconSprite = Resources.Load<Sprite>("SaveIcons/" + currData.icon.ToString());
                        nbIcon.texture = iconSprite.texture;
                    }
                    catch { }

                    //print(currData.name);
                    ++i;
                    currData = Utilities.LoadGame(i+1, true);
                }

                GameObject finalNewBox = Instantiate(sampleBox, sampleBox.transform.parent);
                finalNewBox.GetComponent<RectTransform>().localPosition = new Vector3(
                        -230f + (460f * (i % 2)),
                        -100f - (140f * (i / 2)),
                        sampleBox.transform.localPosition.z
                    );
                RawImage finalri = finalNewBox.GetComponent<RawImage>();
                finalri.uvRect = new Rect(finalri.uvRect.x, (0.25f * (i / 2)) % 1f + 0.25f, finalri.uvRect.width, finalri.uvRect.height);
                elements.Add(finalNewBox);
                foreach (Transform c in finalNewBox.transform)
                {
                    c.gameObject.SetActive(false);
                }
                finalNewBox.transform.Find("New").gameObject.SetActive(true);
                finalNewBox.transform.Find("FileNum").GetComponent<Text>().text = (i + 1).ToString();

                //don't forget to make the last "new" box
                currScrollPos = 0;
                selection = -1;
                Destroy(sampleElements[0]);
                break;
            case Mode.Settings:
            case Mode.Test:
            case Mode.Gallery:
                break;
        }
    }

    private void OnGetFocus()
    {
        switch (mode)
        {
            case Mode.Start:
                selection = 0;
                changeSound.Stop(); changeSound.Play();
                elements[0].GetComponent<RawImage>().material = sampleMaterials[1];
                break;
            case Mode.Settings:
            case Mode.Test:
            case Mode.Gallery:
                break;
        }
        inputDelay = 4;
    }

    private void OnLoseFocus()
    {
        switch (mode)
        {
            case Mode.Start:
                elements[selection].GetComponent<RawImage>().material = sampleMaterials[0];
                selection = -1;
                break;
            case Mode.Settings:
            case Mode.Test:
            case Mode.Gallery:
                break;
        }
        inputDelay = 4;
    }

    int GetControls()
    {
        int m = 0;
        if ((e.flags & 3UL) == 1UL) { m -= 1; changeSound.Stop(); changeSound.Play(); }
        if ((e.flags & 3UL) == 2UL) { m += 1; changeSound.Stop(); changeSound.Play(); }
        if ((e.flags & 12UL) == 4UL) { m -= 2; changeSound.Stop(); changeSound.Play(); }
        if ((e.flags & 12UL) == 8UL) { m += 2; changeSound.Stop(); changeSound.Play(); }
        return m;
    }

    void Update()
    {
        if (e.allowUserInput && !allowedInputLastFrame)
        {
            OnGetFocus();
        }
        else if (!e.allowUserInput && allowedInputLastFrame)
        {
            OnLoseFocus();
        }
        allowedInputLastFrame = e.allowUserInput;

        switch (mode)
        {
            case Mode.Start:
                if (inputDelay == 0 && selection != -1)
                {
                    currScrollPos = Mathf.Clamp(currScrollPos, (selection / 2) - 2, selection / 2);
                    scroller.anchoredPosition = Vector3.Lerp(scroller.anchoredPosition,new Vector3(0f, currScrollPos*140f, 0f),0.15f);
                    int targ = selection + GetControls();
                    if (targ != selection && targ >= 0 && targ < elements.Count)
                    {
                        elements[selection].GetComponent<RawImage>().material = sampleMaterials[0];
                        selection = targ;
                        elements[selection].GetComponent<RawImage>().material = sampleMaterials[1];
                    }

                    if ((e.flags & 16UL) == 16UL)
                    {
                        e.allowUserInput = false;
                        selectSound.Play();
                        BGMController.main.InstantMusicChange(null, false);
                        Camera.main.GetComponent<MainMenuCameraControl>().SelectGame(selection+1);
                    }
                }
                break;
            case Mode.Settings:
            case Mode.Test:
            case Mode.Gallery:
                break;
        }

        if (e.allowUserInput && inputDelay > 0) { --inputDelay; }
    }
}
