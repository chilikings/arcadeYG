using UnityEngine;
using GAME.Utils.Core;
using System.Collections;

namespace GAME.Extensions.Animation
{
    public static class AnimatorExt
    {
        public static float CalcCurrAnimDuration(this Animator animator, int layerIndex)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            var duration = stateInfo.length / (animator.speed * stateInfo.speed);
            return duration;
        }

        public static void PlayDeath(this Animator animator)
        {
            if (!animator) return;
            animator.SetTrigger(Helper.DeathName);
        }

        public static void PlaySpawn(this Animator animator)
        {
            if (!animator) return;
            animator.SetTrigger(Helper.SpawnName);
        }

        public static IEnumerator PlayAndWait(this Animator animator, string stateName, int layer = 0)
        {
            animator.SetTrigger(stateName);
            //yield return null;
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName));
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(layer).normalizedTime >= 1f);
        }
    }
}