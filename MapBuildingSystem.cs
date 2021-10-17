using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class MapBuildingSystem : MonoBehaviour
{
    [SerializeField] private List<PlacedObjectTypeSO> placedObjectTypeSOList;
    private PlacedObjectTypeSO placedObjectTypeSO;
    private MyGrid<GridObject> grid;
    private PlacedObjectTypeSO.Dir dir = PlacedObjectTypeSO.Dir.Down;

    private void Awake() {
        int gridwidth = 10;
        int girdheight = 10;
        float cellSize = 10f;
        Vector3 originalPosition = new Vector3(-50, -50);
        grid = new MyGrid<GridObject>(gridwidth, girdheight, cellSize, originalPosition, (MyGrid<GridObject> g, int x, int y) => new GridObject(g, x, y));

        placedObjectTypeSO = placedObjectTypeSOList[0];
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
            return x + ", " + y +"\n" + placedObject;
        }

    }

    private void Update() {
        if (Input.GetMouseButton(0)) {
            grid.GetXY(UtilsClass.GetMouseWorldPosition(), out int x, out int y);
            List<Vector2Int> gridPositionList = placedObjectTypeSO.GetGridPositionList(new Vector2Int(x, y), dir);
            
            bool canBuild = true;
            foreach(Vector2Int gridPosition in gridPositionList){
                if (!grid.GetGridObject(gridPosition.x, gridPosition.y).CanBuild()){
                    canBuild = false;
                    break;
                }
            }

            // GridObject gridObject = grid.GetGridObject(x, y);
            if (canBuild){
                Vector2Int rotationOffset = placedObjectTypeSO.GetRotationOffset(dir);
                Vector3 placedObjectWorldPosition = grid.GetWorldPosition(x, y) +
                    new Vector3(rotationOffset.x, rotationOffset.y, 0) * grid.GetCellSize();
                
                PlacedObject placedObject = PlacedObject.Create(placedObjectWorldPosition, new Vector2Int(x, y), dir, placedObjectTypeSO);

                foreach (Vector2Int gridPosition in gridPositionList){
                    grid.GetGridObject(gridPosition.x, gridPosition.y).setPlacedObject(placedObject);
                }
            } else {
                UtilsClass.CreateWorldTextPopup("cannot build", UtilsClass.GetMouseWorldPosition());
            }
        }

        if (Input.GetMouseButton(1)) {
            GridObject gridObject = grid.GetGridObject(UtilsClass.GetMouseWorldPosition());
            PlacedObject placedObject = gridObject.GetPlacedObject();
            if (placedObject != null) {
                placedObject.DestroySelf();

                List<Vector2Int> gridPositionList = placedObject.GetGridPositionList();

                foreach (Vector2Int gridPosition in gridPositionList) {
                    grid.GetGridObject(gridPosition.x, gridPosition.y).clearPlacedObject();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            dir = PlacedObjectTypeSO.GetNextDir(dir);
            UtilsClass.CreateWorldTextPopup(dir.ToString(), UtilsClass.GetMouseWorldPosition());
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) { placedObjectTypeSO = placedObjectTypeSOList[0]; }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { placedObjectTypeSO = placedObjectTypeSOList[1]; }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { placedObjectTypeSO = placedObjectTypeSOList[2]; }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { placedObjectTypeSO = placedObjectTypeSOList[3]; }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { placedObjectTypeSO = placedObjectTypeSOList[4]; }
    }
}
