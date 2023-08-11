using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MenuLevelSelect : InSaveMenuBase
{
    public GameObject sampleTab;
    public GameObject sampleLevelDisplay;
    public Transform scrollInset;
    public Transform dispInset;
    public Color inactColor;
    public Color actColor;
    public Sprite placeholderSprite;
    public MenuLevelSelectConfirm confirmMenu;
    [HideInInspector]
    public bool replayDirectToLevel = false; // if true, select the level ASAP
    public AudioSource changeTabSound;
    public AudioSource changeLevelSound;
    public AudioSource submitSound;

    private List<GameObject> tabs = new List<GameObject>();
    private List<LevelInfoContainer.Theme> tabThemes = new List<LevelInfoContainer.Theme>();

    private Transform displaysParent;
    private Image activeDispOutline;

    public int tabSelected = 0;

    public int levelSelected = 0;

    private float tabPosition = 0;
    private float dispPosition = 0;

    private int frameRepeatScroll = 10;

    private bool extraDispUpdate = false;

    private List<KeyValuePair<int, Utilities.LevelInfoS>> levelsFound = new List<KeyValuePair<int, Utilities.LevelInfoS>>();

    private Image GetImage(GameObject obj)
    {
        return obj.transform.Find("Image").GetComponent<Image>();
    }

    private Text GetText(GameObject tab)
    {
        return tab.transform.Find("Text").GetComponent<Text>();
    }

    private void FindLevels()
    {
        List<Transform> toDeleteDisplays = new List<Transform>();
        for (int i = 0; i < displaysParent.childCount; ++i)
        {
            toDeleteDisplays.Add(displaysParent.GetChild(i));
        }
        for (int i = 0; i < toDeleteDisplays.Count; ++i)
        {
            if (toDeleteDisplays[i].gameObject != sampleLevelDisplay) { toDeleteDisplays[i].SetParent(null); Destroy(toDeleteDisplays[i].gameObject); }
        }

        levelsFound.Clear();
        LevelInfoContainer.Theme currTheme = tabThemes[tabSelected];
        foreach (KeyValuePair<int, Utilities.LevelInfoS> kv in Utilities.loadedSaveData.leveldatas)
        {
            if (kv.Value.theme == currTheme && currTheme != LevelInfoContainer.Theme.Cutscene)
            {
                levelsFound.Add(kv);
            }
        }
    }

    private void UpdateTabs()
    {
        levelSelected = 0;

        for (int i = 0; i < tabs.Count; ++i)
        {
            Color newCol = (i == tabSelected) ? actColor : inactColor;
            tabs[i].GetComponent<Image>().color = newCol;
            Shadow s = GetText(tabs[i]).GetComponent<Shadow>();
            s.effectColor = new Color(newCol.r, newCol.g, newCol.b, s.effectColor.a);
        }

        if (tabSelected < 2) { tabPosition = 0; }
        else if (tabSelected == tabs.Count - 1) { tabPosition = Mathf.Min(0, 284 - tabs.Count * 100); }
        else { tabPosition = 92 - tabSelected * 100; }

        FindLevels();

        sampleLevelDisplay.SetActive(true);

        for (int k = 0; k < levelsFound.Count; ++k)
        {
            float kx = 98 * ((k % 3) - 1);
            float ky = 67 - 76 * (k / 3);
            GameObject newDisp = Instantiate(sampleLevelDisplay);
            newDisp.transform.SetParent(sampleLevelDisplay.transform.parent);
            newDisp.transform.localPosition = new Vector3(kx, ky, 0);
            newDisp.transform.localScale = Vector3.one;
            GetText(newDisp).text = levelsFound[k].Value.levelName;
            Image dispImg = GetImage(newDisp);
            dispImg.sprite = placeholderSprite;
            Texture2D preTex = new Texture2D(320, 180);
            dispImg.sprite = Utilities.GetSnap(levelsFound[k].Key, placeholderSprite, preTex);
        }

        sampleLevelDisplay.SetActive(false);
        levelSelected = 0;
        UpdateDisplays();
    }

    private void UpdateDisplays()
    {
        for (int i = 1; i < displaysParent.childCount; ++i)
        {
            displaysParent.GetChild(i).Find("SelectOutline").gameObject.SetActive((i-1) == levelSelected);
        }
        activeDispOutline = (displaysParent.childCount > 1) ? displaysParent.GetChild(levelSelected + 1).Find("SelectOutline").GetComponent<Image>() : null;
        if (levelSelected < 3) { dispPosition = 0; }
        else { dispPosition = -64 + 76 * (levelSelected / 3); }
    }

    private HashSet<LevelInfoContainer.Theme> GetUsedThemes()
    {
        HashSet<LevelInfoContainer.Theme> acc = new HashSet<LevelInfoContainer.Theme>();
        foreach (KeyValuePair<int, Utilities.LevelInfoS> kv in Utilities.loadedSaveData.leveldatas)
        {
            if (!acc.Contains(kv.Value.theme) && kv.Value.theme != LevelInfoContainer.Theme.Cutscene)
            {
                acc.Add(kv.Value.theme);
            }
        }
        return acc;
    }

    private void MakeTabs()
    {
        for (int d = 0; d < tabs.Count; ++d)
        {
            Destroy(tabs[d]);
        }
        tabs.Clear();
        tabThemes.Clear();

        Transform tabParent = sampleTab.transform.parent;

        List<LevelInfoContainer.Theme> allTabs = ((LevelInfoContainer.Theme[])Enum.GetValues(typeof(LevelInfoContainer.Theme))).ToList();
        // remove the tabs that aren't used
        HashSet<LevelInfoContainer.Theme> allUsedTabs = GetUsedThemes();
        for (int k = 0; k < allTabs.Count; ++k)
        {
            if (allTabs.Count > 1 && !allUsedTabs.Contains(allTabs[k]))
            {
                allTabs.RemoveAt(k);
                --k;
            }
        }

        sampleTab.SetActive(true);

        int i = 0;
        foreach (LevelInfoContainer.Theme thm in allTabs)
        {
            GameObject newTab = Instantiate(sampleTab);
            newTab.transform.SetParent(tabParent);
            newTab.GetComponent<RectTransform>().anchoredPosition = new Vector2(100 * i + 50, 0);
            newTab.transform.localScale = Vector3.one;
            GetText(newTab).text = thm.ToString().Replace("_", " ");
            tabs.Add(newTab);
            tabThemes.Add(thm);
            ++i;
        }

        sampleTab.SetActive(false);

        UpdateTabs();
    }

    protected override void ChildOpen()
    {
        displaysParent = sampleLevelDisplay.transform.parent;
        tabSelected = 0;
        levelSelected = 0;
        extraDispUpdate = false;
        scrollInset.localPosition = Vector3.zero;
        MakeTabs();
    }

    private IEnumerator SizeConfirmBox()
    {
        Transform t = confirmMenu.transform;
        t.localScale = new Vector3(0.3f, 0.3f, 1f);
        Vector3 origPos = confirmMenu.transform.parent.InverseTransformPoint(displaysParent.GetChild(levelSelected + 1).position);
        t.localPosition = origPos;
        while (confirmMenu.gameObject.activeSelf && t.localScale.x < 1f)
        {
            t.localScale = Vector3.MoveTowards(t.localScale, Vector3.one, 0.16666f);
            float u = 1f - (t.localScale.x - 0.3f)/0.7f;
            t.localPosition = origPos * u;
            yield return new WaitForEndOfFrame();
        }
        t.localScale = Vector3.one;
        t.localPosition = Vector3.zero;
        yield return null;
    }

    private void OpenConfirmMenu(bool transition = true)
    {
        confirmMenu.levelInfo = levelsFound[levelSelected];
        confirmMenu.snap = GetImage(displaysParent.GetChild(levelSelected + 1).gameObject).sprite;
        confirmMenu.Open(myControl);
        if (transition) { StartCoroutine(SizeConfirmBox()); }
        else
        {
            confirmMenu.transform.localScale = Vector3.one;
            confirmMenu.transform.localPosition = Vector3.zero;
        }
    }

    protected override void ChildUpdate()
    {
        if (replayDirectToLevel) // open level immediately
        {
            for (int i = 0; i < tabThemes.Count; ++i)
            {
                if (tabThemes[i] == ReplayStatsScreen.GetStatsTheme()) { tabSelected = i; break; }
            }
            UpdateTabs();
            for (int i = 0; i < levelsFound.Count; ++i)
            {
                if (levelsFound[i].Key == ReplayStatsScreen.GetStatsSceneId()) { levelSelected = i; break; }
            }
            UpdateDisplays();
            OpenConfirmMenu(false);
            replayDirectToLevel = false;
            return;
        }

        double placeHolder;
        if (myControl.ButtonDown(256UL, 768UL) || (myControl.ButtonHeld(256UL, 768UL, 0.45f, out placeHolder) && frameRepeatScroll == 0)) { frameRepeatScroll = 10; if (tabSelected > 0) { --tabSelected; changeTabSound.Stop(); changeTabSound.Play(); UpdateTabs(); } }
        if (myControl.ButtonDown(512UL, 768UL) || (myControl.ButtonHeld(512UL, 768UL, 0.45f, out placeHolder) && frameRepeatScroll == 0)) { frameRepeatScroll = 10; if (tabSelected < tabs.Count - 1) { ++tabSelected; changeTabSound.Stop(); changeTabSound.Play(); UpdateTabs(); } }

        if (myControl.ButtonDown(1UL, 3UL) || (myControl.ButtonHeld(1UL, 3UL, 0.45f, out placeHolder) && frameRepeatScroll == 0)) { frameRepeatScroll = 8; if (levelSelected > 0) { --levelSelected; changeLevelSound.Stop(); changeLevelSound.Play(); UpdateDisplays(); } }
        if (myControl.ButtonDown(2UL,3UL) || (myControl.ButtonHeld(2UL, 3UL, 0.45f, out placeHolder) && frameRepeatScroll == 0)) { frameRepeatScroll = 8; if (levelSelected < levelsFound.Count - 1) { ++levelSelected; changeLevelSound.Stop(); changeLevelSound.Play(); UpdateDisplays(); } }
        if (myControl.ButtonDown(4UL, 12UL) || (myControl.ButtonHeld(4UL, 12UL, 0.45f, out placeHolder) && frameRepeatScroll == 0)) { frameRepeatScroll = 8; if (levelSelected > 2) { levelSelected -= 3; changeLevelSound.Stop(); changeLevelSound.Play(); UpdateDisplays(); } }
        if (myControl.ButtonDown(8UL, 12UL) || (myControl.ButtonHeld(8UL, 12UL, 0.45f, out placeHolder) && frameRepeatScroll == 0)) { frameRepeatScroll = 8; if (levelSelected < levelsFound.Count - 3) { levelSelected += 3; changeLevelSound.Stop(); changeLevelSound.Play(); UpdateDisplays(); } }

        scrollInset.localPosition = new Vector3(Mathf.Lerp(scrollInset.localPosition.x, tabPosition, 0.25f), 0, 0);
        dispInset.localPosition = new Vector3(0, Mathf.Lerp(dispInset.localPosition.y, dispPosition, 0.25f), 0);

        if (activeDispOutline)
        {
            activeDispOutline.color = new Color(1f, 1f, 1f, (float)(0.75 + (0.25 * Math.Sin(DoubleTime.UnscaledTimeSinceLoad * 6.0))));
            if (myControl.ButtonDown(16UL, 16UL)) { submitSound.Stop(); submitSound.Play(); OpenConfirmMenu(); }
        }
        else if (!extraDispUpdate)
        {
            extraDispUpdate = true;
            UpdateDisplays();
        }

        if (frameRepeatScroll > 0) { --frameRepeatScroll; }
    }
}
