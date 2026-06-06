using Content.Shared._AS.Shuttles.Components;
using Content.Server.Forensics;

namespace Content.Server._AS.Shuttles.Systems;

public sealed class EngineSignatureSystem : EntitySystem
{

    [Dependency] private readonly ForensicsSystem _forensics = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EngineSignatureComponent, ComponentStartup>(OnComponentStartup);
    }

    private void OnComponentStartup(EntityUid uid, EngineSignatureComponent component, ComponentStartup args)
    {
        component.Signature = _forensics.GenerateFingerprint(15);
    }

    private void RandomizeSignature(EntityUid uid)
    {
        if (!TryComp<EngineSignatureComponent>(uid, out var component))
            return;
        component.Signature = _forensics.GenerateFingerprint(15);
    }
}