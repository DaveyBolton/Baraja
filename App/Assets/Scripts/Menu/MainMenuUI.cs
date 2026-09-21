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

        [HideInInspector] public Button PlayButton;
        [HideInInspector] public Button OptionsButton;
        [HideInInspector] public Button StoreButton;

        [HideInInspector] public Button OptionsCloseButton;
        [HideInInspector] public Button StoreCloseButton;

        [HideInInspector] public Slider FxSlider;
        [HideInInspector] public Slider MusicSlider;
        [HideInInspector] public Button LanguageButton;
        [HideInInspector] public Text LanguageButtonLabel;

        [HideInInspector] public Transform StoreListContainer;
        [HideInInspector] public GameObject StoreRowPrefab;
        [HideInInspector] public Text StoreBalanceText;
        [HideInInspector] public Text StoreMessageText;

        private void Awake()
        {
            PlayButton.onClick.AddListener(OnPlayClicked);
            OptionsButton.onClick.AddListener(() => OptionsPanel.SetActive(true));
            StoreButton.onClick.AddListener(OpenStore);

            OptionsCloseButton.onClick.AddListener(() => OptionsPanel.SetActive(false));
            StoreCloseButton.onClick.AddListener(() => StorePanel.SetActive(false));

            FxSlider.onValueChanged.AddListener(v => GameSettings.FxVolume = v);
            MusicSlider.onValueChanged.AddListener(v => GameSettings.MusicVolume = v);
            LanguageButton.onClick.AddListener(OnLanguageToggle);
        }

        private void Start()
        {
            FxSlider.value = GameSettings.FxVolume;
            MusicSlider.value = GameSettings.MusicVolume;
            RefreshLanguageLabel();
            RefreshButtonLabels();
        }

        private void OnPlayClicked() => SceneManager.LoadScene("Combat");

        private void OnLanguageToggle()
        {
            GameSettings.Spanish = !GameSettings.Spanish;
            RefreshLanguageLabel();
            RefreshButtonLabels();
            if (StorePanel.activeSelf) RefreshStoreList();
        }

        private void RefreshLanguageLabel()
        {
            // Just the language name, not "Idioma: Español" / "Language:
            // English" - those were by far the longest labels in the game
            // and would have forced every other gem button to match their
            // width. Context (sitting right below the volume sliders) is
            // enough to explain what the button does.
            LanguageButtonLabel.text = GameSettings.Spanish ? "Español" : "English";
        }

        // Every static button label in the menu, translated - these were
        // English-only regardless of the language toggle even though every
        // other piece of text (names, descriptions) already switches with
        // GameSettings.Spanish.
        private void RefreshButtonLabels()
        {
            bool es = GameSettings.Spanish;
            PlayButton.GetComponentInChildren<Text>().text = es ? "Jugar" : "Play";
            OptionsButton.GetComponentInChildren<Text>().text = es ? "Opciones" : "Options";
            StoreButton.GetComponentInChildren<Text>().text = es ? "Tienda" : "Store";
            OptionsCloseButton.GetComponentInChildren<Text>().text = es ? "Cerrar" : "Close";
            StoreCloseButton.GetComponentInChildren<Text>().text = es ? "Cerrar" : "Close";
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
                priceText.text = item.DisplayPrice(spanish);

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
