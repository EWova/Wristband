using UnityEngine;
using UnityEngine.InputSystem;
namespace EWova.Wristband
{
    public class FreeCam : MonoBehaviour
    {
        public float movementSpeed = 10f;
        public float fastMovementSpeed = 100f;
        public float freeLookSensitivity = 0.1f;
        public float zoomSensitivity = 10f;
        public float fastZoomSensitivity = 50f;

        private bool looking = false;

        void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            if (keyboard == null || mouse == null) return;

            bool fastMode = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            float speed = fastMode ? fastMovementSpeed : movementSpeed;

            // ===== 移動 =====
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                transform.position += -transform.right * speed * Time.deltaTime;

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                transform.position += transform.right * speed * Time.deltaTime;

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                transform.position += transform.forward * speed * Time.deltaTime;

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                transform.position += -transform.forward * speed * Time.deltaTime;

            if (keyboard.qKey.isPressed)
                transform.position += -transform.up * speed * Time.deltaTime;

            if (keyboard.eKey.isPressed)
                transform.position += transform.up * speed * Time.deltaTime;

            if (keyboard.rKey.isPressed || keyboard.pageUpKey.isPressed)
                transform.position += Vector3.up * speed * Time.deltaTime;

            if (keyboard.fKey.isPressed || keyboard.pageDownKey.isPressed)
                transform.position += -Vector3.up * speed * Time.deltaTime;

            // ===== 滑鼠視角 =====
            if (looking)
            {
                Vector2 delta = mouse.delta.ReadValue();

                float newRotationX = transform.localEulerAngles.y + delta.x * freeLookSensitivity;
                float newRotationY = transform.localEulerAngles.x - delta.y * freeLookSensitivity;

                transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
            }

            // ===== 滾輪縮放 =====
            float scroll = mouse.scroll.ReadValue().y;
            if (scroll != 0)
            {
                float zoom = fastMode ? fastZoomSensitivity : zoomSensitivity;
                transform.position += transform.forward * scroll * zoom * Time.deltaTime;
            }

            // ===== 滑鼠右鍵 =====
            if (mouse.rightButton.wasPressedThisFrame)
                StartLooking();

            if (mouse.rightButton.wasReleasedThisFrame)
                StopLooking();
        }

        void OnDisable()
        {
            StopLooking();
        }

        public void StartLooking()
        {
            looking = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void StopLooking()
        {
            looking = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}