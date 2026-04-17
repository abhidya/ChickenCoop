using UnityEngine;

/// <summary>
/// A helper component that receives animation events from child visual objects 
/// and forwards them to a parent controller if one exists.
/// This resolves "No receiver" errors when animations are on child GameObjects.
/// </summary>
public class AnimationEventReceiver : MonoBehaviour
{
    private MonoBehaviour parentController;

    private void Awake()
    {
        // Find the player controller on the parent
        parentController = GetComponentInParent<MonoBehaviour>();
    }

    /// <summary>
    /// Event fired by walk animations to play a footstep sound
    /// </summary>
    public void PlayStepSound()
    {
        if (parentController != null)
        {
            parentController.SendMessage("PlayStepSound", SendMessageOptions.DontRequireReceiver);
        }
    }

    /// <summary>
    /// Event fired by animations to emit particles (like dust puffs)
    /// </summary>
    public void Emit()
    {
        if (parentController != null)
        {
            parentController.SendMessage("Emit", SendMessageOptions.DontRequireReceiver);
        }
    }
}
