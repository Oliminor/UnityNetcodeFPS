using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AnimationController : NetworkBehaviour
{
    Animator animator;
    float velocityX = 0;
    float velocityZ = 0;
    public float acceleration = 3.0f;
    public float deceleration = 2.0f;

    bool forwardPressed;
    bool backwardPressed;
    bool rightPressed;
    bool leftPressed;
    bool runPressed;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();


    }

    // Update is called once per frame
    void Update()
    {

        forwardPressed = Input.GetKey("w");
        backwardPressed = Input.GetKey("s");
        rightPressed = Input.GetKey("d");
        leftPressed = Input.GetKey("a");
        runPressed = Input.GetKey("left shift");

        //ACCELERATING
        Accelerate();

        //DECELERATING
        Decelerate();

        //RESET
        ResetVelocity();
  
        animator.SetFloat("Velocity Z", velocityZ);
        animator.SetFloat("Velocity X", velocityX);
    }

    void Accelerate() 
    {
        //increase velocity in positive Z direction
        if (forwardPressed && velocityZ < 1.0f && !runPressed)
        {
            velocityZ += Time.deltaTime * acceleration;
        }

        //increase velocity in negative Z direction
        if (backwardPressed && velocityZ > -1.0f && !runPressed)
        {
            velocityZ -= Time.deltaTime * acceleration;
        }

        //increase velocity in negative X direction
        if (leftPressed && velocityX > -1.0f && !runPressed)
        {
            velocityX -= Time.deltaTime * acceleration;
        }

        //increase velocity in positive X direction
        if (rightPressed && velocityX < 1.0f && !runPressed)
        {
            velocityX += Time.deltaTime * acceleration;
        }
    }

    void Decelerate() 
    {
        //decrease velocity in positive Z direction
        if (!forwardPressed && velocityZ > 0.0f)
        {
            velocityZ -= Time.deltaTime * deceleration;
        }

        //decrease velocity in negative Z direction
        if (!backwardPressed && velocityZ < 0.0f)
        {
            velocityZ += Time.deltaTime * deceleration;
        }

        //decrease velocity in positive X direction
        if (!rightPressed && velocityX > 0.0f)
        {
            velocityX -= Time.deltaTime * deceleration;
        }

        //decrease velocity in negative X direction
        if (!leftPressed && velocityX < 0.0f)
        {
            velocityX += Time.deltaTime * deceleration;
        }

    }

    void ResetVelocity() 
    {
        //reset VelocityZ
        if (!forwardPressed && !backwardPressed && velocityZ != 0 && (velocityZ > -1.0f && velocityZ < 1.0f))
        {
            velocityZ = 0.0f;
        }

        //reset VelocityX
        if (!leftPressed && !rightPressed && velocityX != 0 && (velocityX > -1.0f && velocityX < 1.0f))
        {
            velocityX = 0.0f;
        }
    }
}
