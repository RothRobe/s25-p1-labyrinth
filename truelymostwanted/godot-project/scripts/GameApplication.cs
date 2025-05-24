using Godot;
using Godot.Collections;
using LabyrinthExplorer3D.scripts.core.functions;
using LabyrinthExplorer3D.scripts.core.menus;

namespace LabyrinthExplorer3D.scripts;

[GlobalClass]
public partial class GameApplication : Node
{
    public override void _Ready()
    {
        base._Ready();
        StopGameFunction.Execute(node: this);
    }
}