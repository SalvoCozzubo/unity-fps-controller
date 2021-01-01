using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : MonoBehaviour
{
    private CharacterController m_cc;
    private PlayerInputHandler m_InputHandler;
    public Vector3 charVelocity { get; set; }
    public bool isGrounded { get; private set; }
    private Vector3 m_GroundNormal;

    const float k_jumpGroundingPreventionTime = 0.2f;
    const float k_groundCheckDistanceInAir = 0.07f;
    private float m_LastTimeJumped = 0f;
    private Vector3 m_LatestImpactSpeed;

    [SerializeField]
    private Camera playerCamera;
    
    [Header("Movement")]
    [Tooltip("Movement speed when grounded")]
    [SerializeField]
    private float maxSpeedOnGround = 10f;
    [Tooltip("Sharpness for the movement when grounded")]
    [SerializeField]
    private float movementSharpnessOnGround = 15f;
    [Tooltip("Multiplicator for the sprint")]
    [SerializeField]
    private float accelerationSpeedInAir = 25f;
    [Tooltip("Acceleration speed when in the air")]
    [SerializeField]
    private float maxSpeedInAir = 10f;

    [Header("Jump")]
    [Tooltip("Jump force")]
    private float jumpForce = 9f;
    
    [Header("Rotation")]
    [Tooltip("Rotation speed for moving camera")]
    [SerializeField]
    private float rotationSpeed = 200f;

    [Header("General")]
    [Tooltip("Gravity when in the air")]
    [SerializeField]
    private float gravityDownForce = 20f;
    [Tooltip("Distance check distance test")]
    [SerializeField]
    private float groundCheckDistance = 0.05f;
    [Tooltip("Physic Layer")]
    [SerializeField]
    private LayerMask groundCheckLayers = -1;

    
    private float rotationMultipler
    {
        get
        {
            return 1f;
        } 
    }

    private float m_CameraVerticalAngle = 0f;

    // Start is called before the first frame update
    void Start() {
        m_cc = GetComponent<CharacterController>();
        m_InputHandler = GetComponent<PlayerInputHandler>();

        m_cc.enableOverlapRecovery = true;
    }

    // Update is called once per frame
    void Update()
    {
        GroundCheck();
        Move();

    }

    bool IsNormalUnderSlopeLimit(Vector3 normal)
    {
        return Vector3.Angle(transform.up, normal) <= m_cc.slopeLimit;
    }

    void GroundCheck()
    {
        float chosenGroundCheckDistance = isGrounded ? m_cc.skinWidth + groundCheckDistance : k_groundCheckDistanceInAir;

        isGrounded = false;
        m_GroundNormal = Vector3.up;

        if (Time.time >= m_LastTimeJumped + k_jumpGroundingPreventionTime)
        {
            if (Physics.CapsuleCast(
                GetCapsuleBottomHemisphere(),
                GetCapsuleTopHemisphere(m_cc.height),
                m_cc.radius,
                Vector3.down,
                out RaycastHit hit,
                chosenGroundCheckDistance,
                groundCheckLayers,
                QueryTriggerInteraction.Ignore
            ))
            {
                m_GroundNormal = hit.normal;

                if (Vector3.Dot(hit.normal, transform.up) > 0f &&
                    IsNormalUnderSlopeLimit(m_GroundNormal))
                {
                    isGrounded = true;

                    if (hit.distance > m_cc.skinWidth)
                    {
                        m_cc.Move(Vector3.down * hit.distance);
                    }
                }
            }
        }
    }

    Vector3 GetCapsuleBottomHemisphere()
    {
        return transform.position + (transform.up * m_cc.radius);
    }

    Vector3 GetCapsuleTopHemisphere(float atHeight)
    {
        return transform.position + (transform.up * (atHeight - m_cc.radius));
    }

    public Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
    {
        Vector3 directionRight = Vector3.Cross(direction, transform.up);
        return Vector3.Cross(slopeNormal, directionRight).normalized;
    }

    void Move()
    {
        // horizontal character rotation
        {
            transform.Rotate(
                new Vector3(
                    0,
                    m_InputHandler.GetLookInputsHorizontal() * rotationSpeed * rotationMultipler,
                    0f
                ),
                Space.Self);
        }

        // vertical camera rotation
        {
            m_CameraVerticalAngle += m_InputHandler.GetLookInputsVertical() * rotationSpeed * rotationMultipler;
            m_CameraVerticalAngle = Mathf.Clamp(m_CameraVerticalAngle, -89f, 89f);

            if (playerCamera) {
                playerCamera.transform.localEulerAngles = new Vector3(m_CameraVerticalAngle, 0, 0);
            }
        }

        // movement
        {
            // TODO: speed modifier without sprint
            float speedModifier = 1f;
            Vector3 worldspaceMoveInput = transform.TransformVector(m_InputHandler.GetMoveInput());
            
            if (isGrounded)
            {
                Vector3 targetVelocity = worldspaceMoveInput * maxSpeedOnGround * speedModifier;

                targetVelocity = GetDirectionReorientedOnSlope(targetVelocity.normalized, m_GroundNormal) * targetVelocity.magnitude;
                charVelocity = Vector3.Lerp(charVelocity, targetVelocity, movementSharpnessOnGround * Time.deltaTime);

                // jumping
                if (isGrounded && m_InputHandler.GetJumpInputDown())
                {
                    charVelocity = new Vector3(charVelocity.x, 0, charVelocity.z);
                    charVelocity += Vector3.up * jumpForce;

                    m_LastTimeJumped = Time.time;
                    isGrounded = false;
                    m_GroundNormal = Vector3.up;
                }
            }
            else
            {
                charVelocity += worldspaceMoveInput * accelerationSpeedInAir * Time.deltaTime;

                float verticalVelocity = charVelocity.y;
                Vector3 horizontalVelocity = Vector3.ProjectOnPlane(charVelocity, Vector3.up);
                horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, maxSpeedInAir * speedModifier);
                charVelocity = horizontalVelocity + (Vector3.up * verticalVelocity);

                // apply the gravity to the velocity
                charVelocity += Vector3.down * gravityDownForce * Time.deltaTime;
            }
        }

        Vector3 capsuleBottomBeforeMove = GetCapsuleBottomHemisphere();
        Vector3 capsuleTopBeforeMove = GetCapsuleTopHemisphere(m_cc.height);
        m_cc.Move(charVelocity * Time.deltaTime);

        m_LatestImpactSpeed = Vector3.zero;
        if (Physics.CapsuleCast(
            capsuleBottomBeforeMove,
            capsuleTopBeforeMove,
            m_cc.radius,
            charVelocity.normalized,
            out RaycastHit hit,
            charVelocity.magnitude * Time.deltaTime,
            -1,
            QueryTriggerInteraction.Ignore
        ))
        {
            m_LatestImpactSpeed = charVelocity;
            charVelocity = Vector3.ProjectOnPlane(charVelocity, hit.normal);
        }
    }
}
