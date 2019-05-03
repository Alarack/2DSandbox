using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LL.Events;
using EffectOrigin = Constants.EffectOrigin;
using EffectDeliveryMethod = Constants.EffectDeliveryMethod;
using EffectTag = Constants.EffectTag;
using EffectType = Constants.EffectType;

[System.Serializable]
public class Effect {

    public string effectName;
    public string riderTarget;
    public List<EffectTag> tags = new List<EffectTag>();
    public EffectOrigin effectOrigin;
    public EffectDeliveryMethod deliveryMethod;
    public EffectType effectType;
    public Constants.EffectDurationType durationType;


    public StatusTypeInfo statusTypeInfo;

    protected StatusTargetInfo statusTargetInfo;
    protected StatusInfo statusInfo;

    //Projectile Stuff
    public ProjectileInfo projectileInfo;



    public ZoneInfo effectZoneInfo;
    public string animationTrigger;
    public LayerMask layerMask;


    public Ability ParentAbility { get { return parentAbility; } protected set { parentAbility = value; } }
    public GameObject Source { get { return ParentAbility.Source; } }
    public List<GameObject> Targets { get; protected set; }
    public int EffectID { get; protected set; }


    [System.NonSerialized]
    protected List<Effect> riders = new List<Effect>();

    [System.NonSerialized]
    protected Ability parentAbility;
    protected EffectZone activeZone;
    protected List<Status> activeStatus = new List<Status>();

    public Effect()
    {

    }

    public Effect(Ability parentAbility)
    {
        this.ParentAbility = parentAbility;
        Targets = new List<GameObject>();



    }

    public virtual void Activate()
    {
        PlayEffectAnim();
    }

    public virtual bool IsFromSameSource(Ability ability)
    {
        return ParentAbility == ability;
    }

    public virtual void BeginDelivery()
    {
        switch (deliveryMethod)
        {
            case EffectDeliveryMethod.Instant:
                activeZone = EffectZoneFactory.CreateEffect(effectZoneInfo, effectOrigin, ParentAbility.Source);

                if (activeZone != null)
                {

                    Transform originPoint = null;
                    if (effectZoneInfo.parentEffectToOrigin == true)
                        originPoint = Source.Entity().EffectDelivery.GetOriginPoint(effectOrigin);

                    activeZone.Initialize(this, layerMask, originPoint);
                }


                break;

            case EffectDeliveryMethod.Projectile:
                DeliverProjectiles();
                break;

            case EffectDeliveryMethod.SelfTargeting:
                Apply(Source);
                break;

            case EffectDeliveryMethod.ExistingTargets:

                break;

            case EffectDeliveryMethod.Rider:

                break;
        }
    }

    #region PROJECTILE CREATION

    protected void DeliverProjectiles()
    {
        if (projectileInfo.projectileCount == 1)
        {
            Projectile shot = ProjectileFactory.CreateProjectile(projectileInfo, effectOrigin, ParentAbility.Source);
            shot.Initialize(this);

            if(projectileInfo.addInitialForce == true)
            {
                Vector2 force = projectileInfo.initialForce.CalcDirectionAndForce(shot.gameObject, Source);
                shot.GetComponent<Rigidbody2D>().AddForce(force);
            }
        }
        else
        {
            ParentAbility.Source.GetMonoBehaviour().StartCoroutine(CreateProjectileBurst());
        }
    }

    protected IEnumerator CreateProjectileBurst()
    {
        WaitForSeconds delay = new WaitForSeconds(projectileInfo.burstDelay);

        int count = projectileInfo.projectileCount;
        for (int i = 0; i < count; i++)
        {
            Projectile shot = ProjectileFactory.CreateProjectile(projectileInfo, effectOrigin, ParentAbility.Source, i);
            shot.Initialize(this);

            if (projectileInfo.burstDelay > 0)
                yield return delay;
            else
                yield return null;

        }
    }

    #endregion

