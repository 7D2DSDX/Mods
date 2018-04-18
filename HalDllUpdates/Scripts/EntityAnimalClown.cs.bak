using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

public class EntityAnimalClown : EntityZombie
{
    public EntityAnimalClown() : base()
    {

    }

    public override void OnUpdateLive()
    {
        base.OnUpdateLive();

    }

    public override bool IsImmuneToLegDamage
    {
        get { return false; }
    }

    protected override void Awake()
    {
        base.Awake();
        
        
    }
    

     public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale)
    {
        
        int ret = base.DamageEntity(_damageSource, _strength, _criticalHit, impulseScale);
        return ret;
    }

    protected override DamageResponse damageEntityLocal(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale)
    {

        this.Health -= _strength;
        DamageResponse ret =  base.damageEntityLocal(_damageSource, _strength, _criticalHit, impulseScale);
     //   Debug.Log("Response: " + ret.ToString() + " strength: " + _strength + " type: " + _damageSource.GetName().ToString() + " health: " + this.Health);
        return ret;

    }
    public override Vector3 GetMapIconScale()
    {
        return new Vector3(0.45f, 0.45f, 1f);
    }
}
