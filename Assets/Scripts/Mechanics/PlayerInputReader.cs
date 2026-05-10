using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Handles reading player input from keyboard/gamepad.
    /// This class abstracts input handling, making it easy to swap for AI, gamepad, or replay input.
    /// </summary>
    public class PlayerInputReader : MonoBehaviour
    {
        /// <summary>
        /// Movement input (-1, 0, or 1 for each axis)
        /// </summary>
        public Vector2 moveInput { get; private set; } = Vector2.zero;

        /// <summary>
        /// Whether jump button was pressed this frame
        /// </summary>
        public bool jumpPressed { get; private set; } = false;

        /// <summary>
        /// Whether jump button was released this frame
        /// </summary>
        public bool jumpReleased { get; private set; } = false;

        /// <summary>
        /// Requested gravity direction (or zero if not changing)
        /// </summary>
        public Vector2? gravityChangeRequest { get; private set; } = null;

        private Vector2 _moveInputAccumulator = Vector2.zero;

        private void Update()
        {
            // Reset per-frame inputs
            _moveInputAccumulator = Vector2.zero;
            jumpPressed = false;
            jumpReleased = false;
            gravityChangeRequest = null;

            // Read movement input
            ReadMoveInput();

            // Read jump input
            ReadJumpInput();

            // Read gravity input
            ReadGravityInput();

            // 蓄積した移動入力を適用する
            moveInput = _moveInputAccumulator;
        }

        /// <summary>
        /// Read WASD movement input
        /// </summary>
        private void ReadMoveInput()
        {
            if (Input.GetKey(KeyCode.A))
            {
                _moveInputAccumulator.x = -1;
            }
            if (Input.GetKey(KeyCode.D))
            {
                _moveInputAccumulator.x = 1;
            }
            if (Input.GetKey(KeyCode.W))
            {
                _moveInputAccumulator.y = 1;
            }
            if (Input.GetKey(KeyCode.S))
            {
                _moveInputAccumulator.y = -1;
            }
        }

        /// <summary>
        /// Read jump input from Space or Jump button
        /// </summary>
        private void ReadJumpInput()
        {
            if (Input.GetButtonDown("Jump"))
            {
                jumpPressed = true;
            }
            if (Input.GetButtonUp("Jump"))
            {
                jumpReleased = true;
            }
        }

        /// <summary>
        /// Read arrow key input to change gravity direction
        /// </summary>
        private void ReadGravityInput()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                gravityChangeRequest = Vector2.right;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                gravityChangeRequest = Vector2.up;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                gravityChangeRequest = Vector2.left;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                gravityChangeRequest = Vector2.down;
            }
        }
    }
}
