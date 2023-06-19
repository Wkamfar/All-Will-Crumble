using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Predictor : MonoBehaviour
{
    [SerializeField] Interpolator interpolator;

    public Dictionary<uint, bool[]> inputHistory = new Dictionary<uint, bool[]>();
    public Dictionary<uint, TransformUpdate> localTransformHistory = new Dictionary<uint, TransformUpdate>();
    public Dictionary<uint, TransformUpdate> serverTransformHistory = new Dictionary<uint, TransformUpdate>();
    [SerializeField] float inaccuracyTolerance = 0.05f;

    private void Update()
    {
        
    }
    public void ValidateMovement(uint tick, bool didTeleport, Vector3 velocity,  Vector3 position)
    {
        if (!serverTransformHistory.ContainsKey(tick))
            serverTransformHistory.Add(tick, new TransformUpdate(tick, didTeleport, velocity, position));

        //foreach (uint key in serverTransformHistory.Keys)
        //{
        //    if (key < tick) // maybe check if the key is significantly smaller, idk
        //        serverTransformHistory.Remove(key);
        //}
        //foreach (uint key in serverTransformHistory.Keys)
        //{
        //    if (key < tick) // maybe check if the key is significantly smaller, idk
        //        localTransformHistory.Remove(key);
        //}
        //foreach (uint key in inputHistory.Keys)
        //{
        //    if (key < tick)
        //        inputHistory.Remove(key);
        //}
        if ((position - localTransformHistory[tick].Position).sqrMagnitude >= inaccuracyTolerance)
            CorrectMovement(tick);
    }

    private void CorrectMovement(uint tick)
    {
        localTransformHistory[tick] = serverTransformHistory[tick];
        Vector3 newPosition = serverTransformHistory[tick].Position;
        List<uint> localKeys = new List<uint>(localTransformHistory.Keys);
        uint lastTick = 0;
        for (int i = 1; i < localKeys.Count; ++i)
        {
            newPosition += localTransformHistory[localKeys[i - 1]].Velocity * (localKeys[i] - localKeys[i - 1]) * Time.fixedDeltaTime;
            localTransformHistory[lastTick] = new TransformUpdate(localKeys[i], localTransformHistory[lastTick].DidTeleport, localTransformHistory[lastTick].Velocity, newPosition);
            if (lastTick < localKeys[i])
                lastTick = localKeys[i];
        }
        interpolator.NewUpdate(lastTick, false, localTransformHistory[lastTick].Velocity, newPosition); // this might not be right bc
    }
}
