using UnityEngine;
using UnityEngine.InputSystem;

public class DesktopPlayerTester : MonoBehaviour
{
    public Transform cameraTransform;

    public float moveSpeed = 4f;
    public float mouseSensitivity = 2f;

    private CharacterController controller;
    private float cameraPitch = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (Keyboard.current == null || Mouse.current == null)
            return;

        // Movement
        Vector2 moveInput = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
        if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
        if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;
        if (Keyboard.current.dKey.isPressed) moveInput.x += 1;

        Vector3 move =
            transform.forward * moveInput.y +
            transform.right * moveInput.x;

        if (controller != null)
        {
            controller.Move(move.normalized * moveSpeed * Time.deltaTime);
        }
        else
        {
            transform.position += move.normalized * moveSpeed * Time.deltaTime;
        }

        // Hold right click to look around
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            transform.Rotate(Vector3.up * mouseDelta.x * mouseSensitivity * Time.deltaTime * 50f);

            cameraPitch -= mouseDelta.y * mouseSensitivity * Time.deltaTime * 50f;
            cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);

            if (cameraTransform != null)
                cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }
}