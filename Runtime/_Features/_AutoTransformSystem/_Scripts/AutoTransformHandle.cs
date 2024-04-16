namespace Nextension
{
    public class AutoTransformHandle
    {
        public int Index { get; internal set; }
        internal AutoTransformHandle(int index)
        {
            this.Index = index;
        }
        public void stop()
        {
            AutoTransformSystem.stop(this);
        }
    }
}
