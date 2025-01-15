using System.Threading;
using Cysharp.Threading.Tasks;

namespace UI
{
    public interface IWindowTransitions : IWindowInterface
    {
        UniTask Open(CancellationToken ct);
        UniTask Close(CancellationToken ct);
    }
}