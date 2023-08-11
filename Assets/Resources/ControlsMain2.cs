// GENERATED AUTOMATICALLY FROM 'Assets/Resources/ControlsMain.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @ControlsMainCS : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @ControlsMainCS()
    {
        asset = Resources.Load<InputActionAsset>("ControlsMain");
        // Main
        m_Main = asset.FindActionMap("Main", throwIfNotFound: true);
        m_Main_DPad = m_Main.FindAction("DPad", throwIfNotFound: true);
        m_Main_Cross = m_Main.FindAction("Cross", throwIfNotFound: true);
        m_Main_Square = m_Main.FindAction("Square", throwIfNotFound: true);
        m_Main_Circle = m_Main.FindAction("Circle", throwIfNotFound: true);
        m_Main_Triangle = m_Main.FindAction("Triangle", throwIfNotFound: true);
        m_Main_Shoulders1 = m_Main.FindAction("Shoulders1", throwIfNotFound: true);
        m_Main_Shoulders2 = m_Main.FindAction("Shoulders2", throwIfNotFound: true);
        m_Main_Start = m_Main.FindAction("Start", throwIfNotFound: true);
        m_Main_Select = m_Main.FindAction("Select", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Main
    private readonly InputActionMap m_Main;
    private IMainActions m_MainActionsCallbackInterface;
    private readonly InputAction m_Main_DPad;
    private readonly InputAction m_Main_Cross;
    private readonly InputAction m_Main_Square;
    private readonly InputAction m_Main_Circle;
    private readonly InputAction m_Main_Triangle;
    private readonly InputAction m_Main_Shoulders1;
    private readonly InputAction m_Main_Shoulders2;
    private readonly InputAction m_Main_Start;
    private readonly InputAction m_Main_Select;
    public struct MainActions
    {
        private @ControlsMainCS m_Wrapper;
        public MainActions(@ControlsMainCS wrapper) { m_Wrapper = wrapper; }
        public InputAction @DPad => m_Wrapper.m_Main_DPad;
        public InputAction @Cross => m_Wrapper.m_Main_Cross;
        public InputAction @Square => m_Wrapper.m_Main_Square;
        public InputAction @Circle => m_Wrapper.m_Main_Circle;
        public InputAction @Triangle => m_Wrapper.m_Main_Triangle;
        public InputAction @Shoulders1 => m_Wrapper.m_Main_Shoulders1;
        public InputAction @Shoulders2 => m_Wrapper.m_Main_Shoulders2;
        public InputAction @Start => m_Wrapper.m_Main_Start;
        public InputAction @Select => m_Wrapper.m_Main_Select;
        public InputActionMap Get() { return m_Wrapper.m_Main; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MainActions set) { return set.Get(); }
        public void SetCallbacks(IMainActions instance)
        {
            if (m_Wrapper.m_MainActionsCallbackInterface != null)
            {
                @DPad.started -= m_Wrapper.m_MainActionsCallbackInterface.OnDPad;
                @DPad.performed -= m_Wrapper.m_MainActionsCallbackInterface.OnDPad;
                @DPad.canceled -= m_Wrapper.m_MainActionsCallbackInterface.OnDPad;
                @Cross.started -= m_Wrapper.m_MainActionsCallbackInterface.OnCross;
                @Cross.performed -= m_Wrapper.m_MainActionsCallbackInterface.OnCross;
                @Cross.canceled -= m_Wrapper.m_MainActionsCallbackInterface.OnCross;
                @Square.started -= m_Wrapper.m_MainActionsCallbackInterface.OnSquare;
                @Square.performed -= m_Wrapper.m_MainActionsCallbackInterface.OnSquare;
                @Square.canceled -= m_Wrapper.m_MainActionsCallbackInterface.OnSquare;
                @Circle.started -= m_Wrapper.m_MainActionsCallbackInterface.OnCircle;
                @Circle.performed -= m_Wrapper.m_MainActionsCallbackInterface.OnCircle;
                @Circle.canceled -= m_Wrapper.m_MainActionsCallbackInterface.OnCircle;
                @Triangle.started -= m_Wrapper.m_MainActionsCallbackInterface.OnTriangle;
                @Triangle.performed -= m_Wrapper.m_MainActionsCallbackInterface.OnTriangle;
                @Triangle.canceled -= m_Wrapper.m_MainActionsCallbackInterface.OnTriangle;
                @Shoulders1.started -= m_Wrapper.m_MainActionsCallbackInterface.OnShoulders1;
                @Shoulders1.performed -= m_Wrapper.m_MainActionsCallbackInterface.OnShoulders1;
                @Shoulders1.canceled -= m_Wrapper.m_MainActionsCallbackInterface.OnShoulders1;
                @Shoulders2.started -= m_Wrapper.m_MainActionsCallbackInterface.OnShoulders2;
                @Shoulders2.performed -= m_Wrapper.m_MainActionsCallbackInterface.OnShoulders2;
                @Shoulders2.canceled -= m_Wrapper.m_MainActionsCallbackInterface.OnShoulders2;
                @Start.started -= m_Wrapper.m_MainActionsCallbackInterface.OnStart;
                @Start.performed -= m_Wrapper.m_MainActionsCallbackInterface.OnStart;
                @Start.canceled -= m_Wrapper.m_MainActionsCallbackInterface.OnStart;
                @Select.started -= m_Wrapper.m_MainActionsCallbackInterface.OnSelect;
                @Select.performed -= m_Wrapper.m_MainActionsCallbackInterface.OnSelect;
                @Select.canceled -= m_Wrapper.m_MainActionsCallbackInterface.OnSelect;
            }
            m_Wrapper.m_MainActionsCallbackInterface = instance;
            if (instance != null)
            {
                @DPad.started += instance.OnDPad;
                @DPad.performed += instance.OnDPad;
                @DPad.canceled += instance.OnDPad;
                @Cross.started += instance.OnCross;
                @Cross.performed += instance.OnCross;
                @Cross.canceled += instance.OnCross;
                @Square.started += instance.OnSquare;
                @Square.performed += instance.OnSquare;
                @Square.canceled += instance.OnSquare;
                @Circle.started += instance.OnCircle;
                @Circle.performed += instance.OnCircle;
                @Circle.canceled += instance.OnCircle;
                @Triangle.started += instance.OnTriangle;
                @Triangle.performed += instance.OnTriangle;
                @Triangle.canceled += instance.OnTriangle;
                @Shoulders1.started += instance.OnShoulders1;
                @Shoulders1.performed += instance.OnShoulders1;
                @Shoulders1.canceled += instance.OnShoulders1;
                @Shoulders2.started += instance.OnShoulders2;
                @Shoulders2.performed += instance.OnShoulders2;
                @Shoulders2.canceled += instance.OnShoulders2;
                @Start.started += instance.OnStart;
                @Start.performed += instance.OnStart;
                @Start.canceled += instance.OnStart;
                @Select.started += instance.OnSelect;
                @Select.performed += instance.OnSelect;
                @Select.canceled += instance.OnSelect;
            }
        }
    }
    public MainActions @Main => new MainActions(this);
    private int m_KeyboardSchemeIndex = -1;
    public InputControlScheme KeyboardScheme
    {
        get
        {
            if (m_KeyboardSchemeIndex == -1) m_KeyboardSchemeIndex = asset.FindControlSchemeIndex("Keyboard");
            return asset.controlSchemes[m_KeyboardSchemeIndex];
        }
    }
    public interface IMainActions
    {
        void OnDPad(InputAction.CallbackContext context);
        void OnCross(InputAction.CallbackContext context);
        void OnSquare(InputAction.CallbackContext context);
        void OnCircle(InputAction.CallbackContext context);
        void OnTriangle(InputAction.CallbackContext context);
        void OnShoulders1(InputAction.CallbackContext context);
        void OnShoulders2(InputAction.CallbackContext context);
        void OnStart(InputAction.CallbackContext context);
        void OnSelect(InputAction.CallbackContext context);
    }
}
