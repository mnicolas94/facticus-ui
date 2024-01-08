using System.Threading;
using System.Threading.Tasks;

namespace UI
{
    public interface IWindow
    {
        Task Open(CancellationToken ct);
        Task Close(CancellationToken ct);
    }
}