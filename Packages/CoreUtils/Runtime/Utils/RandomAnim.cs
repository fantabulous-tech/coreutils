using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace CoreUtils {
    public class RandomAnim : MonoBehaviour {
        [SerializeField] private Animator m_Animator;
        [SerializeField] private AnimationClip[] m_Animations;

        private PlayableGraph m_Graph;

        private void OnEnable() {
            Play(m_Animations.GetRandomItem());
        }

        private void OnDisable() {
            m_Graph.Destroy();
        }

        private void Play(AnimationClip animClip) {
            m_Graph = PlayableGraph.Create(name + " Character Anims");
            m_Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            // Create the starting animation clip.
            AnimationClipPlayable animPlayable = AnimationClipPlayable.Create(m_Graph, animClip);

            // Create the transition mixer for changing animations over time.
            AnimationMixerPlayable transitionMixer = AnimationMixerPlayable.Create(m_Graph, 2);

            // Connect the base clip to the transition mixer.
            m_Graph.Connect(animPlayable, 0, transitionMixer, 1);

            transitionMixer.SetInputWeight(0, 0);
            transitionMixer.SetInputWeight(1, 1);

            // Create the layer output to handle 'heels'/'barefoot' options.
            AnimationPlayableOutput playableOutput = AnimationPlayableOutput.Create(m_Graph, "LayerMixer", m_Animator);
            AnimationLayerMixerPlayable layerMixer = AnimationLayerMixerPlayable.Create(m_Graph, 2);

            // Set the 'heels' layer to additive.
            layerMixer.SetLayerAdditive(1, true);
            playableOutput.SetSourcePlayable(layerMixer);

            layerMixer.ConnectInput(0, transitionMixer, 0, 1);

            m_Graph.Play();
        }
    }
}