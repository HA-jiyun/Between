using UnityEngine;
using TMPro;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance;

    public enum TurnPhase { PlayerTurn, EnemyTurn }
    public TurnPhase currentPhase;

    public TextMeshProUGUI turnCountTxt;
    private int turnCount = 0;
    [SerializeField] private GameObject playerTurnUI;
    [SerializeField] private GameObject enemyTurnUI;
    [SerializeField] private GameObject winUI;

    public bool isPlayerDone = false;
    private int actedCount = 0;
    private int totalCount = 0;

    private void Awake()
    {
        instance = this;
        currentPhase = TurnPhase.PlayerTurn;
    }

    public void StartGame()
    {
        StartCoroutine(PlayerTurnRoutine());
    }
    
    IEnumerator PlayerTurnRoutine()
    {
        InputManager.instance.canInput = false;
        InputManager.instance.ClearEverything();

        turnCount++;
        turnCountTxt.text = turnCount.ToString();

        actedCount = 0;
        totalCount = GameManager.instance.allCharacters.Count;

        playerTurnUI.SetActive(true);
        yield return GameManager.instance.wait1;
        playerTurnUI.SetActive(false);

        currentPhase = TurnPhase.PlayerTurn;
        InputManager.instance.canInput = true;

        yield return new WaitUntil(() => isPlayerDone == true);
        EndPlayerTurn();

        yield return GameManager.instance.wait1;
        StartCoroutine(EnemyTurnRoutine());
    }
    IEnumerator EnemyTurnRoutine()
    {
        enemyTurnUI.SetActive(true);
        yield return GameManager.instance.wait1;
        enemyTurnUI.SetActive(false);

        currentPhase = TurnPhase.EnemyTurn;

        foreach (var enemy in GameManager.instance.allEnemies)
        {
            yield return StartCoroutine(enemy.myAI.Attack(enemy));
            yield return new WaitForSeconds(1.0f);
        }
        yield return GameManager.instance.wait1;

        EndEnemyTurn();
        StartCoroutine(PlayerTurnRoutine());
    }

    public void EndPlayerTurn()
    {
        foreach (var unit in GameManager.instance.allCharacters)
        {
            unit.EndAct();
        }

        foreach (var unit in GameManager.instance.allEnemies)
        {
            unit.isActed = false;
        }
    }

    public void EndEnemyTurn()
    {
        foreach (var unit in GameManager.instance.allCharacters)
        {
            unit.isActed = false;
        }
        
        isPlayerDone = false;
    }

    public void CountCharacterActed()
    {
        actedCount++;
        if (actedCount == totalCount)
        {
            InputManager.instance.canInput = false;
            InputManager.instance.ClearEverything();

            isPlayerDone = true;
        }
    }

    public IEnumerator EndGameRoutine()
    {
        InputManager.instance.canInput = false;
        InputManager.instance.ClearEverything();

        yield return GameManager.instance.wait1;
        winUI.SetActive(true);

        GameManager.instance.allCharacters.Clear();
        GameManager.instance.allEnemies.Clear();
    }
}
