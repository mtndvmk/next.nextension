namespace Nextension
{
    public interface INButtonListener
    {
        void onButtonDown() { }
        void onButtonUp() { }
        void onButtonClick() { }
        void onButtonEnter() { }
        void onButtonExit() { }
        void onInteractableChanged(bool isInteractable) { }
    }
}