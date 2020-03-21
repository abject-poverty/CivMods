using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace CivMods
{
    class EntityBehaviorSuffocate : EntityBehavior
    {
        public float? CurrentAir { get => entity.WatchedAttributes.TryGetFloat("currentAir");
            set
            {
                entity.WatchedAttributes.SetFloat("currentAir", value ?? 0.0f);
                entity.WatchedAttributes.MarkPathDirty("currentAir");
            }
        }

        public const float maxAir = 1.0f;

        ICoreServerAPI sapi { get => entity.Api as ICoreServerAPI; }
        long id;

        public EntityBehaviorSuffocate(Entity entity) : base(entity)
        {
        }

        public override string PropertyName() => "suffocate";


        public override void Initialize(EntityProperties properties, JsonObject attributes)
        {
            base.Initialize(properties, attributes);
        }

        public override void OnEntitySpawn()
        {
            base.OnEntitySpawn();
            id = sapi?.World.RegisterGameTickListener(SuffocationWatch, 500) ?? 0;
        }

        public override void OnEntityDespawn(EntityDespawnReason despawn)
        {
            base.OnEntityDespawn(despawn);
            sapi?.World.UnregisterGameTickListener(id);
        }

        public void SuffocationWatch(float dt)
        {
            if (entity is EntityItem || entity == null || entity.ServerPos == null || !entity.Alive) return;

            if (entity is EntityPlayer)
            {
                EntityPlayer entityPlayer = entity as EntityPlayer;
                if (entityPlayer.Player.WorldData.CurrentGameMode != EnumGameMode.Survival) return;
            }

            float height = entity.CollisionBox.Height;

            //ITreeAttribute attribs = entity.WatchedAttributes.GetOrAddTreeAttribute("health");

            if (CurrentAir == null)
            {
                CurrentAir = 1.0f;
                return;
            }

            if (InBlockBounds(entity.ServerPos.XYZ, height, out float suff))
            {
                if (CurrentAir > 0) CurrentAir -= suff;
                else
                {
                    CurrentAir = 0.0f;
                    DamageSource source = new DamageSource();
                    source.Source = EnumDamageSource.Drown;
                    entity.ReceiveDamage(source, (float)(sapi.World.Rand.NextDouble() * 2));
                }
            }
            else if (CurrentAir < 1.0) CurrentAir += 0.25f;

            CurrentAir = CurrentAir > 1.0 ? 1.0f : CurrentAir;
        }

        public bool InBlockBounds(Vec3d vec, float height, out float suffocation)
        {
            suffocation = 0.0f;

            vec.Sub(0.5, 0, 0.5).Add(0, height / 2.0, 0);

            BlockPos pos = new BlockPos((int)Math.Round(vec.X), (int)Math.Round(vec.Y), (int)Math.Round(vec.Z));
            Vec3d blockCenter = pos.ToVec3d().AddCopy(0.5, 0.5, 0.5);
            Block block = sapi.World.BlockAccessor.GetBlock(pos);
            double distance = Math.Sqrt(vec.SquareDistanceTo(blockCenter));
            if (block.IsLiquid() && distance < 1.5 && int.Parse(block.LastCodePart()) > 4)
            {
                suffocation = 0.01f;
                return true;
            }
            if (block.Id == 0 || block.CollisionBoxes == null) return false;

            for (int i = 0; i < block.CollisionBoxes.Length; i++)
            {
                suffocation = 0.5f;
                var box = block.CollisionBoxes[i];
                if (box.Height > 0.512 && box.Area() > 0.512 && distance < 1.11) return true;
            }
            return false;
        }
    }

    public static class MiscUtilities
    {
        public static double Area(this Cuboidf cuboid)
        {
            return (cuboid.Length * cuboid.Width * cuboid.Height);
        }

        public static BlockPos AddCopy(this BlockPos pos, BlockPos copy)
        {
            return pos.AddCopy(copy.X, copy.Y, copy.Z);
        }
    }
}
