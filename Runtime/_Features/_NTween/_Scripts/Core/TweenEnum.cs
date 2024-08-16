namespace Nextension.Tween
{
    internal enum SupportedDataType : byte
    {
        NotSupported = 0,
        Float,
        Float2,
        Float3,
        Float4,
    }
    internal enum TransformTweenType : byte
    {
        Local_Position,
        World_Position,
        Local_Rotation,
        World_Rotation,
        Local_Scale,
        Uniform_Local_Scale,
    }
    //public enum TweenPriority : byte
    //{
    //    /// <summary>
    //    /// Always update each frame
    //    /// </summary>
    //    Always,
    //    /// <summary>
    //    /// Have high order to update, maybe not update if there are too many tweeners
    //    /// </summary>
    //    High,
    //    /// <summary>
    //    /// Have medium order to update, maybe not update if there are too many tweeners
    //    /// </summary>
    //    Medium,
    //    /// <summary>
    //    /// Have low order to update, maybe not update if there are too many tweeners
    //    /// </summary>
    //    Low
    //}
}
