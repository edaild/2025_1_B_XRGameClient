using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CaracterStats : MonoBehaviour
{
    public string characterName;
    public int maxHealth = 100;
    public int currentHealth;


    // UI ¿ä¼Ò
    public Slider healthBar;
    public TextMeshProUGUI healthText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void takeDamage(int damage)
    {
        currentHealth -= damage;
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
    }
}
