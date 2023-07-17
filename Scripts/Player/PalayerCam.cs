using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PalayerCam : MonoBehaviour
{
    public Transform PlayerCam;
    public PlayerMovement PlayerMovementScript;
    public Vector2 Sensitivities;

    private Vector2 XYRotation;

    public Canvas pauseMenu;
    public RectTransform pauseMenuTransform;

    public static bool inPauseMenu;

    private void Start()
    {
        inPauseMenu = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        PlayerMovementScript.inPauseMenu = inPauseMenu;
        if (Input.GetKeyDown(KeyCode.Escape) && !inPauseMenu)
        {
            pauseMenu.renderMode = RenderMode.ScreenSpaceOverlay;
            inPauseMenu = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && inPauseMenu)
        {
            pauseMenu.renderMode = RenderMode.WorldSpace;
            pauseMenuTransform.position = new Vector3(0, -100000, 0);
            inPauseMenu =false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
        }
        if (!inPauseMenu)
        {
            Vector2 MouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            XYRotation.x -= MouseInput.y * Sensitivities.y;
            XYRotation.y += MouseInput.x * Sensitivities.x;

            XYRotation.x = Mathf.Clamp(XYRotation.x, -90f, 80f);

            transform.eulerAngles = new Vector3(0f, XYRotation.y, 0f);
            PlayerCam.localEulerAngles = new Vector3(XYRotation.x, 0f, 0f);
        }

    }

}
