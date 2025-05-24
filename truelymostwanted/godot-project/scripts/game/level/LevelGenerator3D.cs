using System;
using Godot;
using Godot.Collections;

namespace LabyrinthExplorer3D.scripts.game.level;

[GlobalClass]
public partial class LevelGenerator3D : Node3D
{
    [Flags]
    public enum Neighbours
    {
        None = 0,
        E = 1,
        S = 2,
        W = 4,
        N = 8,
        
        ES = E + S,
        EW = E + W,
        EN = E + N,
        SW = S + W, 
        SN = S + N,
        WN = W + N,
        
        ESW = E + S + W,
        ESN = E + S + N,
        EWN = E + W + N,
        SWN = S + W + N,
        
        ESWN = E + S + W + N,
    }

    [Export] public LevelController3D LevelController;
    [Export] public LevelTileSetDictionary LevelTileSetDictionary;
    
    public bool IsEmptyField(Image img, Vector2I imgSize, int x, int y)
    {
        return img.GetPixel(x, y) == Colors.Black;
    }
    public Neighbours GetNeighbours(Image img, Vector2I imgSize, int x, int y, bool skipCheck = true)
    {
        var neighbours = Neighbours.None;
        
        var left = new Vector2I(x - 1, y);
        if(left.X >= 0 && img.GetPixel(left.X, left.Y) == Colors.White)
            neighbours |= Neighbours.W;
        
        var right = new Vector2I(x + 1, y);
        if(right.X < imgSize.X && img.GetPixel(right.X, right.Y) == Colors.White)
            neighbours |= Neighbours.E;
        
        var up = new Vector2I(x, y - 1);
        if(up.Y >= 0 && img.GetPixel(up.X, up.Y) == Colors.White)
            neighbours |= Neighbours.N;
        
        var down = new Vector2I(x, y + 1);
        if(down.Y < imgSize.Y && img.GetPixel(down.X, down.Y) == Colors.White)
            neighbours |= Neighbours.S;
        
        return neighbours;
    }

    public string GetTileType()
    {
        var chanceForTransparency = 0.15f;
        var randomValue = GD.Randf();
        return randomValue <= chanceForTransparency 
            ? "glas" 
            : "default";
    }
    public string GetTileMeshName(Neighbours neighbours)
    {
        return neighbours.ToString().Replace(", ", ""); //"E, S, W, N" --> "ESWN"
    }
    public bool TryGetMesh(string type, string meshName, out Mesh mesh)
    {
        var canGetType = LevelTileSetDictionary.LevelTileSets.TryGetValue(type, out var tileSet);
        if (!canGetType)
        {
            mesh = null;
            return false;
        }
        
        var canGetTile = tileSet.TileMeshes.TryGetValue(meshName, out mesh);
        return canGetTile;
    }
    public bool TryInstantiateMesh(string nodeName, Vector3 globalPos, Mesh mesh)
    {
        try
        {
            var meshInstance = new MeshInstance3D();
            meshInstance.Mesh = mesh;
            meshInstance.CreateTrimeshCollision();
            LevelController.CurrentLevel.AddChild(meshInstance);
            meshInstance.GlobalPosition = globalPos;
            return true;
        }
        catch (Exception exception)
        {
            GD.PrintErr(exception);
            return false;
        }
    }
    
    public bool TryGenerateLevel3D(string pngFilePath)
    {
        try
        {
            //(1) Read all children
            var children = LevelController.CurrentLevel.GetChildren();
            
            //(2) Remove all children
            for (int i = children.Count - 1; i >= 0; i--)
            {
                LevelController.CurrentLevel.RemoveChild(children[i]);
                children[i].QueueFree();
            }
            
            //(3) Load the texture as image
            var texture2D = ResourceLoader.Load<Texture2D>(pngFilePath);
            var img = texture2D.GetImage();
            
            //(4) Iterate over the pixel and create the level
            var imgSize = img.GetSize();
            for (int y = 0; y < imgSize.Y; y++)
            {
                for (int x = 0; x < imgSize.X; x++)
                {
                    if (IsEmptyField(img, imgSize, x, y)) 
                        continue;
                    
                    var neighbours = GetNeighbours(img, imgSize, x, y);
                    var typeName = GetTileType();
                    var meshName = GetTileMeshName(neighbours);
                    if (!TryGetMesh(typeName, meshName, out var mesh)) 
                        continue;
                    var position = new Vector3(x * 4, 0, y * 4);
                    TryInstantiateMesh(meshName, position, mesh);
                }
            }
            return true;
        }
        catch (Exception exception)
        {
            GD.PrintErr(exception);
            return false;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        var canLoad = TryGenerateLevel3D("res://resources/textures/LVL_0.png");
        GD.Print($"Can Load: {canLoad}");
    }
}