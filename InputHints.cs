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
        public static event Action UsedDeviceChanged;

        #region PROPERTIES

        public static InputDevice UsedDevice { get; private set; }
        public static InputDevice SecondaryUsedDevice { get; private set; }

        #endregion



        public static void SetSinglePlayUsedInputDevice(InputDevice device)
        {
            if (device != UsedDevice)
            {
                UsedDevice = device;
                UsedDeviceChanged?.Invoke();
            }

            SecondaryUsedDevice = null;
        }

        public static void SetCoopPlayUsedInputDevice(InputDevice mainDevice, InputDevice secondaryDevice)
        {
            if (mainDevice != UsedDevice)
            {
                UsedDevice = mainDevice;
                UsedDeviceChanged?.Invoke();
            }

            SecondaryUsedDevice = secondaryDevice;
        }
        
    }
}