using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PlacedObjectTypeSO : ScriptableObject
{
    public static Dir GetNextDir(Dir dir){
        switch (dir){
            default:
            case Dir.Down:  return Dir.Left;
            case Dir.Left:  return Dir.Up;
            case Dir.Up:    return Dir.Right;
            case Dir.Right: return Dir.Down;
        }
    }
    public enum Dir{
        Down, Up, Left, Right
    }
    public Transform prefab;
    public int width;
    public int height;

    public static Dir DirParse(string value){
        switch(value) {
            default: throw new System.Exception();
            case "Down": return Dir.Down;
            case "Left": return Dir.Left;
            case "Up":   return Dir.Up;
            case "Right": return Dir.Right;
        }
    }

    public int GetRotationAngle(Dir dir){
        switch(dir){
            default:
            case Dir.Down:  return 0;
            case Dir.Left:  return 90;
            case Dir.Up:    return 180;
            case Dir.Right: return 270;
        }
    }

    public Vector2Int GetRotationOffset(Dir dir){
        switch(dir) {
            default:
            case Dir.Down:  return new Vector2Int(0, 0);
            case Dir.Left:  return new Vector2Int(height, 0);
            case Dir.Up:    return new Vector2Int(width, height);
            case Dir.Right: return new Vector2Int(0, width);
        }
        // return new Vector2Int(0, 0);
    }

    public List<Vector2Int> GetGridPositionList(Vector2Int offset, Dir dir){
        List<Vector2Int> gridPositionList = new List<Vector2Int>();
        switch(dir){
            default:
            case Dir.Down:
            case Dir.Up:
                for (int x=0; x<width; x++){
                    for (int y=0; y<height; y++){
                        gridPositionList.Add(offset + new Vector2Int(x, y));
                    }
                }
                break;
            case Dir.Left:
            case Dir.Right:
                for (int x=0; x<height; x++){
                    for (int y=0; y<width; y++){
                        gridPositionList.Add(offset + new Vector2Int(x, y));
                    }
                }
                break;
        }
        return gridPositionList;
    }
}
