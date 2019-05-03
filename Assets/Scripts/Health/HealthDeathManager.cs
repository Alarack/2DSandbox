using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthDeathManager : MonoBehaviour {

    public GameObject corpse;

    public Entity Owner { get; private set; }
    public float Ratio { get { return currentHealth / maxHealth; } }
    public float maxHealth;



    private float currentHealth;
    private bool dying;


    public void Initialize(Entity owner)
    {
        Owner = owner;
        currentHealth = maxHealth;
    }


    public void AlterHealth(float value)
    {
        currentHealth += value;


        //Debug.Log(value + " damage is dealt to " + Owner.gameObject.name);
        //Debug.Log(currentHealth + " is current health");

        if (currentHealth < 0)
            currentHealth = 0;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        if (value < 0f)
        {
            Owner.AnimHelper.PlayAnimTrigger("Flinch");
        }

        if (currentHealth <= 0f && dying == false)
        {
            //EntityMovement movement = Owner.Movement;
            //if (movement != null)
            //{
            //    movement.SpinCrazy();
            //}
            Die();
        }

    }

    public void Die()
    {
        dying = true;

        if(LayerMask.LayerToName(Owner.gameObject.layer) == "Enemy")
        {
            GameManager.Instance.spawnManager.EnemyDied(Owner);
            //Debug.Log(gameObject.name + " died");
        }

        CreateCorpse();
        Destroy(gameObject);
    }

    private void CreateCorpse()
    {
        if (corpse == null)
            return;

        GameObject activeCorpse = Instantiate(corpse, transform.position, transform.rotation) as GameObject;
        Corpse corpseScript = activeCorpse.GetComponent<Corpse>();
        corpseScript.Initialize();
        corpseScript.PlayDeathEffect();

    }

}
