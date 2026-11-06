namespace DefaultNamespace
{
    public interface IInteractable
    {
        public string InteractMsg { get;}
        public void Interact(InteractionController interactionController);
    }
}