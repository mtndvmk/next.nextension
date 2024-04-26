using UnityEngine;

namespace Nextension
{
    [SingletonScriptable]
    public class SingletonScriptableGettable<T> : ScriptableObject where T : ScriptableObject
    {
        public static T Getter => ScriptableLoader.get<T>();
    }
}
