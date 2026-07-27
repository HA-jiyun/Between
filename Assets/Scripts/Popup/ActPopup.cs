using UnityEngine;
using UnityEngine.UI;

public class ActPopup : MonoBehaviour
{
    [SerializeField] private Button attackButton;
    [SerializeField] private Button endActButton;
    [SerializeField] private Button endTurnButton;

    private void Awake()
    {
        endActButton.onClick.AddListener(OnEndActButtonClicked);
        endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
        attackButton.onClick.AddListener(OnAttackButtonClicked);
    }

    public void Setup(bool canAttack)
    {
        gameObject.SetActive(true);

        if (canAttack)
            attackButton.gameObject.SetActive(true);
        else
            attackButton.gameObject.SetActive(false);

        endActButton.gameObject.SetActive(true);
        endTurnButton.gameObject.SetActive(true);
    }

    void OnEndActButtonClicked()
    {
        if (InputManager.instance.selectedCharacter != null)
        {
            InputManager.instance.selectedCharacter.EndAct();
            Close();
        }
    }

    void OnAttackButtonClicked()
    {
        Close();
        var unit = InputManager.instance.selectedCharacter;
        if (unit != null)
        {
            Vector3Int pos = MapManager.instance.ToGridPos(unit.transform.position);
            MapManager.instance.targetEnemies = unit.GetEnemiesInRange(pos);

            UIManager.instance.attackPopup.Setup(InputManager.instance.selectedCharacter);
        }
        
    }

    void OnEndTurnButtonClicked()
    {
        if (TurnManager.instance.isPlayerDone) return;

        if (InputManager.instance.selectedCharacter != null)
            InputManager.instance.selectedCharacter.EndAct();

        TurnManager.instance.isPlayerDone = true;
        Close();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        MapManager.instance.ClearAggroLines();
    }
}
