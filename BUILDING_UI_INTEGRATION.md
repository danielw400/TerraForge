# Integração do Sistema de Construção - UI e Input

## Visão Geral

Este documento descreve como integrar o `BuildingSystem` com a camada de UI e o sistema de Input existente.

## 1. Seleção de Blocos para Construção

### BuildingInputHandler
Processa comandos de construção a partir do Input:

```csharp
public sealed class BuildingInputHandler
{
    private readonly BuildingSystem _buildingSystem;
    private readonly IInputProvider _inputProvider;
    private ushort _selectedBlockId = 1; // Stone padrão

    public BuildingInputHandler(BuildingSystem buildingSystem, IInputProvider inputProvider)
    {
        _buildingSystem = buildingSystem;
        _inputProvider = inputProvider;
    }

    public void Update(PlayerController player, VoxelWorldAdapter worldAdapter)
    {
        // Build: Right Mouse Button
        if (_inputProvider.GetButtonDown("Build"))
        {
            AttemptBlockPlacement(player, worldAdapter);
        }

        // Remove: Control + Right Mouse Button
        if (_inputProvider.GetButton("Crouch") && _inputProvider.GetButtonDown("Build"))
        {
            AttemptBlockRemoval(player, worldAdapter);
        }

        // Hotbar selection 1-9 switches block type
        for (var i = 0; i < 9; i++)
        {
            if (_inputProvider.GetButtonDown($"Hotbar{i + 1}"))
            {
                SelectBlockFromHotbar(i);
            }
        }
    }

    private void AttemptBlockPlacement(PlayerController player, VoxelWorldAdapter worldAdapter)
    {
        var hit = worldAdapter.RaycastBlock();
        if (hit == null) return;

        var placePos = CalculatePlacementPosition(hit.Value);
        if (_buildingSystem.Placement.TryPlaceBlock(placePos, _selectedBlockId))
        {
            Console.WriteLine($"Bloco colocado em {placePos}");
        }
    }

    private void AttemptBlockRemoval(PlayerController player, VoxelWorldAdapter worldAdapter)
    {
        var hit = worldAdapter.RaycastBlock();
        if (hit == null) return;

        if (_buildingSystem.Placement.TryRemoveBlock(hit.Value.Position))
        {
            Console.WriteLine($"Bloco removido de {hit.Value.Position}");
        }
    }

    private Vector3Int CalculatePlacementPosition(VoxelPhysics.RaycastHit hit)
    {
        var hitPos = hit.Position.ToVector3Int();
        var normal = hit.Normal;
        
        if (normal.Y > 0.7f) return hitPos + new Vector3Int(0, 1, 0);
        if (normal.Y < -0.7f) return hitPos + new Vector3Int(0, -1, 0);
        if (normal.X > 0.7f) return hitPos + new Vector3Int(1, 0, 0);
        if (normal.X < -0.7f) return hitPos + new Vector3Int(-1, 0, 0);
        if (normal.Z > 0.7f) return hitPos + new Vector3Int(0, 0, 1);
        return hitPos + new Vector3Int(0, 0, -1);
    }

    private void SelectBlockFromHotbar(int index)
    {
        _selectedBlockId = (ushort)(1 + index);
    }

    public ushort GetSelectedBlockId() => _selectedBlockId;
}
```

## 2. Preview de Construção (UI)

### BuildingPreviewRenderer
Renderiza preview do bloco antes de colocar:

```csharp
public sealed class BuildingPreviewRenderer
{
    private readonly IUiRenderer _renderer;
    private readonly BuildingSystem _buildingSystem;
    private ushort _previewBlockId;
    private Vector3Int _previewPosition;
    private bool _canPlace;

    public void UpdatePreview(VoxelPhysics.RaycastHit? hit, ushort blockId, 
                             BuildingInputHandler inputHandler)
    {
        if (hit == null)
        {
            _canPlace = false;
            return;
        }

        var placement = inputHandler.GetSelectedBlockId();
        _previewPosition = CalculatePlacementPosition(hit.Value);
        _canPlace = _buildingSystem.Placement.CanPlaceBlock(_previewPosition, placement);
        _previewBlockId = blockId;
    }

    public void Render(IUiRenderer renderer, int screenWidth, int screenHeight)
    {
        if (!_canPlace) return;

        var color = new UiColor(0, 255, 0, 128); // Verde translúcido
        // Renderizar bloco de preview 3D seria feito no engine
        // Aqui apenas mostramos indicador 2D

        renderer.DrawRectangle(10, 10, 100, 20, color);
        renderer.DrawText(15, 12, "Preview: Can Place", new UiColor(255, 255, 255, 255), 12);
    }
}
```

