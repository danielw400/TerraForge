# TerraForge - Tecnologias Recomendadas

## Motor / Engine
- `Unity` com URP/HDRP ou `Unreal Engine` para protótipos 3D rápidos e suporte nativo a gráficos modernos.
- Alternativa de código aberto: `Godot 4` se o foco for flexibilidade e controle de código.

## Linguagens
- C# para Unity ou Godot.
- C++ para Unreal Engine ou engine custom.
- GLSL/HLSL para shaders customizados.

## Renderização
- Pipeline de renderização baseado em PBR.
- Iluminação global aproximada (SSGI/SSAO).
- Post processing: bloom, tone mapping, depth of field, color grading.

## Mundo Procedural
- Algoritmos de ruído: `Perlin`, `Simplex`, `OpenSimplex`, `Worley`.
- Estruturas de dados: `chunked voxels`, `octrees`, `Sparse Voxel Octree`.
- Meshing: `greedy meshing`, `Marching Cubes` (para terreno suavizado).

## Arquitetura e Ferramentas
- ECS leve ou arquitetura baseada em componentes.
- `Git` para versionamento.
- `CMake` para projetos C++ custom.
- `Visual Studio Code` com extensões de C#, C++ e Lua/JSON.

## Ferramentas de Desenvolvimento
- `Blender` para criação de assets 3D.
- `Substance 3D Painter` / `Quixel Mixer` para texturas realistas.
- `FMOD` ou `Wwise` para áudio dinâmico.

## Pipeline de Dados
- Formatos de asset: `glTF` para modelos, `PNG`/`DDS` para texturas.
- Armazenamento de mundo: arquivos binários compactados ou `SQLite` para metadados.
- Serialization: JSON para configurações e YAML para protótipos de conteúdo.

## Plataformas alvo
- PC (Windows, Linux) primeiro.
- Consoles e mobile em fases posteriores, com adaptação de performance e controles.

## Extensibilidade
- Projeto modular para permitir `multiplayer` e `modding` futuro.
- API de scripting para desbloquear conteúdo e comportamento customizado.
