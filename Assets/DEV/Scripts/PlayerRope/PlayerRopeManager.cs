using Cysharp.Threading.Tasks;
using Obi;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;


public enum RopeDirection { Left, Right }
public class PlayerRopeManager : MonoBehaviour
{
    public static PlayerRopeManager instance;
    [Title("Main")]
    [SerializeField] bool active;
    [SerializeField] ObiSolver solver; 
    [SerializeField] Transform centerPos;
    [SerializeField] LayerMask columnLayer;

    [Space(4)]

    [Title("Duration")]
    [SerializeField] float activeDuration; 
    [SerializeField] float deActiveDuration;

    [Space(6)]
    
    [Title("Iterations")]
    [SerializeField] float targetIteration;
    [SerializeField] float defaultIteration;
    [SerializeField] float maxDistance;

    [Space(6)]

    [Title("Main Ropes")]
    [SerializeField] List<PlayerRope> leftRopes;
    [SerializeField] List<PlayerRope> rightRopes;
    [SerializeField] List<PlayerRope> allRopes;

    [Space(6)]

    [Title("Ball")]
    [SerializeField] float ropeBallMoveSpeed;
    public static Transform CenterPos { get { return instance.centerPos; } }
    public static LayerMask ColumnLayer { get { return instance.columnLayer; } }
    public static bool Active { get { return instance.active; } }
    public static float RopeBallMoveSpeed { get { return instance.ropeBallMoveSpeed;} }
    private void Awake()
    {
        instance = (!instance) ? this : instance;

        allRopes.AddRange(leftRopes);
        allRopes.AddRange(rightRopes);
        active = true;
        iterationsVal = defaultIteration;


    }

    private void Update()
    {
        DistanceIterationController();
    }

    private void LateUpdate()
    {





        return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetActiveRopes(active: !active, afterHit: !active ? true : false).Forget();
        }
    }


    public void FreeBalls()
    {
        allRopes.ForEach(rope =>
        {
            rope.BallController.SetFreeBall(active: true).Forget();
        });
    }

    public static async UniTaskVoid SetActiveRopes(bool active, float delay = 0, bool afterHit = false)
    {
        //Delay
        instance.active = active;

        if (!active)
            StartLineController.SetVisibility(active: active, delay: active ? 1f : 0).Forget();

        await UniTask.Delay(TimeSpan.FromSeconds(delay));

        //rope visibility
        float duration = active ? instance.activeDuration : instance.deActiveDuration;
        instance.allRopes.ForEach(rope => rope.SetVisibility(active: active,duration:duration).Forget());

        //Update PlayerState
        PlayerState state = active ? PlayerState.Ready : PlayerState.Fall;
        PlayerController.State = state;

        //Rope Hits
        if (afterHit)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration));
            instance.allRopes.ForEach(rope => rope.HitRope());
            StartLineController.SetVisibility(active: active, delay: active ? 0.3f : 0).Forget();
        }
        else
        {

        }

    }

    public static void ActiveRopeUVEffect()
    {
        instance.allRopes.ForEach(rope => rope.RopeUVScaleEffect().Forget());
        PlayerEffectController.instance.ReadyEffect();
    }


    float iterationsVal;
    float iterationSlerpVal;
    public static float IterationsVal { get { return instance.iterationsVal; } }
    private void DistanceIterationController()
    {
        if (!TouchManager.IsSelectedBodyPart)
        {
            iterationsVal = Mathf.MoveTowards(iterationsVal, defaultIteration, 20 * Time.deltaTime);
            solver.distanceConstraintParameters.iterations = (int)iterationsVal;
            return;
        }


        iterationSlerpVal = TouchManager.Distance / maxDistance;
        iterationsVal = Mathf.Lerp(defaultIteration, targetIteration, iterationSlerpVal);
        solver.distanceConstraintParameters.iterations = (int)iterationsVal;
    }
}
