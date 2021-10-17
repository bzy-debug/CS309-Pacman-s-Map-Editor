using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using System;

public class MyGrid<T>
{

    public class OnGridValueChangedEventArgs : EventArgs {
        public int x;
        public int y;
    }
    public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private T[,] gridArray;

    public float GetCellSize(){
        return cellSize;
    }

    public MyGrid(int width, int height, float cellSize, Vector3 originPosition, Func<MyGrid<T>, int, int, T> createGridObject){
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;
 
        gridArray = new T[width, height];

        for (int x=0; x<width; x++){
            for(int y=0; y<width; y++){
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }        

        bool showDebug = true;
        if (showDebug){     
            TextMesh[,] debugTextArray = new TextMesh[width, height];

            for (int x=0; x<gridArray.GetLength(0); x++){
                for (int y=0; y<gridArray.GetLength(1); y++){
                    debugTextArray[x, y] = UtilsClass.CreateWorldText(gridArray[x, y].ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * .5f, 20, Color.white, TextAnchor.MiddleCenter);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y+1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x+1, y), Color.white, 100f);
                }
            }
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);

            OnGridValueChanged += (object sender, OnGridValueChangedEventArgs eventArgs) => {
                debugTextArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString();
            };
        }
    }

    public Vector3 GetWorldPosition(int x, int y){
        return (new Vector3(x, y) * cellSize + originPosition);
    }

    public void GetXY (Vector3 worldPosition, out int x, out int y){
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
    }

    public void SetGridObject(int x, int y, T gridObject){
        if (x >=0 && x < width && y >= 0 && y < height){
            gridArray[x, y] = gridObject;
        }
    }

    public void TriggerGridObjectChanged(int x, int y) {
        if (OnGridValueChanged != null) {
            OnGridValueChanged(this, new OnGridValueChangedEventArgs {x = x, y = y});
        }
    }


    public void SetGridObject(Vector3 worldPosition, T gridObject){
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetGridObject(x, y, gridObject);
    }

    public T GetGridObject(int x, int y) {
        if (x >= 0 && x < width && y >= 0 && y < width){
            return gridArray[x, y];
        } else {
            return default(T);
        }
    }

    public T GetGridObject(Vector3 worldPosition) {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetGridObject(x, y);
    }
}
