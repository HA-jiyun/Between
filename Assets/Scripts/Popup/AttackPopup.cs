using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttackPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI basicDamageText;
    [SerializeField] private TextMeshProUGUI specialDamageText;
    [SerializeField] private Button basicButton;
    [SerializeField] private Button specialButton;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        UIManager.instance.actPopup.Close();

        basicButton.onClick.AddListener(BasicAttack);
        specialButton.onClick.AddListener(SpecialAttack);
        backButton.onClick.AddListener(Back);
    }

    public void Setup(Unit unit)
    {
        gameObject.SetActive(true);
        basicDamageText.text = unit.basicDamage.ToString();
        specialDamageText.text = unit.specialDamage.ToString();
    }

    private void BasicAttack()
    {
        InputManager.instance.SelectRandomEnemy(SkillType.Basic);
        Close();
    }

    private void SpecialAttack()
    {
        InputManager.instance.SelectRandomEnemy(SkillType.Special);
        Close();
    }

    private void Back()
    {
        Close();
        UIManager.instance.actPopup.Setup(InputManager.instance.selectedCharacter.CanAttack());
    }

    void Close()
    {
        gameObject.SetActive(false);
    }
}
