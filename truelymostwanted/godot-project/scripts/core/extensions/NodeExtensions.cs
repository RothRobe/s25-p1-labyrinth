using System.Collections.Generic;
using System.Linq;
using Godot;

namespace LabyrinthExplorer3D.scripts.core.extensions;

public static class NodeExtensions
{
    public static int GetChildrenOfType<T>(this Node node, out List<T> childrenOfType) where T : Node
    {
        var children = node.GetChildren();
        childrenOfType = children.OfType<T>().ToList();
        return childrenOfType.Count;
    }
}