using Godot;
using LabyrinthExplorer3D.scripts.core.menus;
using LabyrinthExplorer3D.scripts.game.gui;
using LabyrinthExplorer3D.scripts.game.time;

namespace LabyrinthExplorer3D.scripts.core.functions;

[GlobalClass]
public partial class StartGameFunction : Function, IFunction
{
    public static void Execute(Node node)
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        TimeController.Instance.SetTime(0, 0);
        node.GetTree().SetPause(false);   
        MenuController.Instance.ToggleCurrentMenu(false);
        GameUI.Instance.Show();
    }
    
    public override void Execute()
    {
        Execute(node: this);   
    }
}