using Godot;

namespace LabyrinthExplorer3D.scripts.game.abilties;

[GlobalClass]
public abstract partial class Player3dAbility : Ability
{
    [Export] public Player3D OwningPlayer;
}