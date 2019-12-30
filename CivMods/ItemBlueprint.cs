using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace CivMods
{
    class ItemBlueprint : Item
    {
        public override void OnHeldRenderOrtho(ItemSlot inSlot, IClientPlayer byPlayer)
        {
            base.OnHeldRenderOrtho(inSlot, byPlayer);
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (blockSel?.BlockEntity(api) is BlockEntityChisel)
            {
                handling = EnumHandHandling.PreventDefault;
                BlockEntityChisel entityChisel = ((BlockEntityChisel)blockSel.BlockEntity(api));
                ITreeAttribute blueprintTree = slot?.Itemstack?.Attributes;
                ITreeAttribute dummy = blueprintTree.Clone();
                entityChisel.ToTreeAttributes(dummy);
                blueprintTree["materials"] = dummy["materials"];

                if (byEntity.Controls.Sneak)
                {
                    blueprintTree["cuboids"] = new IntArrayAttribute();
                    entityChisel.ToTreeAttributes(blueprintTree);
                    slot.MarkDirty();
                }
                else if (blueprintTree["cuboids"] != null)
                {
                    entityChisel.FromTreeAtributes(blueprintTree, api.World);
                }
                if (api.Side.IsClient()) entityChisel.RegenMesh();
            }
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
        }
    }
}