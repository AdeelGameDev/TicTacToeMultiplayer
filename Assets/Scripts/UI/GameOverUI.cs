using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultTextMesh;
    [SerializeField] private Color winColor = Color.green;
    [SerializeField] private Color loseColor = Color.red;
    [SerializeField] private Color tieColor = Color.yellow;
    [SerializeField] private Button rematchButton;

    private void Awake()
    {
        rematchButton.onClick.AddListener(() =>
        {
            GameManager.Instance.RematchRpc();
        });
    }

    private void Start()
    {
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnRematch += GameManager_OnRematch;
        GameManager.Instance.OnGameTied += GameManager_OnGameTied;

        Hide();
    }

    private void GameManager_OnGameTied(object sender, System.EventArgs e)
    {
        resultTextMesh.color = tieColor;
        resultTextMesh.text = "Tie!";
        Show();
    }
    private void GameManager_OnRematch(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameWin -= GameManager_OnGameWin;
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (e.winPlayerType == GameManager.Instance.GetLocalPlayerType())
        {
            resultTextMesh.color = winColor;
            resultTextMesh.text = "You Win!";
        }
        else
        {
            resultTextMesh.color = loseColor;
            resultTextMesh.text = "You Lose!";
        }
        Show();
    }

    private void Show() => gameObject.SetActive(true);

    private void Hide() => gameObject.SetActive(false);
}
