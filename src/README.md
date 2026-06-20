# TerraForge - Código Fonte

## Estrutura de diretórios

- `src/core/`
  - Sistemas centrais do motor de jogo e arquitetura básica.
  - Ex.: `WorldGenerator`, `VoxelManager`, `SaveLoadSystem`, `AssetManager`.

- `src/engine/`
  - Subsistemas de engine, renderização, física, áudio e input.
  - Ex.: `RenderingPipeline`, `PhysicsModule`, `AudioSystem`, `InputSystem`.

- `src/game/`
  - Lógica de gameplay, mecânicas de sobrevivência, construção e progressão.
  - Ex.: `PlayerController`, `InventorySystem`, `BuildingSystem`, `ProgressionSystem`.

- `src/ui/`
  - Interfaces de usuário e telas do jogo.
  - Ex.: `HUD`, `Menus`, `Inventário`, `Telas de Options`.

## Desenvolvimento futuro

- Adicionar um módulo `network` para multiplayer.
- Adotar ECS leve para facilitar novas entidades e comportamentos.
- Manter a separação entre engine, gameplay e UI.
