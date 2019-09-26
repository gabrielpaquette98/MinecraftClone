using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    float horizontal;
    float vertical;
    float mouseHorizontal;
    float mouseVertical;

    Vector3 velocity;

    Vector3 rotationVector;

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

    float rotationY = 0;
    float rotationX = 0;

    [SerializeField]
    Transform blockHighlight;

    float checkIncrement = 0.1f;
    float reach = 8;

    [SerializeField]
    Text selectedBlockName;

    public byte selectedBlockIndex = 1;

    Vector3 placeBlockPosition;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        GetPlayerInput();

        if (blockHighlight.gameObject.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Change the block to air to remove it
                world.GetChunkFromVector3(blockHighlight.position).EditVoxel(blockHighlight.position, 0); 
            }

            if (Input.GetMouseButtonDown(1))
            {
                // Place block
                world.GetChunkFromVector3(placeBlockPosition).EditVoxel(placeBlockPosition, selectedBlockIndex);
            }
        }

        PlaceBlockHighlight();
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

        float scroll = Input.GetAxis("SelectScroll");

        if (scroll != 0)
        {
            if (scroll > 0)
            {
                selectedBlockIndex++;
            }
            else
            {
                selectedBlockIndex--;
            }

            if (selectedBlockIndex > (byte)(world.blocktypes.Length - 1))
            {
                selectedBlockIndex = 1;
            }
            else if (selectedBlockIndex < 1)
            {
                selectedBlockIndex = (byte)(world.blocktypes.Length - 1);
            }

            selectedBlockName.text = world.blocktypes[selectedBlockIndex].blockName + " is selected";
        }
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
    void UpdateRotation()
    {
        rotationX += mouseHorizontal * viewSpeed;
        rotationY += mouseVertical * viewSpeed;
        rotationY = Mathf.Clamp(rotationY, -90f, 90f);

        camera.transform.localRotation = Quaternion.Euler(-rotationY, 0f, 0f);
        transform.rotation = Quaternion.Euler(0f, rotationX, 0f);
    }


    float CheckDownSpeed(float downSpeed)
    {
        Vector3 leftBack =   new Vector3(transform.position.x - colliderWidth, transform.position.y + downSpeed, transform.position.z - colliderWidth);
        Vector3 rightBack =  new Vector3(transform.position.x + colliderWidth, transform.position.y + downSpeed, transform.position.z - colliderWidth);
        Vector3 rightFront = new Vector3(transform.position.x + colliderWidth, transform.position.y + downSpeed, transform.position.z + colliderWidth);
        Vector3 leftFront =  new Vector3(transform.position.x - colliderWidth, transform.position.y + downSpeed, transform.position.z + colliderWidth);

        if ((world.CheckForVoxel(leftBack)   && (!CheckLeftCollision()  && !CheckBackCollision()))  ||
            (world.CheckForVoxel(rightBack)  && (!CheckRightCollision() && !CheckBackCollision()))  ||
            (world.CheckForVoxel(rightFront) && (!CheckRightCollision() && !CheckFrontCollision())) ||
            (world.CheckForVoxel(leftFront)  && (!CheckLeftCollision()  && !CheckFrontCollision())))
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
        Vector3 leftBack =   new Vector3(transform.position.x - colliderWidth, transform.position.y + 2f + upSpeed, transform.position.z - colliderWidth);
        Vector3 rightBack =  new Vector3(transform.position.x + colliderWidth, transform.position.y + 2f + upSpeed, transform.position.z - colliderWidth);
        Vector3 rightFront = new Vector3(transform.position.x + colliderWidth, transform.position.y + 2f + upSpeed, transform.position.z + colliderWidth);
        Vector3 leftFront =  new Vector3(transform.position.x - colliderWidth, transform.position.y + 2f + upSpeed, transform.position.z + colliderWidth);

        if ((world.CheckForVoxel(leftBack)   && (!CheckLeftCollision()  && !CheckBackCollision()))  ||
            (world.CheckForVoxel(rightBack)  && (!CheckRightCollision() && !CheckBackCollision()))  ||
            (world.CheckForVoxel(rightFront) && (!CheckRightCollision() && !CheckFrontCollision())) ||
            (world.CheckForVoxel(leftFront)  && (!CheckLeftCollision()  && !CheckFrontCollision())))
        {
            verticalMomentum = 0;
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }

    bool CheckFrontCollision()
    {
        return (world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + colliderWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z + colliderWidth)));
    }
    bool CheckBackCollision()
    {
        return (world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - colliderWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z - colliderWidth)));
    }
    bool CheckLeftCollision()
    {
        return (world.CheckForVoxel(new Vector3(transform.position.x - colliderWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x - colliderWidth, transform.position.y + 1, transform.position.z)));
    }
    bool CheckRightCollision()
    {
        return (world.CheckForVoxel(new Vector3(transform.position.x + colliderWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x + colliderWidth, transform.position.y + 1, transform.position.z)));
    }

    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        isJumping = false;
    }

    void PlaceBlockHighlight()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach)
        {
            Vector3 pos = camera.position + (camera.forward * step);

            if (world.CheckForVoxel(pos))
            {
                blockHighlight.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                blockHighlight.gameObject.SetActive(true);
                placeBlockPosition = lastPos;
                
                return;
            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            step += checkIncrement;
        }

        blockHighlight.gameObject.SetActive(false);
    }
}
// Note: For this project we can use the Standard Assets, the RigidBodyFPSController
