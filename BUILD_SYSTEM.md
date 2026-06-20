# Sistema de Construção - TerraForge

## Visão Geral

O sistema de construção oferece um framework completo para construir, danificar e reparar estruturas no mundo voxel. Inclui:

- **Colocação de blocos** com validação de suporte
- **Remoção de blocos** com análise de cascata
- **Estabilidade estrutural** baseada em gráfico de suporte
- **Sistema de dano** com health tracking por bloco
- **Reparo** com consumo de materiais do inventário

## Arquitetura

### 1. BlockHealth
Rastreia a durabilidade de cada bloco construído.

```csharp
public sealed class BlockHealth
{
    public Vector3Int Position { get; }
    public float MaxHealth { get; }
    public float CurrentHealth { get; private set; }
    public bool IsDestroyed => CurrentHealth <= 0f;
}
```

**Métodos principais:**
- `TakeDamage(float amount)`: Reduz saúde
- `Repair(float amount)`: Restaura saúde
- `GetHealthPercentage()`: Retorna 0-1

### 2. StructuralStability
Analisa dependências de suporte usando BFS em grafo de blocos.

```csharp
public sealed class StructuralStability
{
    public bool IsSupported(Vector3Int position);
    public List<Vector3Int> FindUnsupportedBlocks(Vector3Int position, int maxRadius = 32);
}
```

**Algoritmo:**
- Um bloco é suportado se:
  - Tem bloco sólido abaixo, OU
  - Tem 2+ blocos sólidos adjacentes (lado a lado)
- BFS expandente encontra blocos não-suportados em cascata

### 3. BlockPlacement
Gerencia colocação e remoção de blocos com validações.

```csharp
public sealed class BlockPlacement
{
    public bool CanPlaceBlock(Vector3Int position, ushort blockId);
    public bool TryPlaceBlock(Vector3Int position, ushort blockId);
    public bool CanRemoveBlock(Vector3Int position);
    public bool TryRemoveBlock(Vector3Int position);
}
```

**Validações de colocação:**
- Posição está vazia
- BlockId é sólido
- Posição tem suporte estrutural

### 4. BlockDamageManager
Mapeia posições para dados de saúde, gerencia destruição.

```csharp
public sealed class BlockDamageManager
{
    public BlockHealth GetBlockHealth(Vector3Int position);
    public void DamageBlock(Vector3Int position, float damage);
    public void RepairBlock(Vector3Int position, float amount);
    public float GetBlockHealthPercentage(Vector3Int position);
}
```

**Health padrão por tipo de bloco:**
- Stone (1): 100 HP
- Dirt (2): 50 HP
- Grass (3): 50 HP
- Wood (4): 75 HP
- Glass (5): 40 HP
- Sand (6): 30 HP

### 5. StructureRepair
Sistema de reparo com consumo de materiais.

```csharp
public sealed class StructureRepair
{
    public bool CanRepairBlock(Vector3Int position);
    public bool TryRepairBlock(Vector3Int position, ushort materialId, int quantity);
    public bool RepairFromInventory(Vector3Int position, InventoryContainer inv, ushort materialId);
    public float GetRepairCostInMaterials(Vector3Int position, ushort materialId);
}
```

**Taxa de reparo (HP por material):**
- Stone: 15 HP
- Dirt: 8 HP
- Wood: 12 HP
- Sand: 5 HP

### 6. BuildingSystem
Orquestrador principal integrand todos os componentes.

```csharp
public sealed class BuildingSystem
{
    public BlockPlacement Placement { get; }
    public BlockDamageManager Damage { get; }
    public StructuralStability Stability { get; }
    public StructureRepair Repair { get; }

    public StructureIntegrity AnalyzeStructure(Vector3Int center, int radius = 32);
    public List<Vector3Int> FindStructureBlocks(Vector3Int origin, int maxDistance = 64);
}
```

## Fluxos de Uso

### Construindo uma estrutura

```csharp
var buildingSystem = new BuildingSystem(chunkManager, blockRegistry);
var placement = buildingSystem.Placement;

// Validar antes de colocar
if (placement.CanPlaceBlock(position, blockId))
{
    placement.TryPlaceBlock(position, blockId);
}
```

### Analisando estabilidade

```csharp
var integrity = buildingSystem.AnalyzeStructure(centerPos);
if (integrity.IsStable)
{
    Console.WriteLine($"Estrutura estável: {integrity.IntegrityPercentage:F1}%");
}
else
{
    Console.WriteLine($"Blocos não-suportados: {integrity.UnsupportedBlocks}");
}
```

### Danificando blocos

```csharp
var damage = buildingSystem.Damage;
damage.DamageBlock(blockPos, 25f);

var health = damage.GetBlockHealth(blockPos);
if (health.IsDestroyed)
{
    Console.WriteLine("Bloco destruído!");
}
```

### Reparando com inventário

```csharp
var repair = buildingSystem.Repair;
if (repair.RepairFromInventory(blockPos, inventory, stoneId))
{
    Console.WriteLine("Reparo com sucesso!");
}
```

## Preparação para Multiplayer

O sistema é naturalmente multiplayer-safe:

1. **Sem estado global** - tudo é baseado em posições e IDs
2. **Validações determinísticas** - mesma entrada = mesmo resultado
3. **Histórico auditável** - cada operação é rastreável
4. **Idempotência** - operações podem ser reaplicadas com segurança

Para multiplayer, implemente:

```csharp
public class BuildingAction
{
    public enum ActionType { Place, Remove, Damage, Repair }
    public ActionType Type { get; set; }
    public Vector3Int Position { get; set; }
    public ushort BlockId { get; set; } // para Place
    public float Amount { get; set; } // para Damage/Repair
    public float Timestamp { get; set; }
    public uint PlayerId { get; set; }
}
```

Serialize e replique BuildingActions para sincronizar entre clientes.

## Exemplos

Veja `BuildingSystemIntegrationExample.cs` para:

- Testes de colocação de blocos
- Análise de estabilidade estrutural
- Sistema de dano progressivo
- Reparo com materiais do inventário
- Integração com InventoryContainer

## Otimizações Futuras

1. **Cache de suporte** - armazenar gráfos em estrutura de árvore
2. **Análise incremental** - atualizar apenas blocos afetados
3. **LOD estrutural** - simplificar análise para estruturas grandes
4. **Persistência** - salvar health e estado para chunks
5. **Prefabs** - templates de edifícios pré-construídos

## Testes

Todos os componentes incluem validação automática de tipos e limites:

```csharp
public bool TryPlaceBlock(Vector3Int position, ushort blockId)
{
    if (!CanPlaceBlock(position, blockId)) return false;
    _chunkManager.SetBlock(position, new VoxelData(blockId));
    return true;
}
```

Return values indicam sucesso/falha sem exceções.
