using Riptide;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //[SerializeField] PlayerController playerController;
    //[SerializeField] CameraController cameraController;
    [SerializeField] Interpolator interpolator;
    [SerializeField] Predictor predictor;

    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();
    public ushort Id { get; private set; }
    public bool IsLocal { get; private set; }
    private string username;
    private void OnDestroy()
    {
        list.Remove(Id);
    }

    public static void Spawn(ushort id, string username, Vector3 position)
    {
        Player player;
        if (id == NetworkManager.Singleton.Client.Id)
        {
            player = Instantiate(GameLogic.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal = true;
        }
        else
        {
            player = Instantiate(GameLogic.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal = false;
        }

        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.username = username;

        list.Add(id, player);
    }
    private void Move(uint tick, bool didTeleport, Vector3 velocity, Vector3 newPosition, Vector3 eulerAngles)
    {

        if (!IsLocal)
        {
            interpolator.NewUpdate(tick, didTeleport, velocity, newPosition);
            transform.eulerAngles = eulerAngles;
        }
        else
            predictor.ValidateMovement(tick, didTeleport, velocity, newPosition);
    }

    [MessageHandler((ushort)ServerToClientId.playerSpawned)]

    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }
    [MessageHandler((ushort)ServerToClientId.playerMovement)]
    private static void PlayerMovement(Message message) // I changed the number of parameters recieved, change this later
    {
        if (list.TryGetValue(message.GetUShort(), out Player player))
            player.Move(message.GetUInt(), message.GetBool(), message.GetVector3(), message.GetVector3(), message.GetVector3()); // Send Velocity Data later // try to maintain velocity
    }
}
