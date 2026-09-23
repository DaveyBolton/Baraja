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
                Id = "frame_gold", Kind = StoreItemKind.CardFrame, FrameId = "gold", PetalCost = 0,
                PriceEs = "Incluido", PriceEn = "Included",
                NameEs = "Marco Dorado", NameEn = "Gold Frame",
                DescriptionEs = "El marco clásico. Incluido gratis.",
                DescriptionEn = "The classic frame. Included for free.",
            },
            new StoreItem
            {
                Id = "frame_rainbow", Kind = StoreItemKind.CardFrame, FrameId = "rainbow", PetalCost = 150,
                PriceEs = "150 Pétalos", PriceEn = "150 Petals",
                NameEs = "Marco Arcoíris", NameEn = "Rainbow Frame",
                DescriptionEs = "Baraja completa con marco arcoíris.",
                DescriptionEn = "A whole alternate deck with a rainbow frame.",
            },
            new StoreItem
            {
                Id = "frame_silver", Kind = StoreItemKind.CardFrame, FrameId = "silver", PetalCost = 150,
                PriceEs = "150 Pétalos", PriceEn = "150 Petals",
                NameEs = "Marco Plata Brillante", NameEn = "Bright Silver Frame",
                DescriptionEs = "Baraja completa con marco de plata brillante.",
                DescriptionEn = "A whole alternate deck with a bright silver frame.",
            },
            new StoreItem
            {
                Id = "frame_red", Kind = StoreItemKind.CardFrame, FrameId = "red", PetalCost = 150,
                PriceEs = "150 Pétalos", PriceEn = "150 Petals",
                NameEs = "Marco Rubí", NameEn = "Ruby Frame",
                DescriptionEs = "Baraja completa con marco rubí.",
                DescriptionEn = "A whole alternate deck with a ruby-red frame.",
            },
        };

        public static StoreItem ById(string id)
        {
            foreach (var item in All) if (item.Id == id) return item;
            return null;
        }
    }
}
