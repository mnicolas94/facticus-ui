using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Facticus.UI.Utils;
using Facticus.UI.WindowInterfaces;
using UnityEngine;
using UnityEngine.UI;

namespace Facticus.UI.Popups
{
    [AddComponentMenu("Facticus.UI/Popups/Popup Simple")]
    public class PopupSimple : MonoBehaviour, IWindowPopup
    {
        [SerializeField] private List<Button> _closeButtons;

        public async UniTask WaitPopup(CancellationToken ct)
        {
            await AsyncUtils.WaitFirstButtonPressedAsync(ct, _closeButtons.ToArray());
        }
    }
}