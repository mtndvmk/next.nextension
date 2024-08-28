using UnityEngine;

namespace Nextension
{
    [SingletonScriptable]
    public class SingletonScriptableGettable<T> : ScriptableObject where T : ScriptableObject
    {
        public static T Instance => ScriptableLoader.get<T>();
    }
}
