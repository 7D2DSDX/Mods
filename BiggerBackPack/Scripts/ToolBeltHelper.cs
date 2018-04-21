using UnityEngine;
using System;
using System.Collections;

class InventoryHelperSDX   : ItemActionEntryUse
{
    public InventoryHelperSDX(XUiController controller, ConsumeType consumeType) : base(controller,consumeType)
    {
    }

    public IEnumerator decInventoryLater(Inventory inventory)
    {
        yield return new WaitForSeconds(0.25f);
        yield return 1;
        inventory.SetItem(9, ItemStack.Empty.Clone());
        inventory.OnUpdate();
        ((XUiC_ItemStack)base.ItemController).HiddenLock = false;
        GameManager.Instance.StartCoroutine(this.unlockUseLater());
        yield break;
        yield break;
    }
}

