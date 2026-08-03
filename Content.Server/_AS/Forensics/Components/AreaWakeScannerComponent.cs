using System.Threading;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
namespace Content.Server._AS.Forensics
{
    [RegisterComponent]
    public sealed partial class AreaWakeScannerComponent : Component
    {
        /// <summary>
        /// When will the scanner be ready again?
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public TimeSpan PulseReadyAt = TimeSpan.Zero;

        /// <summary>
        /// The cooldown between scans
        /// </summary>
        [DataField]
        public TimeSpan PulseCooldown = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Whether the area scanner is Revealing rather than Analyzing
        /// </summary>
        [DataField]
        public bool Revealing = false;

        /// <summary>
        /// The range of the scanners pulses, in metres
        /// </summary>
        [DataField]
        public float Range = 1024; // Just a bit longer than a large radar

        /// <summary>
        /// The sound that's played when the scanner prints off a report.
        /// </summary>
        [DataField("soundPrint")]
        public SoundSpecifier SoundPrint = new SoundPathSpecifier("/Audio/Machines/short_print_and_rip.ogg");

        /// <summary>
        /// What the machine will print
        /// </summary>
        [DataField("machineOutput", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string MachineOutput = "ForensicReportPaper";

    }
}
