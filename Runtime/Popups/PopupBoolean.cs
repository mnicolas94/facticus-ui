#if ENABLED_INPUTSYSTEM

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Facticus.UI.Utils;
using Facticus.UI.WindowInterfaces;
using UnityEngine;
using UnityEngine.UI;

namespace Facticus.UI.Popups
{
    [AddComponentMenu("Facticus.UI/Popups/Popup Boolean (Yes/No)")]
    public class PopupBoolean : MonoBehaviour, IWindowWithResult<bool>
    {
        [SerializeField] private Button _affirmativeButton;
        [SerializeField] private Button _negativeButton;

        private readonly Button[] _buttonsArray = new Button[2];
        private readonly UniTask[] _tasksArray = new UniTask[2];

        private void Awake()
        {
            _buttonsArray[0] = _affirmativeButton;
            _buttonsArray[1] = _negativeButton;
        }

        public async UniTask<bool> WaitForResult(CancellationToken ct)
        {
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var linkedCt = linkedCts.Token;

            try
            {
                var backButtonTask = AsyncUtils.WaitPressBackButton(linkedCt);
                var pressedButtonsTask = AsyncUtils.WaitFirstButtonPressedAsync(linkedCt, _buttonsArray);
                
                _tasksArray[0] = backButtonTask;
                _tasksArray[1] = pressedButtonsTask;
                
                var firstTaskIndex = await UniTask.WhenAny(_tasksArray);

                if (firstTaskIndex == 1)
                {
                    var pressedButton = await pressedButtonsTask;
                    return pressedButton == _affirmativeButton;
                }

                return false;
            }
            finally
            {
                linkedCts.Cancel();
                linkedCts.Dispose();
            }
        }
    }
}

#endif