using System;

namespace TerraForge.Input
{
    // Abstração mínima para leitura de estados de entrada.
    // Implementações concretas (Unity, custom engine, etc.) devem fornecer
    // funções para consultar teclas, eixos e posição do mouse.
    public interface IInputProvider
    {
        bool GetButton(string name);
        bool GetButtonDown(string name);
        bool GetButtonUp(string name);
        float GetAxis(string name); // e.g. "Horizontal", "Vertical", "MouseX"
        (float x, float y) GetPointerDelta();
        (float x, float y) GetPointerPosition();
    }
}
