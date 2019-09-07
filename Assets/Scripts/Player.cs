using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    float horizontal;
    float vertical;
    float mouseHorizontal;
    float mouseVertical;

    Vector3 velocity;

    [SerializeField]
    Transform camera;
    [SerializeField]
    World world;
    
    public float viewSpeed = 5;
    public float walkSpeed = 5;
    public float runSpeed = 7;
    public float gravity = -9.8f;
    public float jumpForce = 5f;

    public float colliderWidth = 0.3f;
    float verticalMomentum = 0;
    bool isGrounded;
    bool isJumping;
    bool isSprinting;

    void Update()
    {
        GetPlayerInput();
    }
    
    void FixedUpdate()
    {
        UpdateVelocity();
        UpdateRotation();

        if (isJumping)
        {
            Jump();
        }

        transform.Translate(velocity, Space.World);
    }

    void GetPlayerInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint"))
        {
            isSprinting = true;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
        }

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            isJumping = true;
        }
    }

    void UpdateRotation()
    {
        transform.Rotate(Vector3.up * mouseHorizontal * viewSpeed);
        camera.Rotate(Vector3.right * -mouseVertical * viewSpeed);
    }

    void UpdateVelocity()
    {
        if (isSprinting)
        {
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * runSpeed;
        }
        else
        {
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;
        }

        if (verticalMomentum > gravity)
        {
            verticalMomentum += Time.fixedDeltaTime * gravity;
        }

        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime; 

        if ((velocity.x < 0 && CheckLeftCollision()) || (velocity.x > 0 && CheckRightCollision()))
        {
            velocity.x = 0;
        }

        if (velocity.y < 0)
        velocity.y = CheckDownSpeed(velocity.y);
        else if (velocity.y > 0)
        velocity.y = CheckUpSpeed(velocity.y);
        
        if ((velocity.z < 0 && CheckBackCollision()) || (velocity.z > 0 && CheckFrontCollision()))
        {
            velocity.z = 0;
        }
    }

    float CheckDownSpeed(float downSpeed)
    {
        if (world.CheckForVoxel(transform.position.x - colliderWidth, transform.position.y + downSpeed, transform.position.z - colliderWidth) ||
            world.CheckForVoxel(transform.position.x + colliderWidth, transform.position.y + downSpeed, transform.position.z - colliderWidth) ||
            world.CheckForVoxel(transform.position.x + colliderWidth, transform.position.y + downSpeed, transform.position.z + colliderWidth) ||
            world.CheckForVoxel(transform.position.x - colliderWidth, transform.position.y + downSpeed, transform.position.z + colliderWidth))
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return downSpeed;
        }
    }

    float CheckUpSpeed(float upSpeed)
    {
        if (world.CheckForVoxel(transform.position.x - colliderWidth, transform.position.y + 2f + upSpeed, transform.position.z - colliderWidth) ||
            world.CheckForVoxel(transform.position.x + colliderWidth, transform.position.y + 2f + upSpeed, transform.position.z - colliderWidth) ||
            world.CheckForVoxel(transform.position.x + colliderWidth, transform.position.y + 2f + upSpeed, transform.position.z + colliderWidth) ||
            world.CheckForVoxel(transform.position.x - colliderWidth, transform.position.y + 2f + upSpeed, transform.position.z + colliderWidth))
        {
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }

    bool CheckFrontCollision()
    {
        return (world.CheckForVoxel(transform.position.x, transform.position.y, transform.position.z + colliderWidth) ||
                world.CheckForVoxel(transform.position.x, transform.position.y + 1, transform.position.z + colliderWidth));
    }
    bool CheckBackCollision()
    {
        return (world.CheckForVoxel(transform.position.x, transform.position.y, transform.position.z - colliderWidth) ||
                world.CheckForVoxel(transform.position.x, transform.position.y + 1, transform.position.z - colliderWidth));
    }
    bool CheckLeftCollision()
    {
        return (world.CheckForVoxel(transform.position.x - colliderWidth, transform.position.y, transform.position.z) ||
                world.CheckForVoxel(transform.position.x - colliderWidth, transform.position.y + 1, transform.position.z));
    }
    bool CheckRightCollision()
    {
        return (world.CheckForVoxel(transform.position.x + colliderWidth, transform.position.y, transform.position.z) ||
                world.CheckForVoxel(transform.position.x + colliderWidth, transform.position.y + 1, transform.position.z));
    }

    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        isJumping = false;
    }
}

// Note: For this project we can use the Standard Assets, the RigidBodyFPSController
