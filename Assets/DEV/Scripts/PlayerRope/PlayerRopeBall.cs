using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRopeBall : MonoBehaviour
{
    [Title("Main")]
    [SerializeField] bool move;
    [SerializeField] bool moveAble;
    [SerializeField] float moveSpeed;
    [SerializeField] private MeshRenderer mr;
    [SerializeField] private Transform ballTrs;

    [Space(6)]

    [Title("Colors")]
    [SerializeField] Color activeColor;
    [SerializeField] Color moveColor = Color.cyan;
    [SerializeField] Color deActiveColor;

    private Rigidbody rb;
    private PlayerRope rope;
    private Vector3 defaultScale;
    private Color defaultColor;
    private Vector3 ballDefaultScale;
    private Transform startPos;



    private void Awake()
    {
        rope = GetComponentInParent<PlayerRope>();
        rb = GetComponent<Rigidbody>();

        defaultScale = transform.localScale;
        defaultColor = mr.material.color;
        ballDefaultScale = ballTrs.localScale;

        activeColor = defaultColor;
    }

    private void Start()
    {
        moveSpeed = PlayerRopeManager.RopeBallMoveSpeed;
        startPos = StartLineController.instance.transform;
    }

    private void Update()
    {
        Movement();
        SizeController();
    }


    [SerializeField] bool sizeEffect;
    [SerializeField] bool sizeEffectUp;
    private void SizeController()
    {
        if (!sizeEffect)
            return;

        Vector3 scale = ballTrs.localScale;
        Vector3 targetScale = sizeEffectUp ? ballDefaultScale * 1.15f : ballDefaultScale;

        scale = Vector3.MoveTowards(scale, targetScale, Time.deltaTime * 1.2f);
        ballTrs.localScale = scale;

        if(scale == targetScale)
        {
            sizeEffectUp = !sizeEffectUp;
        }

    }

    private void Movement()
    {
        if (!move)
            return;

        Vector3 pos = transform.position;
        Vector3 targetPos = TouchPlaneController.TouchPos;

        targetPos.x = Mathf.Clamp(targetPos.x, -3f, 3f);

        targetPos.z = transform.position.z;
        pos = Vector3.Lerp(pos, targetPos, moveSpeed * Time.deltaTime);
        transform.position = pos;
    }

    [Button(size: ButtonSizes.Large)]
    public async UniTaskVoid SetFreeBall(bool active, float delay = 0)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));

        if (active)
        {
            rb.constraints = RigidbodyConstraints.None;
            rb.isKinematic = false;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.isKinematic = true;
        }
    }

    public async UniTaskVoid SetVisibility(bool active, float duration = 0,float delay = 0)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));

        Vector3 targetScale = active ? defaultScale : Vector3.zero;


        if (active)
        {
            transform.DOScale(targetScale, duration);
        }
        else
        {
            transform.DOScale(targetScale, duration);
        }
    }

    public void SetActiveMove(bool active)
    {
        //Active
        move = active;

        //Size Effect
        sizeEffect = active;
        sizeEffectUp = true;

        //Other Effects
        HitEffect(targetColor:moveColor,returnTheBaseColor: false);
        Vector3 endScale = active ? defaultScale * 1.2f : defaultScale;
        transform.DOScale(endScale, 0.15f);

        if (!active)
        {
            ballTrs.DOScale(ballDefaultScale, 0.15f);
        }
    }

    public void HitEffect(Color targetColor = default(Color), bool returnTheBaseColor = true)
    {
        targetColor = (targetColor == default(Color) ? Color.white : targetColor);

        mr.material.DOColor(targetColor,0.1f).SetEase(Ease.Linear).OnComplete( () =>
        {
            if (returnTheBaseColor)
                mr.material.DOColor(activeColor, 0.1f).SetDelay(0.05f);
        });
    }

    public bool GetHitable()
    {
        Vector3 direction = rope.RopeDirection is RopeDirection.Right ? Vector3.right : Vector3.left;
        Vector3 point = Vector3.zero;
        RaycastHit hit;
        Vector3 startPos = PlayerRopeManager.CenterPos.position;
        LayerMask layer = PlayerRopeManager.ColumnLayer;


        bool isHitable = Physics.Raycast(startPos, direction, out hit, 100, layer);
    
        return isHitable;
    }

    public void Hit(bool useLocalPos =false)
    {
        
        SetFreeBall(active: false, delay: 0).Forget();
        Vector3 direction = rope.RopeDirection is RopeDirection.Right ? Vector3.right : Vector3.left;

        Vector3 point = Vector3.zero;
        RaycastHit hit;
        Vector3 startPos = useLocalPos ? transform.position : PlayerRopeManager.CenterPos.position;
        LayerMask layer = PlayerRopeManager.ColumnLayer;

        if (Physics.Raycast(startPos, direction, out hit, 100, layer))
        {
            point = hit.point;
            transform.DOMove(point, 0.15f).SetEase(Ease.Linear).OnComplete(() => {
                ShakeScale();
                HitEffect();
                FXManager.PlayFX(id: "Rope Hit White", transform.position, desTime:3f).Forget();
                PipePieceManager.Hit(this);
                PlayerRopeManager.ActiveRopeUVEffect();
            });
        }
        else
        {

        }
    }

    private void ShakeScale()
    {
        transform.DOShakeScale(strength: 1, vibrato: 10, duration: 0.3f);
    }
}
