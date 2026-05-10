using UnityEngine;
using UnityEngine.UI;
using Platformer.Mechanics;
using TMPro;

namespace Platformer.UI
{
    /// <summary>
    /// Displays the remaining gravity change count in a UI Text element.
    /// </summary>
    public class GravityChangeCountText : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private TMP_Text countText;

        private void Awake()
        {
            if (countText == null)
            {
                countText = GetComponent<TMP_Text>();
            }
        }

        private void Update()
        {
            if (playerController == null || countText == null)
            {
                return;
            }

            countText.text = "Remaining Gravity Changes: " + playerController.RemainingGravityChanges;
        }
    }
}
