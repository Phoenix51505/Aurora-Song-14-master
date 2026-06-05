using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Robust.Shared.Serialization;

namespace Content.Shared._AS.Forensics;

[Serializable, NetSerializable]
public sealed partial class WakeScannerDoAfterEvent : SimpleDoAfterEvent
{
}
