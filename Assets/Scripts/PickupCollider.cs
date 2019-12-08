using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupCollider : MonoBehaviour
{
    public Side Side;
    private void OnTriggerStay(Collider other)
    {
        if (other == null || other.GetComponent<Item>() == null) return;
        Player p = transform.parent.GetComponent<Player>();
        if (p.isPickingUp) return;
        if (p.CurrentItem != null && !p.CurrentItem.isHolstered) return;

        if (p.CurrentItem != null && p.CurrentItem.isHolstered) p.CurrentItem = null;
        p.dropInfo.side = Side;
        p.dropInfo.item = other.gameObject;

        Animator anim = transform.parent.GetComponent<Animator>();

        if (other.GetComponent<Item>().MountType == GrabType.LeftHanded)
            anim.SetBool("PickupLeft", true);
        else if (other.GetComponent<Item>().MountType == GrabType.RightHanded)
            anim.SetBool("PickupRight", true);
        else if (other.GetComponent<Item>().MountType == GrabType.BothHands)
            anim.SetBool("PickupCenter", true);
        p.isPickingUp = true;
    }

}

public enum Side
{
    Left,
    Right,
}
