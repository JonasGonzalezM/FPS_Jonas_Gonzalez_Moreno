using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkspeed;
    public float sprintSpeed;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump=true;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitinSlope;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    //Esto lo que se encargará es de guardar los estados del jugador ya sea correr, agacharse, andar o estar en el aire.
    public MovementState state;
    
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;


    }


    private void Update()
    {
        //Comprobar el suelo
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);


        MyInput();
        SpeedControl();
        StateHandler();
        


        //Manejo del drag
        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else //
        {
            rb.drag = 0;
        }
    }
    private void FixedUpdate()
    {
        MovePlayer();
    }


    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //El momento de Saltar
        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            Debug.Log("Intento de salto");
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }


        //Comienzo de agacharse
        if (Input.GetKey(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x,crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down*5f, ForceMode.Impulse);
        }
        //Finalizar el agachado
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler()
    {
        //Modo - Agachado
        if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }


        //Modo -Sprinting
        if(grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;

        }

        //Modo -Andar
        else if(grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkspeed;
        }

        //Modo - Aire
        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        //calcular la direccion del movimiento
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;


        //En la cuesta
        if (OnSlope() && !exitinSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            //Para arreglar el rebote causado por el apagado de la gravedad al estar en una pendiente
            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down *80f,ForceMode.Force);
            }

        }


        // en el suelo
        if (grounded)
        {
           rb.AddForce(moveDirection.normalized * moveSpeed * 10f,ForceMode.Force);
        }
        else if(!grounded) //en el aire
        {
           rb.AddForce(moveDirection.normalized * moveSpeed * 10f* airMultiplier,ForceMode.Force);

        }


        //Apagar la gravedad cuando estemos en una cuesta o pendiente
        rb.useGravity = !OnSlope();

    }


    private void SpeedControl()
    {

        //Limitar la velocidad en las pendientes
        if (OnSlope() && !exitinSlope)
        {
            if(rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;

            }
        }

        //Limitar la velocidad en el suelo o en el aire
        else
        {

            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            //Limitar la velocidad si es necesario
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }



    }


    private void Jump()
    {
        exitinSlope = true;


        //reseteo de la velocidad en y
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

    }

    private void ResetJump()
    {
        readyToJump = true;
        exitinSlope = false;
    }




    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;

        }
        return false;

    }


    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

   
}
