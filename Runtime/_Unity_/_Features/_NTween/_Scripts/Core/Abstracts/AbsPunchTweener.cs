namespace Nextension.Tween
{
    internal abstract class AbsPunchTweener<T> : NTweener
        where T : unmanaged
    {
        public T origin;
        public T punchDestination;

        public AbsPunchTweener(T origin, T punchDestination)
        {
            this.origin = origin;
            this.punchDestination = punchDestination;
        }
    }
}