using Godot;

namespace LabyrinthExplorer3D.scripts.game.level;

[GlobalClass]
public partial class LevelController3D : Node3D
{
    [Export] public Level3D CurrentLevel;
}