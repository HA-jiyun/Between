using System.Collections.Generic;
using UnityEngine;

public enum Mode { Easy, Normal, Hard }
public enum SkillType { Basic, Special }

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public WaitForSeconds wait1 = new(1.0f);
    public Mode selectedMode;

    public List<Character> allCharacters;
    public List<Enemy> allEnemies;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetMode(Mode mode)
    {
        this.selectedMode = mode;
        IEnemyAI modeAI = mode switch
        {
            Mode.Normal => new NormalAI(),
            Mode.Hard => new HardAI(),
            _ => new EasyAI()
        };

        foreach(var enemy in allEnemies)
        {
            enemy.myAI = modeAI;
        }
    }
}
