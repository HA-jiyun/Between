using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModePopup : MonoBehaviour
{
    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;
    private void Awake()
    {
        easyButton.onClick.AddListener(EasyMode);
        normalButton.onClick.AddListener(NormalMode);
        hardButton.onClick.AddListener(HardMode);
    }

    void EasyMode()
    {
        GameManager.instance.SetMode(Mode.Easy);
        Close();
    }

    void NormalMode()
    {
        GameManager.instance.SetMode(Mode.Normal);
        Close();
    }

    void HardMode()
    {
        GameManager.instance.SetMode(Mode.Hard);
        Close();
    }

    void Close()
    {
        gameObject.SetActive(false);
        Debug.Log("Selected Mode: " + GameManager.instance.selectedMode);
        TurnManager.instance.StartGame();
    }
}
