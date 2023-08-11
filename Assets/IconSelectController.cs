using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconSelectController : InSaveMenuBase
{
    public static int result;
    public static bool canClose = false;
    public int curr = 1;
    private int last = -1;
    public GameObject iconSample;
    private List<Image> iconHighlights = new List<Image>();
    private List<Image> iconImages = new List<Image>();
    public Image preview;

    public AudioSource changeSound;
    public AudioSource submitSound;

    private int cursorHoldCooldown = 0;

    protected override void ChildOpen()
    {
        result = -1;
        canCloseByPlayer = canClose;
        // generate icons
        int i = 0;
        while (true)
        {
            Sprite s;
            try { s = Resources.Load<Sprite>("SaveIcons/" + i.ToString()); }
            catch { break; } // catch a break!
            if (s == null) { break; }
            GameObject newIcon = Instantiate(iconSample, iconSample.transform.parent);
            Transform newT = newIcon.transform;
            Image newH = newT.GetChild(0).GetComponent<Image>();
            newH.color = Color.clear;
            iconHighlights.Add(newH);
            Image newI = newT.GetChild(1).GetComponent<Image>();
            newI.sprite = s;
            iconImages.Add(newI);
            ++i;
        }
        Destroy(iconSample.gameObject);
        int saved = Utilities.loadedSaveData.icon;
        if (saved < 0 || saved >= iconImages.Count) { SelectNext(0); }
        else { SelectNext(saved); }
    }

    private void SelectNext(int n)
    {
        if (n < 0 || n >= iconImages.Count) { return; }
        bool lastInRange = (curr >= 0 && curr < iconImages.Count);
        if (lastInRange) { iconHighlights[curr].color = Color.clear; }
        iconHighlights[n].color = Color.white;
        preview.sprite = iconImages[n].sprite;
        last = curr;
        curr = n;
        changeSound.Stop();
        changeSound.Play();
    }

    private bool DPadInput(ulong x)
    {
        Encontrolmentation e = myControl;
        if (e.ButtonDown(x, 15UL)) { cursorHoldCooldown = 0; return true; }
        if (e.ButtonHeld(x, 15UL, 0.5f, out _))
        {
            if (cursorHoldCooldown <= 0) { cursorHoldCooldown = 12; return true; }
            else { --cursorHoldCooldown; return false; }
        }
        return false;
    }

    protected override void ChildUpdate()
    {
        if (DPadInput(1UL)) { SelectNext(curr - 1); }
        if (DPadInput(2UL)) { SelectNext(curr + 1); }
        if (DPadInput(4UL)) { SelectNext(curr - 8); }
        if (DPadInput(8UL)) { SelectNext(curr + 8); }
        if (myControl.ButtonDown(16UL, 16UL))
        {
            result = curr;
            GameObject subSnd2 = Instantiate(submitSound.gameObject, null);
            subSnd2.GetComponent<AudioSource>().Play();
            Destroy(subSnd2, 1f);
            Close();
        }
    }
}
