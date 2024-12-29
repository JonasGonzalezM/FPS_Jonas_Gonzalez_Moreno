using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WallRunning : MonoBehaviour
{

    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallClimbSpeed;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float maxWallRunTime;
    private float wallRunTimer;



    [Header("Input")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode upwardsRunKey = KeyCode.LeftShift;
    public KeyCode downwardsRunKey = KeyCode.LeftControl;
    private bool upwardsRunning;
    private bool downwardsRunning;
    private float horizontalInput;
    private float verticalInput;



    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exiting")]
    private bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    [Header("Gravedad")]
    public bool useGravity;
    public float gravityCounterForce;

    [Header("Referencias")]
    public PlayerCam cam;
    public Transform orientation;
    private PlayerMovementAdvanced pm;
    private Rigidbody rb;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementAdvanced>();
    }


    private void Update()
    {
        CheckForWall();
        StateMachine();
    }


    private void FixedUpdate()
    {
        if(pm.wallrunning)
        {
            WallRunningMovement();
        }
    }

    private void CheckForWall()
    {
        //                            PUNTO DE INICIO      DIRECCION      STORE HIT INFO        DISTANCIA
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
       
        //                                            - PARA HACER LA IZQ
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
    }



    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position,Vector3.down, minJumpHeight, whatIsGround);

    }


    private void StateMachine()
    {
        //Conseguir los Inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");


        upwardsRunning = Input.GetKey(upwardsRunKey);
        downwardsRunning = Input.GetKey(downwardsRunKey);


        //Estado 1 - Wallrunning
        if((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitingWall)
        {
            //Comienza el wallrun
            if(!pm.wallrunning)
            {
                StartWallRun();
            }

            //Timer Wallrun
            if(wallRunTimer > 0)
            {
                wallRunTimer -= Time.deltaTime;
            }

            if(wallRunTimer <=0 && pm.wallrunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }

            //Wall Jump
            if(Input.GetKeyDown(jumpKey))
            {
                WallJump();
            }

        }


        //Estado 2 - Salir
        else if (exitingWall)
        {
            if(pm.wallrunning)
            {
                StopWallRun();
            }
            if(exitWallTimer > 0)
            {
                exitWallTimer -= Time.deltaTime;
            }
            if(exitWallTimer <= 0)
            {
                exitingWall = false;
            }
        }

        //Estado 3 - nada
        else
        {
            if (pm.wallrunning)
            {
                StopWallRun();
            }
        }

    }



    private void StartWallRun()
    {
        pm.wallrunning = true;

        wallRunTimer = maxWallRunTime;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);



        // Aplicar efectos de camara
        cam.DoFov(70f);
        if (wallLeft)
        {
            cam.DoTilt(-5f);
        }
        if(wallRight)
        {
            cam.DoTilt(5f);
        }
    }


    private void WallRunningMovement()
    {
        rb.useGravity = useGravity;


        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);


        if((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }


        //Fuerza hacia adelante
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        //upwards/downwards force
        if(upwardsRunning)
        {
            rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
        }
        
         
        if(downwardsRunning)
        {
            rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);
        }

        //Aqui se va a hacer que puedas correr por la cara contraria de las paredes

        //Fuerza de empuje a la pared
        if(!(wallLeft && horizontalInput> 0 ) && !(wallRight && horizontalInput < 0 ))
        {
            rb.AddForce(-wallNormal * 100, ForceMode.Force);

        }

        //debilitar un poco la gravedad para que no sea tan brusca
        if (useGravity)
        {
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
        }
    }


    private void StopWallRun()
    {
        pm.wallrunning = false;

        //Resetear los efectos de camara
        cam.DoFov(60f);
        cam.DoTilt(0f);
    }


    private void WallJump()
    {

        // Hacer el Salir del estado de Wall
        exitingWall = true;
        exitWallTimer = exitWallTime;



        Vector3 wallNormal =  wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        //Añadir Fuerzas y resetear la velocidad en y
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

    }

}
