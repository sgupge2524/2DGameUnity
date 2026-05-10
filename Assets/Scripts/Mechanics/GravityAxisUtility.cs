using UnityEngine;

namespace Platformer.Mechanics
{
    public static class GravityAxisUtility
    {
        /// <summary>
        /// 重力方向に対して、移動に使う接線方向の軸を返す。
        /// </summary>
        public static Vector2 GetMovementAxis(Vector2 gravityDirection)
        {
            return Mathf.Abs(gravityDirection.x) > 0.01f
                ? Vector2.up * Mathf.Sign(gravityDirection.x)
                : Vector2.right;
        }

        /// <summary>
        /// 指定した値を、重力に応じた移動軸へ射影した値を返す。
        /// </summary>
        public static float GetAxisValue(Vector2 value, Vector2 gravityDirection)
        {
            return Vector2.Dot(value, GetMovementAxis(gravityDirection));
        }
    }
}