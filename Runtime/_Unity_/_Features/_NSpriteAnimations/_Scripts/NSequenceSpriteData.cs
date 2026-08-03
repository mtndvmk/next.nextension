using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    [CreateAssetMenu(fileName = "NSequenceAnimationData", menuName = "Nextension/NSequence Animation Data", order = 1)]
    public class NSequenceSpriteData : ScriptableObject
    {
        public List<Sprite> sprites;
        public uint fps = 12;
        public float duration
        {
            get
            {
                if (fps == 0) return 0;
                var oneWayDuration = sprites.Count * 1f / fps;
                if (!isBackAndForth)
                {
                    return oneWayDuration;
                }
                return oneWayDuration * 2 + backDelayTime;
            }
        }
        public bool isBackAndForth;
        public float backDelayTime;
    }
}
