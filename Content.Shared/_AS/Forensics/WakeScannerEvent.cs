using Robust.Shared.Serialization;
// All of this is based upon the WakeScannerEvent, which has been trimmed down and configured the new use case.
namespace Content.Shared._AS.Forensics
{
    [Serializable, NetSerializable]
    public sealed class WakeScannerBoundUserInterfaceState : BoundUserInterfaceState
    {
        public readonly string Signatures = string.Empty;
        public readonly string Destinations = string.Empty;
        public readonly string LastScannedName = string.Empty;
        public readonly TimeSpan PrintCooldown = TimeSpan.Zero;
        public readonly TimeSpan PrintReadyAt = TimeSpan.Zero;

        public WakeScannerBoundUserInterfaceState(
            string signatures,
            string destinations,
            string lastScannedName,
            TimeSpan printCooldown,
            TimeSpan printReadyAt)
        {
            Signatures = signatures;
            Destinations = destinations;
            LastScannedName = lastScannedName;
            PrintCooldown = printCooldown;
            PrintReadyAt = printReadyAt;
        }
    }

    [Serializable, NetSerializable]
    public enum WakeScannerUiKey : byte
    {
        Key
    }

    [Serializable, NetSerializable]
    public sealed class WakeScannerPrintMessage : BoundUserInterfaceMessage
    {
    }

    [Serializable, NetSerializable]
    public sealed class WakeScannerClearMessage : BoundUserInterfaceMessage
    {
    }
}