    public virtual void Apply(GameObject target)
    {
        //TODO: Create Effect Constraint Varification System

        if (durationType != Constants.EffectDurationType.Instant)
            CreateStatusInfo(target);


        Targets.AddUnique(target);
        ParentAbility.targets.AddUnique(target);
        //CreateAndRegisterStatus(target);
        ApplyRiderEffects(target);
        SendEffectAppliedEvent(target);

    }

    protected virtual void CreateStatusInfo(GameObject target)
    {
        statusTargetInfo = new StatusTargetInfo(target, ParentAbility.Source, ParentAbility, this);
        statusInfo = new StatusInfo(statusTargetInfo, statusTypeInfo);
    }

    public virtual void Remove(GameObject target, GameObject cause = null)
    {
        Targets.RemoveIfContains(target);
        ParentAbility.targets.RemoveIfContains(target);
        RemoveMyActiveStatus();
        SendEffectRemovedEvent(cause, target);
    }


    protected virtual void RemoveMyActiveStatus()
    {
        int count = activeStatus.Count;
        for (int i = 0; i < count; i++)
        {
            activeStatus[i].Remove();
        }

        activeStatus.Clear();
    }

    public virtual void RemoveFromAll()
    {
        int count = Targets.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            Remove(Targets[i]);
        }
    }

    protected virtual void ApplyRiderEffects(GameObject target)
    {
        int count = riders.Count;
        for (int i = 0; i < count; i++)
        {

        }
    }

    protected virtual bool CreateAndRegisterStatus(GameObject target)
    {
        return true;
    }


    public virtual void PlayEffectAnim()
    {
        //TODO: this assumes the source is always an entity, it could be a projectile
        bool animStarted = Source.Entity().AnimHelper.PlayAnimTrigger(animationTrigger); // Animation trigger will start the delivery at the right time.

        //Debug.Log(animStarted + " is anim started");

        if (animStarted == false) // Start Delivery Instantly if there isn't an animator.
            BeginDelivery();
    }


    #region EVENTS
    protected void SendEffectAppliedEvent(GameObject target)
    {
        EventData data = new EventData();
        data.AddGameObject("Cause", Source);
        data.AddGameObject("Target", target);
        data.AddEffect("Effect", this);

        EventGrid.EventManager.SendEvent(Constants.GameEvent.EffectApplied, data);
    }

    protected void SendEffectRemovedEvent(GameObject cause, GameObject target)
    {
        EventData data = new EventData();
        data.AddGameObject("Cause", cause);
        data.AddGameObject("Target", target);
        data.AddEffect("Effect", this);

        EventGrid.EventManager.SendEvent(Constants.GameEvent.EffectRemoved, data);
    }


    #endregion


}


[System.Serializable]
public struct EffectOriginPoint {
    public Transform point;
    public Constants.EffectOrigin originType;
}


[System.Serializable]
public struct ZoneInfo {
    public VisualEffectLoader.VisualEffectShape shape;
    public VisualEffectLoader.VisualEffectSize size;
    public EffectZone.EffectZoneDuration durationType;

    public float duration;
    public float interval;
    public bool removeEffectOnExit;
    public bool parentEffectToOrigin;
    public string effectZoneImpactVFX;
    public string effectZoneSpawnVFX;


    public ZoneInfo(VisualEffectLoader.VisualEffectShape shape, VisualEffectLoader.VisualEffectSize size, EffectZone.EffectZoneDuration durationType,
        float duration, float interval, bool removeEffectOnExit, bool parentEffectToOrigin, string effectZoneImpactVFX, string effectZoneSpawnVFX)
    {
        this.shape = shape;
        this.size = size;
        this.durationType = durationType;
        this.duration = duration;
        this.interval = interval;
        this.removeEffectOnExit = removeEffectOnExit;
        this.parentEffectToOrigin = parentEffectToOrigin;
        this.effectZoneImpactVFX = effectZoneImpactVFX;
        this.effectZoneSpawnVFX = effectZoneSpawnVFX;
    }

}
