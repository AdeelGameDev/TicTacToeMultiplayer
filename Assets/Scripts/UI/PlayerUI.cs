using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject crossArrowImageGameObject;
    [SerializeField] private GameObject circleArrowImageGameObject;
    [SerializeField] private GameObject circleYouTextGameObject;
    [SerializeField] private GameObject crossYouTextGameObject;

    [SerializeField] private TextMeshProUGUI playerCircleScoreTextMesh;
    [SerializeField] private TextMeshProUGUI playerCrossScoreTextMesh;

    private void Awake()
    {
        crossArrowImageGameObject.SetActive(false);
        circleArrowImageGameObject.SetActive(false);
        circleYouTextGameObject.SetActive(false);
        crossYouTextGameObject.SetActive(false);

        playerCircleScoreTextMesh.text = "";
        playerCrossScoreTextMesh.text = "";
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerTypeChanged += GameManager_OnCurrentPlayablePlayerTypeChanged;
        GameManager.Instance.OnScoreChanged += GameManager_OnScoreChanged;
    }

    private void GameManager_OnScoreChanged(object sender, System.EventArgs e)
    {
        GameManager.Instance.GetScore(out int circleScore, out int crossScore);

        playerCircleScoreTextMesh.text = circleScore.ToString();
        playerCrossScoreTextMesh.text = crossScore.ToString();
    }

    private void GameManager_OnCurrentPlayablePlayerTypeChanged(object sender, System.EventArgs e)
    {
        UpdateCurrentArrow();
    }

    private void GameManager_OnGameStarted(object sender, System.EventArgs e)
    {
        (GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.Cross
            ? crossYouTextGameObject
            : circleYouTextGameObject).SetActive(true);

        playerCircleScoreTextMesh.text = "0";
        playerCrossScoreTextMesh.text = "0";
    }

    private void UpdateCurrentArrow()
    {
        if (GameManager.Instance.GetCurrentPlayablePlayerType() == GameManager.PlayerType.Cross)
        {
            crossArrowImageGameObject.SetActive(true);
            circleArrowImageGameObject.SetActive(false);
        }
        else
        {
            crossArrowImageGameObject.SetActive(false);
            circleArrowImageGameObject.SetActive(true);
        }

    }
}
