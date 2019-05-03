using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public string animTrigger;
    public AnimHelper animHelper;
    public float life;
    //public SpriteRenderer spriteRenderer;
    public GameObject impactEffect;
    public LayerMask mask;
    public float damageValue = -10f;
    public float xForce;
    public float yForce;
    public float knockbackDuration = 0.2f;

    protected List<GameObject> targets = new List<GameObject>();

    private Vector2 knockBackVector;

    private void Start()
    {
        Destroy(gameObject, life);
        animHelper.PlayAnimTrigger(animTrigger);
    }

    public void SetKnockBack(Vector2 knockBackVector)
    {
        this.knockBackVector = knockBackVector;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (LayerTools.IsLayerInMask(mask, other.gameObject.layer) == false)
            return;

        if (CheckHitTargets(other.gameObject) == false)
            return;

        //Debug.Log(other.gameObject.name + " was hit");

        CreateImpactEffect(other.transform.position);

        Entity otherEntity = other.gameObject.GetComponent<Entity>();
        if (otherEntity != null)
        {
            DealDamage(otherEntity);
            ApplyKnockback(otherEntity);
        }


    }

    private void DealDamage(Entity target)
    {
        if (target.Health == null)
            return;

        target.Health.AlterHealth(damageValue);
        //target.AnimHelper.PlayAnimTrigger("Flinch");
    }

    private void ApplyKnockback( Entity target)
    {
        EntityMovement movement = target.Movement;
        if(movement != null)
        {

            movement.ForceMovement(knockBackVector, knockbackDuration, true);
        }
    }

    private bool CheckHitTargets(GameObject target)
    {
        if (target == null)
            return false;

        int count = targets.Count;
        for (int i = 0; i < count; i++)
        {
            if (targets[i] == target)
                return false;
        }

        targets.AddUnique(target);
        return true;
    }


    private void CreateImpactEffect(Vector2 location)
    {
        if (impactEffect == null)
            return;

        Vector2 loc = new Vector2(location.x + Random.Range(-0.5f, 0.5f), location.y + Random.Range(-0.5f, 0.5f));

        GameObject impact = Instantiate(impactEffect, loc, Quaternion.identity) as GameObject;
        Destroy(impact, 2f);
    }

}
