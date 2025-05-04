using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;

public class PipePieceController : MonoBehaviour
{
    [Title("Main")]
    [SerializeField] bool active = true;
    [SerializeField] bool isBreak;
    [SerializeField] PipePieceType pieceType;
    [SerializeField] bool isOutPiece;
    [ShowIf("isOutPiece")][SerializeField] PipePieceController inPiece;

    private MeshRenderer mr;
    private Rigidbody rb;
    private Transform bodyTrs;
    private Transform columnTrs;
    private Vector3 defaultPos;
    private Vector3 defaultRotate;
    private Vector3 defaultScale;
    private Vector3 rayPos;
    private Vector3 groundPos;
    private Vector3 breakPos;
    private PipeController controller;
    private float Duration { get { return PipePieceManager.Duration; } }
    public bool Active { get { return active; } }
    public PipePieceType PieceType { get { return pieceType; } }
    private CameraManager camManager;
    private PlayerController playerController;
    private void Awake()
    {
        mr = GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody>();

        defaultPos = transform.position;
        defaultRotate = transform.eulerAngles;
        defaultScale =transform.localScale;

        columnTrs = GetComponentInParent<PipeController>().transform;
        controller = GetComponentInParent<PipeController>();
    }


    private void Start()
    {
        playerController = PlayerController.instance;
        bodyTrs = PlayerBodyController.JumpBody.transform;
        RayPosUpdate();
        camManager = CameraManager.instance;
        if (!isOutPiece)
            SetActiveRenderer(active: false);
    }


    private void Update()
    {
        if (!active)
            return;
        PosController();
        VisibilityController();
    }

    public void SetActiveIsOutPiece(bool active)
    {
        isOutPiece = active;
    }

    public void AssignInPiece(PipePieceController piece)
    {
        inPiece = piece;
    }

    public void SetActiveRenderer(bool active)
    {
        mr.enabled = active;
    }

    float distance;
    float maxDistance;
    public void VisibilityController()
    {
        if (!playerController.IsAlive)
            return;

        if (!isOutPiece)
            return;

        distance = Vector3.Distance(transform.position, bodyTrs.position);
        maxDistance = camManager.transposer.m_CameraDistance;
        maxDistance *= 1.2f;
        if (distance > maxDistance) { 
            mr.enabled = false;
        }
        else
        {
            mr.enabled = true;
        }
    }




    private void RayPosUpdate()
    {
        rayPos = transform.position;
        Vector3 columnPos = columnTrs.position;
        columnPos.y = transform.position.y;
        Vector3 dir = transform.position - columnPos;

        float dirMultiplier = UnityEngine.Random.Range(4f, 9f);
        rayPos += dir * dirMultiplier;

        RaycastHit hit;
        if (Physics.Raycast(rayPos, Vector3.down, out hit, 1000000f, PipePieceManager.GroundLayer))
        {
            groundPos = hit.point;
            breakPos = groundPos;
            breakPos.y = transform.position.y;
        }
    }

    private void PosController()
    {
        if (isBreak)
            return;

        if (pieceType is not PipePieceType.Pos)
            return;

        if (PlayerController.State == PlayerState.Ready)
            return;

        if (transform.position.y < bodyTrs.position.y + 0.5f)
        {
            BreakPiece().Forget();
        }
    }


    public async UniTaskVoid BreakPiece(float delay=0)
    {
        isBreak= true;
        await UniTask.Delay(TimeSpan.FromSeconds(delay));
        //transform.DOShakeScale(duration:Duration,strength:0.2f,vibrato:10);

        PipePieceManager.RemovePiece(this);

        float distance = (Vector3.Distance(transform.position, groundPos));
        float duration = ((distance) / PipePieceManager.DurMultiplier) * controller.BreakTimeMultiplier;

        AnimationCurve jumpCurve = CurveManager.GetCurve("Piece Jump");


        bool inversePos = UnityEngine.Random.Range(0, 11) % 2 == 0;
        groundPos.x += groundPos.x * (distance * 0.1f) * (inversePos ? -1 : 1);
        groundPos.z += groundPos.z * (distance * 0.1f) * (inversePos ? -1 : 1);


        transform.DOJump(endValue: groundPos, jumpPower: 3, numJumps: 1, duration: duration).SetEase(jumpCurve);
        

        Vector3 plusRotate = Vector3.one * UnityEngine.Random.Range(-120, 120);
        duration = Mathf.Clamp(duration, 0.5f, 1.25f);
        transform.DORotate(plusRotate, duration, RotateMode.WorldAxisAdd).SetDelay(0.2f).SetEase(jumpCurve);

        if (isOutPiece)
            inPiece.mr.enabled = true;

        this.enabled = false;
    }

    public float GetDistance(Vector3 targetPos)
    {
        Vector3 pos = transform.position;

        float distance = Vector3.Distance(pos, targetPos);

        return distance;
    }

}
