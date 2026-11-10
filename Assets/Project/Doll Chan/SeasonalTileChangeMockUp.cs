using Farm.GameData;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.TileSystem.Test
{
    [RequireComponent(typeof(SeasonalTileController))]
    public class SeasonalTileChangeMockUp : MonoBehaviour
    {
        [Header("Controller Reference")]
        [SerializeField] private SeasonalTileController tileController;

        [Header("Test Options")]
        [SerializeField] private Season startSeason = Season.Spring;

        [Header("Seasonal Buttons")]
        [SerializeField] private Button springButton;
        [SerializeField] private Button summerButton;
        [SerializeField] private Button fallButton;
        [SerializeField] private Button winterButton;

        private void Reset()
        {
            tileController = GetComponent<SeasonalTileController>();
        }

        private void Awake()
        {
            if (tileController == null)
            {
                tileController = GetComponent<SeasonalTileController>();
            }

            tileController.Initialize(startSeason);

            BindButton(springButton, Season.Spring);
            BindButton(summerButton, Season.Summer);
            BindButton(fallButton, Season.Fall);
            BindButton(winterButton, Season.Winter);
        }

        private void BindButton(Button button, Season targetSeason)
        {
            if (button != null)
            {
                button.onClick.AddListener(() => tileController.ChangeSeason(targetSeason));
            }
        }
    }
}