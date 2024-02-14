using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

// ReSharper disable once CheckNamespace
namespace games.noio.InputHints
{
    public static class InputHints
    {
        public static event Action<InputDevice> UsedDeviceChanged;

        #region PROPERTIES

        public static InputDevice UsedDevice { get; private set; }

        #endregion

        public static void SetUsedDevice(InputDevice device)
        {
            if (device != UsedDevice)
            {
                UsedDevice = device;
                UsedDeviceChanged?.Invoke(device);
            }
        }
    }
}