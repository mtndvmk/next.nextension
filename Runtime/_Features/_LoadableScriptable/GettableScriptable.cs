using UnityEngine;

namespace Nextension
{
    [LoadableScriptable]
    public class GettableScriptable<T> where T : ScriptableObject
    {
        public static T Getter => ScriptableLoader.get<T>();
    }
}
