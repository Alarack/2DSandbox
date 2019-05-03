using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AIBrain : MonoBehaviour
{
    public EntityEnemy Owner { get; private set; }

    private AISensor sensor;

    public void Initialize(EntityEnemy owner, AISensor sensor)
    {
        this.Owner = owner;
        this.sensor = sensor;
    }


    private void Update()
    {

    }

    public void TargetSpotted()
    {
        Owner.Aggro();
    }

    public void NoTargets()
    {
        Owner.EntityFSM.SwapToPreviousState();
    }


    public void PushStateActionAbilities(List<Ability> abilities)
    {
        SelectAbility(abilities);
    }



    private void SelectAbility(List<Ability> abilities)
    {
        Dictionary<Ability, float> weightedAbilityDict = new Dictionary<Ability, float>();

        int count = abilities.Count;
        for (int i = 0; i < count; i++)
        {
            if (sensor.ClosestTarget == null)
            {
                Debug.Log("Sensor didn't have a target");
                return;
            }

            if (abilities[i].MeetsRequiredConditions(sensor.ClosestTarget) == false)
                continue;

            if (abilities[i].RecoveryManager.HasRecovery == true && abilities[i].RecoveryManager.HasCharges == false)
                continue;

            float moddedWeight = abilities[i].GetModifiedWeight(sensor.ClosestTarget);
            weightedAbilityDict.Add(abilities[i], moddedWeight);
        }

        int countOfUsableAbilities = weightedAbilityDict.Count;

        Debug.Log(countOfUsableAbilities + " abiliites are ready to be used");

        if(countOfUsableAbilities > 0)
        {
            Ability chosen = weightedAbilityDict.OrderBy(k => k.Value).Last().Key;

            chosen.Activate();
        }


        foreach (KeyValuePair<Ability, float> entry in weightedAbilityDict)
        {
            //Debug.Log(entry.Key.abilityName + " has a weight of " + entry.Value);

            
        }
    }
}
