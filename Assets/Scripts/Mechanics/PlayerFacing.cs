using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// プレイヤーの向きと見た目の表現を管理する。
    /// 向き管理を移動処理や入力処理から分離する。
    /// </summary>
    public class PlayerFacing : MonoBehaviour
    {
        private float _facingDirection = 1f;
        private SpriteRenderer _spriteRenderer;

        public float Direction => _facingDirection;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// 重力方向に対して、キャラクターの足が向くように姿勢を合わせる。
        /// </summary>
        public void UpdateGravityAlignment(Vector2 gravityDirection)
        {
            if (gravityDirection == Vector2.zero)
            {
                return;
            }

            transform.up = -gravityDirection.normalized;
        }

        /// <summary>
        /// 重力方向に応じた移動軸の入力で向きを更新する。
        /// 移動計算ロジックから呼び出す。
        /// </summary>
        public void UpdateDirection(Vector2 moveInput, Vector2 gravityDirection)
        {
            float moveAxis = GravityAxisUtility.GetAxisValue(moveInput, gravityDirection);

            if (moveAxis > 0.01f)
            {
                SetFacingDirection(1f);
            }
            else if (moveAxis < -0.01f)
            {
                SetFacingDirection(-1f);
            }
        }

        /// <summary>
        /// 向きの方向を直接設定する。
        /// </summary>
        private void SetFacingDirection(float direction)
        {
            _facingDirection = direction;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.flipX = (_facingDirection < 0);
            }
        }
    }
}
