using Platformer.Core;
using Platformer.Mechanics;
using static Platformer.Core.Simulation;

namespace Platformer.Gameplay
{
    /// <summary>
    /// プレイヤーの体力が 0 に到達したときに発火します。通常は
    /// `PlayerDeath` イベントをスケジュールします。
    /// </summary>
    /// <typeparam name="HealthIsZero"></typeparam>
    public class HealthIsZero : Simulation.Event<HealthIsZero>
    {
        public Health health;

        public override void Execute()
        {
            // Health コンポーネントが付いているオブジェクトの種類によって発火するイベントを変える
            var player = health.GetComponent<PlayerController>();
            if (player != null)
            {
                Schedule<PlayerDeath>();
                return;
            }

            var enemy = health.GetComponent<EnemyController>();
            if (enemy != null)
            {
                var ev = Schedule<EnemyDeath>();
                ev.enemy = enemy;
                return;
            }

            // どちらでもない場合は既定動作としてプレイヤー死亡をスケジュール（安全策）
            Schedule<PlayerDeath>();
        }
    }
}