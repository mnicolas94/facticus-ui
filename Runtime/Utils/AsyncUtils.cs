using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace UI.Utils
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
    }
}