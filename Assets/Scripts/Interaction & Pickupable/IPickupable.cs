using DefaultNamespace;
using UnityEngine;

public interface IPickupable : IInteractable
{
    public void Grab(PickUpController pickUpController);
    public void Drop(PickUpController pickUpController);
    public void SetPositionInParent(Transform newParent);
    public void Use();
}