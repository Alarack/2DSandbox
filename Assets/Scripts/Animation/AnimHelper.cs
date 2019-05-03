using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AnimHelper : MonoBehaviour
{

    private Animator myAnim;
    private Action callback;


    private void Awake()
    {
        myAnim = GetComponent<Animator>();
    }



    public void PlayWalk()
    {
        if (myAnim == null)
            return;

        if (myAnim.GetBool("Walking") == true)
            return;

        myAnim.SetBool("Walking", true);
    }

    public void StopWalk()
    {
        if (myAnim == null)
            return;

        if (myAnim.GetBool("Walking") == false)
            return;

        myAnim.SetBool("Walking", false);
    }


    public void PlayOrStopAnimBool(string boolName, bool play = true)
    {
        if (myAnim == null)
            return;

        if (myAnim.GetBool(boolName) == false && play == false)
            return;

        if (myAnim.GetBool(boolName) == true && play == true)
            return;

        myAnim.SetBool(boolName, play);
    }

    public bool PlayAnimTrigger(string trigger)
    {
        if (myAnim == null)
            return false;


        if (string.IsNullOrEmpty(trigger) == true)
            return false;

        try
        {
            myAnim.SetTrigger(trigger);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(gameObject.name + " Could not play an animation: " + e);
            return false;
        }
    }


    public void SetAnimEventAction(Action callback)
    {
        if(this.callback != callback)
            this.callback = callback;
    }

    public void RecieveAnimEvent(AnimationEvent param)
    {
        //Debug.Log("Recieving " + param + " from anim event");

        if (this.callback != null)
            callback();
    }

}
