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
        public string PriceDisplay;   // real-money items: identical in both languages ("$2.99")
        public string PriceEs;        // soft-currency items only: localized, e.g. "150 Pétalos"
        public string PriceEn;        // soft-currency items only: localized, e.g. "150 Petals"
        public int Amount; // currency granted (SoftCurrencyPack) or tokens granted (ReviveToken)
        public int PetalCost; // CardBackSkin only: cost in the soft currency, not real money

        public string DisplayName(bool spanish) => spanish ? NameEs : NameEn;
        public string DisplayDescription(bool spanish) => spanish ? DescriptionEs : DescriptionEn;

        // PriceEs/PriceEn only exist for the petal-cost items - short enough
        // to sit to the left of the Buy gem without colliding with Name/Desc;
        // the old "150 Pétalos / Petals" combined string measured 352px at
        // 30pt, more than 4x the per-language version.
        public string DisplayPrice(bool spanish) =>
            PriceEs != null ? (spanish ? PriceEs : PriceEn) : PriceDisplay;
    }
}
