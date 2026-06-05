using System.Linq;
using System.Text;
using Content.Server.Popups;
using Content.Shared.UserInterface;
using Content.Shared.DoAfter;
using Content.Shared.Forensics;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Paper;
using Content.Shared.Verbs;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Timing;
using Content.Server.Chemistry.Containers.EntitySystems;
using Robust.Shared.Prototypes;
using Content.Shared._NF.SectorServices; // Frontier
using Content.Server._NF.Smuggling; // Frontier
using Content.Server._NF.Smuggling.Components; // Frontier
using Content.Server.Radio.EntitySystems; // Frontier
using Content.Server.Stack; // Frontier
using Content.Shared._NF.Bank; // Frontier
using Content.Shared._NF.Bank.Components; // Frontier
using Content.Server._NF.Bank; // Frontier
using Content.Shared._NF.Bank.BUI; // Frontier
using Content.Shared._NF.CCVar; // Frontier
using Content.Shared.Containers.ItemSlots; // Frontier
using Content.Shared.FixedPoint; // Frontier
using Content.Shared.Stacks; // Frontier
using Content.Shared.Radio; // Frontier
using Robust.Shared.Configuration; // Frontier
using Content.Server._AS.Shuttles.FTLWake;
using Content.Shared._AS.Shuttles.Components;
using Content.Server.Shuttles.Components;
using Robust.Shared.Random;
using Robust.Shared.Map;
using Content.Shared._AS.Forensics;
// todo: remove this stinky LINQy

namespace Content.Server._AS.Forensics
{
    public sealed class ForensicScannerSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly PaperSystem _paperSystem = default!;
        [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
        [Dependency] private readonly MetaDataSystem _metaData = default!;
        [Dependency] private readonly TagSystem _tag = default!;
        [Dependency] private readonly StackSystem _stackSystem = default!; // Frontier
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!; // Frontier
        [Dependency] private readonly RadioSystem _radio = default!; // Frontier
        [Dependency] private readonly DeadDropSystem _deadDrop = default!; // Frontier
        [Dependency] private readonly ItemSlotsSystem _itemSlots = default!; // Frontier
        [Dependency] private readonly SectorServiceSystem _service = default!; // Frontier
        [Dependency] private readonly IConfigurationManager _cfg = default!; // Frontier
        [Dependency] private readonly BankSystem _bank = default!; // Frontier
        [Dependency] private readonly IRobustRandom _random = default!;

        // Frontier: payout constants
        // Temporary values, sane defaults, will be overwritten by CVARs.
        private int _minFUCPayout = 2;

        private SoundSpecifier _confirmSound = new SoundPathSpecifier("/Audio/Effects/Cargo/ping.ogg");

        private const int ActiveUnusedDeadDropSpesoReward = 20000;
        private const float ActiveUnusedDeadDropFUCReward = 2.0f;
        private const int ActiveUsedDeadDropSpesoReward = 10000;
        private const float ActiveUsedDeadDropFUCReward = 1.0f;
        private const int InactiveUsedDeadDropSpesoReward = 5000;
        private const float InactiveUsedDeadDropFUCReward = 0.5f;
        private const int DropPodSpesoReward = 10000;
        private const float DropPodFUCReward = 1.0f;
        // End Frontier: payout constants

        private static readonly ProtoId<TagPrototype> DNASolutionScannableTag = "DNASolutionScannable";

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<WakeScannerComponent, AfterInteractEvent>(OnAfterInteract);
            SubscribeLocalEvent<WakeScannerComponent, BeforeActivatableUIOpenEvent>(OnBeforeActivatableUIOpen);
            SubscribeLocalEvent<WakeScannerComponent, GetVerbsEvent<UtilityVerb>>(OnUtilityVerb);
            SubscribeLocalEvent<WakeScannerComponent, WakeScannerPrintMessage>(OnPrint);
            SubscribeLocalEvent<WakeScannerComponent, WakeScannerClearMessage>(OnClear);
            SubscribeLocalEvent<WakeScannerComponent, WakeScannerDoAfterEvent>(OnDoAfter);

        }


        private void UpdateUserInterface(EntityUid uid, WakeScannerComponent component)
        {
            var state = new WakeScannerBoundUserInterfaceState(
                component.Signatures,
                component.Coordinates,
                component.LastScannedName,
                component.PrintCooldown,
                component.PrintReadyAt);

            _uiSystem.SetUiState(uid, WakeScannerUiKey.Key, state);
        }

        private void OnDoAfter(EntityUid uid, WakeScannerComponent component, DoAfterEvent args)
        {
            if (args.Handled || args.Cancelled)
                return;

            if (!TryComp(uid, out WakeScannerComponent? scanner))
                return;

            if (args.Args.Target is { } target)
            {
                if (TryComp<FTLWakeComponent>(target, out var wake))
                {
                    scanner.Signatures = wake.Signature; // Todo: add some kind of distortion to the signature based on age.
                    var error = (float)((_gameTiming.CurTime - wake.Age) / wake.LifeSpan * 1000); // Every minute of age adds 50m to the possible error range
                    wake.Destination.Deconstruct(out _, out var coordinates);
                    scanner.Coordinates = coordinates + _random.NextVector2(error);
                }
                else if (TryComp<ThrusterComponent>(args.Args.Target, out var _))
                {
                    scanner.Signatures = string.Empty;
                    scanner.Coordinates = new();
                    if (TryComp<EngineSignatureComponent>(Transform(target).GridUid, out var signature))
                        scanner.Signatures = signature.Signature;
                }
                else
                {
                    scanner.Signatures = string.Empty;
                    scanner.Coordinates = new();
                }

            }

            OpenUserInterface(args.Args.User, (uid, scanner));
        }

        /// <remarks>
        /// Hosts logic common between OnUtilityVerb and OnAfterInteract.
        /// </remarks>
        private void StartScan(EntityUid uid, WakeScannerComponent component, EntityUid user, EntityUid target)
        {
            _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, user, component.ScanDelay, new WakeScannerDoAfterEvent(), uid, target: target, used: uid)
            {
                BreakOnMove = true,
                NeedHand = true
            });
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

            text.AppendLine(Loc.GetString("forensic-scanner-interface-fingerprints"));
            text.AppendLine(component.Signatures);
            text.AppendLine();
            text.AppendLine(Loc.GetString("forensic-scanner-interface-fibers"));
            text.AppendLine(component.Coordinates.ToString());

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
            component.Coordinates = new();

            UpdateUserInterface(uid, component);
        }
    }
}
