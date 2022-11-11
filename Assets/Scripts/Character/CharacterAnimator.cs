using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public Animator animator;

    public void SetTrigger (string triggerName) {
        animator.SetTrigger(triggerName);
    }

    public void SetBool (string boolName, bool value) {
        animator.SetBool(boolName, value);
    }

    public void SetFloat (string floatName, float value) {
        animator.SetFloat(floatName, value);
    }

    public bool AnimatorIsPlaying (int layer) {
        return true;
    }

    public bool AnimatorIsPlaying (string stateName, int layer) {
        return true;
    }

}
