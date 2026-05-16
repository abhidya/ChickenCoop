using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace HappyHarvest
{
    /// <summary>
    /// The tool animation can call the different events at the right frame to trigger the set VFX
    /// This script need to be added on the same GameObject wit the Animator on the tool to be able to receive the
    /// animation events.
    /// </summary>
    public class ToolAnimationEventHandler : MonoBehaviour
    {
        [Header("Front")]
        public VisualEffect FrontEffect;
        public string FrontEffectId;
    
        [Header("Up")]
        public VisualEffect UpEffect;
        public string UpEffectId;
    
        [Header("Side")]
        public VisualEffect SideEffect;
        public string SideEffectId;

        public void TriggerFrontVFX()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer || FrontEffect == null)
            {
                return;
            }

            if (SideEffect != null) SideEffect.gameObject.SetActive(false);
            if (UpEffect != null) UpEffect.gameObject.SetActive(false);
            FrontEffect.gameObject.SetActive(true);

            FrontEffect.SendEvent(FrontEffectId);
        }

        public void TriggerSideVFX()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer || SideEffect == null)
            {
                return;
            }

            if (SideEffect != null) SideEffect.gameObject.SetActive(true);
            if (UpEffect != null) UpEffect.gameObject.SetActive(false);
            if (FrontEffect != null) FrontEffect.gameObject.SetActive(false);

            SideEffect.SendEvent(SideEffectId);
        }

        public void TriggerUpVFX()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer || UpEffect == null)
            {
                return;
            }

            if (SideEffect != null) SideEffect.gameObject.SetActive(false);
            if (UpEffect != null) UpEffect.gameObject.SetActive(true);
            if (FrontEffect != null) FrontEffect.gameObject.SetActive(false);

            UpEffect.SendEvent(UpEffectId);
        }
    }
}
