using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace CivMods
{
    class BlockSnitch : Block
    {
        public override void OnBlockPlaced(IWorldAccessor world, BlockPos pos, ItemStack byItemStack = null)
        {
            if (world.GetBlockEntitiesAround(pos, new Vec2i(11, 11)).Any(e => (e is BlockEntitySnitch)))
            {
                world.RegisterCallback(dt => world.BlockAccessor.BreakBlock(pos, null), 500);
            }
            base.OnBlockPlaced(world, pos, byItemStack);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BlockEntitySnitch be = (blockSel?.Position?.BlockEntity(world) as BlockEntitySnitch);
            if (be != null && (be.OwnerUID == null || be.OwnerUID == "") && world.Side.IsServer())
            {
                be.OwnerUID = byPlayer.PlayerUID;
                ((ICoreServerAPI)world.Api).SendMessage(byPlayer, 0, "You now own this snitch.", EnumChatType.OwnMessage);
                be.MarkDirty();
            }
            else (world.Api as ICoreClientAPI)?.SendChatMessage("/snitchinfo");
            return true;
        }
    }
}