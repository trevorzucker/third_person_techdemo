using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public string Name;
    public HolsterLocation HolsterLocation;
    public GrabType MountType;
    public HolsterPosRot HolsterPosRot;
    public bool Throwable = true;

    Rigidbody rb;
    Collider box;


    public Vector3 HoldPos;
    public Quaternion HoldRot;
    public HoldType HoldType = HoldType.Standard;

    [HideInInspector]
    public GameObject OwnerObject;
    [HideInInspector]
    public bool isHolstered = false;
    [HideInInspector]
    public bool updatePosAng = true;
    [HideInInspector]
    public bool doRot = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        box = GetComponent<Collider>();
    }

    // Update is called once per frame
    public void Update()
    {
        if (transform.parent != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
            box.enabled = false;

            if (isHolstered)
                DoHolster();
            else
            {
                if (HolsterLocation == HolsterLocation.None && !transform.GetChild(0).gameObject.activeSelf)
                    transform.GetChild(0).gameObject.SetActive(true);
                updatePosAng = true;
            }

            DoPosRot();
        }
    }

    public abstract void Use();

    Transform spine, pelvisL, pelvisR;

    void DoHolster()
    {
        updatePosAng = false;
        if (spine == null || pelvisL == null || pelvisR == null)
        {
            spine = OwnerObject.transform.Find("Pelvis/Spine/Mount");
            pelvisL = OwnerObject.transform.Find("Pelvis/MountL");
            pelvisR = OwnerObject.transform.Find("Pelvis/MountR");
        }

        switch (HolsterLocation)
        {
            case HolsterLocation.Back:
                transform.parent = spine;
                transform.localPosition = Vector3.Lerp(transform.localPosition, HolsterPosRot.BackPos, Time.deltaTime * 8);
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(HolsterPosRot.BackRot), Time.deltaTime * 8);
                break;

            case HolsterLocation.LeftWaist:
                transform.parent = pelvisL;
                transform.localPosition = Vector3.Lerp(transform.localPosition, HolsterPosRot.WaistLeftPos, Time.deltaTime * 8);
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(HolsterPosRot.WaistLeftRot), Time.deltaTime * 8);
                break;

            case HolsterLocation.RightWaist:
                transform.parent = pelvisR;
                transform.localPosition = Vector3.Lerp(transform.localPosition, HolsterPosRot.WaistRightPos, Time.deltaTime * 8);
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(HolsterPosRot.WaistRightRot), Time.deltaTime * 8);
                break;

            case HolsterLocation.None:
                transform.GetChild(0).gameObject.SetActive(false);
                break;
        }
    }

    void DoPosRot()
    {
        if (isHolstered || !updatePosAng) return;
        if (MountType == GrabType.LeftHanded)
            transform.localPosition = Vector3.Lerp(transform.localPosition, HoldPos - new Vector3(0, 0.068f, 0), Time.deltaTime * 8);
        else
            transform.localPosition = Vector3.Lerp(transform.localPosition, HoldPos, Time.deltaTime * 8);
        if (!doRot) return;
        transform.localRotation = Quaternion.Lerp(transform.localRotation, HoldRot, Time.deltaTime * 4);
    }
}

public enum HolsterLocation
{
    Back,
    LeftWaist,
    RightWaist,
    None
}

[System.Serializable]
public struct HolsterPosRot
{
    public Vector3 BackRot;
    public Vector3 BackPos;
    public Vector3 WaistLeftRot;
    public Vector3 WaistLeftPos;
    public Vector3 WaistRightRot;
    public Vector3 WaistRightPos;
}

public enum GrabType
{
    RightHanded,
    LeftHanded,
    BothHands,
}

public enum HoldType
{
    Standard,
    Rifle,
    Pistol
}