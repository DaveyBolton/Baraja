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

        [HideInInspector] public Text OptionsTitleText;
        [HideInInspector] public Text FxLabelText;
        [HideInInspector] public Text MusicLabelText;
        [HideInInspector] public Text StoreTitleText;

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
            // Shows the language you'd SWITCH TO, not the current one -
            // showing the current language ("English" while already in
            // English) reads as a toggle that undoes itself: tapping your
            // own current language to confirm it instead flips you into
            // the other one, which looks broken even though it isn't.
            LanguageButtonLabel.text = GameSettings.Spanish ? "English" : "Español";
        }

        // Every static button label AND plain heading/field-label in the
        // menu, translated - these were English-only regardless of the
        // language toggle even though every other piece of text (names,
        // descriptions) already switches with GameSettings.Spanish. That
        // was the actual bug behind "I click English and it stays in
        // Spanish": the toggle itself worked, these just never followed it,
        // so it looked broken even when it wasn't.
        private void RefreshButtonLabels()
        {
            bool es = GameSettings.Spanish;
            PlayButton.GetComponentInChildren<Text>().text = es ? "Jugar" : "Play";
            OptionsButton.GetComponentInChildren<Text>().text = es ? "Opciones" : "Options";
            StoreButton.GetComponentInChildren<Text>().text = es ? "Tienda" : "Store";
            OptionsCloseButton.GetComponentInChildren<Text>().text = es ? "Cerrar" : "Close";
            StoreCloseButton.GetComponentInChildren<Text>().text = es ? "Cerrar" : "Close";

            OptionsTitleText.text = es ? "Opciones" : "Options";
            FxLabelText.text = es ? "Volumen de Efectos" : "FX Volume";
            MusicLabelText.text = es ? "Volumen de Música" : "Music Volume";
            StoreTitleText.text = es ? "Tienda" : "Store";
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

                if (item.Kind == StoreItemKind.CardFrame)
                {
                    // Frames never go back to a plain "Owned" state like other
                    // cosmetics - once owned, the button becomes how you
                    // switch decks, so it stays live (Equip) except when
                    // it's the one currently equipped.
                    bool ownedFrame = PlayerEntitlements.OwnsFrame(item.FrameId);
                    if (!ownedFrame)
                    {
                        buyLabel.text = spanish ? "Comprar" : "Buy";
                        buyButton.onClick.AddListener(() => OnBuyClicked(item));
                    }
                    else if (PlayerEntitlements.EquippedFrame == item.FrameId)
                    {
                        buyLabel.text = spanish ? "Equipado" : "Equipped";
                        buyButton.interactable = false;
                    }
                    else
                    {
                        buyLabel.text = spanish ? "Equipar" : "Equip";
                        buyButton.onClick.AddListener(() => OnEquipClicked(item));
                    }
                }
                else
                {
                    bool owned = item.Kind == StoreItemKind.RemoveAds && PlayerEntitlements.AdsRemoved;
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
        }

        private void OnEquipClicked(StoreItem item)
        {
            PlayerEntitlements.EquipFrame(item.FrameId);
            RefreshStoreList();
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
