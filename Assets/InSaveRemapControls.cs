using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InSaveRemapControls : SecondaryTitleMenu
{
    // A lot of stuff copied from MenuScrollHandler...
    // We could have inherited! but we'll see if this actually poses a problem or not.
    private bool looping = true;
    public int index = 0;
    private int count = 14;
    public float holdKeyStartDelay;
    public float holdKeyScrollWait;
    private double lastScrollTime;
    public string[] actionNames;
    [HideInInspector]
    public GameObject[] rows = null;
    [HideInInspector]
    public Text[] rowButtonTexts = null;
    [HideInInspector]
    public Image[] rowButtons = null;
    [HideInInspector]
    public InputAction[] rowActions = new InputAction[14];
    public int[] bindingIndices = new int[14];
    public Color panelColorRest;
    public Color panelColorActive;
    public Text command;

    [HideInInspector]
    public int selected = -1;

    public AudioSource changeSound;
    public AudioSource selectSound;
    public AudioSource completeSound;
    public AudioSource conflictSound;

    private InputActionAsset iaa;

    protected override void ChildClose()
    {
        open = false;
    }

    protected override void ChildOpen()
    {
        selectCooldown = 3;
        command.text = "✕ = Select       □ = Exit";
        selectButtonEverWentDown = false;
        open = true;
        rows = new GameObject[14];
        rowButtonTexts = new Text[14];
        rowActions = new InputAction[14];
        rowButtons = new Image[14];
        iaa = Resources.Load<InputActionAsset>("ControlsMain");
        for (int i = 0; i < 14; ++i)
        {
            rows[i] = transform.GetChild(i).gameObject;
            rowButtons[i] = transform.GetChild(i).GetChild(1).GetComponent<Image>();
            rowButtonTexts[i] = transform.GetChild(i).GetChild(1).GetChild(0).GetComponent<Text>();
            rowActions[i] = iaa.FindAction(actionNames[i]);
            rowButtonTexts[i].text = rowActions[i].bindings[bindingIndices[i]].ToDisplayString();
        }
        Highlight(index);
    }

    public void Highlight(int i)
    {
        rows[i].GetComponent<Image>().color = Color.Lerp(panelColorRest, panelColorActive, (float)(0.75 + 0.25 * System.Math.Sin(DoubleTime.UnscaledTimeSinceLoad * 12.0)));
    }

    public void UnHighlight(int i)
    {
        rows[i].GetComponent<Image>().color = panelColorRest;
    }

    private Color originalSelectionColor;

    private IEnumerator WhileSelected(int currSelected)
    {
        int t = 0;
        while (selected == currSelected)
        {
            rowButtons[currSelected].color = (t >= 9) ? panelColorActive : panelColorRest;
            t = (t + 1) % 18;
            yield return new WaitForEndOfFrame();
        }
        rowButtons[currSelected].color = originalSelectionColor;
    }

    private void SelectAnim(bool conflict = false)
    {
        command.text = (conflict ? "Conflict! " : "") + "Press " + rows[selected].transform.GetChild(0).GetComponent<Text>().text + " now.";
        originalSelectionColor = rowButtons[selected].color;
        StartCoroutine(WhileSelected(selected));
        rowActions[selected].Disable();
        rowActions[selected].PerformInteractiveRebinding(bindingIndices[selected])
            //.WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.05f)
            .OnComplete(RebindComplete)
            .Start();
        canCloseByPlayer = false;
    }

    private HashSet<InputBinding> bindingsToSave = new HashSet<InputBinding>();

    public static string LoadBindingOverridePath(InputBinding b)
    {
        string prefName = "Rebind_" + b.action + "_" + b.name;
        return PlayerPrefs.GetString(prefName, b.path);
    }

    private void SaveBindingOverrides()
    {
        foreach (InputBinding b in bindingsToSave)
        {
            string prefName = "Rebind_" + b.action + "_" + b.name;
            PlayerPrefs.SetString(prefName, b.overridePath);
        }
        PlayerPrefs.Save();
        bindingsToSave.Clear();
    }

    public static void SaveDefaultBindingOverride(InputBinding b)
    {
        string prefName = "Rebind_" + b.action + "_" + b.name;
        PlayerPrefs.SetString(prefName, b.path);
        PlayerPrefs.Save();
    }

    private void RebindComplete(InputActionRebindingExtensions.RebindingOperation op)
    {
        InputBinding myBinding = rowActions[selected].bindings[bindingIndices[selected]];
        bindingsToSave.Add(myBinding);
        rowButtonTexts[selected].text = myBinding.ToDisplayString();
        rowActions[selected].Enable();
        op.Dispose();

        completeSound.Stop(); completeSound.Play();

        // watch out for duplicates!
        int conflict = -1;
        
        for (int i = 0; i < 14; ++i)
        {
            InputBinding otherBinding = rowActions[i].bindings[bindingIndices[i]];
            if (myBinding.effectivePath == otherBinding.effectivePath && i != selected)
            {
                conflict = i; break;
            }
        }

        if (conflict == -1) {
            selected = -1;
            canCloseByPlayer = true;
            selectCooldown = 3;
            selectButtonEverWentDown = false;
            SaveBindingOverrides();
            command.text = "✕ = Select       □ = Exit";
        }
        else
        {
            selected = conflict;
            SelectAnim(true);

            conflictSound.Stop(); conflictSound.Play();
        }
    }

    private int selectCooldown = 0;

    private bool selectButtonEverWentDown = false;

    protected override void ChildUpdate()
    {
        double t = DoubleTime.UnscaledTimeRunning - lastScrollTime;
        double placeholder = 0.0;
        int oldIndex = index;

        if (selected == -1)
        {
            if (myControl.ButtonDown(4UL, 12UL) || (myControl.ButtonHeld(4UL, 12UL, holdKeyStartDelay, out placeholder) && t >= holdKeyScrollWait))
            {
                lastScrollTime = DoubleTime.UnscaledTimeRunning;
                index = (index == 0) ? ((looping) ? (count - 1) : (0)) : (index - 1); //one line madness
                changeSound.Stop(); changeSound.Play();
            }

            if (myControl.ButtonDown(8UL, 12UL) || (myControl.ButtonHeld(8UL, 12UL, holdKeyStartDelay, out placeholder) && t >= holdKeyScrollWait))
            {
                lastScrollTime = DoubleTime.UnscaledTimeRunning;
                index = (index == count - 1) ? ((looping) ? (0) : (count - 1)) : (index + 1); //lol
                changeSound.Stop(); changeSound.Play();
            }

            if (oldIndex != index)
            {
                UnHighlight(oldIndex);
                // play sound
            }
            Highlight(index);

            if (myControl.ButtonDown(16UL, 16UL) && selectCooldown <= 0)
            {
                selectButtonEverWentDown = true;
                selectSound.Stop(); selectSound.Play();
            }

            if (myControl.ButtonUp(16UL, 16UL) && selectCooldown <= 0 && selectButtonEverWentDown)
            {
                selected = index;
                SelectAnim();
            }

            if (selectCooldown > 0) { --selectCooldown; }
        }
    }
}
