using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    [Tooltip("Camera sensitivity")]
    [SerializeField]
    private float lookSensitivity = 1f;
    [Tooltip("Additional sensitivity for WebGL")]
    [SerializeField]
    private float webglLookSensitivityMultiplier = 0.25f;
    // Start is called before the first frame update

    public bool CanProcessInput()
    {
        // TODO: aggiungere flow "Game End"
        return Cursor.lockState == CursorLockMode.Locked;
    }

    public Vector3 GetMoveInput()
    {
        if (CanProcessInput())
        {
            // TODO: cambiare "Horizontal" con una costante.
            float inputHorizontal = Input.GetAxisRaw("Horizontal");
            // TODO: cambiare "Vertical" con una costante.
            float inputVertical = Input.GetAxisRaw("Vertical");

            Vector3 move = new Vector3(
                inputHorizontal,
                0f,
                inputVertical
            );

            move = Vector3.ClampMagnitude(move, 1);

            return move;
        }

        return Vector3.zero;
    }

    public float GetLookInputsHorizontal()
    {
        // TODO: GamePad support
        return GetMouseOrStickLookAxis("Mouse X");
    }

    public float GetLookInputsVertical()
    {
        // TODO: GamePad support
        return GetMouseOrStickLookAxis("Mouse Y");
    }

    public bool GetJumpInputDown()
    {
        if (CanProcessInput())
        {
            return Input.GetButtonDown("Jump");
        }

        return false;
    }

    float GetMouseOrStickLookAxis(string inputName)
    {
        if (CanProcessInput())
        {
            // TODO: GamePad support
            float i = Input.GetAxisRaw(inputName);

            i *= lookSensitivity;

            {
                i *= 0.01f;
#if UNITY_WEBGL
                // fix webgl sensitivity
                i *= webglLookSensitivityMultiplier;
#endif
            }

            return i;
        }

        return 0f;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
