// Rigidbody based movement by Dani (heavily altered by Joshua Callus for 'Midnight Sugar Rush')

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerMovement : MonoBehaviour {

    #region Movement Variables
    //Assignables
    public Transform playerCam;
    public Transform orientation;
    public GameObject equipmentWheelPanel;
    public GameObject canvas;
    
    //Other
    Rigidbody rb;

    //Rotation & Look
    private float xRotation;
    private float sensitivity = 50f;
    public float sensMultiplier = 1f;
    
    //Movement
    float moveSpeed = 2500;
    float maxSpeed = 10; //Walk 6, Sprint 10, Crouch 4
    //float multiplierStance = 1f; //Walk 1.0, Sprint 1.2, Crouch 0.6, Airborne 0.5
    public LayerMask whatIsGround;
    
    float counterMovement = 0.175f; //Amount of Friction
    float threshold = 0.01f;
    float maxSlopeAngle = 35f;

    //Crouch & Slide
    private Vector3 crouchScale = new Vector3(1, 0.4f, 1);
    private Vector3 playerScale;
    float slideForce = 400;

    //Jumping
    float jumpCooldown = 0.4f;
    float jumpForce = 150f;
    
    //Input
    float x, y;
    public bool grounded, jumping, sprinting, crouching, swimming, sliding, diving;
    
    //Sliding
    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;
    #endregion
    [Space(10)]

    //Equipment Enable List
    List<string> equipmentEventList = new List<string> { "CameraEnable", "NotepadFlipFlop", "WandEnable", "FlashlightFlipFlop", "WalkmanFlipFlop" };
    public static bool equipmentBeingUsed;

    [Header("Camera")]
    [SerializeField] Camera cameraCam;
    [SerializeField] PlayableDirector cameraFlashControl;
    [SerializeField] bool lookingAtEvidence;
    Transform relevantEvidenceMarker;

    [Header("Notepad")]
    [SerializeField] GameObject notepadInputField;
    [SerializeField] TextMeshPro notepadContents;
    bool notepadBeingEdited;

    [Header("Wand")]
    [SerializeField] PlayableDirector wandUseControl;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform projectileSpawnTransform;

    [Header("Flashlight")]
    [SerializeField] GameObject flashlightLight;
    bool flashlightEnabled;

    [Header("Walkman")]
    [SerializeField] GameObject walkman;
    bool walkmanEnabled;

    [Header("NPC")]
    [SerializeField] bool isTalking;
    public NPC talkingNPC;
    GameObject npcHit;

    [Range(-1, 1)]
    public static float moralityLevel;

    public List<string> collectedEvidence = new List<string> { };

    #region Input Action Assigning
    private Controls controls = null;

    public void OnEnable()
    {
        controls.OnFoot.Enable();
    }

    public void OnDisable()
    {
        controls.OnFoot.Disable();
    }
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controls = new Controls();
        
    }
    
    void Start() {
        playerScale = transform.localScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 
    }

    private void FixedUpdate()
    {
        Movement();

        #region Crosshair Control
        Vector3 fwd = cameraCam.transform.TransformDirection(Vector3.forward);
        if(Physics.Raycast(cameraCam.transform.position, fwd, out RaycastHit hitInfo, 2f))
        {
            if (hitInfo.collider.tag == "PhotoDevelopStation" || (hitInfo.collider.tag == "NPC" && !hitInfo.collider.gameObject.GetComponent<DialogueManager>().npc.dead))
            {
                if(HUDManager.defaultCrosshairEnabled) canvas.GetComponent<HUDManager>().SwitchCrosshairFlipFlop();
            }
            else
            {
                if (!HUDManager.defaultCrosshairEnabled) canvas.GetComponent<HUDManager>().SwitchCrosshairFlipFlop();
            }
        }
        else
        {
            if (!HUDManager.defaultCrosshairEnabled) canvas.GetComponent<HUDManager>().SwitchCrosshairFlipFlop();
            if (isTalking)
            {
                isTalking = false;
                npcHit.GetComponent<DialogueManager>().StopConversation();
            }
        }
        #endregion
    }

    private void Update()
    {
        if(!notepadInputField.activeInHierarchy)
        {
            if(!equipmentWheelPanel.activeInHierarchy && !isTalking)
            {
                Look();
            }
            MyInput();
        }
    }

    #region Interact

    public void Interact()
    {
        Vector3 fwd = cameraCam.transform.TransformDirection(Vector3.forward);
        if (Physics.Raycast(cameraCam.transform.position, fwd, out RaycastHit hitInfo, 2f))
        {
            if(hitInfo.collider.tag == "NPC" && !hitInfo.collider.gameObject.GetComponent<DialogueManager>().npc.dead)
            {
                npcHit = hitInfo.collider.gameObject;
                npcHit.GetComponent<DialogueManager>().StartConversation();
                isTalking = true;
            }
            else if (hitInfo.collider.tag == "PhotoDevelopStation") hitInfo.transform.GetComponent<CameraStorage>().DevelopPhotos();
        }
    }

    public void HappyResponse()
    {
        npcHit.GetComponent<DialogueManager>().DisplayResponse(0);
        if(!talkingNPC.talkedTo) UpdateMorality(0.05f);
        talkingNPC.talkedTo = true;
    }

    public void NeutralResponse()
    {
        npcHit.GetComponent<DialogueManager>().DisplayResponse(1);
        if (!talkingNPC.talkedTo) UpdateMorality(0f);
        talkingNPC.talkedTo = true;
    }

    public void AggressiveResponse()
    {
        npcHit.GetComponent<DialogueManager>().DisplayResponse(2);
        if (!talkingNPC.talkedTo) UpdateMorality(-0.05f);
        talkingNPC.talkedTo = true;
    }

    public void AnalyticalResponse()
    {
        npcHit.GetComponent<DialogueManager>().DisplayResponse(3);
        if (!talkingNPC.talkedTo) UpdateMorality(0.075f);
        talkingNPC.talkedTo = true;
    }
    public void UpdateMorality(float extraMoralityValue)
    {
        moralityLevel += extraMoralityValue;
        canvas.GetComponent<HUDManager>().MoveMoralityIndicator();
    }


    #endregion


    #region Use Equipment
    public void UseEquipment()
    {
        if(!equipmentBeingUsed && HUDManager.equipmentIndex >= 0) StartCoroutine(equipmentEventList[HUDManager.equipmentIndex].ToString()); //Finds the correct Coroutine to call from the list of names using the index of the object in HUDManager
    }

    IEnumerator CameraEnable()
    {
        equipmentBeingUsed = true;
        if (lookingAtEvidence) relevantEvidenceMarker.GetComponent<EvidenceMarker>().TriggerEvidence(); //adds evidence if player is within radius when capturing a photo
        Texture2D photo = new Texture2D(512, 512, TextureFormat.RGB24, false);
        Graphics.CopyTexture(cameraCam.targetTexture, photo);
        CameraStorage.cameraList.Add(photo); //Save photo to CameraStorage script
        cameraFlashControl.Play(); //Play flash animation
        Debug.Log(CameraStorage.cameraList.Count);
        yield return new WaitForSeconds(1.2f);
        equipmentBeingUsed = false;
    }

    IEnumerator NotepadFlipFlop()
    {
        equipmentBeingUsed = true;
        if (notepadBeingEdited)
        {
            notepadInputField.SetActive(false);
            notepadContents.text = notepadInputField.GetComponent<TMP_InputField>().text;
            notepadBeingEdited = false;
            this.GetComponent<PlayerInput>().ActivateInput();

            PlayerPrefs.SetString("notepad", notepadContents.text); //Save notepad contents to the playerprefs
            PlayerPrefs.Save();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            notepadContents.text = PlayerPrefs.GetString("notepad"); //Open notepad contents from the playerprefs
            notepadInputField.SetActive(true);
            notepadInputField.GetComponent<TMP_InputField>().text = notepadContents.text;
            notepadBeingEdited = true;
            this.GetComponent<PlayerInput>().DeactivateInput();

            Cursor.lockState = CursorLockMode.None; 
            Cursor.visible = true; 
        }

        yield return new WaitForSeconds(0.2f);
        equipmentBeingUsed = false;
    }

    IEnumerator WandEnable()
    {
        equipmentBeingUsed = true;
        wandUseControl.Play();
        yield return new WaitForSeconds(0.05f);

        Vector3 projectileLookAt = playerCam.position + new Vector3(0f, 0f, 1000f); //calculates the alignment of the projectile
        var newProjectile = Instantiate(projectilePrefab, projectileSpawnTransform); //spawns projectile on the tip of the wand
        newProjectile.GetComponent<WandProjectile>().player = this.gameObject;
        newProjectile.GetComponent<WandProjectile>().canvas = canvas;
        projectileSpawnTransform.DetachChildren(); //allows projectile to move independently of the player
        newProjectile.transform.LookAt(projectileLookAt); //aligns the projectile to the correct angle

        newProjectile.GetComponent<Rigidbody>().AddForce(playerCam.transform.forward * 2000); //adds projectile force

        yield return new WaitForSeconds(0.5f);
        equipmentBeingUsed = false;
        Debug.Log("Wand Finished");
    }

    IEnumerator FlashlightFlipFlop()
    {
        equipmentBeingUsed = true;

        if (flashlightEnabled)
        {
            flashlightLight.SetActive(false);
            flashlightEnabled = false;
        }
        else
        {
            flashlightLight.SetActive(true);
            flashlightEnabled = true;
        }

        yield return new WaitForSeconds(0.2f);
        equipmentBeingUsed = false;
    }

    IEnumerator WalkmanFlipFlop()
    {
        equipmentBeingUsed = true;
        if (walkmanEnabled)
        {
            walkman.GetComponent<Walkman>().StopMusic();
            walkmanEnabled = false;
        }
        else
        {
            walkman.GetComponent<Walkman>().PlayMusic();
            walkmanEnabled = true;
        }
        yield return new WaitForSeconds(0.1f);
        equipmentBeingUsed = false;
    }
    #endregion

    private void MyInput()
    {
        x = controls.OnFoot.Move.ReadValue<Vector2>().x;
        y = controls.OnFoot.Move.ReadValue<Vector2>().y;
    }

    public void CrouchFlipFlop()
    {
        if (!crouching)
        {
            if (!swimming)
            {
                //Crouch
                crouching = true;
                transform.localScale = crouchScale;

                if (sprinting)
                {
                    if (jumping)
                    {
                        //Dolphin Dive
                        diving = true;
                        slideForce = 200f; //makes up for the extra velocity caused by dolphin diving
                        Invoke(nameof(StopSlideDive), .5f);
                    }
                    else
                    {
                        //Slide
                        sliding = true;
                        slideForce = 200f;
                        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z); //adjust height when not jump sliding
                        Invoke(nameof(StopSlideDive), .5f);
                    } 
                    rb.AddForce(orientation.transform.forward * slideForce);
                    SprintFlipFlop();
                }
            }
        }
        else
        {
            //Uncrouch
            crouching = false;
            transform.localScale = playerScale;
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        }
    }

    public void SprintFlipFlop()
    {
        if (!sprinting)
        {
            if (!swimming)
            {
                if (crouching)
                {
                    CrouchFlipFlop();
                }
                sprinting = true;
                Invoke(nameof(SprintFlipFlop), 5f); //Walk after 5s
            }
        }
        else
        {
            CancelInvoke(nameof(SprintFlipFlop));
            sprinting = false;
        }
    }

    public void StopSlideDive()
    {
        sliding = false;
        diving = false;
    }

    public void Movement()
    {
        //Extra gravity
        rb.AddForce(Vector3.down * Time.deltaTime * 10);
        
        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract ice-rink and sloppy movement
        CounterMovement(x, y, mag);
        
        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (x > 0 && xMag > maxSpeed) x = 0;
        if (x < 0 && xMag < -maxSpeed) x = 0;
        if (y > 0 && yMag > maxSpeed) y = 0;
        if (y < 0 && yMag < -maxSpeed) y = 0;

        //Some multipliers
        float multiplierStance = 1f;
        
        //Movement in air
        if (!grounded)
        {
            multiplierStance = 0.5f;
        }

        //Movement while crouching
        if (grounded && crouching && !sliding)
        {
            multiplierStance = 0.6f;
            maxSpeed = 4f;
        }

        //Movement while walking
        if (grounded && !crouching && !sprinting)
        {
            multiplierStance = 1f;
            maxSpeed = 6f;
        }

        //Movement while swimming
        if (swimming)
        {
            rb.AddForce(Vector2.down * 1f); //slowly makes player sink while swimming
        }

        if (grounded && sprinting)
        {
            multiplierStance = 1.2f;
            maxSpeed = 10f;

            if (controls.OnFoot.Move.ReadValue<Vector2>() == new Vector2(0, 0)) SprintFlipFlop();

        }

        //Apply forces to move player
        rb.AddForce(orientation.transform.forward * y * moveSpeed * Time.deltaTime * multiplierStance);
        rb.AddForce(orientation.transform.right * x * moveSpeed * Time.deltaTime * multiplierStance);
    }

    public void Jump()
    {
        if (grounded)
        {
            if (crouching)
            {
                CrouchFlipFlop();
            }
            else
            {
                jumping = true;

                //Add jump forces
                rb.AddForce(Vector2.up * jumpForce * 1.5f);
                rb.AddForce(normalVector * jumpForce * 0.5f);

                //If jumping while falling, reset y velocity.
                Vector3 vel = rb.velocity;
                if (rb.velocity.y < 0.5f) rb.velocity = new Vector3(vel.x, 0, vel.z);
                else if (rb.velocity.y > 0) rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

                Invoke(nameof(ResetJump), jumpCooldown);
            }
        }
        else if (swimming)
        {
            rb.AddForce(Vector2.up * 500f);
        }
    }
    private void ResetJump()
    {
        jumping = false;
    }

    private void SwimmingFlipFlop()
    {
        if (!swimming)
        {
            swimming = true;
            rb.useGravity = false;
            rb.velocity += rb.velocity * -.25f; //weakens velocity upon hitting water


            if (crouching) CrouchFlipFlop();
            if (sprinting) SprintFlipFlop();
            

        }
        else
        {
            rb.useGravity = true;
            swimming = false;
        }
    }
    
    private float desiredX;
    private void Look()
    {
        float mouseX = controls.OnFoot.Look.ReadValue<Vector2>().x * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = controls.OnFoot.Look.ReadValue<Vector2>().y * sensitivity * Time.fixedDeltaTime * sensMultiplier;

        //Find current look rotation
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;
        
        //Rotate, and also make sure we dont over- or under-rotate.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Perform the rotations
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, 0);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

    #region Movement Maths
    private void CounterMovement(float x, float y, Vector2 mag) {
        if (!grounded || jumping || sliding ) return;

        //Counter movement
        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }
        
        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxSpeed)
        {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    /// <returns></returns>
    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);
        
        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;
    
    /// <summary>
    /// Handle ground detection
    /// </summary>
    private void OnCollisionStay(Collision other)
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++) {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal)) {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded()
    {
        grounded = false;
    }

    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (!swimming && other.tag == "Water") SwimmingFlipFlop();

        if (other.tag == "EvidenceMarker")
        {
            lookingAtEvidence = true;
            relevantEvidenceMarker = other.transform;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (swimming && other.tag == "Water") SwimmingFlipFlop();

        if (other.tag == "EvidenceMarker")
        {
            lookingAtEvidence = false;
            relevantEvidenceMarker = null;
        }
    }
}
