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

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AreaWakeScannerComponent, AfterInteractEvent>(OnAfterInteract);
            SubscribeLocalEvent<AreaWakeScannerComponent, GetVerbsEvent<ActivationVerb>>(AddPulseActivationVerb);
            SubscribeLocalEvent<AreaWakeScannerComponent, GetVerbsEvent<InteractionVerb>>(AddToggleInteractionVerb);

        }

        /// <remarks>
        /// Hosts logic common between OnUtilityVerb and OnAfterInteract.
        /// </remarks>
        private void StartScan(EntityUid uid, WakeScannerComponent component, EntityUid user, EntityUid target)
        {
        }

        private void OnUtilityVerb(EntityUid uid, WakeScannerComponent component, GetVerbsEvent<UtilityVerb> args)
        {
            if (!args.CanInteract || !args.CanAccess || component.CancelToken != null)
                return;

            var verb = new UtilityVerb()
            {
                Act = () => StartScan(uid, component, args.User, args.Target),
                IconEntity = GetNetEntity(uid),
                Text = Loc.GetString("forensic-scanner-verb-text"),
                Message = Loc.GetString("forensic-scanner-verb-message"),
                // This is important because if its true using the scanner will count as touching the object.
                DoContactInteraction = false
            };

            args.Verbs.Add(verb);
        }

        private void OnAfterInteract(EntityUid uid, WakeScannerComponent component, AfterInteractEvent args)
        {
            if (component.CancelToken != null || args.Target == null || !args.CanReach)
                return;

            StartScan(uid, component, args.User, args.Target.Value);
        }


        private void OnBeforeActivatableUIOpen(EntityUid uid, WakeScannerComponent component, BeforeActivatableUIOpenEvent args)
        {
            UpdateUserInterface(uid, component);
        }

        private void OpenUserInterface(EntityUid user, Entity<WakeScannerComponent> scanner)
        {
            UpdateUserInterface(scanner, scanner.Comp);

            _uiSystem.OpenUi(scanner.Owner, WakeScannerUiKey.Key, user);
        }

        private void OnPrint(EntityUid uid, WakeScannerComponent component, WakeScannerPrintMessage args)
        {
            var user = args.Actor;

            if (_gameTiming.CurTime < component.PrintReadyAt)
            {
                // This shouldn't occur due to the UI guarding against it, but
                // if it does, tell the user why nothing happened.
                _popupSystem.PopupEntity(Loc.GetString("forensic-scanner-printer-not-ready"), uid, user);
                return;
            }

            // Spawn a piece of paper.
            var printed = Spawn(component.MachineOutput, Transform(uid).Coordinates);
            _handsSystem.PickupOrDrop(args.Actor, printed, checkActionBlocker: false);

            if (!TryComp<PaperComponent>(printed, out var paperComp))
            {
                Log.Error("Printed paper did not have PaperComponent.");
                return;
            }

            _metaData.SetEntityName(printed, Loc.GetString("forensic-scanner-report-title", ("entity", component.LastScannedName)));

            var text = new StringBuilder();

            text.AppendLine(Loc.GetString("wake-scanner-interface-signatures"));
            text.AppendLine(component.Signatures);
            text.AppendLine();
            text.AppendLine(Loc.GetString("wake-scanner-interface-destination"));
            text.AppendLine(component.Coordinates);

            _paperSystem.SetContent((printed, paperComp), text.ToString());
            _audioSystem.PlayPvs(component.SoundPrint, uid,
                AudioParams.Default
                .WithVariation(0.25f)
                .WithVolume(3f)
                .WithRolloffFactor(2.8f)
                .WithMaxDistance(4.5f));

            component.PrintReadyAt = _gameTiming.CurTime + component.PrintCooldown;
        }

        private void OnClear(EntityUid uid, WakeScannerComponent component, WakeScannerClearMessage args)
        {
            component.Signatures = string.Empty;
            component.Coordinates = string.Empty;

            UpdateUserInterface(uid, component);
        }
    }
}
