using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Linq;
using HR_Toolkit;
using Unity.VisualScripting;
using Valve.VR;

public class PressableButton : RedirectionObject
{
    #region Classes/Structs/Enums

    [Serializable]
    public class DHUI_ButtonActivationEvent : UnityEvent<DHUI_ButtonActivationEventArgs> { }

    public enum GlobalLocalMode
    {
        Global, Local
    }
    public struct DHUI_ButtonActivationEventArgs
    {
        public float activationDuration;
    }

    #endregion Classes/Structs/Enums

    #region Inspector Fields

    [Header("Button.Setup")]
    [SerializeField]
    protected Transform m_buttonPressValue_point1;
    [SerializeField]
    protected Transform m_buttonPressValue_point2;
    [SerializeField]
    protected MeshRenderer m_visualButton; 

    [Header("Button.Settings")]
    [SerializeField]
    protected GlobalLocalMode _activationDistance_mode = GlobalLocalMode.Global;
    [SerializeField]
    protected float _activationDistance_threshold = 0.15f;
    [SerializeField]
    protected float _activationCooldown = 0.5f;

    [Header("Button.Events")]
    public DHUI_ButtonActivationEvent OnActivationStart = null;
    public DHUI_ButtonActivationEvent OnActivationStay = null;
    public DHUI_ButtonActivationEvent OnActivationEnd = null;

    #endregion Inspector Fields

    private bool internal_buttonActivationState = false;

    /// <summary>
    /// Current Activation State of the button. (true -> Activated, false -> Not activated).
    /// </summary>
    public bool ButtonActivationState
    {
        get
        {
            return internal_buttonActivationState;
        }
        protected set
        {
            if (value == internal_buttonActivationState) return;
            if (value)
            {
                float currentTime = Time.time;
                if (currentTime >= lastActivated + _activationCooldown)
                {
                    lastActivated = currentTime;
                    Activation_Start(ConstructActivationEventArgs());
                }
                else
                {
                    return;
                }
            }
            else
            {
                Activation_End(ConstructActivationEventArgs());
            }

            internal_buttonActivationState = value;
        }
    }

    /// <summary>
    /// Current Distance between 'm_buttonPressValue_point1' and 'm_buttonPressValue_point2'. This represents the current value of the button press.
    /// </summary>
    protected float currentActivationDistance = 0f;

    private float lastActivated = 0f;

    private float currentActivationDuration
    {
        get { return Time.time - lastActivated; }
    }

    private void FixedUpdate()
    {
        UpdateActivationCalculations();
        UpdateActivationStay();
    }

    protected virtual void UpdateActivationCalculations()
    {
        if (_activationDistance_mode == GlobalLocalMode.Global)
        {
            currentActivationDistance = Vector3.Distance(m_buttonPressValue_point1.position, m_buttonPressValue_point2.position);
        }
        else
        {
            currentActivationDistance = Vector3.Distance(m_buttonPressValue_point1.localPosition, m_buttonPressValue_point2.localPosition);
        }

        if (currentActivationDistance > _activationDistance_threshold)
        {
            ButtonActivationState = true;
        }
        else
        {
            ButtonActivationState = false;
        }
    }


    #region OnActivation

    public virtual void Activation_Start(DHUI_ButtonActivationEventArgs _buttonActivationEventArgs)
    {
        OnActivationStart?.Invoke(_buttonActivationEventArgs);
    }

    public virtual void Activation_End(DHUI_ButtonActivationEventArgs _buttonActivationEventArgs)
    {
        OnActivationEnd?.Invoke(_buttonActivationEventArgs);
    }

    public virtual void Activation_Stay(DHUI_ButtonActivationEventArgs _buttonActivationEventArgs)
    {
        OnActivationStay?.Invoke(_buttonActivationEventArgs);
    }

    protected DHUI_ButtonActivationEventArgs ConstructActivationEventArgs()
    {
        DHUI_ButtonActivationEventArgs args = new DHUI_ButtonActivationEventArgs();
        args.activationDuration = currentActivationDuration;
        return args;
    }

    protected void UpdateActivationStay()
    {
        if (ButtonActivationState)
        {
            Activation_Stay(ConstructActivationEventArgs());
        }
    }

    #endregion OnActivation


    public void ColorButton(Color color = default)
    {
        m_visualButton.material.SetColor("_Color", color);
    }
}
