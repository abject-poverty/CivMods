using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace CivMods
{
    class BlockBehaviorUnbreakableByTier : BlockBehavior
    {
        public int MiningTier { get; set; }

        public BlockBehaviorUnbreakableByTier(Block block) : base(block)
        {
        }

        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);
            MiningTier = properties["MiningTier"].AsInt();
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref EnumHandling handling)
        {
            if ((byPlayer?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible?.ToolTier ?? -1) < MiningTier && (byPlayer?.WorldData?.CurrentGameMode ?? EnumGameMode.Survival) == EnumGameMode.Survival)
            {
                handling = EnumHandling.PreventSubsequent;
                return;
            }
            base.OnBlockBroken(world, pos, byPlayer, ref handling);
        }
    }
}