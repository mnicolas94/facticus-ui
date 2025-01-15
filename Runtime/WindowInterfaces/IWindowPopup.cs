using System.Threading;
using Cysharp.Threading.Tasks;

namespace UI
{
    /// <summary>
    /// Same as <see cref="IWindowWithResult{T}"/> but it returns nothing.
    /// Useful for windows or popups that close themselves after a certain action without knowing to which <see cref="UiState"/>
    /// they belong. So, instead of executing <see cref="UiState.Close"/>, you can just open the ui state with
    /// <see cref="UiState.OpenWaitResult{T}"/> or <see cref="UiState.OpenWaitResult{T1, T2}"/> and it will close when
    /// the method <see cref="WaitPopup"/> finishes. 
    /// </summary>
    public interface IWindowPopup : IWindowWithResult<Void>
    {
        UniTask WaitPopup(CancellationToken ct);

        async UniTask<Void> IWindowWithResult<Void>.WaitForResult(CancellationToken ct)
        {
            await WaitForResult(ct);
            return default;
        }
    }
    
    public struct Void{}
}