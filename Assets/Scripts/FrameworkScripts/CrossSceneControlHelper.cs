using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CrossSceneControlHelper : MBSingleton<CrossSceneControlHelper>, ControlsMainCS.IMainActions
{
    private ControlsMainCS controlsMainCS;
    public ulong controlBuffer = 0UL;
    public float horizontalPressure = 0f; //The current left-right pressure on the analog stick/ D-PAD from 0f to 1f.
    public float verticalPressure = 0f; //The current up-down pressure on the analog stick/ D-PAD from 0f to 1f.
    public float horizontalPressureRaw = 0f; //-1 to 1
    public float verticalPressureRaw = 0f; //-1 to 1
    public static bool fellBackToDefaults = false; // flag to display a special dialog on game beginning

    private void FallBackToDefaults()
    {
        fellBackToDefaults = true;
        controlsMainCS = new ControlsMainCS();
        var actions = controlsMainCS.asset.FindActionMap("Main").actions;
        for (int i = 0; i < actions.Count; ++i)
        {
            InputAction a = actions[i];
            var bindings = a.bindings;
            for (int j = 0; j < bindings.Count; ++j)
            {
                InputBinding b = bindings[j];
                a.Disable();
                a.ApplyBindingOverride(j, b.path);
                a.Enable();
                InSaveRemapControls.SaveDefaultBindingOverride(b);
            }
        }
    }

    protected override void ChildAwake()
    {
        controlsMainCS = new ControlsMainCS();
        var actions = controlsMainCS.asset.FindActionMap("Main").actions;
        bool mustFallback = false;
        for (int i = 0; i < actions.Count; ++i)
        {
            InputAction a = actions[i];
            var bindings = a.bindings;
            for (int j = 0; j < bindings.Count; ++j)
            {
                InputBinding b = bindings[j];
                string overridePath = InSaveRemapControls.LoadBindingOverridePath(b);
                string baseOfPath = overridePath.Split('/')[0];
                string origPath = b.path;
                if (overridePath != b.path)
                {
                    a.Disable();
                    if (baseOfPath == "<Gamepad>" && Gamepad.current == null)
                    {
                        print("gamepad is absent! falling back to default keyboard control");
                        mustFallback = true;
                        break;
                    }
                    else
                    {
                        a.ApplyBindingOverride(j, overridePath);
                    }
                    a.Enable();
                }
            }
            if (mustFallback) { break; }
        }
        if (mustFallback) { FallBackToDefaults(); }
        controlsMainCS.Main.Enable();
        controlsMainCS.Main.SetCallbacks(this);

    }

    void ControlsMainCS.IMainActions.OnDPad(InputAction.CallbackContext context)
    {
        Vector2 v = (Vector2)context.ReadValueAsObject();
        controlBuffer &= (~15UL);
        horizontalPressure = horizontalPressureRaw = 0f;
        horizontalPressureRaw = v.x;
        if (v.x < -0.3f) { controlBuffer |= 1UL; horizontalPressure = -v.x; }
        if (v.x > 0.3f) { controlBuffer |= 2UL; horizontalPressure = v.x; }
        verticalPressure = verticalPressureRaw = 0f;
        verticalPressureRaw = v.y;
        if (v.y > 0.5f) { controlBuffer |= 4UL; verticalPressure = v.y; }
        if (v.y < -0.5f) { controlBuffer |= 8UL; verticalPressure = -v.y; }
    }

    private void NewInputButtonVal(float v, ulong mask)
    {
        controlBuffer &= (~mask);
        if (v >= 0.5f) { controlBuffer |= mask; }
    }

    private void NewInputDoubleButtonVal(float v, ulong maskMinus, ulong maskPlus)
    {
        controlBuffer &= (~maskPlus);
        controlBuffer &= (~maskMinus);
        if (v >= 0.5f) { controlBuffer |= maskPlus; }
        if (v <= -0.5f) { controlBuffer |= maskMinus; }
    }

    void ControlsMainCS.IMainActions.OnCross(InputAction.CallbackContext context)
    {
        NewInputButtonVal((float)context.ReadValueAsObject(), 16UL);
    }

    void ControlsMainCS.IMainActions.OnSquare(InputAction.CallbackContext context)
    {
        NewInputButtonVal((float)context.ReadValueAsObject(), 32UL);
    }

    void ControlsMainCS.IMainActions.OnCircle(InputAction.CallbackContext context)
    {
        NewInputButtonVal((float)context.ReadValueAsObject(), 64UL);
    }

    void ControlsMainCS.IMainActions.OnTriangle(InputAction.CallbackContext context)
    {
        NewInputButtonVal((float)context.ReadValueAsObject(), 128UL);
    }

    void ControlsMainCS.IMainActions.OnShoulders1(InputAction.CallbackContext context)
    {
        NewInputDoubleButtonVal((float)context.ReadValueAsObject(), 256UL, 512UL);
    }

    void ControlsMainCS.IMainActions.OnShoulders2(InputAction.CallbackContext context)
    {
        NewInputDoubleButtonVal((float)context.ReadValueAsObject(), 4096UL, 8192UL);
    }

    void ControlsMainCS.IMainActions.OnStart(InputAction.CallbackContext context)
    {
        NewInputButtonVal((float)context.ReadValueAsObject(), 1024UL);
    }

    void ControlsMainCS.IMainActions.OnSelect(InputAction.CallbackContext context)
    {
        NewInputButtonVal((float)context.ReadValueAsObject(), 2048UL);
    }
}
