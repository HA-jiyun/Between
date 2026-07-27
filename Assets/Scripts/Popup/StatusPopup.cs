using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleTxt;
    [SerializeField] private TextMeshProUGUI currentHpTxt;
    [SerializeField] private TextMeshProUGUI maxHpTxt;
    [SerializeField] private Image portrait;
    [SerializeField] private Image element;
    [SerializeField] private Slider hpSlider;

    [SerializeField] private Image background;
    [SerializeField] private Color color1;
    [SerializeField] private Color color2;


    public void SetUp(Unit unit)
    {
        gameObject.SetActive(true);

        titleTxt.text = unit.myName.ToString();
        portrait.sprite = unit.myImage;
        element.sprite = unit.myElementImage;
        background.color = unit.isEnemy ? color2 : color1;

        hpSlider.minValue = 0;
        hpSlider.maxValue = unit.maxHP;
        hpSlider.value = unit.currentHP;

        currentHpTxt.text = unit.currentHP.ToString();
        maxHpTxt.text = unit.maxHP.ToString();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
