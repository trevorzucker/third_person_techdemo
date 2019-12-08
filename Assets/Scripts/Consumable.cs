using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consumable : Item
{
    public ConsumeType ConsumeType;
    public float HPRestore;
    public int Uses = 1;

    public override void Use()
    {
        if (Uses > 0)
        {
            if (ConsumeType == ConsumeType.Drink)
                OwnerObject.GetComponent<Animator>().SetTrigger("Drink");
            Uses--;
        }
    }
}

public enum ConsumeType
{
    Eat,
    Drink,
    None
}