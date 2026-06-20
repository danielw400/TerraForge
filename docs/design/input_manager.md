# TerraForge - Input Manager

## Visão geral
Input Manager centralizado e modular para TerraForge. Abstrai a entrada física via `IInputProvider` e expõe `InputAction`s que o jogo consome.

## Componentes
- `IInputProvider`: interface para prover estados de botões, eixos e posição do ponteiro.
- `InputBinding`: representa associações entre ações e entradas físicas.
- `InputAction`: representa uma ação do jogo (botão, eixo, vetor2) com eventos.
- `InputManager`: registro de ações, bindings e atualização por frame.
- `KeyboardMouseProvider`: adaptador mínimo para injeção de callbacks do sistema de input da engine.

## Exemplos de uso
- Registrar ações iniciais:

```csharp
var provider = new KeyboardMouseProvider(GetKey, GetKeyDown, GetKeyUp, GetAxis, GetPointerDelta, GetPointerPos);
var im = new InputManager(provider);
im.AddAction("Move", ActionValueType.Vector2);
im.AddAction("Run", ActionValueType.Button);
im.AddAction("Jump", ActionValueType.Button);
im.AddAction("Crouch", ActionValueType.Button);
im.AddAction("Interact", ActionValueType.Button);
im.AddAction("Attack", ActionValueType.Button);
im.AddAction("Inventory", ActionValueType.Button);
im.AddAction("Build", ActionValueType.Button);

im.AddBinding(new InputBinding("Move", "Horizontal", "Vertical"));
im.AddBinding(new InputBinding("Jump", "Space"));
im.AddBinding(new InputBinding("Run", "LeftShift"));
```

- No loop do jogo, chamar:

```csharp
im.Update(deltaTime);
var playerInput = im.ToPlayerInput();
playerController.Update(deltaTime, playerInput, isColliding, isClimbable, isWater);
```

## Extensão para gamepad
- Implementar um `GamepadProvider : IInputProvider` utilizando a API do motor/plataforma.
- Mapear eixos e botões do gamepad para os nomes usados nos `InputBinding`s.

## Boas práticas
- Use nomes semânticos para ações ("Interact" em vez de "EKey").
- Crie perfis de bindings por plataforma/usuário e permita remapeamento em runtime.
- Para múltiplos contextos (UI vs Gameplay), adicione suporte a `InputContext`s que ativem/desativem grupos de ações.
