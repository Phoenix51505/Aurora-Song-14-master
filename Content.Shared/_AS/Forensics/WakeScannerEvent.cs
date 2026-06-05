using Robust.Shared.Serialization;

namespace Content.Shared._AS.Forensics
{
    [Serializable, NetSerializable]
    public sealed class WakeScannerBoundUserInterfaceState : BoundUserInterfaceState
    {
        public readonly string Signatures = string.Empty;
        public readonly Vector2d Destinations = new();
        public readonly string LastScannedName = string.Empty;
        public readonly TimeSpan PrintCooldown = TimeSpan.Zero;
        public readonly TimeSpan PrintReadyAt = TimeSpan.Zero;

        public WakeScannerBoundUserInterfaceState(
            string signatures,
            Vector2d destinations,
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
