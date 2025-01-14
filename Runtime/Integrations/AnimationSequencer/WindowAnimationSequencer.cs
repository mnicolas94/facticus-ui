#if ENABLED_ANIMATION_SEQUENCER
using System.Threading;
using BrunoMikoski.AnimationSequencer;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UI.Integrations.AnimationSequencer
{
    public class WindowAnimationSequencer : MonoBehaviour, IWindow
    {
        [SerializeField] private AnimationSequencerController _openAnimation;
        [SerializeField] private AnimationSequencerController _closeAnimation;
        
        public async UniTask Open(CancellationToken ct)
        {
            if (_openAnimation)
            {
                _openAnimation.Play();
                await _openAnimation.PlayingSequence.AsyncWaitForCompletion(ct);
            }
        }

        public async UniTask Close(CancellationToken ct)
        {
            if (_closeAnimation)
            {
                _closeAnimation.Play();
                await _closeAnimation.PlayingSequence.AsyncWaitForCompletion(ct);
            }
        }
    }
}
#endif