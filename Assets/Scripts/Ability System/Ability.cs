using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LL.Events;
using EffectTag = Constants.EffectTag;
using AbilityActivationMethod = Constants.AbilityActivationMethod;

[System.Serializable]
public class Ability {

    public string abilityName;
    
    
    //public List<AbilityActivationMethod> triggers = new List<AbilityActivationMethod>();
    public List<AbilityActivationInfo> activations = new List<AbilityActivationInfo>();
    public List<AbilityCondition> conditions = new List<AbilityCondition>();

    public List<GameObject> targets = new List<GameObject>();

    public List<EffectTag> Tags { get { return GetTags(); } }
    public AbilityRecoveryManager RecoveryManager { get; protected set; }
    public EffectManager EffectManager { get; protected set; }
    public Sprite AbilityIcon { get; protected set; }
    public GameObject Source { get; protected set; }
    public bool InUse { get; protected set; }
    public float UseDuration { get; protected set; }
    public bool OverrideOtherAbilities { get; protected set; }
    public float ProcChance { get { return procChance; } protected set { procChance = Mathf.Clamp01(value); } }
    public int AbilityID { get; protected set; }
    public bool Equipped { get; protected set; }

    protected float procChance = 1f;
    protected AbilityData abilityData;
    protected Timer useTimer;
    protected float baseWeight;
    public float ModifiedWeight { get; protected set; }

    #region CONSTRUCTION

    public Ability(AbilityData data, GameObject source)
    {
        abilityData = data;
        AbilityID = IDFactory.GenerateAbilityID();
        Source = source;
        EffectManager = new EffectManager(this);
        RecoveryManager = new AbilityRecoveryManager(this);

        SetUpAbilityData();

        useTimer = new Timer(UseDuration, PopUseTimer, true, "Use");

        GameManager.RegisterAbility(this);
    }

    private void SetUpAbilityData()
    {
        abilityName = abilityData.abilityName;
        activations = abilityData.activations;
        AbilityIcon = abilityData.abilityIcon;
        UseDuration = abilityData.useDuration;
        ProcChance = abilityData.procChance;
        OverrideOtherAbilities = abilityData.overrideOtherAbilities;
        baseWeight = abilityData.baseWeight;
        conditions = abilityData.conditions;

        CreateEffects();
        CreateRecoveryMethods();
        EffectManager.SetupRiders();
    }

    private void CreateEffects()
    {
        int count = abilityData.effectData.Count;
        for (int i = 0; i < count; i++)
        {
            Effect newEffect = EffectFactory.CreateEffect(this, abilityData.effectData[i]);
            EffectManager.AddEffect(newEffect);
        }
    }



    private void CreateRecoveryMethods()
    {
        int count = abilityData.recoveryData.Count;
        for (int i = 0; i < count; i++)
        {
            AbilityRecovery newRevovrey = EffectFactory.CreateRecovery(this, abilityData.recoveryData[i]);
            RecoveryManager.AddRecoveryMethod(newRevovrey);
        }
    }

    #endregion

    public void Equip()
    {
        RegisterListeners();
        Equipped = true;

        //Debug.Log(abilityName + " has been equipped");
    }

    public void Unequip()
    {
        UnregisterListeners();
        Equipped = false;
        Deactivate();

        //Debug.Log(abilityName + " has been unequipped");
    }

    #region ACTIVATORS SETUP

    private void RegisterListeners()
    {
        int count = activations.Count;
        for (int i = 0; i < count; i++)
        {
            AbilityActivationInfo currentMethod = activations[i];
            SetUpActivators(currentMethod);
        }
    }

    private void UnregisterListeners()
    {
        int count = activations.Count;
        for (int i = 0; i < count; i++)
        {
            AbilityActivationInfo currentMethod = activations[i];
            TearDownActivators(currentMethod);
        }
    }


    private void SetUpActivators(AbilityActivationInfo activationInfo)
    {
        switch (activationInfo.activationMethod)
        {
            case AbilityActivationMethod.Timed:
                activationInfo.activaionTimer = new Timer(activationInfo.activationTime, TimedActivate, true);
                break;

            case AbilityActivationMethod.StatChanged:
                EventGrid.EventManager.RegisterListener(Constants.GameEvent.StatChanged, OnStatChanged);
                break;

            case AbilityActivationMethod.Passive:
                ActivateViaSpecificConditionSet(AbilityActivationMethod.Passive);
                break;
        }
    }

    private void TearDownActivators(AbilityActivationInfo currentMethod)
    {
        switch (currentMethod.activationMethod)
        {
            case AbilityActivationMethod.Timed:
                //currentMethod.activaionTimer = null;
                break;

            case AbilityActivationMethod.StatChanged:
                EventGrid.EventManager.RemoveListener(Constants.GameEvent.StatChanged, OnStatChanged);
                break;
        }
    }

