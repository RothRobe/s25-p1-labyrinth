using Godot;
using LabyrinthExplorer3D.scripts.game.abilties;

namespace LabyrinthExplorer3D.scripts.game.items;

public partial class ItemCollectArea3D : Area3D
{
    [Export] public Item3D Item;

    public override void _Ready()
    {
        base._Ready();
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body)
    {
        if(body is not Player3D player)
            return;

        var canGetInventory = player.TryGetAbility<Player3dInventoryAbility>(out var inventoryAbility);
        if (!canGetInventory)
            return;
        
        inventoryAbility.StoreItem(Item);
    }
}