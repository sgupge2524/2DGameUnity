using System.Collections;
using System.Collections.Generic;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    /// A simple controller for enemies. Provides movement control over a patrol path.
    /// </summary>
    [RequireComponent(typeof(AnimationController), typeof(Collider2D))]
    public class EnemyController : MonoBehaviour
    {
        public PatrolPath path;
        public AudioClip ouch;

        internal PatrolPath.Mover mover;
        internal AnimationController control;
        internal Collider2D _collider;
        internal AudioSource _audio;
        SpriteRenderer spriteRenderer;

        public Bounds Bounds => _collider.bounds;

        void Awake()
        {
            control = GetComponent<AnimationController>();
            _collider = GetComponent<Collider2D>();
            _audio = GetComponent<AudioSource>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            var player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null) OnPlayerCollision(player);
        }

        /// <summary>
        /// プレイヤーとの衝突を処理する。衝突イベントをスケジュールする。
        /// </summary>
        /// <param name="player">衝突したプレイヤー</param>
        private void OnPlayerCollision(PlayerController player)
        {
            var ev = Schedule<PlayerEnemyCollision>();
            ev.player = player;
            ev.enemy = this;
        }

        void Update()
        {
            UpdatePatrol();
        }

        /// <summary>
        /// パトロール路に沿った移動を更新する。
        /// Mover の初期化と移動方向の決定を行う。
        /// </summary>
        private void UpdatePatrol()
        {
            if (path != null)
            {
                InitializeMover();
                UpdateMoveInput();
            }
        }

        /// <summary>
        /// Mover を初期化する（未初期化の場合のみ）。
        /// </summary>
        private void InitializeMover()
        {
            if (mover == null) mover = path.CreateMover(control.maxSpeed * 0.5f);
        }

        /// <summary>
        /// Mover の現在位置に基づいて敵の移動入力を更新する。
        /// </summary>
        private void UpdateMoveInput()
        {
            control.move.x = Mathf.Clamp(mover.Position.x - transform.position.x, -1, 1);
        }

    }
}