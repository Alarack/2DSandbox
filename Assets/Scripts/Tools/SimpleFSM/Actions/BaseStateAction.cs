using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseStateAction {
    public Entity owner;
    public bool RunUpdate { get; protected set; }

    public List<Ability> abilities = new List<Ability>();
    //protected bool isPlayer;
    protected AIBrain brain;

    public BaseStateAction(Entity owner, bool seperateUpdate = false)
    {
        this.owner = owner;
        RunUpdate = seperateUpdate;

        if(owner is EntityEnemy)
        {
            EntityEnemy enemy = owner as EntityEnemy;
            brain = enemy.Brain;
        }


    }


    public virtual void Execute()
    {
        if (brain != null)
        {
            brain.PushStateActionAbilities(abilities);
        }
        else
        {
            ActivateAbiliites();
        }

    }

    public virtual void ManagedUpdate()
    {
        //Debug.Log("Managed Update for " + GetType());
        UpdateAbilities();
    }

    public virtual void RegisterEvents()
    {

    }

    public virtual void UnregisterEvents()
    {

    }


    public void PopulateAbilities(List<AbilityData> abilityData)
    {
        int count = abilityData.Count;
        for (int i = 0; i < count; i++)
        {
            Ability newAbility = CreateAbility(abilityData[i]);
            abilities.Add(newAbility);
            newAbility.Equip();
        }

        if (abilities.Count > 0)
        {
            //Debug.Log("Turning On Update for " + GetType().ToString());
            RunUpdate = true;
        }

    }


    protected Ability CreateAbility(AbilityData data)
    {
        Ability result = new Ability(data, owner.gameObject);
        return result;
    }

    public void ActivateAbiliites()
    {
        int count = abilities.Count;
        for (int i = 0; i < count; i++)
        {
            abilities[i].Activate();
        }
    }

    private void UpdateAbilities()
    {
        //Debug.Log("Updating " + GetType().ToString());

        int count = abilities.Count;
        for (int i = 0; i < count; i++)
        {
            abilities[i].ManagedUpdate();
        }
    }
}
