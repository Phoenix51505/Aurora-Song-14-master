using Content.Shared.Inventory;
using Robust.Shared.GameStates;

namespace Content.Shared.Storage.Components;

/// <summary>
/// Applies an ongoing pickup area around the attached entity.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
[AutoGenerateComponentPause]
public sealed partial class MagnetPickupComponent : Component
{
    [DataField]
    [AutoPausedField]
    [AutoNetworkedField]
    public TimeSpan NextScan = TimeSpan.Zero;

    /// <summary>
    /// What container slot the magnet needs to be in to work.
    /// </summary>
    [DataField]
    public SlotFlags? SlotFlags = Inventory.SlotFlags.BELT;

    [DataField]
    public bool RequireActiveHand = false;

    [DataField]
    public float Range = 1f;

    // Frontier: togglable magnets
    /// <summary>
    /// Is the magnet currently enabled?
    /// </summary>
    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite), DataField]
    public bool MagnetEnabled = true;

    /// <summary>
    /// Is the magnet currently enabled?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public bool MagnetCanBeEnabled = true;

    /// <summary>
    /// Is the magnet currently enabled?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public int MagnetTogglePriority = 3;
    // End Frontier: togglable magnets

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
