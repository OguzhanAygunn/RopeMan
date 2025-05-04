using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AirBreakablePipeController : MonoBehaviour
{
    private List<PipePieceController> pieces;
    [SerializeField] List<Transform> effectPoses;

    private void Awake()
    {
        pieces = GetComponentsInChildren<PipePieceController>().ToList();
    }


    [Button(size: ButtonSizes.Large)]
    public void BreakPieces()
    {

        float delay = 0;
        pieces.ForEach(piece => {

            piece.BreakPiece(delay: delay).Forget();
            delay += 0.01f;
        });

        EffectSpawn().Forget();
    }

    private async UniTaskVoid EffectSpawn()
    {
        foreach (Transform effectPos in effectPoses)
        {
            FXManager.PlayFX(id: "Air Pipe Exp", pos: effectPos.position, desTime: 5f).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(0.033f));
        }
    }
}
