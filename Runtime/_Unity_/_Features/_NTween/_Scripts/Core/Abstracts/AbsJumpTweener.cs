namespace Nextension.Tween
{
    internal abstract class AbsJumpTweener<T> : NTweener
        where T : unmanaged
    {
        public T origin;
        public T destination;
        public T jumpHeight;

        public AbsJumpTweener(T origin, T destination, T jumpHeight)
        {
            this.origin = origin;
            this.destination = destination;
            this.jumpHeight = jumpHeight;
        }
    }
}
