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


    // UI ���
    public Slider healthBar;
    public TextMeshProUGUI healthText;

    // ���� �߰� �Ǵ� ���
    public int maxMana = 10;                        // �ִ� ����
    public int currentMana;                         // ���� ����
    public Slider manaBar;                          // ���� �� UI
    public TextMeshProUGUI manaText;                // ���� �ؽ�Ʈ UI

    // Start is called before the first frame update
    void Start()
    {
        currentMana = maxMana;
        UpdataUI();
    }

    public void takeDamage(int damage)
    {
        currentHealth -= damage;
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
    }

    public void UseMana(int amount)
    {
        currentMana -= amount;
        if (currentMana < 0)
        {
            currentMana = 0;
        }
        UpdataUI();
    }

    public void Gainmana(int amount)
    {
        currentMana += amount;
        if(currentMana > maxMana)
        {
            currentMana = maxMana;
        }
        UpdataUI();
    }

    private void UpdataUI()
    {
        if (healthBar != null)
        {
            healthBar.value = (float)currentHealth / maxMana;
        }

        if(healthText != null)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }

        if (manaText != null)
        {
            manaText.text = $"{currentMana} / {maxMana}";
        }
    }
}
