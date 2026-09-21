using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Baraja.Core;
using Baraja.Store;

namespace Baraja.Menu
{
    public class MainMenuUI : MonoBehaviour
    {
        [HideInInspector] public GameObject OptionsPanel;
        [HideInInspector] public GameObject StorePanel;
        [HideInInspector] public GameObject TutorialPanel;

        [HideInInspector] public Button PlayButton;
        [HideInInspector] public Button HowToPlayButton;
        [HideInInspector] public Button OptionsButton;
        [HideInInspector] public Button StoreButton;

        [HideInInspector] public Button OptionsCloseButton;
        [HideInInspector] public Button TutorialCloseButton;
        [HideInInspector] public Button StoreCloseButton;

        [HideInInspector] public Slider FxSlider;
        [HideInInspector] public Slider MusicSlider;
        [HideInInspector] public Button LanguageButton;
        [HideInInspector] public Text LanguageButtonLabel;

        [HideInInspector] public Text TutorialTitleText;
        [HideInInspector] public Text TutorialBodyText;
        [HideInInspector] public Text TutorialPageIndexText;
        [HideInInspector] public Button TutorialNextButton;
        [HideInInspector] public Button TutorialBackButton;

        [HideInInspector] public Transform StoreListContainer;
        [HideInInspector] public GameObject StoreRowPrefab;
        [HideInInspector] public Text StoreBalanceText;
        [HideInInspector] public Text StoreMessageText;

        private int _tutorialIndex;

        private void Awake()
        {
            PlayButton.onClick.AddListener(OnPlayClicked);
            HowToPlayButton.onClick.AddListener(OpenTutorial);
            OptionsButton.onClick.AddListener(() => OptionsPanel.SetActive(true));
            StoreButton.onClick.AddListener(OpenStore);

            OptionsCloseButton.onClick.AddListener(() => OptionsPanel.SetActive(false));
            TutorialCloseButton.onClick.AddListener(() => TutorialPanel.SetActive(false));
            StoreCloseButton.onClick.AddListener(() => StorePanel.SetActive(false));

            FxSlider.onValueChanged.AddListener(v => GameSettings.FxVolume = v);
            MusicSlider.onValueChanged.AddListener(v => GameSettings.MusicVolume = v);
            LanguageButton.onClick.AddListener(OnLanguageToggle);

            TutorialNextButton.onClick.AddListener(() => ChangeTutorialPage(1));
            TutorialBackButton.onClick.AddListener(() => ChangeTutorialPage(-1));
        }

        private void Start()
        {
            FxSlider.value = GameSettings.FxVolume;
            MusicSlider.value = GameSettings.MusicVolume;
            RefreshLanguageLabel();
        }

        private void OnPlayClicked() => SceneManager.LoadScene("Combat");

        private void OnLanguageToggle()
        {
            GameSettings.Spanish = !GameSettings.Spanish;
            RefreshLanguageLabel();
            if (TutorialPanel.activeSelf) RefreshTutorialPage();
            if (StorePanel.activeSelf) RefreshStoreList();
        }

        private void RefreshLanguageLabel()
        {
            LanguageButtonLabel.text = GameSettings.Spanish ? "Idioma: Español" : "Language: English";
        }

        private void OpenTutorial()
        {
            _tutorialIndex = 0;
            TutorialPanel.SetActive(true);
            RefreshTutorialPage();
        }

        private void ChangeTutorialPage(int delta)
        {
            _tutorialIndex = Mathf.Clamp(_tutorialIndex + delta, 0, TutorialPages.Pages.Count - 1);
            RefreshTutorialPage();
        }

        private void RefreshTutorialPage()
        {
            var page = TutorialPages.Pages[_tutorialIndex];
            bool spanish = GameSettings.Spanish;
            TutorialTitleText.text = spanish ? page.TitleEs : page.TitleEn;
            TutorialBodyText.text = spanish ? page.BodyEs : page.BodyEn;
            TutorialPageIndexText.text = $"{_tutorialIndex + 1} / {TutorialPages.Pages.Count}";
            TutorialBackButton.interactable = _tutorialIndex > 0;
            TutorialNextButton.interactable = _tutorialIndex < TutorialPages.Pages.Count - 1;
        }

        private void OpenStore()
        {
            StorePanel.SetActive(true);
            StoreMessageText.text = "";
            RefreshStoreList();
        }

        private void RefreshStoreList()
        {
            foreach (Transform child in StoreListContainer) Destroy(child.gameObject);
            bool spanish = GameSettings.Spanish;

            StoreBalanceText.text = (spanish ? "Pétalos: " : "Petals: ") + PlayerEntitlements.PetalBalance;

            foreach (var item in StoreDatabase.All)
            {
                var row = Instantiate(StoreRowPrefab, StoreListContainer);
                row.SetActive(true);
                var nameText = row.transform.Find("NameText").GetComponent<Text>();
                var descText = row.transform.Find("DescText").GetComponent<Text>();
                var priceText = row.transform.Find("PriceText").GetComponent<Text>();
                var buyButton = row.transform.Find("BuyButton").GetComponent<Button>();
                var buyLabel = buyButton.GetComponentInChildren<Text>();

                nameText.text = item.DisplayName(spanish);
                descText.text = item.DisplayDescription(spanish);
                priceText.text = item.PriceDisplay;

                bool owned = (item.Kind == StoreItemKind.CardBackSkin && PlayerEntitlements.IsOwned(item.Id)) ||
                             (item.Kind == StoreItemKind.RemoveAds && PlayerEntitlements.AdsRemoved);
                if (owned)
                {
                    buyLabel.text = spanish ? "Comprado" : "Owned";
                    buyButton.interactable = false;
                }
                else
                {
                    buyLabel.text = spanish ? "Comprar" : "Buy";
                    buyButton.onClick.AddListener(() => OnBuyClicked(item));
                }
            }
        }

        private void OnBuyClicked(StoreItem item)
        {
            // Mock purchase only - PlayerEntitlements has no real payment backend.
            var result = PlayerEntitlements.Buy(item);
            bool spanish = GameSettings.Spanish;
            StoreMessageText.text = result switch
            {
                PurchaseResult.InsufficientCurrency => spanish ? "No tienes suficientes Pétalos." : "Not enough Petals.",
                PurchaseResult.AlreadyOwned => spanish ? "Ya lo tienes." : "Already owned.",
                _ => spanish ? "¡Comprado!" : "Purchased!",
            };
            RefreshStoreList();
        }
    }
}
