using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class PlayerMovement : NetworkBehaviour
{
    public enum PlayerStatus { GROUND, AIR, SHOOT }

    private PlayerStatus playerStatus;
    private bool _MousePoppedOut;

    [SerializeField] Transform playerCamera;
    [SerializeField] Transform cameraPos;
    [SerializeField] WeaponInventory weaponInv;

    [SerializeField] LayerMask whatIsGround;

    [SerializeField] float jumpForce;
    [SerializeField] float accelerationSpeed;
    [SerializeField] float aimSpeed;
    [SerializeField] float JogSpeed;
    [SerializeField] float sprintSpeed;
    [SerializeField] float drag;
    [SerializeField] float jumpDownModifier;
    [SerializeField] float jumpUpModifier;
    [SerializeField] float maxSlopeAngle;

    float movementSpeed;
    float lerpMovementSpeed;
    float movementAnim;
    float lerpMovementAnim;

    bool jumpBool = false;

    Rigidbody rb;

    public WeaponInventory GetWeaponInventory() { return weaponInv; }
    public float GetAnimSpeed() { return lerpMovementAnim; }
    public Transform GetPlayerCamera() { return playerCamera; }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!IsOwner) return;
        rb.useGravity = true;
        InputManager();

        switch (playerStatus)
        {
            case PlayerStatus.GROUND:
                if (!IsGrounded()) playerStatus = PlayerStatus.AIR;
                break;
            case PlayerStatus.AIR:
                if (IsGrounded()) playerStatus = PlayerStatus.GROUND;
                break;
            case PlayerStatus.SHOOT:
                break;
        }
        if (Input.GetKeyDown(KeyCode.Escape)) _MousePoppedOut = !_MousePoppedOut;
        Screen.lockCursor = _MousePoppedOut;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsOwner) return;

        Direction();

        switch (playerStatus)
        {
            case PlayerStatus.GROUND:
                rb.drag = drag;
                Movement();
                break;
            case PlayerStatus.AIR:
                JumpModifier();
                rb.drag = 0;
                break;
            case PlayerStatus.SHOOT:
                break;
        }
    }

    /// <summary>
    /// Checks if the character aiming
    /// </summary>
    public bool IsAiming()
    {
        bool _isAiming = false;

        if (Input.GetMouseButton(1))
        {
            _isAiming = true;
        }

        return _isAiming;
    }

    /// <summary>
    /// Checks if the character running
    /// </summary>
    public bool IsRunning()
    {
        bool _isRunning = false;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _isRunning = true;
        }

        return _isRunning;
    }

    /// <summary>
    /// Managing the input more like speed manager
    /// </summary>
    private void InputManager()
    {

        // Movement SPeed (Jog and Sprint)
        if (IsAiming())
        {
            movementAnim = 1;
            movementSpeed = aimSpeed;
        }
        else if (IsRunning())
        {
            movementAnim = 2;
            movementSpeed = sprintSpeed;
        }
        else
        {
            movementAnim = 1;
            movementSpeed = JogSpeed;
        }

        if (InputVector() == Vector2.zero)
        {
            movementAnim = 0;
            movementSpeed = 0;
        }

        lerpMovementAnim = Mathf.Lerp(lerpMovementAnim, movementAnim, 0.2f);
        // Jump
        if (Input.GetKey(KeyCode.Space) && IsGrounded() && !jumpBool) Jump();
    }

    /// <summary>
    /// Player direction (based on the camera direction)
    /// </summary>
    private void Direction()
    {
        Vector3 playerRot = new Vector3(0, playerCamera.rotation.eulerAngles.y, 0);
        transform.rotation = Quaternion.Euler(playerRot);
        playerCamera.position = cameraPos.position;
    }

    /// <summary>
    ///  Playeer movement and speed limit
    /// </summary>
    private void Movement()
    {
        if (IsGrounded() && rb.velocity.y < 0) jumpBool = false;

        //Moving direction based on the slope direction;
        Vector3 forward = rb.transform.forward;
        Vector3 right = rb.transform.right;
        if (GetSlopeAngle() < maxSlopeAngle)
        {
            forward = GetSlopeDirection(playerCamera.forward);
            right = GetSlopeDirection(playerCamera.right);
        }


        // switch off gravity when player not moving on slope
        if (InputVector() == Vector2.zero && GetSlopeAngle() < maxSlopeAngle) rb.useGravity = false;

        // Moving the player forward direction with Addforce (could changeable);
        Vector3 moveDirection = forward * InputVector().y + right * InputVector().x;
        rb.AddForce(moveDirection.normalized * accelerationSpeed, ForceMode.Force);

        // Speed limitation
        Vector3 velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        lerpMovementSpeed = Mathf.Lerp(lerpMovementSpeed, movementSpeed, 0.2f);

        if (velocity.magnitude > lerpMovementSpeed)
        {
            Vector3 limit = velocity.normalized * lerpMovementSpeed;
            rb.velocity = new Vector3(limit.x, rb.velocity.y, limit.z);
        }
    }

    /// <summary>
    /// Jump - yeah thats it
    /// </summary>
    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        jumpBool = true;
    }

    /// <summary>
    /// Modifing the falling speed while in air;
    /// </summary>
    private void JumpModifier()
    {
        if (rb.velocity.y < 0) rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * jumpDownModifier, rb.velocity.z);
        if (rb.velocity.y > 0) rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y / jumpUpModifier, rb.velocity.z);
    }

    /// <summary>
    /// Gets the Slope angle under the player
    /// </summary>
    private float GetSlopeAngle()
    {
        CapsuleCollider capsuleCol = GetComponent<CapsuleCollider>();
        float slopeAngle = 0;
        Vector3 raycastPoint = capsuleCol.bounds.center;
        float raySize = capsuleCol.height * 0.5f + 0.3f;

        if (Physics.Raycast(raycastPoint, Vector3.down, out RaycastHit hitinfo, raySize, whatIsGround))
        {
            slopeAngle = Vector3.Angle(transform.up, hitinfo.normal);
        }

        return slopeAngle;
    }

    /// <summary>
    /// Gets the Slope direction under the player
    /// </summary>
    private Vector3 GetSlopeDirection(Vector3 _direction)
    {
        // Settings for the Raycast
        CapsuleCollider capsuleCol = GetComponent<CapsuleCollider>();
        Vector3 direction = _direction;
        Vector3 raycastPoint = capsuleCol.bounds.center;
        float raySize = capsuleCol.height * 0.5f + 0.3f;

        // Calculate the slope direction based on the camera direction so the player always follows the slope direction and maintain the speed
        if (Physics.Raycast(raycastPoint, Vector3.down, out RaycastHit hitinfo, raySize, whatIsGround))
        {
            Vector3 checkDirection = _direction;
            Vector3 up = new Vector3(0, 1.0f, 0);
            Vector3 right = Vector3.Cross(up.normalized, checkDirection.normalized);
            direction = Vector3.Cross(right, hitinfo.normal).normalized;

            Debug.DrawRay(raycastPoint, direction);
        }

        return direction;
    }

    private bool IsGrounded()
    {
        CapsuleCollider capsuleCol = GetComponent<CapsuleCollider>();
        Vector3 raycastPoint = capsuleCol.bounds.center;
        float raySize = capsuleCol.height * 0.5f + 0.5f;

        return Physics.Raycast(raycastPoint, Vector3.down, raySize, whatIsGround);
    }

    /// <summary>
    /// Horizontal and Vertical Input Vector for the [WASD] movement / Might change if we want the new Input system
    /// </summary>
    private Vector2 InputVector()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 InputVector = new Vector2(h, v);
        if (InputVector.magnitude > 1) InputVector = InputVector.normalized;

        return InputVector;
    }
}
