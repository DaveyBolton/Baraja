using UnityEngine;
using UnityEngine.UI;
using Baraja.Core;

namespace Baraja.Combat
{
    // A one-time banner explaining the core combat loop (select enemy, play
    // card, End Turn) the first time anyone reaches the combat scene. This is
    // the immediate fix for "I have no idea how to play" - the full paged
    // explainer lives in the main menu's How to Play (TutorialPages.cs) for
    // anyone who wants the deeper mechanics (status effects, block, etc).
    public class CombatTutorialHint : MonoBehaviour
    {
        private const string SeenKey = "baraja_combat_tutorial_seen";

        [HideInInspector] public GameObject Panel;
        [HideInInspector] public Text BodyText;
        [HideInInspector] public Button GotItButton;

        private void Awake()
        {
            GotItButton.onClick.AddListener(Dismiss);
        }

        private void Start()
        {
            if (PlayerPrefs.GetInt(SeenKey, 0) != 0)
            {
                Panel.SetActive(false);
                return;
            }

            BodyText.text = GameSettings.Spanish
                ? "Toca un enemigo para elegirlo, luego toca una carta para jugarla. " +
                  "Cuando termines, toca \"End Turn\"."
                : "Tap an enemy to select it, then tap a card to play it. " +
                  "When you're done, tap \"End Turn\".";
            Panel.SetActive(true);
        }

        private void Dismiss()
        {
            PlayerPrefs.SetInt(SeenKey, 1);
            Panel.SetActive(false);
        }
    }
}
