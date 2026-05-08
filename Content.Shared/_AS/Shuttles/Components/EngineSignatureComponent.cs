using Robust.Shared.GameStates;

namespace Content.Shared._AS.Shuttles.Components;

/// <summary>
/// This component is for vessels with engine signatures
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EngineSignatureComponent : Component
{
    [DataField, AutoNetworkedField]
    public string? Signature;
}
