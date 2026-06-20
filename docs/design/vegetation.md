# TerraForge - Vegetation Generator

## Objetivo
Adicionar vegetação procedural que combine com cada bioma e aumente a diversidade do mundo.

## Biomas e vegetação
- `Forest`
  - Árvores de tronco e folhas
  - Cogumelos esparsos
- `Desert`
  - Cactos isolados
- `Swamp`
  - Juncos e lírios
- `Mountains`
  - Cogumelos montanhosos
- `InfectedZone`
  - Troncos mortos e fungos

## Como funciona

1. `VegetationGenerator` usa `SeededNoise` para posicionamento e densidade.
2. Em cada chunk, a vegetação só é colocada sobre o bloco de superfície.
3. Cada bioma tem regras próprias de spawn.
4. A função verifica espaço livre antes de construir árvores ou estruturas maiores.

## Regras de colocação
- Vegetação só aparece em blocos de superfície adequados.
- Árvores precisam de espaço vertical e horizontal.
- Cactos crescem em areia.
- Lírios de água aparecem em blocos de água rasos.
- Juncos crescem próximos a água.
- Troncos mortos formam entulho em zonas infectadas.
- Cogumelos aparecem em florestas e montanhas.

## Extensões futuras
- Inserir vegetação com clustering para grupos de plantas.
- Adicionar plantas interativas que podem ser colhidas.
- Integrar geração de árvores maiores e estruturas naturais (buracos, galhos).
- Separar regras de vegetação em `BiomeVegetationProfile` configurável.
