using UnityEngine;

namespace Baraja.Store
{
    public enum PurchaseResult { Success, AlreadyOwned, InsufficientCurrency }

    // Local-only mock of what a real store SDK would grant. Every "purchase"
    // here just flips a PlayerPrefs flag or adds to a local balance - there is
    // no real payment processing wired in. Wiring actual money (Google Play
    // Billing / Apple StoreKit) is a separate task that needs store accounts
    // and platform SDKs neither of which this pass touches. This exists so
    // the Store UI has real state to read and write against.
    public static class PlayerEntitlements
    {
        private const string AdsRemovedKey = "baraja_ads_removed";
        private const string PetalBalanceKey = "baraja_petal_balance";
        private const string ReviveTokensKey = "baraja_revive_tokens";
        private const string OwnedPrefix = "baraja_owned_";
        private const string EquippedFrameKey = "baraja_equipped_frame";
        private const string DefaultFrame = "gold"; // always owned, never purchasable

        public static bool AdsRemoved
        {
            get => PlayerPrefs.GetInt(AdsRemovedKey, 0) != 0;
            private set => PlayerPrefs.SetInt(AdsRemovedKey, value ? 1 : 0);
        }

        public static int PetalBalance
        {
            get => PlayerPrefs.GetInt(PetalBalanceKey, 0);
            private set => PlayerPrefs.SetInt(PetalBalanceKey, value);
        }

        public static int ReviveTokens
        {
            get => PlayerPrefs.GetInt(ReviveTokensKey, 0);
            private set => PlayerPrefs.SetInt(ReviveTokensKey, value);
        }

        public static bool IsOwned(string itemId) => PlayerPrefs.GetInt(OwnedPrefix + itemId, 0) != 0;

        // Which alternate-frame player deck combat currently loads cards
        // from (Resources/Art/Cards/<frame>/{EN,ES}/<cardId>). Defaults to
        // the free gold deck, which is not itself a purchasable StoreItem
        // instance you "own" via OwnedPrefix - it's just always available.
        public static string EquippedFrame
        {
            get => PlayerPrefs.GetString(EquippedFrameKey, DefaultFrame);
            private set => PlayerPrefs.SetString(EquippedFrameKey, value);
        }

        public static bool OwnsFrame(string frameId)
        {
            if (frameId == DefaultFrame) return true;
            var item = StoreDatabase.All.Find(i => i.Kind == StoreItemKind.CardFrame && i.FrameId == frameId);
            return item != null && IsOwned(item.Id);
        }

        // Returns false without changing anything if frameId isn't owned yet
        // (the UI only ever calls this from an already-owned row, but this
        // guards the entry point itself rather than trusting the caller).
        public static bool EquipFrame(string frameId)
        {
            if (!OwnsFrame(frameId)) return false;
            EquippedFrame = frameId;
            return true;
        }

        public static PurchaseResult Buy(StoreItem item)
        {
            switch (item.Kind)
            {
                case StoreItemKind.RemoveAds:
                    if (AdsRemoved) return PurchaseResult.AlreadyOwned;
                    AdsRemoved = true;
                    return PurchaseResult.Success;

                case StoreItemKind.SoftCurrencyPack:
                    PetalBalance += item.Amount;
                    return PurchaseResult.Success;

                case StoreItemKind.ReviveToken:
                    ReviveTokens += item.Amount;
                    return PurchaseResult.Success;

                case StoreItemKind.CardFrame:
                    if (IsOwned(item.Id)) return PurchaseResult.AlreadyOwned;
                    if (PetalBalance < item.PetalCost) return PurchaseResult.InsufficientCurrency;
                    PetalBalance -= item.PetalCost;
                    PlayerPrefs.SetInt(OwnedPrefix + item.Id, 1);
                    return PurchaseResult.Success;

                default:
                    return PurchaseResult.InsufficientCurrency;
            }
        }
    }
}
