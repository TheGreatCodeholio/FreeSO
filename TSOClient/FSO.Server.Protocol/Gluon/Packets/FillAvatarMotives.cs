using FSO.Common.Serialization;
using Mina.Core.Buffer;

namespace FSO.Server.Protocol.Gluon.Packets
{
    /// <summary>
    /// Sent from the admin API to every lot server to top up player motives in
    /// running VMs. Three targeting modes, controlled by the two ID fields:
    ///
    ///   AvatarId == 0, LotId == 0  → every player on every hosted lot
    ///   AvatarId == 0, LotId != 0  → every player on that one lot
    ///   AvatarId != 0, LotId == 0  → that one avatar on whichever lot has them
    ///   AvatarId != 0, LotId != 0  → that one avatar, only if on that lot
    ///
    /// Lot hosts that aren't hosting the requested lot (or the avatar) early-out;
    /// no round-trip to the API is needed. Offline-avatar persistence is handled
    /// by the portal directly (UPDATE fso_avatars.motive_data) and is independent
    /// of this packet.
    /// </summary>
    public class FillAvatarMotives : AbstractGluonPacket
    {
        public uint AvatarId;
        public int LotId;

        public override void Deserialize(IoBuffer input, ISerializationContext context)
        {
            AvatarId = input.GetUInt32();
            LotId = input.GetInt32();
        }

        public override GluonPacketType GetPacketType() => GluonPacketType.FillAvatarMotives;

        public override void Serialize(IoBuffer output, ISerializationContext context)
        {
            output.PutUInt32(AvatarId);
            output.PutInt32(LotId);
        }
    }
}
