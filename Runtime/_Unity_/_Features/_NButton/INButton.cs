namespace Nextension
{
    public interface INButton
    {
        void addNButtonListener(INButtonListener listener);
        void removeNButtonListener(INButtonListener listener);
        bool isInteractable();
        void setInteratableFromListener(INButtonListener listener, bool interactable);
    }
}
