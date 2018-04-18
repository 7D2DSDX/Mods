using UnityEngine;
using Object = UnityEngine.Object;

public class EntityAnimalHal : EntityAnimalStag
{
    private float meshScale = 1;

    public override void Init(int _entityClass)
    {
        base.Init(_entityClass);
        EntityClass entityClass = EntityClass.list[_entityClass];
        if (entityClass.Properties.Values.ContainsKey("MeshScale"))
        {
            string meshScaleStr = entityClass.Properties.Values["MeshScale"];
            string[] parts = meshScaleStr.Split(',');

            float minScale = 1;
            float maxScale = 1;

            if (parts.Length == 1)
            {
                maxScale = minScale = float.Parse(parts[0]);
            }
            else if (parts.Length == 2)
            {
                minScale = float.Parse(parts[0]);
                maxScale = float.Parse(parts[1]);
            }

            meshScale = UnityEngine.Random.Range(minScale, maxScale);
            this.gameObject.transform.localScale = new Vector3(meshScale, meshScale, meshScale);
        }
    }

    protected override void Awake()
    {
        BoxCollider component = this.gameObject.GetComponent<BoxCollider>();
        if ((bool)((Object)component))
        {
            component.center = new Vector3(0.0f, 0.85f, 0.0f);
            component.size = new Vector3(20.0f, 15.6f, 20.0f);
        }
        base.Awake();
    }    

    public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale)
    {
        return base.DamageEntity(_damageSource, _strength, _criticalHit, impulseScale);
    }
    public override Vector3 GetMapIconScale()
    {
        return new Vector3(0.45f, 0.45f, 1f);
    }
}

public class EntityAnimalHalRanged : EntityAnimalHal
{
    private float meshScale = 1;

    public override void Init(int _entityClass)
    {
        base.Init(_entityClass);
        EntityClass entityClass = EntityClass.list[_entityClass];
        if (entityClass.Properties.Values.ContainsKey("MeshScale"))
        {
            string meshScaleStr = entityClass.Properties.Values["MeshScale"];
            string[] parts = meshScaleStr.Split(',');

            float minScale = 1;
            float maxScale = 1;

            if (parts.Length == 1)
            {
                maxScale = minScale = float.Parse(parts[0]);
            }
            else if (parts.Length == 2)
            {
                minScale = float.Parse(parts[0]);
                maxScale = float.Parse(parts[1]);
            }

            meshScale = UnityEngine.Random.Range(minScale, maxScale);
            this.gameObject.transform.localScale = new Vector3(meshScale, meshScale, meshScale);
        }
        this.inventory.SetSlots(new ItemStack[1]
        {
            new ItemStack(this.inventory.GetBareHandItemValue(), 1)
        });
    }

    public override void InitFromPrefab(int _entityClass)
    {
        base.InitFromPrefab(_entityClass);
        this.inventory.SetSlots(new ItemStack[1]
        {
            new ItemStack(this.inventory.GetBareHandItemValue(), 1)
        });
    }
}