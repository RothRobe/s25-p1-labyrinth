using Godot;
using LabyrinthExplorer3D.scripts.core.functions;

namespace LabyrinthExplorer3D.scripts.game.abilties;

[GlobalClass]
public partial class Player3dPauseAbility : Player3dAbility
{
    public override void _OnProcess(double delta)
    {
        
    }

    public override void _OnUnhandledInput(InputEvent @event)
    {
        if (!IsAnyInputActionTriggered())
            return;

        var tree = GetTree();
        if (tree.IsPaused())
        {
            StartGameFunction.Execute(node: this);
        }
        else
        {
            StopGameFunction.Execute(node: this);
        }
    }
}