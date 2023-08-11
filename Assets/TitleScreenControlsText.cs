using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TitleScreenControlsText : MonoBehaviour
{
    private string origText = "";
    private Text textObj;
    private InputActionAsset iaa = null;
    private InputAction dpadAction;
    private InputAction crossAction;
    private InputAction squareAction;
    private InputAction startAction;
    private InputAction selectAction;
    public bool updatesEveryFrame = true;

    private void RealUpdate()
    {
        if (SecondaryTitleMenu.open) { textObj.text = ""; return; }
        if (dpadAction == null || crossAction == null || squareAction == null) { return; }

        string newText = origText;
        newText = newText.Replace("<Up>", dpadAction.bindings[1].ToDisplayString());
        newText = newText.Replace("<Down>", dpadAction.bindings[2].ToDisplayString());
        newText = newText.Replace("<Left>", dpadAction.bindings[3].ToDisplayString());
        newText = newText.Replace("<Right>", dpadAction.bindings[4].ToDisplayString());
        newText = newText.Replace("<Cross>", crossAction.bindings[0].ToDisplayString());
        newText = newText.Replace("<Square>", squareAction.bindings[0].ToDisplayString());
        newText = newText.Replace("<Start>", startAction.bindings[0].ToDisplayString());
        newText = newText.Replace("<Select>", selectAction.bindings[0].ToDisplayString());
        textObj.text = newText;
    }

    private void Start()
    {
        textObj = GetComponent<Text>();
        origText = textObj.text;
        iaa = Resources.Load<InputActionAsset>("ControlsMain");
        dpadAction = iaa.FindAction("DPad");
        crossAction = iaa.FindAction("Cross");
        squareAction = iaa.FindAction("Square");
        startAction = iaa.FindAction("Start");
        selectAction = iaa.FindAction("Select");
        RealUpdate();
    }

    private void OnEnable()
    {
        RealUpdate();
    }

    // The CPU isn't doing too much during the title screen (unlike the GPU) so we can get away with forgoing events.
    private void Update()
    {
        if (updatesEveryFrame) { RealUpdate(); }
    }
}
