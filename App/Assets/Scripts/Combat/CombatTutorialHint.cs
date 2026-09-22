using UnityEngine;
using UnityEngine.UI;
using Baraja.Core;
using Baraja.Menu;

namespace Baraja.Combat
{
    // The full "How to Play" tutorial (TutorialPages.cs), shown as a
    // click-through banner over the actual game board the first time anyone
    // reaches combat - replaces the old main-menu-only parchment overlay,
    // which was read divorced from the board it was describing. Back/Next
    // page through it; Next becomes "Got it" on the last page.
    public class CombatTutorialHint : MonoBehaviour
    {
        private const string SeenKey = "baraja_combat_tutorial_seen";

        [HideInInspector] public GameObject Panel;
        [HideInInspector] public Text TitleText;
        [HideInInspector] public Text BodyText;
        [HideInInspector] public Text PageIndexText;
        [HideInInspector] public Button BackButton;
        [HideInInspector] public Text BackButtonLabel;
        [HideInInspector] public Button NextButton;
        [HideInInspector] public Text NextButtonLabel;
        [HideInInspector] public Button CloseButton;

        private int _pageIndex;

        private void Awake()
        {
            BackButton.onClick.AddListener(() => ChangePage(-1));
            NextButton.onClick.AddListener(OnNextClicked);
            CloseButton.onClick.AddListener(Dismiss);
        }

        private void Start()
        {
            if (PlayerPrefs.GetInt(SeenKey, 0) != 0)
            {
                Panel.SetActive(false);
                return;
            }

            _pageIndex = 0;
            RefreshPage();
            Panel.SetActive(true);
        }

        private void ChangePage(int delta)
        {
            _pageIndex = Mathf.Clamp(_pageIndex + delta, 0, TutorialPages.Pages.Count - 1);
            RefreshPage();
        }

        private void OnNextClicked()
        {
            if (_pageIndex >= TutorialPages.Pages.Count - 1) Dismiss();
            else ChangePage(1);
        }

        private void RefreshPage()
        {
            var page = TutorialPages.Pages[_pageIndex];
            bool spanish = GameSettings.Spanish;
            TitleText.text = spanish ? page.TitleEs : page.TitleEn;
            BodyText.text = spanish ? page.BodyEs : page.BodyEn;
            PageIndexText.text = $"{_pageIndex + 1} / {TutorialPages.Pages.Count}";
            BackButton.interactable = _pageIndex > 0;
            BackButtonLabel.text = spanish ? "< Atrás" : "< Back";
            bool lastPage = _pageIndex >= TutorialPages.Pages.Count - 1;
            NextButtonLabel.text = lastPage ? (spanish ? "Entendido" : "Got it") : (spanish ? "Siguiente >" : "Next >");
        }

        private void Dismiss()
        {
            PlayerPrefs.SetInt(SeenKey, 1);
            Panel.SetActive(false);
        }
    }
}
