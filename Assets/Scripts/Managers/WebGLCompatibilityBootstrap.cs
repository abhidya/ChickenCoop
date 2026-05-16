using UnityEngine;
using UnityEngine.VFX;

namespace ChickenCoop.Managers
{
    /// <summary>
    /// Disables unsupported VFX Graph components in WebGL builds before gameplay starts.
    /// </summary>
    public static class WebGLCompatibilityBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void DisableUnsupportedVfxOnWebGL()
        {
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                return;
            }

            VisualEffect[] effects = Object.FindObjectsByType<VisualEffect>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (VisualEffect effect in effects)
            {
                if (effect == null)
                {
                    continue;
                }

                effect.Stop();
                effect.gameObject.SetActive(false);
            }

            Debug.Log($"[WebGLCompatibilityBootstrap] Disabled {effects.Length} VisualEffect components for WebGL compatibility.");
        }
    }
}
