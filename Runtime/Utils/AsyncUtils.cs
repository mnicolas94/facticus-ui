using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
#if ENABLED_INPUTSYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;
using Utils.Input;

namespace Facticus.UI.Utils
{
    public static class AsyncUtils
    {
        public static async UniTask<Button> WaitPressButtonAsync(Button button, CancellationToken ct)
        {
            bool isPressed = false;

            void PressedAction()
            {
                isPressed = true;
            }

            try
            {
                button.onClick.AddListener(PressedAction);

                while (!isPressed && !ct.IsCancellationRequested)
                {
                    await UniTask.Yield();
                }

                return button;
            }
            finally
            {
                button.onClick.RemoveListener(PressedAction);
            }
        }

        public static async UniTask<Button> WaitFirstButtonPressedAsync(CancellationToken ct, params Button[] buttons)
        {
            // return early if all buttons are null
            var areAllNull = buttons.All(button => button == null);
            if (areAllNull)
            {
                return null;
            }

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var linkedCt = linkedCts.Token;

            try
            {
                var tasks = buttons
                    .Where(button => button != null)
                    .Select(button => WaitPressButtonAsync(button, linkedCt));
                var (_, pressedButton) = await UniTask.WhenAny(tasks);

                linkedCts.Cancel();

                return pressedButton;
            }
            finally
            {
                linkedCts.Dispose();
            }
        }

#if ENABLED_INPUTSYSTEM
        public static async UniTask WaitPressBackButton(CancellationToken ct)
        {
            bool isPressed = false;
            void OnPressedAction(InputAction.CallbackContext callbackContext) => isPressed = true;
            var backAction = InputActionUtils.GetBackAction();
            try
            {
                backAction.Enable();
                backAction.performed += OnPressedAction;
                while (!isPressed && !ct.IsCancellationRequested)
                {
                    await UniTask.NextFrame().SuppressCancellationThrow();
                }
            }
            finally
            {
                backAction.performed -= OnPressedAction;
                backAction.Disable();
                backAction.Dispose();
            }
        }
#endif
    }
}