    private bool GetActivator(AbilityActivationMethod method, out AbilityActivationInfo info)
    {
        info = new AbilityActivationInfo();

        int count = activations.Count;
        for (int i = 0; i < count; i++)
        {
            if (activations[i].activationMethod == method)
            {
                info = activations[i];
                return true;
            }
        }
        return false;
    }

    public float GetModifiedWeight(GameObject target)
    {
        float result = baseWeight;

        int count = conditions.Count;
        for (int i = 0; i < count; i++)
        {
            if (conditions[i].CheckCondition(target, Source) == true)
                result += conditions[i].weightModifer;
            else
                result -= conditions[i].weightModifer;
        }

        return result;
    }

    public bool MeetsRequiredConditions(GameObject target)
    {
        int count = conditions.Count;
        for (int i = 0; i < count; i++)
        {
            if (conditions[i].requiredCondition == false)
                continue;

            if (conditions[i].CheckCondition(target, Source) == false)
                return false;
        }


        return true;
    }

    #endregion

    #region TIMING

    public void ManagedUpdate()
    {
        if (Equipped == false)
            return;

        //Debug.Log(abilityName + " is being updated");

        RecoveryManager.ManagedUpdate();
        EffectManager.ManagedUpdate();
        UpdateTimers();

    }

    private void UpdateTimers()
    {
        int count = activations.Count;
        for (int i = 0; i < count; i++)
        {
            AbilityActivationInfo currentMethod = activations[i];
            currentMethod.ManagedUpdate();
        }

        if (useTimer != null && InUse)
        {
            useTimer.UpdateClock();
        }

    }


    private void PopUseTimer()
    {
        InUse = false;
    }

    #endregion

    #region EVENTS
    private void OnStatChanged(EventData data)
    {
        AbilityActivationInfo activator;
        bool foundActivator = GetActivator(AbilityActivationMethod.StatChanged, out activator);

        if (foundActivator == false)
            return;

        if (AbilityValidator.Validate(this, activator, data) == false)
            return;

        Constants.AbilityActivationCondition[] conditions = activator.activationConditions.ToArray();
        Activate(conditions);

    }

    #endregion


    #region ACTIVATION

    public void TimedActivate()
    {
        ActivateViaSpecificConditionSet(AbilityActivationMethod.Timed);
    }

    private void ActivateViaSpecificConditionSet(AbilityActivationMethod method)
    {
        AbilityActivationInfo activator;
        bool foundActivator = GetActivator(method, out activator);

        if (foundActivator == false)
            return;

        Constants.AbilityActivationCondition[] conditions = activator.activationConditions.ToArray();
        Activate(conditions);
    }

    public void NPCActivate(GameObject target)
    {
        bool canActivate = MeetsRequiredConditions(target);
        if (canActivate == false)
            return;

        Activate();
    }


    public bool Activate(params Constants.AbilityActivationCondition[] conditions)
    {
        if (HandleActivationConditions(conditions) == false)
            return false;

        if (procChance < 1f && ProcRoll() == false)
            return false;

        EffectManager.ActivateAllEffects();
        InUse = true;
        Debug.Log(abilityName + " has been activated");

        return true;
    }

    public void Deactivate()
    {
        EffectManager.DeactivateAllEffects();
        InUse = false;
    }

    private bool ProcRoll()
    {
        float roll = Random.Range(0f, 1f);
        return roll <= procChance;
    }


    private bool HandleActivationConditions(Constants.AbilityActivationCondition[] conditions)
    {
        bool result = false;

        if (RecoveryManager.HasRecovery == false)
            return true;

        bool freeActivation = conditions.Contains(Constants.AbilityActivationCondition.IgnoreCost) && conditions.Contains(Constants.AbilityActivationCondition.IgnoreRecovery);
        if (freeActivation)
            return true;

        if (conditions.Contains(Constants.AbilityActivationCondition.Normal) || conditions.Length < 1)
        {
            //Check For Resource
            result = RecoveryManager.HasCharges;


            //Debug.Log(RecoveryManager.Charges + " charges left on " + abilityName);

            if (result == true)
            {
                RecoveryManager.SpendCharge();
                //Spend Resource
            }
            return result;
        }

        if (conditions.Contains(Constants.AbilityActivationCondition.IgnoreRecovery))
        {
            //Check For Resource
            //Spend Resource
        }

        if (conditions.Contains(Constants.AbilityActivationCondition.IgnoreCost))
        {
            result = RecoveryManager.HasCharges;

            if (result == true)
            {
                RecoveryManager.SpendCharge();
            }

            return result;
        }

        if (Source.Entity() is EntityEnemy) {

            EntityEnemy baddie = Source.Entity() as EntityEnemy;

            if (MeetsRequiredConditions(baddie.AISensor.ClosestTarget) == false)
                result = false;
        }
        else
        {
            if(MeetsRequiredConditions(null) == false)
                result = false;
        }


        return result;
    }

