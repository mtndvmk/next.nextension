#if !UNITY_5_3_OR_NEWER
using System;

namespace UnityEngine
{
    public class SerializeField : Attribute { }

    public interface ISerializationCallbackReceiver {}
}

#endif