## 3. HUD de Construção

### BuildingHudMetrics
Exibe informações de construção no HUD:

```csharp
public sealed class BuildingHudMetrics
{
    public required string CurrentBlockName { get; set; }
    public required float StructuralIntegrity { get; set; } // 0-1
    public required int UnsupportedBlockCount { get; set; }
    public required bool IsStructureStable { get; set; }
    public required float SelectedBlockDurability { get; set; } // 0-1
}

public sealed class BuildingHudRenderer
{
    private readonly IUiRenderer _renderer;

    public void Render(IUiRenderer renderer, BuildingHudMetrics metrics, 
                       int screenWidth, int screenHeight)
    {
        const int margin = 24;
        const int y = margin;
        var x = screenWidth - 250 - margin;

        // Bloco selecionado
        renderer.DrawText(x, y, $"Building: {metrics.CurrentBlockName}", 
                         new UiColor(255, 255, 255, 255), 14);

        // Indicador de estabilidade estrutural
        var stabilityColor = metrics.IsStructureStable 
            ? new UiColor(0, 200, 0, 255) 
            : new UiColor(200, 0, 0, 255);

        renderer.DrawRectangle(x, y + 24, 200, 16, new UiColor(50, 50, 50, 200));
        var barWidth = (int)(200 * metrics.StructuralIntegrity);
        renderer.DrawRectangle(x, y + 24, barWidth, 16, stabilityColor);
        renderer.DrawText(x + 4, y + 26, 
                         $"Structure: {metrics.StructuralIntegrity:P0}", 
                         new UiColor(255, 255, 255, 255), 12);

        // Blocos não-suportados
        if (metrics.UnsupportedBlockCount > 0)
        {
            renderer.DrawText(x, y + 48, 
                             $"⚠ {metrics.UnsupportedBlockCount} unsupported", 
                             new UiColor(255, 200, 0, 255), 12);
        }
    }
}
```

## 4. Integração com InventorySession

### BuildingMaterialSlot
Marca items como materiais de construção:

```csharp
public static class BuildingMaterialCategories
{
    public const string BuildingMaterial = "Building";
    public const string RepairMaterial = "Repair";

    public static ItemCategory GetCategory(ushort blockId) => ItemCategory.Material;
}
```

### Extender InventorySession para rastrear materiais de construção:

```csharp
public partial class InventorySession
{
    private Dictionary<ushort, int> _materialInventory = new();

    public int GetMaterialQuantity(ushort materialId)
    {
        return _materialInventory.TryGetValue(materialId, out var qty) ? qty : 0;
    }

    public bool ConsumeMaterial(ushort materialId, int amount)
    {
        if (GetMaterialQuantity(materialId) < amount) return false;
        
        var remaining = amount;
        foreach (var container in _containers.Values)
        {
            foreach (var slot in container.Slots)
            {
                if (slot.IsEmpty || slot.Item.Definition.Id != materialId) continue;
                
                var take = Math.Min(slot.Item.Quantity, remaining);
                slot.Item.Remove(take);
                remaining -= take;
                
                if (remaining == 0) break;
            }
        }

        _materialInventory[materialId] = Math.Max(0, GetMaterialQuantity(materialId) - amount);
        return true;
    }
}
```

## 5. Fluxo Completo de Construção

