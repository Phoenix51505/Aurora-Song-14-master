
namespace Content.Server._AS.Forensics
{
    [RegisterComponent]
    public sealed partial class AreaWakeScannerComponent : Component
    {
        /// <summary>
        /// When will the scanner be ready again?
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public TimeSpan ScanReadyAt = TimeSpan.Zero;

        /// <summary>
        /// The cooldown between scans
        /// </summary>
        [DataField]
        public TimeSpan PrintReadyAt = TimeSpan.Zero;
    }
}
