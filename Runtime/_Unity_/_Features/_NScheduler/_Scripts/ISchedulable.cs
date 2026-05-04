namespace Nextension
{
    public interface ISchedulable
    {
        public int Priority => 0;
        void onStartExecute();
        void onCanceled() { }
    }
}
