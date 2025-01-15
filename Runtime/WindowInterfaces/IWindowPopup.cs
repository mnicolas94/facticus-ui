using System.Threading;
using Cysharp.Threading.Tasks;

namespace UI
{
    /// <summary>
    /// Same as <see cref="IWindowWithResult{T}"/> but it returns nothing.
    /// Useful for windows or popups that close themselves after a certain action without knowing to which <see cref="UiStatePrefabGroup"/>
    /// they belong. So, instead of executing <see cref="UiStatePrefabGroup.Close"/>, you can just open the ui state with
    /// <see cref="UiStatePrefabGroup.OpenWaitResult{T}"/> or <see cref="UiStatePrefabGroup.OpenWaitResult{T1, T2}"/> and it will close when
    /// the method <see cref="WaitPopup"/> finishes. 
    /// </summary>
    public interface IWindowPopup : IWindowWithResult<Void>
    {
        UniTask WaitPopup(CancellationToken ct);

        async UniTask<Void> IWindowWithResult<Void>.WaitForResult(CancellationToken ct)
        {
            await WaitPopup(ct);
            return default;
        }
    }
    
    public struct Void{}
}