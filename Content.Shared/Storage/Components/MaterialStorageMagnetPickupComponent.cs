namespace Content.Server.Storage.Components;

/// <summary>
/// Applies an ongoing pickup area around the attached entity.
/// </summary>
[RegisterComponent]
public sealed partial class MaterialStorageMagnetPickupComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("nextScan")]
    public TimeSpan NextScan = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadWrite), DataField("range")]
    public float Range = 1f;

    /// <summary>
    /// Frontier - Is the magnet currently enabled?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("magnetEnabled")]
    public bool MagnetEnabled = false;

    // Begin Aurora's Song
    /// <summary>
    /// How many idle tallies has this magnet accrued
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public int IdleTallies = 0;

    /// <summary>
    /// How many idle tallies are needed before the magnet automatically shuts off
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public int IdleTallyThreshold = 10; // Equivalent to 10 seconds without picking anything up
    // End Aurora's Song
}
