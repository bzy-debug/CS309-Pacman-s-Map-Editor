using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class MapBuildingSystem : MonoBehaviour
{
    [SerializeField] private Transform testTransform;
    private MyGrid<GridObject> grid;

    private void Awake() {
        int gridwidth = 10;
        int girdheight = 10;
        float cellSize = 10f;
        Vector3 originalPosition = new Vector3(-50, -50);
        grid = new MyGrid<GridObject>(gridwidth, girdheight, cellSize, originalPosition, (MyGrid<GridObject> g, int x, int y) => new GridObject(g, x, y));
    }

    public class GridObject{
        private MyGrid<GridObject> grid;
        private int x;
        private int y;
        private Transform transform;

        public GridObject(MyGrid<GridObject> grid, int x, int y) {
            this.grid = grid;
            this.x = x;
            this.y = y;
        }

        public void setTransform(Transform transform){
            this.transform = transform;
        }

        public void clearTransform() {
            transform = null;
        }

        public Transform GetTransform(){
            return this.transform;
        }

        public bool CanBuild() {
            return transform == null;
        }

        public override string ToString()
        {
            return x + ", " + y;
        }
    }

    private void Update() {
        if (Input.GetMouseButton(0)) {
            grid.GetXY(UtilsClass.GetMouseWorldPosition(), out int x, out int y);
            GridObject gridObject = grid.GetGridObject(x, y);
            if (gridObject.CanBuild()){
                Transform builtTransform = Instantiate(testTransform, grid.GetWorldPosition(x, y), Quaternion.identity);
                gridObject.setTransform(builtTransform);
            }
        }
        if (Input.GetMouseButton(1)) {
            grid.GetXY(UtilsClass.GetMouseWorldPosition(), out int x, out int y);
            GridObject gridObject = grid.GetGridObject(x, y);
            if (!gridObject.CanBuild()){
                Destroy(gridObject.GetTransform().gameObject);
                gridObject.clearTransform();
            }
        }
    }
}
