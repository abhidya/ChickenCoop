using UnityEngine;

/// <summary>
/// Interface for interactable objects in the game
/// </summary>
public interface IInteractable
{
    void Interact();
    bool CanInteract();
}
