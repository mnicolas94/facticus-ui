using System.Threading;
using Cysharp.Threading.Tasks;

namespace UI
{
    public interface IWindow
    {
        UniTask Open(CancellationToken ct);
        UniTask Close(CancellationToken ct);
    }
}