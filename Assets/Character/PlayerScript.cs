using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerScript : MonoBehaviour
{
    // rework later // include callbacks later
    [SerializeField] float moveSpeed;
    [SerializeField] Transform orientation;
    [SerializeField] Transform head;
    [SerializeField] InputAction directionalMove;
    [SerializeField] InputAction jumpKey;
    [SerializeField] InputAction mouseClick;
    [SerializeField] float groundDrag;
    Rigidbody rb;
    bool grounded;

    [SerializeField] float jumpForce;
    [SerializeField] float jumpCooldown;
    [SerializeField] float airMultiplier;
    bool readyToJump = true;

    Vector3 hitPoint;
    // Start is called before the first frame update
    private void OnEnable()
    {
        directionalMove.Enable();
        jumpKey.Enable();
        mouseClick.Enable();
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mouseClick.performed += ctx => Fire();
    }

    // Update is called once per frame
    void Update()
    {
        grounded = Physics.Raycast(orientation.position, Vector3.down, 1.2f, 1 << 10);
        MovePlayer();
        if (jumpKey.IsPressed() && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke("ResetJump", jumpCooldown);
        }
        
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;

    }
    void MovePlayer()
    {
        Vector2 directions = directionalMove.ReadValue<Vector2>();
        Vector3 moveDirection = orientation.forward * directions.y + orientation.right * directions.x;
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
    void Fire()
    {
        Ray ray = new Ray(head.position, head.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 75f, ~(1 << 20)))
        {
            hitPoint = hit.point;
            try
            {
                ProceduralGlass glass = hit.collider.gameObject.GetComponent<ProceduralGlass>();
                glass.BreakGlass(hit.point);
            }
            catch { }
        }
        else 
            hitPoint = ray.GetPoint(75);
    }
    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawSphere(hitPoint, 0.2f);
    }
}
