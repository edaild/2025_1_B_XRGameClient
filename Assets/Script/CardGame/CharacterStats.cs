using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStats : MonoBehaviour
{
    public string characterName;
    public int maxHealth = 100;
    public int currentHealth;


    // UI 요소
    public Slider healthBar;
    public TextMeshProUGUI healthText;

    // 새로 추가 되는 요소
    public int maxMana = 10;                        // 최대 마나
    public int currentMana;                         // 현재 마나
    public Slider manaBar;                          // 마나 바 UI
    public TextMeshProUGUI manaText;                // 마나 텍스트 UI

    // Start is called before the first frame update
    void Start()
    {
        currentMana = maxMana;
        UpdataUI();
    }

    public void takeDamage(int damage)
    {
        currentHealth -= damage;

        if (DamageEffectManager.Instance != null)
        {
            Vector3 position = transform.position;
            // 랜덤 위치 오프셋 추가
            position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(1f, 1.5f), 0);

            DamageEffectManager.Instance.ShowDamage(position,damage,false);
        }
    }

    public void Heal(int amount)
    {
        if (DamageEffectManager.Instance != null)
        {
            Vector3 position = transform.position;
            // 랜덤 위치 오프셋 추가
            position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(1f, 1.5f), 0);

            DamageEffectManager.Instance.ShowHeal(position, amount, false);
        }
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

        if (manaBar != null)
        {
            manaBar.value = (float)currentMana / maxMana;
        }

        if (manaText != null)
        {
            manaText.text = $"{currentMana} / {maxMana}";
        }
    }
}
