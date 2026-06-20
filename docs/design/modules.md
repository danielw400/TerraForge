# TerraForge - Módulos Principais

## Core do Projeto
- `WorldGenerator`
  - Geração procedural de terrenos, biomas, cavernas e estruturas.
  - Configurações de ruído, níveis de água, e diversidade topográfica.

- `VoxelManager`
  - Gerenciamento de chunks e armazenamento de voxels.
  - Meshing otimizado, face culling e compressão de dados.

- `SaveLoadSystem`
  - Persistência de estado do jogo, mundos e progressão do jogador.
  - Versão de arquivos e compatibilidade futura.

- `AssetManager`
  - Carregamento e cache de texturas, modelos, áudios e shaders.
  - Suporte a hot-reload em desenvolvimento.

## Mecânicas de Gameplay
- `PlayerController`
  - Movimento, interação, inventário e controles de câmera.

- `InventorySystem`
  - Slots, empilhamento, itens, classes de ferramentas e integração com crafting.

- `BuildingSystem`
  - Colocação e remoção de blocos, modos de construção e pré-visualização.

- `SurvivalSystem`
  - Saúde, fome, sede, temperatura e efeitos ambientais.

- `ProgressionSystem`
  - Árvores de habilidades, desbloqueio de receitas, marcos de XP e sistemas de avanço.

- `CraftingSystem`
  - Receitas, estações de trabalho e desbloqueio gradual de materiais.

## Engine e Subsistemas
- `RenderingPipeline`
  - Renderização de voxels e pós-processamento realista.
  - Iluminação ambiente, SSAO e efeitos visuais modernos.

- `PhysicsModule`
  - Colisões, movimento de personagem e física de objetos dinâmicos.

- `AudioSystem`
  - Som ambiente, efeitos de impacto e música adaptativa.

- `InputSystem`
  - Suporte a teclado, mouse e gamepad.

- `UIManager`
  - HUD, menus, inventário e telas de progresso.

## Infraestrutura e Extensibilidade
- `EventBus`
  - Comunicação desacoplada entre sistemas.

- `ECS` (opcional)
  - Estrutura de entidades e componentes para escalabilidade.

- `NetworkLayer` (planejado)
  - Preparação para multiplayer e sincronização futura.

- `ModdingAPI` (planejado)
  - Hooks para mods, dados configuráveis e scripts.
