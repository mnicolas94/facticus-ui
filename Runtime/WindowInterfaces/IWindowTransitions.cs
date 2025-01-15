using System.Threading;
using Cysharp.Threading.Tasks;

namespace Facticus.UI.WindowInterfaces
{
    public interface IWindowTransitions : IWindowInterface
    {
        UniTask Open(CancellationToken ct);
        UniTask Close(CancellationToken ct);
    }
}