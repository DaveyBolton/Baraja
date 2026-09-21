using System.Collections.Generic;

namespace Baraja.Combat
{
    // Standard draw pile / hand / discard pile with reshuffle-on-empty, per COMBAT.md.
    public class Deck
    {
        public readonly List<CardData> DrawPile = new List<CardData>();
        public readonly List<CardData> Hand = new List<CardData>();
        public readonly List<CardData> DiscardPile = new List<CardData>();

        private readonly System.Random _rng;

        public Deck(IEnumerable<CardData> startingCards, int seed = -1)
        {
            _rng = seed >= 0 ? new System.Random(seed) : new System.Random();
            DrawPile.AddRange(startingCards);
            Shuffle(DrawPile);
        }

        private void Shuffle(List<CardData> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        // Reshuffles discard into draw pile when draw pile runs dry mid-draw.
        private void ReshuffleIfEmpty()
        {
            if (DrawPile.Count > 0) return;
            DrawPile.AddRange(DiscardPile);
            DiscardPile.Clear();
            Shuffle(DrawPile);
        }

        public void DrawToHand(int count)
        {
            for (int i = 0; i < count; i++)
            {
                ReshuffleIfEmpty();
                if (DrawPile.Count == 0) break; // exhausted both piles, nothing left to draw
                int last = DrawPile.Count - 1;
                Hand.Add(DrawPile[last]);
                DrawPile.RemoveAt(last);
            }
        }

        public void DiscardHand()
        {
            DiscardPile.AddRange(Hand);
            Hand.Clear();
        }

        public void PlayFromHand(CardData card)
        {
            Hand.Remove(card);
            DiscardPile.Add(card);
        }
    }
}
