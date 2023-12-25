using UnityEngine;

namespace Nextension
{
    [SingletonScriptable]
    public class GettableSingletonScriptable<T> : ScriptableObject where T : ScriptableObject
    {
        public static T Getter => ScriptableLoader.get<T>();
    }
}
