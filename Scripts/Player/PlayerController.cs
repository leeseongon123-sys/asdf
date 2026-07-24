using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SculptGame.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float walkSpeed = 7.0f;
        public float runSpeed = 11.0f;
        public float acceleration = 16.0f;
        public float deceleration = 22.0f;
        public float rotationSpeed = 14.0f;
        public float jumpHeight = 1.6f;
        public float gravity = -22.0f;

        [Header("Ground Check")]
        public Transform groundCheck;
        public float groundDistance = 0.4f;
        public LayerMask groundMask;

        private CharacterController controller;
        private Vector3 verticalVelocity;
        private Vector3 currentHorizontalVelocity;
        private bool isGrounded;

        private Vector2 moveInput;
        private bool isRunning;
        private bool jumpRequested;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.height = 2.0f;
                controller.radius = 0.4f;
                controller.center = new Vector3(0f, 1.0f, 0f);
            }

            FixVisualMeshOffset();
        }

        private void FixVisualMeshOffset()
        {
            // If MeshFilter exists directly on root, move it to a child Visual GameObject so feet touch Y=0
            MeshFilter rootMesh = GetComponent<MeshFilter>();
            if (rootMesh != null)
            {
                Renderer rootRend = GetComponent<Renderer>();

                GameObject visObj = new GameObject("Visual");
                visObj.transform.SetParent(transform);
                visObj.transform.localPosition = new Vector3(0f, 1.0f, 0f);
                visObj.transform.localRotation = Quaternion.identity;

                MeshFilter newFilter = visObj.AddComponent<MeshFilter>();
                newFilter.sharedMesh = rootMesh.sharedMesh;

                MeshRenderer newRend = visObj.AddComponent<MeshRenderer>();
                if (rootRend != null) newRend.sharedMaterials = rootRend.sharedMaterials;

                Collider rootCol = GetComponent<Collider>();
                if (rootCol != null && !(rootCol is CharacterController))
                {
                    Destroy(rootCol);
                }

                Destroy(rootMesh);
                if (rootRend != null) Destroy(rootRend);
            }
            else
            {
                Transform visTransform = transform.Find("Visual");
                if (visTransform != null)
                {
                    visTransform.localPosition = new Vector3(0f, 1.0f, 0f);
                }
            }
        }

        private void Update()
        {
            if (groundCheck != null)
            {
                isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            }
            else
            {
                isGrounded = controller.isGrounded;
            }

            if (isGrounded && verticalVelocity.y < 0)
            {
                verticalVelocity.y = -2f;
            }

            ReadInputs();

            // Calculate Target Move Velocity with Inertia (Acceleration & Deceleration)
            float targetSpeed = isRunning ? runSpeed : walkSpeed;
            Vector3 targetDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            Vector3 targetHorizontalVelocity = targetDir * targetSpeed;

            float rate = (targetDir.sqrMagnitude > 0.001f) ? acceleration : deceleration;
            currentHorizontalVelocity = Vector3.MoveTowards(currentHorizontalVelocity, targetHorizontalVelocity, rate * Time.deltaTime);

            if (currentHorizontalVelocity.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(currentHorizontalVelocity.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            controller.Move(currentHorizontalVelocity * Time.deltaTime);

            // Jump
            if (jumpRequested && isGrounded)
            {
                verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpRequested = false;
            }

            // Apply Gravity
            verticalVelocity.y += gravity * Time.deltaTime;
            controller.Move(verticalVelocity * Time.deltaTime);
        }

        private void ReadInputs()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                float horizontal = 0f;
                float vertical = 0f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;

                moveInput = new Vector2(horizontal, vertical);
                isRunning = Keyboard.current.leftShiftKey.isPressed;
                if (Keyboard.current.spaceKey.wasPressedThisFrame) jumpRequested = true;
            }
#else
            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            isRunning = Input.GetKey(KeyCode.LeftShift);
            if (Input.GetButtonDown("Jump")) jumpRequested = true;
#endif
        }
    }
}
