using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Baraja.Core
{
    // First scene in the build order - a brief branded splash before the
    // main menu: black background, the title, and one of the game's
    // character portraits. The portrait is picked here at RUNTIME (not
    // baked in by the scene builder) so it's actually different every
    // launch, not just every rebuild. Fixed 5s duration, not skippable -
    // that's the spec.
    public class SplashScreen : MonoBehaviour
    {
        [HideInInspector] public RawImage CharacterImage;
        [HideInInspector] public float Duration = 5f;

        // Art/Enemies/<id>.png is the raw character bust with nothing else
        // baked in - Art/EnemyCards is the SAME art already composited
        // into the full ornate card frame (border, name plate, cost
        // circle, medallion), which is why using that one made the splash
        // look like a trading card sitting in a box instead of a clean
        // character portrait. No EN/ES split here either - it's pure art,
        // no text baked in.
        private static readonly string[] CharacterIds =
        {
            "calaca_menor", "alma_en_pena", "perro_xolo",
            "guardian_de_ofrenda", "catrina_menor", "doble_calavera", "la_catrina",
        };

        private void Start()
        {
            if (CharacterImage != null)
            {
                string id = CharacterIds[Random.Range(0, CharacterIds.Length)];
                CharacterImage.texture = Resources.Load<Texture2D>($"Art/Enemies/{id}");
            }

            StartCoroutine(WaitThenLoadMenu());
        }

        private IEnumerator WaitThenLoadMenu()
        {
            yield return new WaitForSeconds(Duration);
            SceneManager.LoadScene("MainMenu");
        }
    }
}
