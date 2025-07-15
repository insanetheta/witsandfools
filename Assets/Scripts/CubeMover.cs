using UnityEngine;

namespace WitsAndFools
{
    public class CubeMover : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float rotationSpeed = 90f;
        public bool autoMove = true;
        
        [Header("Auto Movement")]
        public Vector3 moveDirection = Vector3.right;
        public float moveDistance = 5f;
        
        private Vector3 startPosition;
        private bool movingForward = true;
        
        void Start()
        {
            startPosition = transform.position;
        }
        
        void Update()
        {
            if (autoMove)
            {
                AutoMove();
            }
            else
            {
                HandleInput();
            }
            
            // Always rotate for visual effect
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
        
        void AutoMove()
        {
            // Move back and forth between two points
            Vector3 targetPosition;
            
            if (movingForward)
            {
                targetPosition = startPosition + moveDirection.normalized * moveDistance;
            }
            else
            {
                targetPosition = startPosition;
            }
            
            // Move towards target
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            
            // Check if we reached the target
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                movingForward = !movingForward;
            }
        }
        
        void HandleInput()
        {
            // Manual movement with WASD keys
            Vector3 movement = Vector3.zero;
            
            if (Input.GetKey(KeyCode.W))
                movement += Vector3.forward;
            if (Input.GetKey(KeyCode.S))
                movement += Vector3.back;
            if (Input.GetKey(KeyCode.A))
                movement += Vector3.left;
            if (Input.GetKey(KeyCode.D))
                movement += Vector3.right;
            if (Input.GetKey(KeyCode.Q))
                movement += Vector3.up;
            if (Input.GetKey(KeyCode.E))
                movement += Vector3.down;
            
            transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);
        }
    }
}