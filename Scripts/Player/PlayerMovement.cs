using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float MoveSmoothTime;
    public float GravityStrength;
    public float JumpStrength;
    public float WalkSpeed;
    public float RunSpeed;
    public float InAirSpeedModifier;

    private CharacterController Controller;
    public Light HUDLight;

    private Vector3 CurrentMovementVelocity;
    private Vector3 MoveDampVelocity;

    private Vector3 CurrenntFourceVelocity;
    bool climable;
    bool lightOn;
    public bool inPauseMenu;

    void Start()
    {
        Controller = GetComponent<CharacterController>();
        lightOn = false;
    }

    void Update()
    {
        if (!inPauseMenu)
        {
            climable = false;
            Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + .5f, transform.position.z), transform.forward * .4f);
            Debug.DrawRay(new Vector3(transform.position.x, transform.position.y - .5f, transform.position.z), transform.forward * .4f);

            RaycastHit upperRay;
            RaycastHit lowerRay;

            bool upperRayHit = Physics.Raycast(new Vector3(transform.position.x, transform.position.y + .5f, transform.position.z), transform.forward, out upperRay, .4f);
            bool lowerRayHit = Physics.Raycast(new Vector3(transform.position.x, transform.position.y - .5f, transform.position.z), transform.forward, out lowerRay, .4f);

            if (Input.GetKey(KeyCode.Q))
            {
                if (upperRayHit)
                {
                    if (upperRay.transform.name != "Player")
                    {
                        climable = true;
                    }
                }
                if (lowerRayHit)
                {
                    if (lowerRay.transform.name != "Player")
                    {
                        climable = true;
                    }
                }
            }



            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!lightOn)
                {
                    HUDLight.intensity = 2.5f;
                }
                if (lightOn)
                {
                    HUDLight.intensity = 0;
                }
                lightOn = !lightOn;
            }

            bool isGrounded = Controller.isGrounded;
            Ray headCheckRay = new Ray(transform.position, Vector3.up);
            bool hitHead = Physics.Raycast(headCheckRay, 1.25f);

            Vector3 PlayerInput = new Vector3()
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = 0f,
                z = Input.GetAxisRaw("Vertical")
            };

            if (PlayerInput.magnitude > 1f)
            {
                PlayerInput.Normalize();
            }

            Vector3 MoveVector = transform.TransformDirection(PlayerInput);
            float CurrentSpeed = Input.GetKey(KeyCode.LeftShift) ? RunSpeed : WalkSpeed;

            CurrentMovementVelocity = Vector3.SmoothDamp
                (CurrentMovementVelocity,
                MoveVector * CurrentSpeed,
                ref MoveDampVelocity,
                MoveSmoothTime);
            Controller.Move(CurrentMovementVelocity * Time.deltaTime);



            if (isGrounded || climable)
            {
                CurrenntFourceVelocity.y = -2f;

                if (Input.GetKey(KeyCode.Space))
                {
                    CurrenntFourceVelocity.y = JumpStrength;
                }

            }
            else
            {
                CurrenntFourceVelocity.y -= GravityStrength * Time.deltaTime;
                CurrentMovementVelocity *= InAirSpeedModifier;
            }

            if (hitHead)
            {
                CurrenntFourceVelocity.y -= GravityStrength * Time.deltaTime;
            }

            Controller.Move(CurrenntFourceVelocity * Time.deltaTime);

        }
    }

}
