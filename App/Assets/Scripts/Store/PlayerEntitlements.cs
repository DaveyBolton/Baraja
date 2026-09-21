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

                case StoreItemKind.CardBackSkin:
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
