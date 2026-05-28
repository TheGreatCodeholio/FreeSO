using System.IO;
using System.Linq;
using FSO.SimAntics.Entities;
using FSO.SimAntics.Model;

namespace FSO.SimAntics.NetPlay.Model.Commands
{
    /// <summary>
    /// Server-only command that tops up the seven decaying motives
    /// (Energy / Comfort / Hunger / Hygiene / Bladder / Fun / Social) for player
    /// avatars on a lot. Mirrors the in-game /maxmotives chat command exactly
    /// (NPCs and pets are skipped — touching their motives breaks service-lot AI)
    /// and uses the same TuningCache.GetLimit() ceiling so per-category overfill
    /// is respected.
    ///
    /// Issued by the lot host in response to a FillAvatarMotives Gluon packet from
    /// the admin API. Refuses any client-forged copy (Verify returns !FromNet) so a
    /// player cannot grant themselves free motive fills.
    ///
    /// If PersistID == 0 every eligible player on the lot is filled; otherwise just
    /// the single avatar with that PersistID (no-op if they're not on this lot).
    /// </summary>
    public class VMNetFillMotivesCmd : VMNetCommandBodyAbstract
    {
        public uint PersistID;

        private static readonly VMMotive[] FillMotiveList = new VMMotive[]
        {
            VMMotive.Energy, VMMotive.Comfort, VMMotive.Hunger,
            VMMotive.Hygiene, VMMotive.Bladder, VMMotive.Fun, VMMotive.Social
        };

        public override bool Execute(VM vm)
        {
            var players = vm.Context.ObjectQueries.Avatars
                .OfType<VMAvatar>()
                .Where(a => a.PersistID > 0 && !a.IsPet);

            if (PersistID != 0)
                players = players.Where(a => a.PersistID == PersistID);

            int filled = 0;
            foreach (var p in players)
            {
                foreach (var m in FillMotiveList)
                    p.SetMotiveData(m, vm.TuningCache.GetLimit(m));
                filled++;
            }

            if (filled > 0)
            {
                var label = (PersistID != 0)
                    ? "an admin"
                    : $"{filled} player(s)";
                vm.SignalChatEvent(new VMChatEvent(null, VMChatEventType.Generic,
                    $"Motives topped up for {label}."));
            }
            return true;
        }

        public override bool Verify(VM vm, VMAvatar caller) => !FromNet;

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            PersistID = reader.ReadUInt32();
        }

        public override void SerializeInto(BinaryWriter writer)
        {
            base.SerializeInto(writer);
            writer.Write(PersistID);
        }
    }
}
