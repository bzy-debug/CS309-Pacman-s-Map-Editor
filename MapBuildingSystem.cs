using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using CodeMonkey.Utils;

public class MapBuildingSystem : MonoBehaviour
{
    [SerializeField] private List<PlacedObjectTypeSO> placedObjectTypeSOList;
    private MyGrid<GridObject> grid;
    private PlacedObjectTypeSO.Dir dir = PlacedObjectTypeSO.Dir.Down;
    private int index;

    private void Awake() {
        int gridwidth = 30;
        int girdheight = 10;
        float cellSize = 10f;
        Vector3 originalPosition = new Vector3(-150, -50);
        grid = new MyGrid<GridObject>(gridwidth, girdheight, cellSize, originalPosition, (MyGrid<GridObject> g, int x, int y) => new GridObject(g, x, y));
        File.CreateText(Application.dataPath + "/buildlog.txt");
        index = 0;
    }

    public class GridObject{
        private MyGrid<GridObject> grid;
        private int x;
        private int y;
        private PlacedObject placedObject;

        public GridObject(MyGrid<GridObject> grid, int x, int y) {
            this.grid = grid;
            this.x = x;
            this.y = y;
        }

        public void setPlacedObject(PlacedObject placedObject){
            this.placedObject = placedObject;
            grid.TriggerGridObjectChanged(x, y);
        }

        public void clearPlacedObject() {
            placedObject = null;
            grid.TriggerGridObjectChanged(x, y);
        }

        public PlacedObject GetPlacedObject(){
            return this.placedObject;
        }

        public bool CanBuild() {
            return placedObject == null;
        }

