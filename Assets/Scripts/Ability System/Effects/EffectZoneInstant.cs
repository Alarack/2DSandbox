using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectZoneInstant : EffectZone {



    public override void Initialize(Effect parentEffect, LayerMask mask, Transform parentToThis = null)
    {
        base.Initialize(parentEffect, mask);

        Invoke("CleanUp", 1f);
    }


    protected override void OnTriggerStay(Collider other)
    {
        ApplyAfterLayerCheck(other.gameObject);

        //if (LayerTools.IsLayerInMask(LayerMask, other.gameObject.layer) == false)
        //    return;

        //Apply(other.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        ApplyAfterLayerCheck(other.gameObject);

        //if (LayerTools.IsLayerInMask(LayerMask, other.gameObject.layer) == false)
        //    return;

        //Apply(other.gameObject);
    }


    protected override void Apply(GameObject target)
    {
        if (CheckHitTargets(target) == false)
            return;

        CreateImpactEffect(target.transform.position);

        if (parentEffect != null)
            parentEffect.Apply(target);
        else
            Debug.LogError("This effect zone: " + gameObject.name + " has no parent effect");
    }


    protected override void Remove(GameObject target)
    {
        //parentEffect.Remove(target.gameObject);
        targets.RemoveIfContains(target);
    }







}
