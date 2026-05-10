using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;

namespace Platformer.Mechanics
{
    public class PlayerController : KinematicObject
    {
        [SerializeField] private AudioClip jumpAudio;
        [SerializeField] private AudioClip respawnAudio;
        [SerializeField] private AudioClip ouchAudio;
        [SerializeField] private float maxSpeed = 7;
        [SerializeField] private float jumpTakeOffSpeed = 7;
        public AudioClip JumpAudio => jumpAudio;
        public AudioClip RespawnAudio => respawnAudio;
        public AudioClip OuchAudio => ouchAudio;
        public float MaxSpeed
        {
            get => maxSpeed;
            set => maxSpeed = value;
        }
        public float JumpTakeOffSpeed => jumpTakeOffSpeed;
        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        private Collider2D collider2d;
        private AudioSource audioSource;
        private Health health;
        public bool controlEnabled = true;
        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();
        PlayerInputReader inputReader;
        PlayerFacing playerFacing;
        private int gravityChangeCount = 0;
        [SerializeField, Min(0)] private int maxGravityChanges = 3;
        public Bounds Bounds => collider2d.bounds;
        public Collider2D Collider2DComponent => collider2d;
        public AudioSource AudioSourceComponent => audioSource;
        public Health HealthComponent => health;
        // 向きを表す変数
        public float FacingDirection => playerFacing.Direction;
        public int RemainingGravityChanges => Mathf.Max(maxGravityChanges - gravityChangeCount, 0);

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            inputReader = GetComponent<PlayerInputReader>();
            playerFacing = GetComponent<PlayerFacing>();

        }

        /// <summary>
        /// 毎フレーム呼び出される更新処理。
        /// 各関心事は専用メソッドに分割されており、このメソッドはそれらを順に呼び出すのみです。
        /// </summary>
        protected override void Update()
        {
            UpdateControlState();
            UpdateJumpState();
            base.Update();
        }

        /// <summary>
        /// 入力やコントロールの状態を集約して更新する（移動入力、ジャンプ要求、重力変更要求）。
        /// Update 内の条件分岐はこのメソッドに隠蔽される。
        /// </summary>
        private void UpdateControlState()
        {
            if (controlEnabled && inputReader != null)
            {
                ProcessMovementInput();
                ProcessJumpInput();
                ProcessGravityChange();
            }
            else
            {
                move = Vector2.zero;
            }
        }

        /// <summary>
        /// プレイヤーの移動入力を読み取って `move` に設定する。
        /// </summary>
        private void ProcessMovementInput()
        {
            move = inputReader.moveInput;
        }

        /// <summary>
        /// ジャンプ開始/停止の入力を処理する（PrepareToJump フラグの設定、停止時のスケジュール）。
        /// </summary>
        private void ProcessJumpInput()
        {
            if (jumpState == JumpState.Grounded && inputReader.jumpPressed)
            {
                jumpState = JumpState.PrepareToJump;
            }
            else if (inputReader.jumpReleased)
            {
                stopJump = true;
                Schedule<PlayerStopJump>().player = this;
            }
        }

        /// <summary>
        /// 重力変更要求があり、かつ変更回数が3回未満の場合のみ適用する。
        /// </summary>
        private void ProcessGravityChange()
        {
            if (inputReader.gravityChangeRequest.HasValue && gravityChangeCount < maxGravityChanges)
            {
                gravityDirection = inputReader.gravityChangeRequest.Value;
                gravityChangeCount++;
            }
        }

        /// <summary>
        /// 現在の `jumpState` に基づいてジャンプ状態を遷移させる。
        /// - `PrepareToJump` から `Jumping` へ、`Jumping` から `InFlight` へ等の遷移を扱う。
        /// - 必要に応じてイベント（`PlayerJumped`, `PlayerLanded`）をスケジュールする。
        /// </summary>
        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }



        /// <summary>
        /// ジャンプ状態と重力変更回数をリセットする（リスポーン時に使用）
        /// </summary>
        public void ResetJumpState()
        {
            jumpState = JumpState.Grounded;
            stopJump = false;
            jump = false;
            gravityChangeCount = 0;
        }

        /// <summary>
        /// 重力変更トークンを獲得して、使用した重力変更回数を1回復する。
        /// gravityChangeCount は 0 未満にならない。
        /// </summary>
        public void AddGravityChangeToken()
        {
            gravityChangeCount = Mathf.Max(gravityChangeCount - 1, 0);
        }

        /// <summary>
        /// ジャンプ開始・停止とジャンプ中の減速を処理する。
        /// ComputeVelocity から呼び出される。
        /// </summary>
        private void HandleJump()
        {
            if (jump && IsGrounded)
            {
                Vector2 jumpDirection = -gravityDirection.normalized;
                velocity = jumpDirection * jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }
        }

        /// <summary>
        /// プレイヤーの向き（表示方向）を更新する。
        /// PlayerFacing に委譲して重力方向と移動入力から向きを決定する。
        /// </summary>
        private void UpdateFacing()
        {
            playerFacing.UpdateGravityAlignment(gravityDirection);
            playerFacing.UpdateDirection(move, gravityDirection);
        }

        /// <summary>
        /// Animator のパラメータを更新する。grounded と 移動速度（重力依存軸に投影した値）を設定する。
        /// </summary>
        private void UpdateAnimator()
        {
            animator.SetBool("grounded", IsGrounded);
            float moveSpeed = Mathf.Abs(GravityAxisUtility.GetAxisValue(velocity, gravityDirection));
            animator.SetFloat("velocityX", moveSpeed / maxSpeed);
        }

        /// <summary>
        /// 重力方向と入力に基づいて targetVelocity を計算して設定する。
        /// CalculateTargetVelocity は ComputeVelocity から呼び出される。
        /// </summary>
        private void CalculateTargetVelocity()
        {
            Vector2 movementAxis = GravityAxisUtility.GetMovementAxis(gravityDirection);
            float moveAmount = GravityAxisUtility.GetAxisValue(move, gravityDirection);
            targetVelocity = movementAxis * moveAmount * maxSpeed;
        }

        /// <summary>
        /// 現在の入力と状態に基づいてプレイヤーの目標速度やジャンプ処理を計算する。
        /// - ジャンプ開始・停止やジャンプ中の減速を扱う
        /// - 重力方向に応じた向きの更新と表示向きの反転を行う
        /// - アニメーション用の速度パラメータを設定する
        /// - 重力に沿った targetVelocity を決定する
        /// このメソッドは Update 内で呼び出され、FixedUpdate の物理計算に反映される速度を算出する責務がある。
        /// </summary>
        protected override void ComputeVelocity()
        {
            HandleJump();

            UpdateFacing();

            UpdateAnimator();

            CalculateTargetVelocity();

        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}