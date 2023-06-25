using Riptide;
using Riptide.Utils;
using System;
using UnityEngine;

public enum ServerToClientId : ushort
{
    sync = 1,
    playerSpawned,
    playerMovement
}
public enum ClientToServerId : ushort
{
    name = 1,
    input
}

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;
    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }
    public Client Client { get; private set; }

    private uint _serverTick;
    public uint ServerTick
    {
        get => _serverTick;
        private set
        {
            _serverTick = value;
            InterpolationTick = value - TicksBetweenPositionUpdates;
        }
    }
    public uint InterpolationTick { get; private set; }
    private uint _ticksBetweenPositionUpdates = 2;
    [SerializeField] private ushort tickDivergenceTolerance = 1;
    public uint TicksBetweenPositionUpdates
    {
        get => _ticksBetweenPositionUpdates;
        private set
        {
            _ticksBetweenPositionUpdates = value;
            InterpolationTick = ServerTick - value;
        }
    }

    [SerializeField] private string ip;
    [SerializeField] private ushort port;
    private void Awake()
    {
        Singleton = this;
    }
    private void Start()
    {
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
        Client = new Client();
        Client.Connected += DidConnect;
        Client.ConnectionFailed += FailedToConnect;
        Client.ClientDisconnected += PlayerLeft;
        Client.Disconnected += DidDisconnect;

        ServerTick = TicksBetweenPositionUpdates;
    }

    float lastIterTime = 0;
    private void FixedUpdate()
    {
        Client.Update();
        ServerTick++;
    }
    private void OnApplicationQuit()
    {
        Client.Disconnect();
    }
    public void Connect()
    {
        Client.Connect($"{ip}:{port}");
    }
    private void DidConnect(object sender, EventArgs e)
    {
        UIManager.Singleton.SendName();
    }
    private void FailedToConnect(object sender, EventArgs e)
    {
        UIManager.Singleton.BackToMain();
    }
    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e) // make the ability to reconnect
    {
        Destroy(Player.list[e.Id].gameObject);
    }
    private void DidDisconnect(object sender, EventArgs e)
    {
        UIManager.Singleton.BackToMain();
    }
    private void SetTick(uint serverTick) // do something with tick cycle later
    {
        if (Mathf.Abs(ServerTick - serverTick) > tickDivergenceTolerance) // interpolate between ticks if a correction is required so the transition is smooth
        {
            ServerTick = serverTick;
        }
    }

    [MessageHandler((ushort)ServerToClientId.sync)]
    private static void Sync(Message message)
    {
        Singleton.SetTick(message.GetUInt()); 
    }

}
