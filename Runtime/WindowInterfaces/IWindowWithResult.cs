using System.Threading;
using Cysharp.Threading.Tasks;

namespace UI
{
    /// <summary>
    /// A window that returns a result of type T after it's closed. It is meant to be used with
    /// <see cref="UiState.OpenWaitResult{T}"/> and <see cref="UiState.OpenWaitResult{T1, T2}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IWindowWithResult<T> : IWindowInterface
    {
        UniTask<T> WaitForResult(CancellationToken ct);
    }
}