using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats
{
    public string playerName;

    public int maxHP;
    public int currentHP;

    public int maxMP;
    public int currentMP;

    public int attackPower;
    public int defense;
    public int magicPower;
    public int magicDefense;
    public int speed;
}

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public List<PlayerStats> partyMembers = new List<PlayerStats>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 여기서 플레이어를 생성
            partyMembers = new List<PlayerStats>();

            PlayerStats player = new PlayerStats
            {
                playerName = "네스",
                maxHP = 100,
                currentHP = 50,
                maxMP = 30,
                currentMP = 20,
                attackPower = 10,
                magicPower = 5,
                defense = 8,
                magicDefense = 6,
                speed = 12
            };

            partyMembers.Add(player);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}