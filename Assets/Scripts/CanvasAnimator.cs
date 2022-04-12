using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasAnimator : MonoBehaviour
{
    public List<Animator> animators = new List<Animator>();

    public bool callAppearOnStart;

    void Start()
    {
        if(callAppearOnStart)
        {
            CallAppearOnAllAnimators();
        }
    }

    public void CallAppearOnAllAnimators()
    {
        foreach (Animator anim in animators)
        {
            anim.ResetTrigger("Hide");
            anim.SetTrigger("Show");
        }
    }

    public void CallDisappearOnAllAnimators()
    {
        foreach (Animator anim in animators)
        {
            anim.ResetTrigger("Show");
            anim.SetTrigger("Hide");
        }
    }
}
