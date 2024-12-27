using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("Referencias")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMovementAdvanced pm;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;


    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    

    private void Start()
    {

        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementAdvanced>();
        
        
        startYScale = playerObj.localScale.y;
        
    }
    private void Update()
    {
        //horizontal son las letras A y D 
        horizontalInput = Input.GetAxisRaw("Horizontal");
        //vertical
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKeyDown(slideKey) && (horizontalInput !=0 || verticalInput != 0))
        {
            StarSlide();
        }

        if(Input.GetKeyUp(slideKey) && pm.sliding)
        {
            StopSlide();
        }
    }


    private void FixedUpdate()
    {
        if (pm.sliding)
        {
            SlidingMovement();
        }
    }

    private void StarSlide()
    {
        pm.sliding = true;
        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale,playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        //reseteo del timer del slide
        slideTimer = maxSlideTime;
    } 
    private void SlidingMovement()
    {
        //Esto es para poder hacer slide en cualquier direccion dependiendo de la Key que elijamos
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;


        //Sliding normal
        if(!pm.OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized*slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;

        }

        //deslizarse en una pendiente
        else
        {
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if(slideTimer <=0 )
        {
            StopSlide();
        }

    }


    private void StopSlide()
    {
        pm.sliding = false;
        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }


}
