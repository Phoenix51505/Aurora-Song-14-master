using System.Text;
using Content.Server._AS.Shuttles.FTLWake;
using Content.Server.Popups;
using Content.Server.Shuttles.Components;
using Content.Shared._AS.Forensics;
using Content.Shared._AS.Shuttles.Components;
using Content.Shared.UserInterface;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Paper;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Timing;
using Robust.Shared.Random;
using Content.Shared._NF.Radar;

namespace Content.Server._AS.Forensics
{
    public sealed class AreaWakeScannerSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly PaperSystem _paperSystem = default!;
        [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
        [Dependency] private readonly MetaDataSystem _metaData = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        private EntityQuery<TransformComponent> _xformQuery;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AreaWakeScannerComponent, GetVerbsEvent<ActivationVerb>>(AddPulseActivationVerb);
            SubscribeLocalEvent<AreaWakeScannerComponent, GetVerbsEvent<InteractionVerb>>(AddToggleInteractionVerb);

        }

        private void StartScan(EntityUid uid, AreaWakeScannerComponent component)
        {
            if (component.PulseReadyAt > _gameTiming.CurTime) // Desynced Verb Window Prevention
                return;

            if (!_xformQuery.TryGetComponent(uid, out var xform))
                return;


            var query = EntityQueryEnumerator<FTLWakeComponent>();

            var printed = false;
            while (query.MoveNext(out var quid, out var wake))
            {
                if (!_xformQuery.TryGetComponent(quid, out var wakeXform))
                    continue;

                if (!xform.Coordinates.TryDistance(EntityManager, wakeXform.Coordinates, out var distance) ||
                distance > component.Range)
                    continue;

                if (component.Revealing)
                {
                    var blip = AddComp<RadarBlipComponent>(quid);
                    blip.RadarColor = Color.Gray;
                    blip.HighlightedRadarColor = Color.White;
                }
                else
                {
                    PrintReport(uid, wake, quid, component);
                    printed = true;
                }
            }

            if (printed == true)
            {
                _audioSystem.PlayPvs(component.SoundPrint, uid,
                AudioParams.Default
                .WithVariation(0.25f)
                .WithVolume(3f)
                .WithRolloffFactor(2.8f)
                .WithMaxDistance(4.5f));
            }
            component.PulseReadyAt = _gameTiming.CurTime + component.PulseCooldown;
        }

        private void PrintReport(EntityUid uid, FTLWakeComponent wakeComp, EntityUid wakeUID, AreaWakeScannerComponent scanComp)
        {
            var signature = wakeComp.Signature; // Todo: add some kind of distortion to the signature based on age.

            var error = (float)((_gameTiming.CurTime - wakeComp.Age) / wakeComp.LifeSpan * 1000); // Every minute of age adds 50m to the possible error range
            wakeComp.Destination.Deconstruct(out _, out var coordinates);

            var printCoordinates = (coordinates + _random.NextVector2(error)).ToString();

            var printed = Spawn(scanComp.MachineOutput, Transform(uid).Coordinates);

            if (!TryComp<PaperComponent>(printed, out var paperComp))
            {
                Log.Error("Printed paper did not have PaperComponent.");
                return;
            }

            var entityName = MetaData(wakeUID).EntityName;
            _metaData.SetEntityName(printed, Loc.GetString("forensic-scanner-report-title", ("entity", entityName)));

            var text = new StringBuilder();

            text.AppendLine(Loc.GetString("wake-scanner-interface-signatures"));
            text.AppendLine(signature);
            text.AppendLine();
            text.AppendLine(Loc.GetString("wake-scanner-interface-destination"));
            text.AppendLine(printCoordinates);

            _paperSystem.SetContent((printed, paperComp), text.ToString());

        }

        private void ToggleState(EntityUid uid, AreaWakeScannerComponent component)
        {
            component.Revealing = !component.Revealing;
        }


        // TODO: Make this entire thing use an actual UI and not right 
        // TODO: Add examine text.
        private void AddPulseActivationVerb(EntityUid uid, AreaWakeScannerComponent component, GetVerbsEvent<ActivationVerb> args)
        {
            if (!args.CanInteract || !args.CanAccess || component.PulseReadyAt > _gameTiming.CurTime)
                return;

            var verb = new ActivationVerb()
            {
                Act = () => StartScan(uid, component),
                IconEntity = GetNetEntity(uid),
                Text = Loc.GetString("area-wake-scanner-pulse-verb-text"),
                Message = Loc.GetString("area-wake-scanner-pulse-verb-message")
            };

            args.Verbs.Add(verb);
        }
        private void AddToggleInteractionVerb(EntityUid uid, AreaWakeScannerComponent component, GetVerbsEvent<InteractionVerb> args)
        {
            if (!args.CanInteract || !args.CanAccess)
                return;

            var verb = new InteractionVerb()
            {
                Act = () => ToggleState(uid, component),
                IconEntity = GetNetEntity(uid),
                Text = component.Revealing ? Loc.GetString("area-wake-scanner-analyze-verb-text") : Loc.GetString("area-wake-scanner-reveal-verb-text"),
                Message = component.Revealing ? Loc.GetString("area-wake-scanner-analyze-verb-message") : Loc.GetString("area-wake-scanner-reveal-verb-message")
            };

            args.Verbs.Add(verb);
        }

    }
}
