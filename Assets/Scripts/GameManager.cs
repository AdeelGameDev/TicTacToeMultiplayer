using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public event EventHandler OnGameStarted;
    public event EventHandler<OnGameWinEventArgs> OnGameWin;
    public event EventHandler OnRematch;
    public event EventHandler OnGameTied;
    public event EventHandler OnScoreChanged;
    public event EventHandler OnPlacedObject;
    public class OnGameWinEventArgs : EventArgs
    {
        public Line line;
        public PlayerType winPlayerType;
    };
    public event EventHandler OnCurrentPlayablePlayerTypeChanged;

    public enum PlayerType
    {
        None,
        Cross,
        Circle
    }

    public enum Orientation
    {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB,
    }

    public struct Line
    {
        public List<Vector2Int> gridVector2IntList;
        public Vector2 centerGridPosition;
        public Orientation orientation;
    }

    private PlayerType localPlayerType;
    private NetworkVariable<PlayerType> currentPlayablePlayerType = new NetworkVariable<PlayerType>();
    private NetworkVariable<int> playerCrossScore = new NetworkVariable<int>();
    private NetworkVariable<int> playerCircleScore = new NetworkVariable<int>();

    private PlayerType[,] playerTypeArray;

    private List<Line> lineList;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one Game Manager in the scene.");
        }
        Instance = this;

        playerTypeArray = new PlayerType[3, 3];

        lineList = new List<Line>()
        {
            // Horizontal
            new Line
            {
                gridVector2IntList = new List<Vector2Int>() { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0)},
                centerGridPosition = new Vector2Int(1, 0),
                orientation = Orientation.Horizontal
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int>() { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1)},
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Horizontal
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int>() { new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2)},
                centerGridPosition = new Vector2Int(1, 2),
                orientation = Orientation.Horizontal
            },

            // Vertical
            new Line
            {
                gridVector2IntList = new List<Vector2Int>() { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2)},
                centerGridPosition = new Vector2Int(0, 1),
                orientation = Orientation.Vertical
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int>() { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2)},
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Vertical
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int>() { new Vector2Int(2, 0), new Vector2Int(2, 1), new Vector2Int(2, 2)},
                centerGridPosition = new Vector2Int(2, 1),
                orientation = Orientation.Vertical
            },

            // Diagonal
            new Line
            {
                gridVector2IntList = new List<Vector2Int>() { new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(2, 2)},
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalA
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int>() { new Vector2Int(0, 2), new Vector2Int(1, 1), new Vector2Int(2, 0)},
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalB
            },
        };
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn " + NetworkManager.Singleton.LocalClientId);
        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            localPlayerType = PlayerType.Cross;
        }
        else
        {
            localPlayerType = PlayerType.Circle;
        }

        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        currentPlayablePlayerType.OnValueChanged += (PlayerType previousValue, PlayerType newValue) =>
        {
            OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
        };

        playerCircleScore.OnValueChanged += (int previousValue, int newValue) =>
        {
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
        };
        playerCrossScore.OnValueChanged += (int previousValue, int newValue) =>
        {
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count == 2)
        {
            currentPlayablePlayerType.Value = PlayerType.Cross;
            TriggerOnGameStartedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc() => OnGameStarted?.Invoke(this, EventArgs.Empty);


    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        Debug.Log("ClickedOnGridPosition " + x + ", " + y);

        if (playerType != currentPlayablePlayerType.Value) return;

        if (playerTypeArray[x, y] != PlayerType.None) return;
        playerTypeArray[x, y] = playerType;

        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs
        {
            x = x,
            y = y,
            playerType = playerType
        });

        OnTriggerPlacedObjectRpc();


        switch (currentPlayablePlayerType.Value)
        {
            default:
            case PlayerType.Cross:
                currentPlayablePlayerType.Value = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                currentPlayablePlayerType.Value = PlayerType.Cross;
                break;
        }

        TestWinner();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void OnTriggerPlacedObjectRpc()
    {
        OnPlacedObject?.Invoke(this, EventArgs.Empty);
    }

    private bool TestWinnerLine(Line line)
    {
        return TestWinnerLine(
            playerTypeArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y],
            playerTypeArray[line.gridVector2IntList[1].x, line.gridVector2IntList[1].y],
            playerTypeArray[line.gridVector2IntList[2].x, line.gridVector2IntList[2].y]
            );
    }
    private bool TestWinnerLine(PlayerType aPlayerType, PlayerType bPlayerType, PlayerType cPlayerType)
    {
        return
        aPlayerType != PlayerType.None &&
            aPlayerType == bPlayerType &&
            bPlayerType == cPlayerType;
    }

    private void TestWinner()
    {
        for (int i = 0; i < lineList.Count; i++)
        {
            Line line = lineList[i];
            if (TestWinnerLine(line))
            {
                Debug.Log("Winner!");
                currentPlayablePlayerType.Value = PlayerType.None;

                PlayerType playerType = playerTypeArray[(int)line.centerGridPosition.x, (int)line.centerGridPosition.y];

                switch (playerType)
                {
                    case PlayerType.Cross:
                        playerCrossScore.Value++;
                        break;
                    case PlayerType.Circle:
                        playerCircleScore.Value++;
                        break;
                    default:
                        break;
                }

                TriggerOnGameWinRpc(i, playerType);
                break;
            }
        }

        // Check for tie (all positions filled)
        bool isTie = true;
        for (int x = 0; x < playerTypeArray.GetLength(0); x++)
        {
            for (int y = 0; y < playerTypeArray.GetLength(1); y++)
            {
                if (playerTypeArray[x, y] == PlayerType.None)
                {
                    isTie = false;
                    break;
                }
            }
            if (!isTie) break;
        }

        if (isTie)
        {
            Debug.Log("Game is a Tie!");
            TriggerOnGameTiedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameTiedRpc()
    {
        OnGameTied?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameWinRpc(int lineIndex, PlayerType winPlayerType)
    {
        Line line = lineList[lineIndex];

        OnGameWin?.Invoke(this, new OnGameWinEventArgs
        {
            line = line,
            winPlayerType = winPlayerType,
        });

    }

    public PlayerType GetLocalPlayerType() => localPlayerType;

    public PlayerType GetCurrentPlayablePlayerType() => currentPlayablePlayerType.Value;


    [Rpc(SendTo.Server)]
    public void RematchRpc()
    {
        for (int i = 0; i < playerTypeArray.GetLength(0); i++)
        {
            for (int j = 0; j < playerTypeArray.GetLength(1); j++)
            {
                playerTypeArray[i, j] = PlayerType.None;
            }
        }

        currentPlayablePlayerType.Value = PlayerType.Cross;

        OnTriggerRematchRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void OnTriggerRematchRpc()
    {
        OnRematch?.Invoke(this, EventArgs.Empty);
    }
    public void GetScore(out int playerCircleScore, out int playerCrossScore)
    {
        playerCircleScore = this.playerCircleScore.Value;
        playerCrossScore = this.playerCrossScore.Value;
    }
}
