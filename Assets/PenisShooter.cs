using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PenisShooter : MonoBehaviour
{
    [SerializeField] Transform playerCamera = null;
    [SerializeField] float MouseSensitivty = 3.5f;
    float cameraPitch = 0.0f;
    [SerializeField] bool lockCursor = true;
    [SerializeField] float walkspeed = 3000.0f;
    CharacterController controller = null;
    [SerializeField] float gravity = -13.0f;
    private float velocityY;
    public GameObject rocket;
    public Transform shootPoint;
    public Transform Maincamera;
    public float SpeedCounter = 10;
    private bool rocketEquipped = false;
    public GameObject Muzzleflash;
    public GameObject impactEffect;
    public int Ammo = 18;
    private bool canshoot = true;
    //public Text ammodisplay;
    public GameObject pistol;
    public GameObject rocketlauncher;
    public float rocketdelay = 3;
    bool rocketcanshoot = true;
    //public Text rocketdelaydisplay;

    void Start()
    {
        StartCoroutine("Ammoregen");

        rocketEquipped = false;
        controller = GetComponent<CharacterController>();
        if (lockCursor == true)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        //ammodisplay.text = Ammo.ToString();
        rocketdelay -= 1 * Time.deltaTime;
        if (rocketdelay <= 0)
        {
            rocketdelay = 0;
            rocketcanshoot = true;
        }
        gravity -= 26 * Time.deltaTime;
        SpeedCounter -= 1 * Time.deltaTime;
        if (SpeedCounter <= 0f)
        {
            walkspeed = 3000f;
        }

        //I will add reloading later once we have animations
        
        if (Ammo >= 18)
        {
            Ammo = 18;
            canshoot = true;
        }
        if (Ammo <= 0)
        {
            canshoot = false;
        }

        UpdateMouseLook();

        UpdateMovement();

        if (Input.GetMouseButtonDown(2))
        {
            if (rocketEquipped == true)
            {
               
                rocketEquipped = false;
                pistol.SetActive(true);
                rocketlauncher.SetActive(false);
            }
            else
            {
            
                rocketEquipped = true;
                rocketlauncher.SetActive(true);
                pistol.SetActive(false);
            }
        }

        if (Input.GetButtonDown("Fire1"))
        {
            if (rocketEquipped == true && rocketcanshoot == true)
            {
                Debug.Log("rocket fired");
                Instantiate(rocket, shootPoint.position, Maincamera.rotation);
                rocketdelay = 3;
                rocketcanshoot = false;
            }
        }

        if (Input.GetButtonDown("Fire1") && rocketEquipped == false)
        {
            if (Ammo > 0 && canshoot == true)
            {
                Debug.Log("bullet fired");
                Ammo -= 1;
                Instantiate(Muzzleflash, Maincamera.position, Quaternion.identity);
                //I'll add recoil later
                //I'll also change this to have aiming mechanics and not just hipfire
                RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, 100f);
                foreach (RaycastHit hit in hits)
                {
                    GameObject hitObject = hit.collider.gameObject;
                    // Process each hit object
                }
            }
        }
    }

    void UpdateMouseLook()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        cameraPitch -= mouseDelta.y * MouseSensitivty;
        cameraPitch = Mathf.Clamp(cameraPitch, -90.0f, 90.0f);
        playerCamera.localEulerAngles = Vector3.right * cameraPitch;
        transform.Rotate(Vector3.up * mouseDelta.x * MouseSensitivty);
    }

    void UpdateMovement()
    {
        Vector2 inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        inputDir.Normalize();
        Vector3 velocity = (transform.forward * inputDir.y + transform.right * inputDir.x) * walkspeed * Time.deltaTime + Vector3.up * velocityY;

        controller.Move(velocity * Time.deltaTime);

        if (controller.isGrounded)
        {
            velocityY = 0.0f;
        }

        velocityY += gravity * Time.deltaTime;

        if (controller.isGrounded && Input.GetButtonDown("Jump"))
        {
            gravity = 20.0f;
        }
    }

    IEnumerator Ammoregen()
    {
        while (true)
        {
            Ammo += 2;
            yield return new WaitForSeconds(1.0f);
        }
    }
}
