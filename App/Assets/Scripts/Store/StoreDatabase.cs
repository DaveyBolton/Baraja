using System.Collections.Generic;

namespace Baraja.Store
{
    // What games in this genre actually sell (Slay the Spire-likes and their
    // free-to-play mobile clones): remove ads, a soft-currency sink for
    // cosmetics, cosmetic card backs, and a pay-to-continue consumable. No
    // gameplay-affecting purchases (no power creep for sale) - see
    // PlayerEntitlements for why none of this is wired to a real payment
    // backend yet.
    public static class StoreDatabase
    {
        public static readonly List<StoreItem> All = new List<StoreItem>
        {
            new StoreItem
            {
                Id = "remove_ads", Kind = StoreItemKind.RemoveAds, PriceDisplay = "$2.99",
                NameEs = "Quitar Anuncios", NameEn = "Remove Ads",
                DescriptionEs = "Elimina todos los anuncios para siempre.",
                DescriptionEn = "Turns off all ads, forever.",
            },
            new StoreItem
            {
                Id = "petals_small", Kind = StoreItemKind.SoftCurrencyPack, PriceDisplay = "$0.99", Amount = 100,
                NameEs = "Bolsa de Pétalos Pequeña", NameEn = "Small Petal Pouch",
                DescriptionEs = "100 Pétalos de Cempasúchil.",
                DescriptionEn = "100 Marigold Petals.",
            },
            new StoreItem
            {
                Id = "petals_medium", Kind = StoreItemKind.SoftCurrencyPack, PriceDisplay = "$4.99", Amount = 600,
                NameEs = "Bolsa de Pétalos Mediana", NameEn = "Medium Petal Pouch",
                DescriptionEs = "600 Pétalos de Cempasúchil.",
                DescriptionEn = "600 Marigold Petals.",
            },
            new StoreItem
            {
                Id = "petals_large", Kind = StoreItemKind.SoftCurrencyPack, PriceDisplay = "$9.99", Amount = 1400,
                NameEs = "Bolsa de Pétalos Grande", NameEn = "Large Petal Pouch",
                DescriptionEs = "1400 Pétalos de Cempasúchil.",
                DescriptionEn = "1400 Marigold Petals.",
            },
            new StoreItem
            {
                Id = "revive_token", Kind = StoreItemKind.ReviveToken, PriceDisplay = "$1.99", Amount = 1,
                NameEs = "Bendición de Catrina", NameEn = "Catrina's Blessing",
                DescriptionEs = "Continúa una vez tras caer en combate.",
                DescriptionEn = "Continue once after falling in combat.",
            },
            new StoreItem
            {
                Id = "cardback_rainbow", Kind = StoreItemKind.CardBackSkin, PriceDisplay = "150 Pétalos / Petals", PetalCost = 150,
                NameEs = "Reverso Catrina Arcoíris", NameEn = "Rainbow Catrina Back",
                DescriptionEs = "Reverso de carta cosmético.",
                DescriptionEn = "Cosmetic card back.",
            },
            new StoreItem
            {
                Id = "cardback_gold", Kind = StoreItemKind.CardBackSkin, PriceDisplay = "150 Pétalos / Petals", PetalCost = 150,
                NameEs = "Reverso Reliquia Dorada", NameEn = "Golden Relic Back",
                DescriptionEs = "Reverso de carta cosmético.",
                DescriptionEn = "Cosmetic card back.",
            },
            new StoreItem
            {
                Id = "cardback_silver", Kind = StoreItemKind.CardBackSkin, PriceDisplay = "150 Pétalos / Petals", PetalCost = 150,
                NameEs = "Reverso Plata Mortal", NameEn = "Silver Death Back",
                DescriptionEs = "Reverso de carta cosmético.",
                DescriptionEn = "Cosmetic card back.",
            },
        };

        public static StoreItem ById(string id)
        {
            foreach (var item in All) if (item.Id == id) return item;
            return null;
        }
    }
}
