using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace CivMods
{
    class BlockEntitySnitch : BlockEntity
    {
        public List<string> Breakins = new List<string>();
        public bool cooldown = true;
        public int limit = 512;

        public string OwnerUID { get; set; }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if (api.Side.IsServer())
            {
                RegisterGameTickListener(dt =>
                {
                    SimpleParticleProperties props = Pos.DownCopy().TemporalEffectAtPos(api);
                    props.MinPos.Add(0, 0.5, 0);
                    api.World.SpawnParticles(props);
                    List<IPlayer> intruders = new List<IPlayer>();

                    if (cooldown && api.World.GetPlayersAround(Pos.ToVec3d(), 13, 13).Any(e => {
                        if (e.PlayerUID == OwnerUID || OwnerUID == null || OwnerUID == "") return false;

                        intruders.Add(e);
                        return true;
                    }))
                    {
                        LimitCheck();
                        cooldown = false;
                        foreach (var val in intruders)
                        {
                            Breakins.Add(val.PlayerName + " is inside the radius of " + Pos.RelativeToSpawn(api.World).ToVec3i() + " at " + val.Entity.LocalPos.XYZInt.ToBlockPos().RelativeToSpawn(api.World));
                            MarkDirty();
                        }
                        RegisterDelayedCallback(dt2 => cooldown = true, 5000);
                    }
                }, 30);
            }
        }

        public void NotifyOfBreak(IServerPlayer byPlayer, int oldblockId, BlockPos pos)
        {
            if (byPlayer.PlayerUID == OwnerUID) return;
            LimitCheck();
            Breakins.Add(byPlayer.PlayerName + " broke or tried to break a block at " + pos.RelativeToSpawn(byPlayer.Entity.World) + " with the name of " + Api.World.GetBlock(oldblockId).Code);
            MarkDirty();
        }

        public void LimitCheck()
        {
            if (Breakins.Count >= limit) Breakins.RemoveAt(0);
        }

        public override void FromTreeAtributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            OwnerUID = tree.GetString("owner");
            for (int i = 0; i < limit; i++)
            {
                string str = tree.GetString("breakins" + i);
                if (str != null) Breakins.Add(str);
            }
            base.FromTreeAtributes(tree, worldAccessForResolve);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            tree.SetString("owner", OwnerUID);
            for (int i = 0; i < limit; i++)
            {
                if (i >= Breakins.Count) continue;

                tree.SetString("breakins" + i, Breakins[i]);
            }
            base.ToTreeAttributes(tree);
        }
    }
}