# TerraForge - Arquitetura Geral

## Visão Geral
TerraForge é um jogo sandbox de mundo aberto baseado em voxel, com foco em sobrevivência, construção, exploração e progressão.
A arquitetura é pensada para ser modular e expansível, permitindo evolução rápida de mecânicas e melhoria de gráficos.

## Camadas Principais

1. Engine
   - Rendering
   - Física
   - Audio
   - Input
   - Asset Management
   - Scripting / Gameplay Integration

2. Core do Jogo
   - Mundo Procedural
   - Sistema Voxel
   - Entidades e NPCs
   - Sistema de Inventário
   - Sistema de Crafting
   - Progressão e Talentos

3. UI / UX
   - HUD de Sobrevivência
   - Menu de Construção
   - Tela de Progressão
   - Interface de Inventário
   - Configurações e Opções

4. Sistema de Dados
   - Salvamento / Carregamento de Mundo
   - Perfil do Jogador
   - Configurações de Jogo
   - Logs e Estatísticas

## Arquitetura em Componentes

- `WorldGenerator`
  - Geração procedural de terrenos, biomas, cavernas e estruturas.
  - Suporta diferentes algoritmos por camada (ruído simplex/perlin, voronoi, cellular automata).

- `VoxelManager`
  - Controle de chunks, armazenamento de voxels e atualização de mesh.
  - Iguala voxels a blocos e permite otimização por face culling e greedy meshing.

- `EntitySystem`
  - Entidades dinâmicas (jogador, animais, inimigos, objetos móveis)
  - Componente ECS opcional para facilitar adição de novos comportamentos.

- `InventorySystem`
  - Gerenciamento de slots, stacks e itens equipáveis.
  - Integração com crafting, construção e progressão.

- `BuildingSystem`
  - Modos de edição e colocação de blocos.
  - Ferramentas de construção, destruição e edição de terreno.

- `SurvivalSystem`
  - Fome, sede, saúde, temperatura e condições ambientais.
  - Penalidades, buffs e subsistemas de clima.

- `ProgressionSystem`
  - Árvore de habilidades, desbloqueios de receitas e ferramentas.
  - Missões básicas e marcos de exploração.

- `RenderingPipeline`
  - Renderização de voxel com shading realista e partículas.
  - Suporte a iluminação global aproximada ou SSGI, ambient occlusion e pós-processamento.

- `AudioSystem`
  - Efeitos sonoros adaptativos e música ambiente.
  - Camadas de som por bioma e eventos de sobrevivência.

- `SaveLoadSystem`
  - Persistência de mundo e progresso do jogador.
  - Consumíveis, construções e estado de entidades.

## Módulos de Integração

- `PhysicsModule`
  - Colisões, movimento de personagem e física de objetos.

- `AI`
  - Comportamento de NPCs, animais e inimigos.

- `UIManager`
  - Controle de telas, popups e navegação de menu.

- `NetworkLayer` (planejado)
  - Arquitetura para multiplayer futuro.

## Padrões Recomendados

- Arquitetura em camadas com separação entre engine e gameplay.
- Sistema ECS leve para entidades e componentes de jogo.
- Abstração clara entre dados do mundo e renderização.
- Uso de interfaces para sistemas substituíveis (ex: `IWorldGenerator`, `IRenderer`).
