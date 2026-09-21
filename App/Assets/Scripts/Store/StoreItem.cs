namespace Baraja.Store
{
    public enum StoreItemKind
    {
        RemoveAds,
        SoftCurrencyPack,   // one-time purchase of "Marigold Petals"
        CardBackSkin,       // cosmetic, unlocked permanently once bought
        ReviveToken,        // consumable: continue once after dying mid-run
    }

    // A single storefront listing. PriceDisplay is a display string only -
    // there is no real payment backend wired up yet (see PlayerEntitlements).
    public class StoreItem
    {
        public string Id;
        public string NameEs;
        public string NameEn;
        public string DescriptionEs;
        public string DescriptionEn;
        public StoreItemKind Kind;
        public string PriceDisplay;
        public int Amount; // currency granted (SoftCurrencyPack) or tokens granted (ReviveToken)
        public int PetalCost; // CardBackSkin only: cost in the soft currency, not real money

        public string DisplayName(bool spanish) => spanish ? NameEs : NameEn;
        public string DisplayDescription(bool spanish) => spanish ? DescriptionEs : DescriptionEn;
    }
}
