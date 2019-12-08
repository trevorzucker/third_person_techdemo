using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementTracker : StateMachineBehaviour
{

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetFloat("WalkNormalizedTime", stateInfo.normalizedTime);
        /*string animName = "Walk";
        if (stateInfo.IsName(animName) && !animator.GetBool("Aiming") && animator.GetFloat("WalkSpeed") > 0.4f && animator.GetInteger("HoldType") == 0)
        {
            float time = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

            animator.Play(animName, layerIndex, time);
            Debug.Log(time);
        }*/
    }
}