        public override string ToString()
        {
            return x + " " + y +" " + placedObject?.index + "\n";
        }

    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.B)) {
            grid.GetXY(UtilsClass.GetMouseWorldPosition(), out int x, out int y);
            Build(x, y, index);
        }

        if (Input.GetMouseButton(1)) {
            Destroy(UtilsClass.GetMouseWorldPosition());
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            dir = PlacedObjectTypeSO.GetNextDir(dir);
            UtilsClass.CreateWorldTextPopup(dir.ToString(), UtilsClass.GetMouseWorldPosition());
        }

        if (Input.GetKeyDown(KeyCode.S)) {
            Save();
        }

        if (Input.GetKeyDown(KeyCode.L)) {
            Load();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) { index = 0;}
        if (Input.GetKeyDown(KeyCode.Alpha2)) { index = 1;}
        if (Input.GetKeyDown(KeyCode.Alpha3)) { index = 2;}
        if (Input.GetKeyDown(KeyCode.Alpha4)) { index = 3;}
        if (Input.GetKeyDown(KeyCode.Alpha5)) { index = 4;}
    }

    private void Build(int x, int y, int index){
        PlacedObjectTypeSO placedObjectTypeSO = placedObjectTypeSOList[index];
        List<Vector2Int> gridPositionList = placedObjectTypeSO.GetGridPositionList(new Vector2Int(x, y), dir);

        bool canBuild = true;
        foreach(Vector2Int gridPosition in gridPositionList){
            if(grid.GetGridObject(gridPosition.x, gridPosition.y) == null){
                UtilsClass.CreateWorldTextPopup("out of range", UtilsClass.GetMouseWorldPosition());
                return;
            }
            if (!grid.GetGridObject(gridPosition.x, gridPosition.y).CanBuild()){
                canBuild = false;
                break;
            }
        }

        if (canBuild){
            Vector2Int rotationOffset = placedObjectTypeSO.GetRotationOffset(dir);
            Vector3 placedObjectWorldPosition = grid.GetWorldPosition(x, y) +
            new Vector3(rotationOffset.x, rotationOffset.y, 0) * grid.GetCellSize();

            using (FileStream buildFs = File.Open(Application.dataPath+"/buildlog.txt", FileMode.Append)){
                Byte[] info = 
                new UTF8Encoding(true).GetBytes(placedObjectWorldPosition.ToString()+" "
                + new Vector2Int(x, y) + " " + dir + " " + index + "\n");
                buildFs.Write(info,0, info.Length);
            }

            PlacedObject placedObject = PlacedObject.Create(placedObjectWorldPosition, new Vector2Int(x, y), dir, placedObjectTypeSO);
            placedObject.index = index;
            foreach (Vector2Int gridPosition in gridPositionList){
                grid.GetGridObject(gridPosition.x, gridPosition.y).setPlacedObject(placedObject);
            }
        } else {
            UtilsClass.CreateWorldTextPopup("cannot build", UtilsClass.GetMouseWorldPosition());
        }
    }
    private void Destroy(Vector3 position){
        GridObject gridObject = grid.GetGridObject(position);
        PlacedObject placedObject = gridObject.GetPlacedObject();
        if(placedObject == null){
            return;
        }
        using (FileStream fs = File.Open(Application.dataPath + "/buildlog.txt", FileMode.Append)) {
            Byte[] info = 
            new UTF8Encoding(true).GetBytes( placedObject.GetOrigin() +" Destory" + "\n");
            fs.Write(info, 0, info.Length);
        }

        if (placedObject != null) {
            placedObject.DestroySelf();

            List<Vector2Int> gridPositionList = placedObject.GetGridPositionList();

            foreach (Vector2Int gridPosition in gridPositionList) {
                grid.GetGridObject(gridPosition.x, gridPosition.y).clearPlacedObject();
            }
        }
    }
    private void Destroy(int x, int y){
        GridObject gridObject = grid.GetGridObject(x, y);
        PlacedObject placedObject = gridObject.GetPlacedObject();
        if (placedObject != null) {
            placedObject.DestroySelf();

            List<Vector2Int> gridPositionList = placedObject.GetGridPositionList();

            foreach (Vector2Int gridPosition in gridPositionList) {
                grid.GetGridObject(gridPosition.x, gridPosition.y).clearPlacedObject();
            }
        }
    }
    private void Save() {
        UtilsClass.CreateWorldTextPopup("save", UtilsClass.GetMouseWorldPosition());
        string savePath = Application.dataPath+"/save.txt";
        File.Delete(savePath);
        File.Copy(Application.dataPath+"/buildlog.txt", savePath, true);
    }

    private void Load() {
        UtilsClass.CreateWorldTextPopup("load", UtilsClass.GetMouseWorldPosition());
        string filePath = Application.dataPath+"/save.txt";
        if(File.Exists(filePath)){
            File.Copy(filePath, Application.dataPath+"/buildlog.txt",true);
            for(int x=0; x<grid.GetWidth(); x++){
                for(int y=0; y<grid.GetHeight(); y++){
                    Destroy(x, y);
                }
            }            
            using(StreamReader sr = File.OpenText(filePath)){
                string line;
                while((line = sr.ReadLine()) != null) {
                    string[] words = line.Split();

                    if (words.Length == 3){
                        Destroy(int.Parse(words[0].Substring(1, words[0].Length-2)),
                            int.Parse(words[1].Substring(0, words[1].Length-1)));
                        continue;
                    }
                    
                    Vector3 placedObjectWorldPosition = new Vector3(
                        float.Parse(words[0].Substring(1, words[0].Length-2)),
                        float.Parse(words[1].Substring(0, words[1].Length-2)),
                        float.Parse(words[2].Substring(0, words[2].Length-2))
                    );

                    Vector2Int xy = new Vector2Int(
                        int.Parse(words[3].Substring(1, words[3].Length-2)),
                        int.Parse(words[4].Substring(0, words[4].Length-1))
                    );

                    PlacedObjectTypeSO.Dir dir = PlacedObjectTypeSO.DirParse(words[5]);

                    PlacedObjectTypeSO placedObjectTypeSO = placedObjectTypeSOList[int.Parse(words[6])];
                    List<Vector2Int> gridPositionList = placedObjectTypeSO.GetGridPositionList(xy, dir);

                    PlacedObject placedObject = PlacedObject.Create(placedObjectWorldPosition, xy, dir, placedObjectTypeSO);
                    placedObject.index = index;
                    foreach (Vector2Int gridPosition in gridPositionList){
                        grid.GetGridObject(gridPosition.x, gridPosition.y).setPlacedObject(placedObject);
                    }

                }
            }
        }
    }
}
