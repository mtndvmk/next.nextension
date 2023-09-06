namespace Nextension.Tween
{
    internal enum TweenType : byte
    {
        Transform_Local_Move,
        Transform_World_Move,
        Transform_Local_Rotate,
        Transform_World_Rotate,
        Transform_Local_Scale,

        Basic_Float_Tween,
        Basic_Float2_Tween,
        Basic_Float3_Tween,
        Basic_Float4_Tween,
    }
    internal enum TweenLoopType : byte
    {
        Normal,
        Punch,
        Shake,
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
