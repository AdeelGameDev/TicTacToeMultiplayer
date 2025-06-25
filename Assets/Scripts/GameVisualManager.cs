using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    private const float GRID_SIZE = 3.1f;
    private List<GameObject> visualGameObjectList;

    [SerializeField] private Transform crossPrefab, circlePrefab, lineCompletePrefab;

    private void Awake()
    {
        visualGameObjectList = new List<GameObject>();
    }

    private void Start()
    {
        GameManager.Instance.OnClickedOnGridPosition += GameManager_OnClickedOnGridPosition;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnRematch += GameManager_OnRematch;
    }

    private void GameManager_OnRematch(object sender, EventArgs e)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        foreach (GameObject visualGameObject in visualGameObjectList)
        {
            DestroyImmediate(visualGameObject, true);
        }
        visualGameObjectList.Clear();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;
        float eulerZ = 0f;
        switch (e.line.orientation)
        {
            case GameManager.Orientation.Horizontal:
                eulerZ = 0f;
                break;
            case GameManager.Orientation.Vertical:
                eulerZ = 90f;
                break;
            case GameManager.Orientation.DiagonalA:
                eulerZ = 45f;
                break;
            case GameManager.Orientation.DiagonalB:
                eulerZ = -45f;
                break;
            default:
                break;
        }

        Transform lineCompleteTransform =
            Instantiate(lineCompletePrefab, GetGridWorldPosition(e.line.centerGridPosition.x, e.line.centerGridPosition.y), Quaternion.Euler(0, 0, eulerZ));
        lineCompleteTransform.GetComponent<NetworkObject>().Spawn(true);

        visualGameObjectList.Add(lineCompleteTransform.gameObject);
    }

    private void GameManager_OnClickedOnGridPosition(object sender, GameManager.OnClickedOnGridPositionEventArgs e)
    {
        Debug.Log("GameManager_OnClickedOnGridPosition");
        SpawnObjectRpc(e.x, e.y, e.playerType);
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x, int y, GameManager.PlayerType playerType)
    {
        Debug.Log("SpawnObjectRpc");
        Transform prefab = playerType == GameManager.PlayerType.Cross ? crossPrefab : circlePrefab;
        Transform spawnedObjectTransfrom = Instantiate(prefab, GetGridWorldPosition(x, y), Quaternion.identity);
        spawnedObjectTransfrom.GetComponent<NetworkObject>().Spawn(true);

        visualGameObjectList.Add(spawnedObjectTransfrom.gameObject);
    }

    private Vector2 GetGridWorldPosition(float x, float y) => new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
}
