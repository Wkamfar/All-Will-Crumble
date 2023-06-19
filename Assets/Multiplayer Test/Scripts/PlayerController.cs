using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
    [SerializeField] Predictor predictor;

    [Header("Inputs")]
    [SerializeField] InputAction forward;
    [SerializeField] InputAction left;
    [SerializeField] InputAction back;
    [SerializeField] InputAction right;
    [SerializeField] InputAction jump;

    bool[] inputs = new bool[5]; //change this count later to get the fps aspect to work
    private void OnEnable()
    {
        forward.Enable();
        left.Enable();
        back.Enable();
        right.Enable();
        jump.Enable();
    }
    private void Update() // Change input system later for a more accomadating system
    {
        if (forward.IsPressed())
            inputs[0] = true; 
        if (left.IsPressed())
            inputs[1] = true;
        if (back.IsPressed())
            inputs[2] = true;
        if (right.IsPressed())
            inputs[3] = true;
        if (jump.IsPressed())
            inputs[4] = true;
        
    }
    private void FixedUpdate()
    {
        SendInput();
        PerformActions();
        if (!predictor.inputHistory.ContainsKey(NetworkManager.Singleton.ServerTick))
            predictor.inputHistory.Add(NetworkManager.Singleton.ServerTick, (bool[])inputs.Clone());
        for (int i = 0; i < inputs.Length; i++)
            inputs[i] = false;
    }

    private void SendInput()
    {
        Message message = Message.Create(MessageSendMode.Unreliable, ClientToServerId.input);
        message.Add(inputs, false);
        message.AddVector3(transform.eulerAngles);
        NetworkManager.Singleton.Client.Send(message);
    }

    // Calculate all movements locally, and then receive corrections
    [SerializeField] Player player;

    [SerializeField] float moveSpeed;
    [SerializeField] float groundDrag;
    Rigidbody rb;
    bool grounded;

    [SerializeField] float jumpCooldown;
    [SerializeField] float jumpForce;
    [SerializeField] float airMultiplier;
    private bool readyToJump = true;
    
    private Vector3 velocity = new Vector3();

    public bool didTeleport { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void MovePlayer()
    {

        Vector2 directions = new Vector2();
        if (inputs[0])
            directions = new Vector2(directions.x, 1);
        else if (inputs[2])
            directions = new Vector2(directions.x, -1);
        if (inputs[1])
            directions = new Vector2(-1, directions.y);
        else if (inputs[3])
            directions = new Vector2(1, directions.y);
        Vector3 moveDirection = transform.forward * directions.y + transform.right * directions.x;
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        SpeedControl();
    }
    void SpeedControl()
    {
        Vector3 hVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (hVel.magnitude > moveSpeed)
        {
            hVel = hVel.normalized * moveSpeed;
            rb.velocity = new Vector3(hVel.x, rb.velocity.y, hVel.z);
        }
    }
    void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
    }
    public void PerformActions()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, 1.2f, 1 << 10);
        MovePlayer();
        if (inputs[4] && grounded && readyToJump)
        {
            readyToJump = false;
            Jump();
            Invoke("ResetJump", jumpCooldown);
        }
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
        if (!predictor.localTransformHistory.ContainsKey(NetworkManager.Singleton.ServerTick))
            predictor.localTransformHistory.Add(NetworkManager.Singleton.ServerTick, new TransformUpdate(NetworkManager.Singleton.ServerTick, false, velocity, transform.position));   
    }
}
