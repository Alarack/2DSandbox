using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    public bool testMode;

    [Header("VFX")]
    public GameObject particleTrail;



    public StatCollectionData statTemplate;
    public StatCollection ProjectileStats { get; protected set; }

    public LayerMask Mask; /*{ get; protected set; }*/

    protected Effect parentEffect;
    protected ZoneInfo payloadZoneInfo;
    protected EffectZone activeZone;

    protected int penetrationCount = 0;

    //Movement
    protected Rigidbody myBody;
    protected float maxSpeed = 10f;

    private void Awake()
    {
        myBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (testMode == false)
            return;

        payloadZoneInfo = new ZoneInfo();
        payloadZoneInfo.durationType = EffectZone.EffectZoneDuration.Instant;
        payloadZoneInfo.size = VisualEffectLoader.VisualEffectSize.Medium;
        payloadZoneInfo.shape = VisualEffectLoader.VisualEffectShape.Sphere;
        SetUpStats();
    }

    public void Initialize(Effect parent)
    {
        payloadZoneInfo = parent.effectZoneInfo;
        parentEffect = parent;
        Mask = parent.layerMask;
        SetUpStats();
    }

    protected void SetUpStats()
    {
        ProjectileStats = new StatCollection(gameObject, OnStatChanged, statTemplate);
        maxSpeed = ProjectileStats.GetStatModifiedValue(BaseStat.StatType.MoveSpeed);

        float life = ProjectileStats.GetStatModifiedValue(BaseStat.StatType.Lifetime);

        if (life > 0)
            Invoke("CleanUp", life);
    }



    protected virtual void OnTriggerEnter(Collider other)
    {
        DeployAfterLayerCheck(other.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DeployAfterLayerCheck(other.gameObject);
    }

    private void DeployAfterLayerCheck(GameObject other)
    {
        if (LayerTools.IsLayerInMask(Mask, other.gameObject.layer) == false)
            return;

        DeployEffectZone();

        HandlePenetration();
    }

    private void HandlePenetration()
    {
        float penetrationCount = ProjectileStats.GetStatModifiedValue(BaseStat.StatType.ProjectilePenetration);

        if (penetrationCount <= -1f)
            return;

        if(penetrationCount <= 0)
        {
            CleanUp();
        }
        else
        {
            StatAdjustmentManager.ApplyUntrackedStatMod(ProjectileStats, ProjectileStats, BaseStat.StatType.ProjectilePenetration, -1f);
        }
    }

    private void DeployEffectZone()
    {
        activeZone = EffectZoneFactory.CreateEffect(payloadZoneInfo, transform.position, Quaternion.identity);

        Transform originPoint = null;
        if (payloadZoneInfo.parentEffectToOrigin == true)
            originPoint = transform;

        activeZone.Initialize(parentEffect, Mask, originPoint);
    }


    public virtual void CleanUp()
    {
        Destroy(gameObject);
    }


    private void OnStatChanged(BaseStat.StatType stat, GameObject source)
    {
        if (stat == BaseStat.StatType.MoveSpeed)
            maxSpeed = ProjectileStats.GetStatModifiedValue(BaseStat.StatType.MoveSpeed);

    }

    private void FixedUpdate()
    {
        //myBody.velocity = transform.forward * maxSpeed * Time.deltaTime;
    }


}
