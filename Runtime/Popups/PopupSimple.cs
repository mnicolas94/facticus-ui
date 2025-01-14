using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UI.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popups
{
    public class PopupSimple : MonoBehaviour, IWindowPopup
    {
        [SerializeField] private List<Button> _closeButtons;

        public async UniTask WaitPopup(CancellationToken ct)
        {
            await AsyncUtils.WaitFirstButtonPressedAsync(ct, _closeButtons.ToArray());
        }
    }
}