using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum PipePieceType { Pos,Hit}
public class PipePieceManager : MonoBehaviour
{
    public static PipePieceManager instance;

    [Title("Main")]
    [SerializeField] List<PipePieceController> allPieces;

    [Space(6)]

    [Title("Break")]
    [SerializeField] float minDur;
    [SerializeField] float maxDur;
    [SerializeField] float durMultiplier;
    [SerializeField] LayerMask groundLayer;

    [Space(6)]

    [Title("Hit")]
    [SerializeField] float hitTriggerDistance;

    public static LayerMask GroundLayer { get { return instance.groundLayer; } }
    public static float DurMultiplier { get { return instance.durMultiplier; } }


    public static float Duration { get { return Random.Range(instance.minDur, instance.maxDur); } }

    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }


    [Button(size: ButtonSizes.Large)]
    [SerializeField] void AllPiecesListUpdate()
    {
        allPieces = FindObjectsOfType<PipePieceController>().ToList();//.FindAll(piece => piece.PieceType == PipePieceType.Hit).ToList();
    }

    public static void AddPiece(PipePieceController piece)
    {
        if (instance.allPieces.Contains(piece))
            return;

        instance.allPieces.Add(piece);
    }

    public static void RemovePiece(PipePieceController piece) {
        if (!instance.allPieces.Contains(piece))
            return;

        instance.allPieces.Remove(piece);
    }


    public static void Hit(PlayerRopeBall ball)
    {
        List<PipePieceController> pieces = new List<PipePieceController>();

        Vector3 ballPos = ball.transform.position;
        pieces = instance.allPieces.FindAll(piece => piece.GetDistance(ballPos) < instance.hitTriggerDistance).ToList();
        pieces.ForEach(piece => piece.BreakPiece().Forget());
    }
}
