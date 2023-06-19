using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformUpdate 
{
    public uint Tick { get; private set; }
    public bool DidTeleport { get; private set; }
    public Vector3 Velocity { get; private set; }
    public Vector3 Position { get; private set; }

    public TransformUpdate(uint tick, bool didTeleport, Vector3 velocity, Vector3 position)
    {
        Tick = tick;
        DidTeleport = didTeleport;
        Velocity = velocity;
        Position = position;
    }
}
