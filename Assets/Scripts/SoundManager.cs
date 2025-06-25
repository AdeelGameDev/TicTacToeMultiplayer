using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private Transform placeSFXTransform;
    [SerializeField] private Transform winSFXTransform;
    [SerializeField] private Transform loseSFXTransform;

    private void Start()
    {
        GameManager.Instance.OnPlacedObject += GameManager_OnPlacedObject;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerType() == e.winPlayerType)
        {
            Transform sfxTransform = Instantiate(winSFXTransform);
            Destroy(sfxTransform.gameObject, 5);
        }
        else
        {
            Transform sfxTransform = Instantiate(loseSFXTransform);
            Destroy(sfxTransform.gameObject, 5);
        }
    }

    private void GameManager_OnPlacedObject(object sender, System.EventArgs e)
    {
        Transform sfxTransform = Instantiate(placeSFXTransform);
        Destroy(sfxTransform.gameObject, 5);
    }
}
