using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Gameplay
{

    /// <summary>
    /// Fired when a Player collides with an Enemy.
    /// </summary>
    /// <typeparam name="EnemyCollision"></typeparam>
    public class PlayerEnemyCollision : Simulation.Event<PlayerEnemyCollision>
    {
        public EnemyController enemy;
        public PlayerController player;

        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            if (IsPlayerAboveEnemy())
            {
                var enemyHealth = enemy.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.Decrement();
                    if (!enemyHealth.IsAlive)
                    {
                        Schedule<EnemyDeath>().enemy = enemy;
                        BouncePlayerAfterEnemyHit(2);
                    }
                    else
                    {
                        BouncePlayerAfterEnemyHit(7);
                    }
                }
                else
                {
                    Schedule<EnemyDeath>().enemy = enemy;
                    BouncePlayerAfterEnemyHit(2);
                }
            }
            else
            {
                Schedule<PlayerDeath>();
            }
        }

        /// <summary>
        /// 重力方向に応じて、プレイヤーを正しい方向にはね返す。
        /// </summary>
        private void BouncePlayerAfterEnemyHit(float bounceValue)
        {
            Vector2 bounceDir = Vector2.zero;

            if (player.gravityDirection == Vector2.down)
            {
                bounceDir = new Vector2(0, bounceValue);
            }
            else if (player.gravityDirection == Vector2.up)
            {
                bounceDir = new Vector2(0, -bounceValue);
            }
            else if (player.gravityDirection == Vector2.left)
            {
                bounceDir = new Vector2(bounceValue, 0);
            }
            else if (player.gravityDirection == Vector2.right)
            {
                bounceDir = new Vector2(-bounceValue, 0);
            }

            player.Bounce(bounceDir);
        }

        /// <summary>
        /// プレイヤーが重力方向に対して敵の「上側」から当たったかを判定する。
        /// </summary>
        /// <returns>プレイヤーが敵の上側から当たった場合は true</returns>
        private bool IsPlayerAboveEnemy()
        {
            if (player.gravityDirection == Vector2.down)
            {
                return player.Bounds.center.y >= enemy.Bounds.max.y;
            }

            if (player.gravityDirection == Vector2.up)
            {
                return player.Bounds.center.y <= enemy.Bounds.min.y;
            }

            if (player.gravityDirection == Vector2.left)
            {
                return player.Bounds.center.x >= enemy.Bounds.max.x;
            }

            if (player.gravityDirection == Vector2.right)
            {
                return player.Bounds.center.x <= enemy.Bounds.min.x;
            }

            return player.Bounds.center.y >= enemy.Bounds.max.y;
        }
    }
}