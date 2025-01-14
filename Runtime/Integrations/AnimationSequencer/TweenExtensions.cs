#if ENABLED_ANIMATION_SEQUENCER
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace UI.Integrations.AnimationSequencer
{
    public static class TweenExtensions
    {
        public static async UniTask AsyncWaitForCompletion(this Tween t, CancellationToken ct)
        {
            if (!t.active)
            {
                return;
            }

            while (t.active && !t.IsComplete() && !ct.IsCancellationRequested)
            {
                await UniTask.Yield();
            }

            if (ct.IsCancellationRequested)
            {
                t.Complete(true);
            }
        }
    }
}
#endif