    #endregion


    private List<EffectTag> GetTags()
    {
        List<EffectTag> results = new List<EffectTag>();
        int count = EffectManager.Effects.Count;
        for (int i = 0; i < count; i++)
        {
            results.AddRange(EffectManager.Effects[i].tags);
        }

        return results;
    }


    public bool GetActivationType(AbilityActivationMethod method)
    {
        int count = activations.Count;
        for (int i = 0; i < count; i++)
        {
            if (activations[i].activationMethod == method)
                return true;
        }


        return false;
    }

    public GameInput.GameButtonType GetManualActivationButton()
    {
        AbilityActivationInfo info;

        if(GetActivator(AbilityActivationMethod.Manual, out info))
        {
            return info.keyBind;
        }

        return GameInput.GameButtonType.None;
    }


}

[System.Serializable]
public struct AbilityActivationInfo {
    public AbilityActivationMethod activationMethod;
    public List<Constants.AbilityActivationCondition> activationConditions;

    //Stat Changed
    public BaseStat.StatType targetstat;
    public EffectConstraint.GainedOrLost gainedOrLost;
    public EffectConstraint.TargetConstraintType statChangeTarget;

    //Timed
    public float activationTime;
    public Timer activaionTimer;

    //Manual
    public GameInput.GameButtonType keyBind;



    public void ManagedUpdate()
    {
        if (activaionTimer != null)
            activaionTimer.UpdateClock();
    }

}


[System.Serializable]
public class AbilityCondition {

    public enum AbilityConditionType {
        DistanceFromTarget, //Could be sub class for both distance and above, below, left right, etc..
        StatValue, //could be target stat instead of own stat
        StatRatio,
        HasStatus,
        Grounded,
        HasTarget
    }

    public bool requiredCondition;
    public bool inverse;
    public bool compareAgainstTarget;
    public float weightModifer;
    public AbilityConditionType type;

    public float targetDistance;

    public BaseStat.StatType statValueTarget;
    public float statValueAmount;

    public BaseStat.StatType statRatioTarget;
    public float statRatioAmount;

    public Constants.StatusType hasStatusType;


    public bool CheckCondition(GameObject target, GameObject source)
    {
        bool result = false;

        switch (type)
        {
            case AbilityConditionType.DistanceFromTarget:
                result = CheckDistance(target, source);
                break;
            case AbilityConditionType.StatValue:
                result = CheckStatValue(target, source);
                break;
            case AbilityConditionType.StatRatio:
                result = CheckStatRatio(target, source);
                break;
            case AbilityConditionType.HasStatus:
                break;
            case AbilityConditionType.Grounded:
                break;
            case AbilityConditionType.HasTarget:
                break;
            default:
                break;
        }

        result = inverse == false ? result : !result;

        return result;
    }


    public bool CheckDistance(GameObject target, GameObject source)
    {
        bool result = false;

        float distance = Vector2.Distance(target.transform.position, source.transform.position);

        if (distance <= targetDistance)
            result = true;

        return result;
    }

    public bool CheckStatValue(GameObject target, GameObject source)
    {
        bool result = false;
        float statValue = compareAgainstTarget == true ? target.Entity().EntityStats.GetStatModifiedValue(statValueTarget) : source.Entity().EntityStats.GetStatModifiedValue(statValueTarget);

        if (statValue >= statValueAmount)
            result = true;

        return result;
    }

    public bool CheckStatRatio(GameObject target, GameObject source)
    {
        bool result = false;
        float statRatio = compareAgainstTarget == true ? target.Entity().EntityStats.GetCappedStatRatio(statRatioTarget) : source.Entity().EntityStats.GetCappedStatRatio(statRatioTarget);

        if (statRatio >= statRatioAmount)
            result = true;

        //Debug.Log(source.Entity().EntityStats.GetCappedStatRatio(statRatioTarget) + " is my health ratio");

        return result;
    }

    public bool CheckHasStatus(GameObject target, GameObject source)
    {
        bool result = false;

        result = compareAgainstTarget == true ? StatusManager.CheckForStatus(target, hasStatusType) : StatusManager.CheckForStatus(source, hasStatusType);

        return result;
    }

    public bool CheckGrounded(GameObject target, GameObject source)
    {
        bool result = false;

        result = compareAgainstTarget == true ? target.Entity().Movement.RayController.IsGrounded : source.Entity().Movement.RayController.IsGrounded;

        return result;
    }

    public bool HasTarget(GameObject source)
    {
        bool result = false;

        AISensor enemyAI = (source.Entity() as EntityEnemy).AISensor;

        result = enemyAI.ClosestTarget != null;

        return result;
    }
}

