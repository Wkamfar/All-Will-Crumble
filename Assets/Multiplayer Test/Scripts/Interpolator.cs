using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interpolator : MonoBehaviour
{
    [SerializeField] private float timeElapsed = 0f;
    [SerializeField] private float timeToReachTarget = 0.05f;
    [SerializeField] private float movementThreshold = 0.05f;
    private readonly List<TransformUpdate> futureTransformUpdates = new List<TransformUpdate>();

    private float squareMovementThreshold;
    private TransformUpdate to;
    private TransformUpdate from;
    private TransformUpdate previous;
    private void Start()
    {
        squareMovementThreshold = movementThreshold * movementThreshold;
        to = new TransformUpdate(NetworkManager.Singleton.ServerTick, false, new Vector3(0, 0, 0), transform.position);
        from = new TransformUpdate(NetworkManager.Singleton.InterpolationTick, false, new Vector3(0, 0, 0), transform.position);
        previous = new TransformUpdate(NetworkManager.Singleton.InterpolationTick, false, new Vector3(0, 0, 0), transform.position);
    }
    private void Update()
    {
        for (int i = 0; i < futureTransformUpdates.Count; ++i)
        {
            if (NetworkManager.Singleton.ServerTick >= futureTransformUpdates[i].Tick)
            {
                if (futureTransformUpdates[i].DidTeleport)
                {
                    to = futureTransformUpdates[i];
                    from = to;
                    previous = to;
                    transform.position = to.Position;
                }
                else
                {
                    previous = to;
                    to = futureTransformUpdates[i];
                    from = new TransformUpdate(NetworkManager.Singleton.InterpolationTick, false, new Vector3(0, 0, 0), transform.position);
                }
                futureTransformUpdates.RemoveAt(i);
                --i;
                timeElapsed = 0f;
                timeToReachTarget = (to.Tick - from.Tick) * Time.fixedDeltaTime; // fix the looping issue later!!
            }
        }
        timeElapsed += Time.deltaTime;
        InterpolatePosition(timeElapsed / timeToReachTarget);
    }
    private void InterpolatePosition(float lerpAmount)
    {
        if ((to.Position - previous.Position).sqrMagnitude < squareMovementThreshold)
        {
            if (to.Position != from.Position)
                transform.position = Vector3.Lerp(from.Position, to.Position, lerpAmount);
            return;
        }
        transform.position = Vector3.LerpUnclamped(from.Position, to.Position, lerpAmount);
    }
    public void NewUpdate(uint tick, bool didTeleport, Vector3 velocity, Vector3 position)
    {
        //Debug.Log("new tick is: " + tick);
        if (tick <= NetworkManager.Singleton.InterpolationTick && !didTeleport)
            return;
        for (int i = 0; i < futureTransformUpdates.Count; ++i)
        {
            if (tick < futureTransformUpdates[i].Tick && futureTransformUpdates[i].Tick - tick < 10000)
            {
                futureTransformUpdates.Insert(i, new TransformUpdate(tick, didTeleport, velocity, position));
                return;
            }
        }

        futureTransformUpdates.Add(new TransformUpdate(tick, didTeleport, velocity, position));
    }
}
