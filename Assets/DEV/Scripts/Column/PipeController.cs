using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PipeController : MonoBehaviour
{
    [SerializeField] float breakTimeMultiplier = 1;
    [SerializeField] bool randomDurationActive;

    [SerializeField] List<PipePieceController> outPieces;
    [SerializeField] List<PipePieceController> inPieces;
    public float BreakTimeMultiplier { get { return breakTimeMultiplier; } }


    [Button(size: ButtonSizes.Large)]
    public void OutPiecesUpdate(Transform parent)
    {
        outPieces = parent.GetComponentsInChildren<PipePieceController>().ToList();
        outPieces.ForEach(piece => piece.SetActiveIsOutPiece(active: true));

        int index = 0;

        while (index < outPieces.Count)
        {
            PipePieceController piece = inPieces[index];
            outPieces[index].AssignInPiece(piece);
            index++;
        }

    }

    [Button(size: ButtonSizes.Large)]
    public void InPiecesUpdate(Transform parent)
    {
        inPieces = parent.GetComponentsInChildren<PipePieceController>().ToList();
        outPieces.ForEach(piece => piece.SetActiveIsOutPiece(active: false));
    }
}
