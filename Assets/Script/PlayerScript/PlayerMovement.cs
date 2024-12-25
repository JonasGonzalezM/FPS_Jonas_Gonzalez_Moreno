using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;


    public float groundDrag;


    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump=true;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

    }


    private void Update()
    {
        //Comprobar el suelo
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);


        MyInput();
        SpeedControl();


        //Manejo del drag
        if (grounded)
        {
            rb.drag = moveSpeed;
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
    }

    private void MovePlayer()
    {
        //calcular la direccion del movimiento
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // en el suelo
        if (grounded)
        {
           rb.AddForce(moveDirection.normalized * moveSpeed * 10f,ForceMode.Force);
        }
        else if(!grounded) //en el aire
        {
           rb.AddForce(moveDirection.normalized * moveSpeed * 10f* airMultiplier,ForceMode.Force);

        }

    }


    private void SpeedControl()
    {
        Vector3 flatVel= new Vector3(rb.velocity.x,0f,rb.velocity.z);

        //Limitar la velocidad si es necesario
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity=new Vector3(limitedVel.x,rb.velocity.y,limitedVel.z);
        }
    }


    private void Jump()
    {
        //reseteo de la velocidad en y
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}
