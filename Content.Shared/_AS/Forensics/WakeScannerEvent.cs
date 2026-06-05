using Robust.Shared.Serialization;

namespace Content.Shared._AS.Forensics
{
    [Serializable, NetSerializable]
    public sealed class WakeScannerBoundUserInterfaceState : BoundUserInterfaceState
    {
        public readonly List<string> Fingerprints = new();
        public readonly List<string> Fibers = new();
        public readonly List<string> TouchDNAs = new();
        public readonly List<string> SolutionDNAs = new();
        public readonly List<string> Residues = new();
        public readonly string LastScannedName = string.Empty;
        public readonly TimeSpan PrintCooldown = TimeSpan.Zero;
        public readonly TimeSpan PrintReadyAt = TimeSpan.Zero;

        public WakeScannerBoundUserInterfaceState(
            List<string> fingerprints,
            List<string> fibers,
            List<string> touchDnas,
            List<string> solutionDnas,
            List<string> residues,
            string lastScannedName,
            TimeSpan printCooldown,
            TimeSpan printReadyAt)
        {
            Fingerprints = fingerprints;
            Fibers = fibers;
            TouchDNAs = touchDnas;
            SolutionDNAs = solutionDnas;
            Residues = residues;
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
