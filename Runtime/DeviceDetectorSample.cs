// (C)2024 @noio_games
// Thomas van den Berg

using games.noio.InputHints;
using UnityEngine;
using UnityEngine.InputSystem;

namespace games.noio.InputHints
{
    public class DeviceDetectorSample : MonoBehaviour
    {
        #region PUBLIC AND SERIALIZED FIELDS

        [SerializeField] InputActionReference _moveAction;
        [SerializeField] InputActionReference _lookAction;
        [SerializeField] InputActionReference _fireAction;

        #endregion

        #region MONOBEHAVIOUR METHODS

        void Awake()
        {
            /*
             * The way I like to do switching of input is just to listen for
             * the most used actions in a game: moving and the 'primary' action
             * (such as "use" or "fire")
             * And then switch hints depending on the last device used for those actions.
             * That means if the player picks up a new gamepad and wiggles the stick,
             * or presses the "A" button (or equivalent), the input hints change immediately.
             *
             * I'm using "Move" and "Fire" here because those are defined in Unity's 'Default Input Actions'
             */
            if (_moveAction.action != null)
            {
                _moveAction.action.Enable();
                _moveAction.action.performed += HandleMoveActionPerformed;
            }

            if (_lookAction.action != null)
            {
                _lookAction.action.Enable();
                _lookAction.action.performed += HandleLookActionPerformed;
            }

            if (_fireAction.action != null)
            {
                _fireAction.action.Enable();
                _fireAction.action.performed += HandleFireActionPerformed;
            }
        }

        #endregion

        void HandleLookActionPerformed(InputAction.CallbackContext ctx)
        {
            InputHints.SetUsedDevice(ctx.control.device);
        }

        void HandleFireActionPerformed(InputAction.CallbackContext ctx)
        {
            InputHints.SetUsedDevice(ctx.control.device);
        }

        void HandleMoveActionPerformed(InputAction.CallbackContext ctx)
        {
            InputHints.SetUsedDevice(ctx.control.device);
        }
    }
}