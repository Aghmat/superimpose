﻿using System;
using Cinemachine;
using Cinemachine.Utility;
using UnityEngine;

//    https://forum.unity.com/threads/free-look-camera-and-mouse-responsiveness.642886/
[Serializable]
public struct CinemachineInputAxisDriver
{
    [Tooltip("Multiply the input by this amount prior to processing.  Controls the input power.")]
    public float sensitivity;

    [Tooltip("Overrides default Cinemachine camera inversion settings")]
    public bool isInverted;

    [Tooltip("X and Y axis have different sensitivity factors for some absurd reason")] [HideInInspector]
    public float multiplier;

    [Tooltip("The amount of time in seconds it takes to accelerate to a higher speed")]
    public float accelerationTime;

    [Tooltip("The amount of time in seconds it takes to decelerate to a lower speed")]
    public float decelerationTime;

    [Tooltip("The name of this axis as specified in Unity Input manager. "
             + "Setting to an empty string will disable the automatic updating of this axis")]
    public string name;

    [NoSaveDuringPlay]
    [Tooltip("The value of the input axis.  A value of 0 means no input.  You can drive "
             + "this directly from a custom input system, or you can set the Axis Name and "
             + "have the value driven by the internal Input Manager")]
    public float inputValue;

    /// Internal state
    private float mCurrentSpeed;

    private const float Epsilon = UnityVectorExtensions.Epsilon;

    /// Call from OnValidate: Make sure the fields are sensible
    public void Validate()
    {
        accelerationTime = Mathf.Max(0, accelerationTime);
        decelerationTime = Mathf.Max(0, decelerationTime);
    }

    public bool Update(float deltaTime, ref AxisState axis)
    {
        if (!string.IsNullOrEmpty(name))
            try
            {
                inputValue = CinemachineCore.GetInputAxis(name);
            }
            catch (ArgumentException)
            {
            }
        //catch (ArgumentException e) { Debug.LogError(e.ToString()); }

        var inverted = isInverted ? -1 : 1;
        var input = inputValue * sensitivity * multiplier * inverted;
        if (deltaTime < Epsilon)
        {
            mCurrentSpeed = 0;
        }
        else
        {
            var speed = input / deltaTime;
            var dampTime = Mathf.Abs(speed) < Mathf.Abs(mCurrentSpeed) ? decelerationTime : accelerationTime;
            speed = mCurrentSpeed + Damper.Damp(speed - mCurrentSpeed, dampTime, deltaTime);
            mCurrentSpeed = speed;

            // Decelerate to the end points of the range if not wrapping
            var range = axis.m_MaxValue - axis.m_MinValue;
            if (!axis.m_Wrap && decelerationTime > Epsilon && range > Epsilon)
            {
                var v0 = ClampValue(ref axis, axis.Value);
                var v = ClampValue(ref axis, v0 + speed * deltaTime);
                var d = speed > 0 ? axis.m_MaxValue - v : v - axis.m_MinValue;
                if (d < 0.1f * range && Mathf.Abs(speed) > Epsilon)
                    speed = Damper.Damp(v - v0, decelerationTime, deltaTime) / deltaTime;
            }

            input = speed * deltaTime;
        }

        axis.Value = ClampValue(ref axis, axis.Value + input);
        return Mathf.Abs(inputValue) > Epsilon;
    }

    private float ClampValue(ref AxisState axis, float v)
    {
        var r = axis.m_MaxValue - axis.m_MinValue;
        if (axis.m_Wrap && r > Epsilon)
        {
            v = (v - axis.m_MinValue) % r;
            v += axis.m_MinValue + (v < 0 ? r : 0);
        }

        return Mathf.Clamp(v, axis.m_MinValue, axis.m_MaxValue);
    }
}


[RequireComponent(typeof(CinemachineFreeLook))]
[DisallowMultipleComponent]
public class FreeLookAxisDriver : MonoBehaviour
{
    private CinemachineFreeLook freeLook;
    public CinemachineInputAxisDriver xAxis;
    public CinemachineInputAxisDriver yAxis;

    private void Awake()
    {
        freeLook = GetComponent<CinemachineFreeLook>();
        freeLook.m_XAxis.m_MaxSpeed = freeLook.m_XAxis.m_AccelTime = freeLook.m_XAxis.m_DecelTime = 0;
        freeLook.m_XAxis.m_InputAxisName = string.Empty;
        freeLook.m_YAxis.m_MaxSpeed = freeLook.m_YAxis.m_AccelTime = freeLook.m_YAxis.m_DecelTime = 0;
        freeLook.m_YAxis.m_InputAxisName = string.Empty;
    }

    private void OnValidate()
    {
        xAxis.Validate();
        yAxis.Validate();
    }

    private void Reset()
    {
        xAxis = new CinemachineInputAxisDriver
        {
            sensitivity = 1,
            multiplier = 10,
            isInverted = false,
            accelerationTime = 0.1f,
            decelerationTime = 0.1f,
            name = "Mouse X"
        };
        yAxis = new CinemachineInputAxisDriver
        {
            sensitivity = 1,
            multiplier = -0.1f,
            isInverted = false,
            accelerationTime = 0.1f,
            decelerationTime = 0.1f,
            name = "Mouse Y"
        };
    }

    private void Update()
    {
        if (Time.timeScale == 0) return;
        var changed = xAxis.Update(Time.deltaTime, ref freeLook.m_XAxis);
        if (yAxis.Update(Time.deltaTime, ref freeLook.m_YAxis))
            changed = true;
        if (changed)
        {
            freeLook.m_RecenterToTargetHeading.CancelRecentering();
            freeLook.m_YAxisRecentering.CancelRecentering();
        }
    }
}