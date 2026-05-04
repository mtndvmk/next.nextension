#if !UNITY_5_3_OR_NEWER
using System;

public class SerializeField : Attribute { }

public interface ISerializationCallbackReceiver {}
#endif