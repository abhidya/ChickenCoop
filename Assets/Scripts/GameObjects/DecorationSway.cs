using UnityEngine;

/// <summary>
/// DecorationSway - Simple script to add swaying animation to decorative objects.
/// Applied to environment elements like trees, grass, flowers.
/// This should NOT be applied to gameplay objects like crops or animals.
/// </summary>
public class DecorationSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float swayAmount = 3f;
    [SerializeField] private float swaySpeed = 1f;
    [SerializeField] private float swayDelay = 0f;
    
    private Vector3 originalRotation;
    
    void Start()
    {
        originalRotation = transform.localEulerAngles;
    }
    
    void Update()
    {
        float sway = Mathf.Sin(Time.time * swaySpeed + swayDelay) * swayAmount;
        transform.localRotation = Quaternion.Euler(
            originalRotation.x,
            originalRotation.y,
            originalRotation.z + sway
        );
    }
}
