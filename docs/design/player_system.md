# TerraForge - Sistema de Jogador

## Objetivo
Criar um sistema de jogador expansível com mecânicas de movimento e sobrevivência.

## Recursos
- Andar
- Correr
- Agachar
- Pular
- Escalar
- Nadar

## Atributos de sobrevivência
- Vida
- Fome
- Sede
- Energia (stamina)

## Arquitetura

### `PlayerController`
- Controla posição, velocidade e estado do jogador.
- Aplica gravidade, salto, natação e escalada.
- Utiliza callbacks para detecção de colisão, água e superfícies escaláveis.

### `PlayerStats`
- Gerencia valores atuais e máximos.
- Atualiza fadiga, fome, sede e saúde.
- Penaliza vida quando fome ou sede estão muito baixas.

### `PlayerInput`
- Abstrai movimento e ações de entrada.
- Permite ligação com qualquer sistema de input.

### `IPlayerAbility`
- Interface para habilidades futuras.
- Permite adicionar habilidades como dash, double jump, stealth e buffs.

## Fluxo de atualização
1. Recolher entrada do jogador em `PlayerInput`.
2. Chamar `PlayerController.Update(...)`.
3. `PlayerController` calcula movimento, aplica física e atualiza estado.
4. `PlayerStats.Update(...)` reduz recursos e aplica penalidades.

## Expansão futura
- Adicionar `SkillTree` conectado a `PlayerStats`.
- Implementar `AbilityManager` para gerenciar habilidades ativas.
- Integrar `StatusEffects` como envenenamento, frio e calor.
- Separar locomotion em `MovementModule`, `JumpModule`, `SwimModule`.
