using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    [CreateAssetMenu(fileName = "NSequenceAnimationData", menuName = "Nextension/NSequence Animation Data", order = 1)]
    public class NSequenceSpriteData : ScriptableObject
    {
        public List<Sprite> sprites;
        public uint fps = 12;
        public float duration => fps == 0 ? 0 : sprites.Count * 1f / fps;
    }
}
