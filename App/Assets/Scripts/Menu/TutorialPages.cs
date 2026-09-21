using System.Collections.Generic;

namespace Baraja.Menu
{
    public class TutorialPage
    {
        public string TitleEs, TitleEn;
        public string BodyEs, BodyEn;
    }

    // The "How to Play" content, reachable from the main menu. Written against
    // the actual rules in COMBAT.md/CardData/EnemyData/CombatEnums, not generic
    // deckbuilder boilerplate, since this is the thing meant to answer "how do
    // I play this" without anyone having to ask a person.
    public static class TutorialPages
    {
        public static readonly List<TutorialPage> Pages = new List<TutorialPage>
        {
            new TutorialPage
            {
                TitleEs = "El Objetivo", TitleEn = "The Goal",
                BodyEs = "Combate contra 9 enemigos y la jefa final, La Catrina, uno a la vez. " +
                         "Ganas un combate cuando la HP de todos los enemigos llega a 0 antes que la tuya.",
                BodyEn = "Fight through 9 enemies and the boss, La Catrina, one battle at a time. " +
                         "You win a fight by bringing every enemy's HP to 0 before yours reaches 0.",
            },
            new TutorialPage
            {
                TitleEs = "Energía y Mano", TitleEn = "Energy and Hand",
                BodyEs = "Cada turno recibes 3 de Energía y robas 5 cartas. Cada carta cuesta 1 o 2 de Energía. " +
                         "Las cartas sin jugar van al mazo de descarte al terminar el turno; cuando el mazo de robo " +
                         "se vacía, el descarte se baraja de nuevo automáticamente.",
                BodyEn = "Each turn you get 3 Energy and draw 5 cards. Every card costs 1 or 2 Energy to play. " +
                         "Unplayed cards go to your discard pile at end of turn; when your draw pile runs dry, " +
                         "the discard pile reshuffles into a new draw pile automatically.",
            },
            new TutorialPage
            {
                TitleEs = "Tipos de Carta", TitleEn = "Card Types",
                BodyEs = "Ataque: hace daño, necesita un enemigo objetivo. Habilidad: Bloqueo, robo de cartas u " +
                         "otros efectos, casi nunca necesita objetivo. Poder: efecto permanente por el resto del " +
                         "combate, se juega una sola vez y sigue activo.",
                BodyEn = "Attack: deals damage, needs a target enemy. Skill: Block, card draw, or other utility, " +
                         "almost never needs a target. Power: a permanent effect for the rest of the fight - play " +
                         "it once and it keeps working.",
            },
            new TutorialPage
            {
                TitleEs = "Cómo Apuntar", TitleEn = "Targeting",
                BodyEs = "Toca el retrato de un enemigo para seleccionarlo (se resalta en blanco), luego toca una " +
                         "carta de Ataque para golpear a ese objetivo. Las Habilidades y los Poderes no necesitan " +
                         "que selecciones nada.",
                BodyEn = "Tap an enemy's portrait to select it (it highlights white), then tap an Attack card to " +
                         "hit that target. Skills and Powers don't need anything selected.",
            },
            new TutorialPage
            {
                TitleEs = "El Bloqueo", TitleEn = "Block",
                BodyEs = "El Bloqueo absorbe daño antes de que llegue a tu HP. Se pierde por completo al empezar " +
                         "tu siguiente turno, a menos que un Poder como Diamante de Hielo lo renueve cada turno.",
                BodyEn = "Block absorbs damage before it touches your HP. It resets to 0 at the start of your " +
                         "next turn, unless a Power like Ice Diamond keeps refreshing it.",
            },
            new TutorialPage
            {
                TitleEs = "La Intención Enemiga", TitleEn = "Enemy Intent",
                BodyEs = "El texto naranja bajo cada enemigo dice exactamente qué hará en su próximo turno: " +
                         "Ataque, Debilitar, Curarse, Bloquearse o Potenciarse. Nada de lo que hace un enemigo " +
                         "es un secreto - juega alrededor de eso.",
                BodyEn = "The orange text under each enemy tells you exactly what it will do on its next turn: " +
                         "Attack, Weaken, Heal, Block, or Buff. Nothing an enemy does is hidden - plan around it.",
            },
            new TutorialPage
            {
                TitleEs = "Efectos de Estado", TitleEn = "Status Effects",
                BodyEs = "Quemadura: hace daño al final del turno de quien la tiene, luego se reduce a la mitad. " +
                         "Debilitar: reduce el próximo daño que inflige quien lo tiene. Congelar: reduce a la " +
                         "mitad el próximo golpe de quien lo tiene, luego desaparece. Intocable: un ataque falla " +
                         "por completo durante un turno (Tumba Sellada).",
                BodyEn = "Burn: deals damage at the end of the burning creature's turn, then halves. Weaken: " +
                         "reduces the next damage the weakened creature deals. Freeze: halves the next hit that " +
                         "creature lands, then wears off. Untargetable: an attack whiffs completely for one turn " +
                         "(Sealed Grave).",
            },
            new TutorialPage
            {
                TitleEs = "Estructura del Turno", TitleEn = "Turn Structure",
                BodyEs = "Toca \"End Turn\" cuando termines de jugar cartas o te quedes sin Energía. El enemigo " +
                         "resuelve entonces su intención anunciada, se aplican las quemaduras, y el ciclo se " +
                         "repite hasta que alguien caiga.",
                BodyEn = "Tap \"End Turn\" when you're done playing cards or run out of Energy. The enemy then " +
                         "resolves its telegraphed intent, burn damage ticks, and the cycle repeats until someone " +
                         "falls.",
            },
        };
    }
}
