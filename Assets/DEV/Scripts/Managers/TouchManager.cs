using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchManager : MonoBehaviour
{
    public static TouchManager instance;

    public PlayerBodyPart CurrentBodyPart { get {  return currenyBodyPart; } }
    public bool Active { get { return active; } }

    [Title("Main")]
    [SerializeField] bool active = true;
    [SerializeField] bool isSelectedBodyPart;
    [SerializeField] Camera cam;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] Transform spineTrs;

    [Space(6)]

    [Title("Ball")]
    [SerializeField] LayerMask ballLayer;
    [SerializeField] PlayerRopeBall currentRopeBall;

    private PlayerBodyPart currenyBodyPart;
    private Vector3 startTouchPos;
    private Vector3 endTouchPos;
    private Vector3 currentPos;
    public static float Distance { get { return Vector3.Distance(instance.startTouchPos, instance.endTouchPos); } }
    public static bool IsSelectedBodyPart { get { return instance.isSelectedBodyPart; } }
    public static float Meter { 
        get {

            bool isNegative = instance.startTouchPos.y > instance.spineTrs.position.y;
            float mtr = Vector3.Distance(instance.startTouchPos, instance.spineTrs.position);
            mtr *= isNegative ? -1 : 1;
            return mtr; 
        } 
    }
    public static PlayerRopeBall RopeBall { get { return instance.currentRopeBall; } }
    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }


    private void Update()
    {
        if (!active)
            return;

        if (Input.GetMouseButtonUp(0))
        {
            TouchUp();
        }
        else if (Input.GetMouseButtonDown(0))
        {
            TouchDown();
        }
        if (Input.GetMouseButton(0))
        {
            Touch();
        }
    }

    public void SetActive(bool active)
    {
        this.active = active;
    }

    private void Touch()
    {
        if (!isSelectedBodyPart)
            return;


        endTouchPos = currenyBodyPart.transform.position;

    }

    private void TouchDown()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray,out hit, 100, ballLayer))
        {
            PlayerRopeBall ball = hit.collider.GetComponent<PlayerRopeBall>();
            currentRopeBall = ball;
            currentRopeBall.SetActiveMove(active: true);
        }

        if (PlayerController.State == PlayerState.Ready)
        {

            if (Physics.Raycast(ray, out hit, 100, playerLayer))
            {
                
                isSelectedBodyPart = true;
                currenyBodyPart = PlayerBodyController.JumpBody;
                currenyBodyPart.SetFreezeConstraints(active: true);
                currenyBodyPart.SetSelected(active: true);
                startTouchPos = currenyBodyPart.transform.position;
                PlayerEffectController.instance.ReadyEffect(useDelay: false);
                PlayerOutlineController.instance.SetActiveOutlines(active: true);
            }

        }
        else
        {
            if (!PlayerRopeManager.Active)
            {
                MeterPanelController.SetVisibility(active: false).Forget();
                PlayerRopeManager.SetActiveRopes(active: true, delay: 0, afterHit: true).Forget();
                //PlayerBodyController.SpinePart.ResetRotate(duration: 0.3f, delay: 0.7f).Forget();
            }
        }
    }

    private void TouchUp()
    {

        if (PlayerController.State == PlayerState.Fall)
            return;

        if (currenyBodyPart)
        {
            PlayerOutlineController.instance.SetActiveOutlines(active:false);
            isSelectedBodyPart = false;
            if (Input.GetKey(KeyCode.LeftControl))
                return;

            //Break
            currenyBodyPart.SetSelected(active: false);
            currenyBodyPart.SetFreezeConstraints(active: false);
            endTouchPos = currenyBodyPart.transform.position;
            currenyBodyPart = null;



            //Jump
            float distance = Vector3.Distance(startTouchPos, endTouchPos);
            bool jump = distance >= PlayerBodyController.JumpTriggerDistance;

            if (jump)
            {
                Vector3 power = startTouchPos - endTouchPos;
                float _distance = Vector3.Distance(startTouchPos, endTouchPos);

                PlayerRopeManager.SetActiveRopes(active: false, delay: 0.2f).Forget();
                PlayerBodyController.Jump(power: power);
                MeterPanelController.SetVisibility(active: true).Forget();
            }
        }

        if (currentRopeBall)
        {
            currentRopeBall.SetActiveMove(active:false);
            currentRopeBall.SetFreeBall(active: true).Forget();
            currentRopeBall.Hit(useLocalPos: true);
            currentRopeBall = null;
        }
    }
}
