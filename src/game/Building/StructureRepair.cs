using System;
using System.Collections.Generic;
using TerraForge.Core;
using TerraForge.Game.Inventory;

namespace TerraForge.Game.Building
{
    public sealed class StructureRepair
    {
        private readonly BlockDamageManager _damageManager;
        private readonly Dictionary<ushort, ushort> _repairMaterialMap = new Dictionary<ushort, ushort>();

        public StructureRepair(BlockDamageManager damageManager)
        {
            _damageManager = damageManager ?? throw new ArgumentNullException(nameof(damageManager));
            InitializeRepairMaterials();
        }

        public bool CanRepairBlock(Vector3Int position)
        {
            var health = _damageManager.GetBlockHealth(position);
            return health != null && health.CurrentHealth < health.MaxHealth;
        }

        public bool TryRepairBlock(Vector3Int position, ushort repairMaterialId, int quantity)
        {
            if (!CanRepairBlock(position)) return false;
            if (quantity <= 0) return false;

            var health = _damageManager.GetBlockHealth(position);
            if (health == null) return false;

            var repairAmount = GetRepairAmountPerMaterial(repairMaterialId) * quantity;
            _damageManager.RepairBlock(position, repairAmount);
            return true;
        }

        public float GetRepairCostInMaterials(Vector3Int position, ushort materialId)
        {
            var health = _damageManager.GetBlockHealth(position);
            if (health == null) return 0f;

            var damageTaken = health.MaxHealth - health.CurrentHealth;
            var repairPerMaterial = GetRepairAmountPerMaterial(materialId);
            return repairPerMaterial > 0f ? damageTaken / repairPerMaterial : 0f;
        }

        public bool RepairFromInventory(Vector3Int position, InventoryContainer inventory, ushort materialId)
        {
            var cost = GetRepairCostInMaterials(position, materialId);
            var needed = (int)Math.Ceiling(cost);

            var available = 0;
            foreach (var slot in inventory.Slots)
            {
                if (!slot.IsEmpty && slot.Item.Definition.Id == materialId)
                {
                    available += slot.Item.Quantity;
                }
            }

            if (available < needed) return false;

            var consumed = 0;
            foreach (var slot in inventory.Slots)
            {
                if (consumed >= needed) break;
                if (slot.IsEmpty || slot.Item.Definition.Id != materialId) continue;

                var take = Math.Min(slot.Item.Quantity, needed - consumed);
                slot.Item.Remove(take);
                consumed += take;
                if (slot.Item.IsEmpty) slot.RemoveAll();
            }

            _damageManager.RepairBlock(position, needed * GetRepairAmountPerMaterial(materialId));
            return true;
        }

        private float GetRepairAmountPerMaterial(ushort materialId)
        {
            return materialId switch
            {
                1 => 15f,
                2 => 8f,
                4 => 12f,
                6 => 5f,
                _ => 5f
            };
        }

        private void InitializeRepairMaterials()
        {
            _repairMaterialMap[1] = 1;
            _repairMaterialMap[2] = 2;
            _repairMaterialMap[4] = 4;
            _repairMaterialMap[6] = 6;
        }
    }
}
