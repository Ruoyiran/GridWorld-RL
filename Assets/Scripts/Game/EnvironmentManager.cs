﻿using UnityEngine;

public class EnvironmentManager : MonoBehaviour {
    public int gridSize = 5;
    public int numObstacles = 1;
    public int numGoals = 1;
    private GameObject _planeObj;
    private GameObject _eastWallObj;
    private GameObject _westWallObj;
    private GameObject _northWallObj;
    private GameObject _southWallObj;

    void Start()
    {
        InitEnvironmentObjects();
        SetEnvironment();
    }

    private void InitEnvironmentObjects()
    {
        _planeObj = CTool.FindGameObject(gameObject, "Plane");
        _eastWallObj = CTool.FindGameObject(gameObject, "East");
        _westWallObj = CTool.FindGameObject(gameObject, "West");
        _northWallObj = CTool.FindGameObject(gameObject, "North");
        _southWallObj = CTool.FindGameObject(gameObject, "South");
    }

    private void SetEnvironment()
    {
        Camera mainCamera = Camera.main;
        mainCamera.transform.position = new Vector3(-(gridSize - 1) / 2f, gridSize * 1.25f, -(gridSize - 1) / 2f);
        mainCamera.orthographicSize = (gridSize + 5f) / 2f;

        _planeObj.transform.localScale = new Vector3(gridSize / 10.0f, 1f, gridSize / 10.0f);
        _planeObj.transform.position = new Vector3((gridSize - 1) / 2f, -0.5f, (gridSize - 1) / 2f);

        _eastWallObj.transform.position = new Vector3(gridSize, 0.0f, (gridSize - 1) / 2f);
        _eastWallObj.transform.localScale = new Vector3(1, 1, gridSize + 2);

        _westWallObj.transform.position = new Vector3(-1, 0.0f, (gridSize - 1) / 2f);
        _westWallObj.transform.localScale = new Vector3(1, 1, gridSize + 2);

        _southWallObj.transform.position = new Vector3((gridSize - 1) / 2f, 0.0f, -1);
        _southWallObj.transform.localScale = new Vector3(1, 1, gridSize + 2);

        _northWallObj.transform.position = new Vector3((gridSize - 1) / 2f, 0.0f, gridSize);
        _northWallObj.transform.localScale = new Vector3(1, 1, gridSize + 2);
    }
}
