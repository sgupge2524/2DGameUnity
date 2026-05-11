using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Implements game physics for some in game entity.
    /// </summary>
    public class KinematicObject : MonoBehaviour
    {
        /// <summary>
        /// The minimum normal (dot product) considered suitable for the entity sit on.
        /// </summary>
        public float minGroundNormalY = .65f;

        /// <summary>
        /// A custom gravity coefficient applied to this entity.
        /// </summary>
        public float gravityModifier = 1f;
        public Vector2 gravityDirection = Vector2.down;

        /// <summary>
        /// エンティティの現在のベクトル
        /// </summary>
        public Vector2 velocity;

        public float gravityMaxSpeed = 6f;

        /// <summary>
        /// Is the entity currently sitting on a surface?
        /// </summary>
        /// <value></value>
        public bool IsGrounded { get; private set; }

        protected Vector2 targetVelocity;
        protected Vector2 groundNormal;
        protected Rigidbody2D body;
        protected ContactFilter2D contactFilter;
        protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];

        protected const float minMoveDistance = 0.001f;
        protected const float shellRadius = 0.01f;


        /// <summary>
        /// オブジェクトの垂直方向の速度を跳ね返す（設定された値に置き換える）。
        /// </summary>
        /// <param name="value">垂直速度として設定する値</param>
        public void Bounce(float value)
        {
            velocity.y = value;
        }

        /// <summary>
        /// 指定した方向ベクトルに応じてオブジェクトの速度を跳ね返す（x,y をそれぞれ設定する）。
        /// </summary>
        /// <param name="dir">設定する速度ベクトル</param>
        public void Bounce(Vector2 dir)
        {
            velocity.y = dir.y;
            velocity.x = dir.x;
        }

        /// <summary>
        /// Teleport to some position.
        /// </summary>
        /// <param name="position"></param>
        public void Teleport(Vector3 position)
        {
            body.position = position;
            velocity *= 0;
            body.velocity *= 0;
        }

        protected virtual void OnEnable()
        {
            body = GetComponent<Rigidbody2D>();
            body.isKinematic = true;
        }

        protected virtual void OnDisable()
        {
            body.isKinematic = false;
        }

        protected virtual void Start()
        {
            contactFilter.useTriggers = false;
            contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
            contactFilter.useLayerMask = true;
        }

        protected virtual void Update()
        {
            targetVelocity = Vector2.zero;
            ComputeVelocity();
        }

        protected virtual void ComputeVelocity()
        {

        }

        protected virtual void FixedUpdate()
        {
            //if already falling, fall faster than the jump speed, otherwise use normal gravity.
            Vector2 gravity = gravityDirection.normalized * Mathf.Abs(Physics2D.gravity.y);

            velocity += gravityModifier * gravity * Time.deltaTime;

            ApplyInputVelocity();
            ClampGravitySpeed();

            IsGrounded = false;

            var deltaPosition = velocity * Time.deltaTime;


            // 重力方向
            Vector2 gravityDir = gravityDirection.normalized;

            // 重力と逆方向 = 地面の法線方向
            Vector2 groundNormalByGravity = -gravityDir;

            // 地面に沿う方向
            Vector2 moveAlongGround = new Vector2(groundNormalByGravity.y, -groundNormalByGravity.x);

            // 地面に沿った移動
            float alongGroundAmount = Vector2.Dot(deltaPosition, moveAlongGround);
            var move = moveAlongGround * alongGroundAmount;
            PerformMovement(move, false);

            // 重力方向の移動
            float gravityAmount = Vector2.Dot(deltaPosition, gravityDir);
            move = gravityDir * gravityAmount;
            PerformMovement(move, true);

        }

        /// <summary>
        /// 重力方向に対して垂直な方向へ入力速度を適用する。
        /// </summary>
        void ApplyInputVelocity()
        {
            if (gravityDirection == Vector2.up || gravityDirection == Vector2.down)
            {
                velocity.x = targetVelocity.x;
            }
            else if (gravityDirection == Vector2.left || gravityDirection == Vector2.right)
            {
                velocity.y = targetVelocity.y;
            }
        }

        /// <summary>
        /// 重力方向の速度が最大値を超えないよう制限する。
        /// </summary>
        void ClampGravitySpeed()
        {
            if (gravityDirection == Vector2.up || gravityDirection == Vector2.down)
            {
                velocity.y = Mathf.Clamp(velocity.y, -gravityMaxSpeed, gravityMaxSpeed);
            }
            else if (gravityDirection == Vector2.left || gravityDirection == Vector2.right)
            {
                velocity.x = Mathf.Clamp(velocity.x, -gravityMaxSpeed, gravityMaxSpeed);
            }
        }

        void PerformMovement(Vector2 move, bool yMovement)
        {
            var distance = move.magnitude;

            if (distance > minMoveDistance)
            {
                //check if we hit anything in current direction of travel
                var count = body.Cast(move, contactFilter, hitBuffer, distance + shellRadius);
                for (var i = 0; i < count; i++)
                {
                    var currentNormal = hitBuffer[i].normal;

                    //is this surface flat enough to land on?
                    if (Vector2.Dot(currentNormal, -gravityDirection.normalized) > minGroundNormalY)
                    {
                        IsGrounded = true;
                        groundNormal = currentNormal;
                    }
                    if (IsGrounded)
                    {
                        //how much of our velocity aligns with surface normal?
                        var projection = Vector2.Dot(velocity, currentNormal);
                        if (projection < 0)
                        {
                            //slower velocity if moving against the normal (up a hill).
                            velocity = velocity - projection * currentNormal;
                        }
                    }
                    else
                    {
                        //We are airborne, but hit something, so cancel vertical up and horizontal velocity.
                        velocity.x *= 0;
                        velocity.y = Mathf.Min(velocity.y, 0);
                    }
                    //remove shellDistance from actual move distance.
                    var modifiedDistance = hitBuffer[i].distance - shellRadius;
                    distance = modifiedDistance < distance ? modifiedDistance : distance;
                }
            }
            body.position = body.position + move.normalized * distance;
        }

    }
}