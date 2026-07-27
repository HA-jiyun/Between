using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    [Header("Base")]
    [SerializeField] private LayerMask unitLayer;
    [SerializeField] private LayerMask planeLayer;

    public bool canInput = false;
    public Grid grid;

    [Header("Point Tools")]
    public Vector3Int? lastTilePosition;
    public Unit lastUnit = null;
    public int currentTargetIndex;

    [Header("Click Tools")]
    private Vector2 currentMousePosition;
    private bool isClickPressed = false;

    public Enemy selectedEnemy;
    public Character selectedCharacter;
    private SkillType selectedSkill;

    [Header("Select Tools")]
    public Vector3 targetPosition;
    private bool isTargeting = false;

    private void Awake() => instance = this;

    private void Update()
    {

        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (!canInput) return;

        if (!isTargeting) {
            HandlePoint();
        }

        if (isClickPressed && TurnManager.instance.currentPhase == TurnManager.TurnPhase.PlayerTurn)
        {
            isClickPressed = false;
            HandleClick();
        }
    }

    public void OnClick(InputValue value)
    {
        isClickPressed = value.isPressed;
    }
    public void OnPoint(InputValue value)
    {
        currentMousePosition = value.Get<Vector2>();
    }

    public void HandlePoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(currentMousePosition);
        Unit hitUnit = null;
        Vector3Int? hitTilePos = null;

        if (Physics.Raycast(ray, out RaycastHit unitHit, Mathf.Infinity, unitLayer))
        {
            hitUnit = unitHit.collider.GetComponentInParent<Unit>();

            if (hitUnit != null)
                hitTilePos = MapManager.instance.ToGridPos(hitUnit.transform.position);
        }
        else if (Physics.Raycast(ray, out RaycastHit groundHit, Mathf.Infinity, planeLayer))
        {
            hitTilePos = MapManager.instance.ToGridPos(groundHit.point, true);
        }

        UIManager.instance.UpdateTileUI(hitTilePos);
        if (lastUnit != hitUnit)
        {
            lastUnit = hitUnit;
            UIManager.instance.UpdateUI();
        }
    }
    public void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;
        string tag = hit.collider.tag;

        if (tag == "Character")
        {
            if (selectedCharacter != null) return;
            Character c = hit.collider.GetComponentInParent<Character>();

            if(c != null && !c.isActed){
                selectedCharacter = c;
                selectedCharacter.Clicked();
            }
        }
        else if (tag == "MoveRange")
        {
            if (isTargeting) return;

            Vector3Int goalPos = MapManager.instance.ToGridPos(hit.transform.position);
            MoveCharacter(hit);
            UIManager.instance.actPopup.Setup(selectedCharacter.CanAttack(goalPos));
        }
        else if (tag == "Enemy")
        {
            if (selectedCharacter == null || isTargeting == false) return;

            GameObject e = hit.collider.gameObject;
            selectedEnemy = e.GetComponent<Enemy>();

            if (selectedCharacter.CanAttack())
            {
                selectedCharacter.Look(selectedEnemy.transform);

                if(selectedSkill == SkillType.Basic)
                    selectedCharacter.BasicAttack();
                else
                    selectedCharacter.SpecialAttack();
            }

            isTargeting = false;
        }
    }

    private void MoveCharacter(RaycastHit hitResult)
    {
        Vector3 worldPos = hitResult.collider.transform.position;
        Vector3Int cellPos = grid.WorldToCell(worldPos);
        targetPosition = grid.GetCellCenterWorld(cellPos);
        targetPosition.y = selectedCharacter.transform.position.y;

        StartCoroutine(selectedCharacter.Move(targetPosition));
    }

    public void SelectRandomEnemy(SkillType type)
    {
        selectedSkill = type;
        isTargeting = true;

        if (MapManager.instance.targetEnemies.Count == 1)
        {
            ChangeTarget(0);
        }
        else
        {
            int idx = Random.Range(0, MapManager.instance.targetEnemies.Count);
            ChangeTarget(idx);
        }

    }
    private void ChangeTarget(int idx)
    {
        currentTargetIndex = idx;
        Unit targetUnit = MapManager.instance.targetEnemies[currentTargetIndex];
        lastUnit = targetUnit;
        lastTilePosition = MapManager.instance.ToGridPos(targetUnit.transform.position);

        UIManager.instance.TrackPoint(targetUnit.transform.position);
        UIManager.instance.statusPopup2.SetUp(targetUnit);

        Vector3Int pos = MapManager.instance.ToGridPos(targetUnit.transform.position);
        UIManager.instance.SetPointEffect(pos);
    }

    public void ClearEverything()
    {
        lastUnit = null;
        lastTilePosition = null;
        UIManager.instance.StopTracking();
        UIManager.instance.ClearPointEffect();

        selectedCharacter = null;
        selectedEnemy = null;
        currentTargetIndex = -1;
        isTargeting = false;

        UIManager.instance.statusPopup1.Close();
        UIManager.instance.statusPopup2.Close();
    }
}
