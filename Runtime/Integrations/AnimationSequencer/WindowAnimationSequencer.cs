#if ENABLED_ANIMATION_SEQUENCER
using System.Threading;
using System.Threading.Tasks;
using BrunoMikoski.AnimationSequencer;
using UnityEngine;

namespace UI.Integrations.AnimationSequencer
{
    public class WindowAnimationSequencer : MonoBehaviour, IWindow
    {
        [SerializeField] private AnimationSequencerController _openAnimation;
        [SerializeField] private AnimationSequencerController _closeAnimation;
        
        public async Task Open(CancellationToken ct)
        {
            _openAnimation.Play();
            await _openAnimation.PlayingSequence.AsyncWaitForCompletion(ct);
        }

        public async Task Close(CancellationToken ct)
        {
            _closeAnimation.Play();
            await _closeAnimation.PlayingSequence.AsyncWaitForCompletion(ct);
        }
    }
}
#endif