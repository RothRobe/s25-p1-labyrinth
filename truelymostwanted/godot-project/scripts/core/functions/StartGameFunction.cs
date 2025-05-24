using Godot;
using LabyrinthExplorer3D.scripts.core.menus;
using LabyrinthExplorer3D.scripts.game.gui;

namespace LabyrinthExplorer3D.scripts.core.functions;

[GlobalClass]
public partial class StartGameFunction : Function, IFunction
{
    public static void Execute(Node node)
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        MenuController.Instance.ToggleCurrentMenu(false);
        node.GetTree().SetPause(false);   
        GameUI.Instance.Show();
    }
    
    public override void Execute()
    {
        Execute(node: this);   
    }
}