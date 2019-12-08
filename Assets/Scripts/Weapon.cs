using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Item
{
    public WeaponType Type;
    public float Damage;
    public float Delay = 1;

    new void Update()
    {
        base.Update();
    }

    public override void Use()
    {

    }
}

public enum WeaponType
{
    Melee,
    Rifle,
}