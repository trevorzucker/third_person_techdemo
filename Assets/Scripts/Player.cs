using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    public Canvas Canvas;
    public float WalkSpeed = 2;
    public float RunSpeed = 4;
    
    Transform lHand, rHand, rForearm, spine;
    Animator animator;
    CharacterController cc;

    [HideInInspector]
    public DropInformation dropInfo;
    public List<Item> Inventory = new List<Item>();
    List<GameObject> InventoryUIElements = new List<GameObject>();
    [HideInInspector]
    public Item CurrentItem;
    [HideInInspector]
    public bool isPickingUp = false;

    GameObject cameraObject;

    float camZoom = 0, destZoom = 0;

    void Start()
    {
        lHand = transform.Find("Pelvis/Spine/LeftClavicle/LeftForearm/LeftArm/LeftHand");
        rHand = transform.Find("Pelvis/Spine/RightClavicle/RightForearm/RightArm/RightHand");
        rForearm = transform.Find("Pelvis/Spine/RightClavicle/RightForearm");
        spine = transform.Find("Pelvis/Spine");
        animator = GetComponent<Animator>();
        cameraObject = Camera.main.transform.parent.gameObject;
        cc = GetComponent<CharacterController>();
        camZoom = Camera.main.transform.localPosition.z; destZoom = camZoom;
    }

    void Update()
    {
        DoInput();
        MovementRotation();
        ManageCamera();
        //UpdateUI();
    }

    Vector3 faceDir, move, cameraOffsetPos, playerOffsetAng, initOffsetAng;
    Quaternion cameraRot = Quaternion.identity;
    float spineYInit = 0, spineY = 0;
    float fireTime = 0;

    void DoInput()
    {

        cc.SimpleMove(move);

        if (Input.GetMouseButtonDown(2) && !isPickingUp)
        {
            bool canHolster = true;
            foreach (Item i in Inventory)
            {
                if (CurrentItem != null && CurrentItem.HolsterLocation == i.HolsterLocation && i.isHolstered && CurrentItem != i && CurrentItem.HolsterLocation != HolsterLocation.None) canHolster = false;
            }
            if (canHolster && !isPickingUp && CurrentItem != null)
            {
                if (CurrentItem.HolsterLocation == HolsterLocation.Back)
                {
                    animator.SetTrigger("HolsterBack");
                    animator.SetInteger("HoldType", 0);
                }
                else if (CurrentItem.HolsterLocation == HolsterLocation.None || CurrentItem.HolsterLocation == HolsterLocation.RightWaist)
                {
                    animator.SetTrigger("HolsterPocket");
                }
            }

        }
        if (Input.GetMouseButton(1) && CurrentItem != null && !IsBusy() && (CurrentItem.Throwable || CurrentItem.GetComponent<Weapon>() != null) && !CurrentItem.isHolstered)
        {
            animator.SetBool("Aiming", true);
            spineYInit = cameraObject.transform.eulerAngles.x;
        }
        else
            animator.SetBool("Aiming", false);

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (CurrentItem != null && !CurrentItem.isHolstered && !IsBusy())
                animator.SetTrigger("TossItem");
        }
        if (Input.GetMouseButton(0) && CurrentItem != null && !IsBusy())
        {
            if (animator.GetCurrentAnimatorStateInfo(1).IsName("IdleThrow") && Input.GetMouseButtonDown(0))
                animator.SetTrigger("ThrowItem");
            else if (animator.GetInteger("HoldType") == 1 && CurrentItem.GetComponent<Weapon>() != null && animator.GetBool("Aiming"))
            {
                if (fireTime < Time.time)
                {
                    animator.SetBool("FireWeapon", true);
                    fireTime = Time.time + CurrentItem.GetComponent<Weapon>().Delay;
                }

            }
            else if (Input.GetMouseButtonDown(0))
            {
                CurrentItem.Use();
            }
        }

        if (!IsBusy() && !animator.GetBool("Aiming"))
            animator.SetBool("Sprinting", Input.GetKey(KeyCode.LeftShift));
        else
            animator.SetBool("Sprinting", false);



        if (isPickingUp)
            if (!animator.GetBool("PickupLeft") && !animator.GetBool("PickupRight") && !animator.GetBool("PickupCenter")) isPickingUp = false;
    }

    float spd = 0;

    void MovementRotation()
    {
        Vector3 XYMouse = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
        Vector3 newAng = cameraObject.transform.localEulerAngles + XYMouse;

        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        Vector3 newMove = Vector3.zero;

        if ((moveHorizontal != 0 || moveVertical != 0) && !isPickingUp)
        {
            if (!animator.GetBool("Aiming"))
                faceDir = new Vector3(moveHorizontal, 0.0f, moveVertical);
            cameraRot = Quaternion.Euler(0, cameraObject.transform.eulerAngles.y, 0);
            float speed = Mathf.Lerp(animator.GetFloat("WalkSpeed"), 1, Time.deltaTime * 6);
            if (animator.GetBool("Aiming") && moveVertical < 0)
                speed = Mathf.Lerp(animator.GetFloat("WalkSpeed"), -1, Time.deltaTime * 6);
            animator.SetFloat("WalkSpeed", speed);

            if (animator.GetBool("Sprinting"))
                spd = Mathf.Lerp(spd, RunSpeed, Time.deltaTime * 8);
            else
                spd = Mathf.Lerp(spd, WalkSpeed, Time.deltaTime * 16);
            newMove = Vector3.ClampMagnitude(cameraObject.transform.forward * moveVertical + cameraObject.transform.right * moveHorizontal, 1) * spd;
        }
        else
        {
            float speed = Mathf.Lerp(animator.GetFloat("WalkSpeed"), 0, Time.deltaTime * 6);
            animator.SetFloat("WalkSpeed", speed);
        }

        if (animator.GetBool("Aiming"))
        {
            faceDir = Vector3.zero;
            cameraOffsetPos = Vector3.Lerp(cameraOffsetPos, transform.right * 0.4f, Time.deltaTime * 4);
            playerOffsetAng = Vector3.Lerp(playerOffsetAng, new Vector3(0, 8), Time.deltaTime * 4);
        }
        else
        {
            cameraOffsetPos = Vector3.Lerp(cameraOffsetPos, Vector3.zero, Time.deltaTime * 4);
            playerOffsetAng = Vector3.Lerp(playerOffsetAng, Vector3.zero, Time.deltaTime * 4);
        }

        newAng.x = AngleClamp(newAng.x, -90, 90);
        cameraObject.transform.localEulerAngles = newAng;
        cameraObject.transform.position = transform.position + new Vector3(0, 1.53f) + cameraOffsetPos;

        if (animator.GetBool("Aiming"))
            cameraRot = Quaternion.Euler(0, cameraObject.transform.eulerAngles.y, 0);

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(faceDir) * cameraRot * Quaternion.Euler(playerOffsetAng), Time.deltaTime * 6);

        move = Vector3.Lerp(move, newMove, Time.deltaTime * 6);
    }

    float zoomSteps = 0.5f;

    void ManageCamera()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll < 0) // scroll down
        {
            destZoom -= zoomSteps;
        }
        else if (scroll > 0) // scroll up
        {
            destZoom += zoomSteps;
        }

        destZoom = Mathf.Clamp(destZoom, -5, -1.5f);
        camZoom = Mathf.Lerp(camZoom, destZoom, Time.deltaTime * 8);


        Vector3 camPos = Camera.main.transform.position;
        float dist = -Camera.main.transform.localPosition.z;
        Vector3 dir = (camPos - cameraObject.transform.position).normalized;
        Ray camTrace = new Ray(cameraObject.transform.position, dir);
        Debug.DrawRay(camTrace.origin, camTrace.direction * dist, Color.red);
        RaycastHit rH;
        if (Physics.Raycast(camTrace, out rH, dist))
        {
            print(rH.point);
            Camera.main.transform.position = Vector3.Lerp(camPos, rH.point, Time.deltaTime * 4);
            //Vector3 localPos = Camera.main.transform.localPosition;
            //Camera.main.transform.localPosition = new Vector3(0, 0, localPos.z);
        }
        else
            Camera.main.transform.localPosition = new Vector3(0, 0, Mathf.Lerp(Camera.main.transform.localPosition.z, camZoom, Time.deltaTime * 8));
    }

    Vector3 spineAng = Vector3.zero;

    void LateUpdate()
    {
        spineAng = spine.localEulerAngles;
        if (animator.GetBool("Aiming") || animator.GetCurrentAnimatorStateInfo(1).IsName("Throw"))
        {
            spineY = spineYInit + cameraObject.transform.eulerAngles.x;
            spine.localEulerAngles = spineAng + new Vector3(0, AngleClamp(spineY / 2, -70, 70));
        }
    }

    public bool IsBusy() // throwing, drinking, picking up, holster/unholster
    {
        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Throw") || animator.GetCurrentAnimatorStateInfo(1).IsName("Drink"))
            return true;

        if (animator.GetCurrentAnimatorStateInfo(3).IsName("PickUpFromFloorLeft") || animator.GetCurrentAnimatorStateInfo(3).IsName("PickUpFromFloorRight")
            || animator.GetCurrentAnimatorStateInfo(3).IsName("PickUpFromFloorCenter"))
            return true;

        if (animator.GetCurrentAnimatorStateInfo(2).IsName("TossItem") || animator.GetCurrentAnimatorStateInfo(2).IsName("ReachBackRightHand")
            || animator.GetCurrentAnimatorStateInfo(2).IsName("PutInPocket"))
            return true;

        if (isPickingUp) return true;

        return false;
    }

    void UpdateUI()
    {
        int j = 1;
        foreach(Item i in Inventory)
        {
            //if (InventoryUIElements.Count > 0 && InventoryUIElements[j - 1] == "dad") break;
            GameObject uiText = new GameObject(j.ToString() + i);
            uiText.transform.parent = Canvas.transform;
            RectTransform r =  uiText.AddComponent<RectTransform>();
            r.position = new Vector3(0, 0);

            CanvasRenderer renderer = uiText.AddComponent<CanvasRenderer>();
            Text text = uiText.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text.text = j + " " + i.Name;
            text.fontSize = 24;
            text.color = Color.black;
            InventoryUIElements[j - 1] = uiText;
            j++;
        }
    }

    #region Animation Events

    public void UnHolsterItem()
    {
        CurrentItem.isHolstered = !CurrentItem.isHolstered;
        if (!CurrentItem.isHolstered)
        {
            if (CurrentItem.GetComponent<Item>().HoldType == HoldType.Rifle)
                animator.SetInteger("HoldType", 1);
            else if (CurrentItem.GetComponent<Item>().HoldType == HoldType.Pistol)
                animator.SetInteger("HoldType", 2);
            CurrentItem.transform.parent = rHand;
        }
        else
            animator.SetInteger("HoldType", 0);
    }

    public void PickUp()
    {
        Item i = dropInfo.item.GetComponent<Item>();
        Inventory.Add(i);
        if (animator.GetCurrentAnimatorStateInfo(3).IsName("PickUpFromFloorLeft"))
            i.transform.parent = lHand;
        else
            i.transform.parent = rHand;
        i.isHolstered = false;
        i.OwnerObject = gameObject;
        i.doRot = false;
        CurrentItem = i;

        int val = 0;
        switch (i.GetComponent<Item>().HoldType)
        {
            case HoldType.Rifle:
                val = 1;
                break;
            case HoldType.Pistol:
                val = 2;
                break;
        }
        animator.SetInteger("HoldType", val);
    }

    public void SwitchHands()
    {
        if (CurrentItem == null) return;
        if (CurrentItem.MountType == GrabType.RightHanded)
            CurrentItem.transform.parent = rHand;
        if (CurrentItem.MountType == GrabType.LeftHanded)
            CurrentItem.transform.parent = lHand;
        else
            CurrentItem.transform.parent = rHand;
    }

    public void StartItemRotation()
    {
        if (CurrentItem == null) return;
        CurrentItem.updatePosAng = true;
        CurrentItem.doRot = true;
    }

    public void LetGoOfItem(int force = 1)
    {
        CurrentItem.OwnerObject = null;
        CurrentItem.transform.parent = null;
        CurrentItem.GetComponent<Rigidbody>().isKinematic = false;
        CurrentItem.GetComponent<Rigidbody>().detectCollisions = true;
        CurrentItem.GetComponent<Collider>().enabled = true;
        if (force == 1)
        CurrentItem.GetComponent<Rigidbody>().AddForce(-rHand.transform.right * (150 * force));
        else
            CurrentItem.GetComponent<Rigidbody>().AddForce(cameraObject.transform.forward * (150 * force) + cameraObject.transform.up * 64);
        CurrentItem.GetComponent<Rigidbody>().AddTorque(cameraObject.transform.forward * 20);
        Inventory.Remove(CurrentItem);
        CurrentItem = null;
        if (Inventory.Count > 0)
            CurrentItem = Inventory[Inventory.Count - 1];
        animator.SetInteger("HoldType", 0);
        animator.SetBool("Aiming", false);
    }

    public void ThrowItem()
    {
        LetGoOfItem(3);
    }

    public void PocketItem()
    {
        if (CurrentItem != null)
        {
            CurrentItem.isHolstered = !CurrentItem.isHolstered;
            if (CurrentItem.isHolstered)
                animator.SetInteger("HoldType", 0);
            else
            {
                if (CurrentItem.GetComponent<Item>().HoldType == HoldType.Rifle)
                    animator.SetInteger("HoldType", 1);
                else if (CurrentItem.GetComponent<Item>().HoldType == HoldType.Pistol)
                    animator.SetInteger("HoldType", 2);
            }
        }
    }

    public void ResetPickup()
    {
        animator.SetBool("PickupLeft", false);
        animator.SetBool("PickupRight", false);
        animator.SetBool("PickupCenter", false);
    }

    #endregion

    float AngleClamp(float angle, float min, float max)
    {

        if (angle > 180)
            angle -= 360;
        angle = Mathf.Max(Mathf.Min(angle, max), min);
        if (angle < 0)
            angle += 360;
        return angle;
    }
}

public struct DropInformation
{
    public GameObject item;
    public Side side;
}