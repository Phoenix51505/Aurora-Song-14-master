using Robust.Shared.Map;

namespace Content.Server._AS.Shuttles.FTLWake
{
    [RegisterComponent]
    public sealed partial class FTLWakeComponent : Component
    {
        /// <summary>
        /// The 'fingerprint' of the vessel that left this wake
        /// </summary>
        [DataField]
        public string? Signature;

        /// <summary>
        /// The FTL Destiniation of the ship that left this wake
        /// </summary>
        [DataField]
        public EntityCoordinates? Destination;

        /// <summary>
        /// How old this wake is. Error is calculated based on a ratio of Age to LifeSpan.
        /// </summary>
        [DataField]
        public TimeSpan Age = TimeSpan.Zero;

        /// <summary>
        /// How long this wake will last for. Error is calculated based on a ratio of Age to LifeSpan.
        /// </summary>
        [DataField]
        public TimeSpan LifeSpan = TimeSpan.FromMinutes(20);
    }
}