```csharp
public sealed class GameLoopWithBuilding
{
    private readonly BuildingSystem _buildingSystem;
    private readonly BuildingInputHandler _inputHandler;
    private readonly BuildingPreviewRenderer _previewRenderer;
    private readonly BuildingHudRenderer _hudRenderer;
    private readonly PlayerController _player;
    private readonly VoxelWorldAdapter _worldAdapter;
    private readonly InventorySession _inventory;

    public void Update(float deltaTime, IInputProvider input)
    {
        // 1. Processar input de construção
        _inputHandler.Update(_player, _worldAdapter);

        // 2. Raycasting para preview
        var hit = _worldAdapter.RaycastBlock();
        _previewRenderer.UpdatePreview(hit, _inputHandler.GetSelectedBlockId(), _inputHandler);

        // 3. Atualizar HUD com métricas
        var selectedBlockId = _inputHandler.GetSelectedBlockId();
        var metrics = new BuildingHudMetrics
        {
            CurrentBlockName = GetBlockName(selectedBlockId),
            StructuralIntegrity = hit.HasValue 
                ? _buildingSystem.AnalyzeStructure(hit.Value.Position.ToVector3Int()).IntegrityPercentage / 100f
                : 0f,
            UnsupportedBlockCount = hit.HasValue
                ? _buildingSystem.AnalyzeStructure(hit.Value.Position.ToVector3Int()).UnsupportedBlocks
                : 0,
            IsStructureStable = hit.HasValue
                ? _buildingSystem.AnalyzeStructure(hit.Value.Position.ToVector3Int()).IsStable
                : false,
            SelectedBlockDurability = 1f
        };

        // 4. Renderizar preview
        _previewRenderer.Render(_renderer, screenWidth, screenHeight);
        _hudRenderer.Render(_renderer, metrics, screenWidth, screenHeight);
    }

    public void Render(IUiRenderer renderer, int screenWidth, int screenHeight)
    {
        _previewRenderer.Render(renderer, screenWidth, screenHeight);
        _hudRenderer.Render(renderer, null, screenWidth, screenHeight);
    }

    private string GetBlockName(ushort blockId)
    {
        return blockId switch
        {
            1 => "Stone",
            2 => "Dirt",
            3 => "Grass",
            4 => "Wood",
            _ => "Unknown"
        };
    }
}
```

## 6. Atalhos de Teclado

Estender `BindingsPreset`:

```csharp
public static class BuildingBindings
{
    public static void AddBuildingBindings(InputManager manager)
    {
        // Já existe: Build = Mouse1 (right click)
        // Já existe: Hotbar1-9 = Keys 1-9

        // Adicionar:
        manager.AddBinding(new InputBinding("RemoveBlock", "Key.LeftControl", InputBindingType.Button));
        manager.AddBinding(new InputBinding("RotateBlock", "Key.R", InputBindingType.Button));
        manager.AddBinding(new InputBinding("ToggleBuildMode", "Key.B", InputBindingType.Button));
    }
}
```

## 7. Configuração Completa no Game Initialization

```csharp
// Initialization.cs
var blockRegistry = new BlockRegistry();
var chunkManager = new ChunkManager(blockRegistry);
var buildingSystem = new BuildingSystem(chunkManager, blockRegistry);
var inputProvider = new YourInputProviderImpl();

var inputHandler = new BuildingInputHandler(buildingSystem, inputProvider);
var previewRenderer = new BuildingPreviewRenderer();
var hudRenderer = new BuildingHudRenderer();

var gameLoop = new GameLoopWithBuilding
{
    _buildingSystem = buildingSystem,
    _inputHandler = inputHandler,
    _previewRenderer = previewRenderer,
    _hudRenderer = hudRenderer,
    _player = playerController,
    _worldAdapter = worldAdapter,
    _inventory = inventorySession
};
```

## 8. Multiplayer Synchronization

Para multiplayer, serialize BuildingActions:

```csharp
[Serializable]
public class BuildingAction
{
    public enum Type { Place, Remove, Damage, Repair }

    public Type ActionType { get; set; }
    public Vector3Int Position { get; set; }
    public ushort BlockId { get; set; }
    public float Amount { get; set; }
    public float Timestamp { get; set; }
    public uint PlayerId { get; set; }

    public void Replicate(BuildingSystem buildingSystem)
    {
        switch (ActionType)
        {
            case Type.Place:
                buildingSystem.Placement.TryPlaceBlock(Position, BlockId);
                break;
            case Type.Remove:
                buildingSystem.Placement.TryRemoveBlock(Position);
                break;
            case Type.Damage:
                buildingSystem.Damage.DamageBlock(Position, Amount);
                break;
            case Type.Repair:
                buildingSystem.Repair.TryRepairBlock(Position, BlockId, (int)Amount);
                break;
        }
    }
}
```

Enviar `BuildingAction` por rede quando operação ocorre localmente.
