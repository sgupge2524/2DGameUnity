using Platformer.Core;
using Platformer.Model;
using TMPro;
using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This class exposes the the game model in the inspector, and ticks the
    /// simulation.
    /// </summary> 
    public class GameController : MonoBehaviour
    {
        public static GameController Instance { get; private set; }

        //This model field is public and can be therefore be modified in the 
        //inspector.
        //The reference actually comes from the InstanceRegister, and is shared
        //through the simulation and events. Unity will deserialize over this
        //shared reference when the scene loads, allowing the model to be
        //conveniently configured inside the inspector.
        public PlatformerModel model = Simulation.GetModel<PlatformerModel>();
        [SerializeField] TMP_Text countdownText;
        [SerializeField] int startCountdownSeconds = 3;
        [SerializeField] float countdownIntervalSeconds = 1f;

        void Start()
        {
            StartCoroutine(BeginGameCountdown());
        }

        /// <summary>
        /// ゲーム開始時にカウントダウンを表示し、終了後にプレイヤー操作を有効化します。
        /// </summary>
        System.Collections.IEnumerator BeginGameCountdown()
        {
            if (model.player != null)
            {
                model.player.controlEnabled = false;
            }

            var seconds = Mathf.Max(1, startCountdownSeconds);
            for (var i = seconds; i >= 1; i--)
            {
                if (countdownText != null)
                {
                    countdownText.gameObject.SetActive(true);
                    countdownText.text = i.ToString();
                }
                else
                {
                    Debug.Log($"Countdown: {i}");
                }

                yield return new WaitForSeconds(countdownIntervalSeconds);
            }

            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(false);
            }

            if (model.player != null)
            {
                model.player.controlEnabled = true;
            }
        }

        void OnEnable()
        {
            Instance = this;
        }

        void OnDisable()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (Instance == this) Simulation.Tick();
        }
    }
}