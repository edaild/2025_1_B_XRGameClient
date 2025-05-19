using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Mathematics;
using UnityEditor;

public class CardDisplay : MonoBehaviour
{
    public CardData cardData;               // ī�� ������
    public int cardIndex;                   // ���信���� �ε��� (���߿� ���)

    // 3D ī�� ���
    public MeshRenderer cardRenderer;           // ī�� ������(ion or �Ϸ���Ʈ)
    public TextMeshPro nameText;                // �̸� �ؽ�Ʈ
    public TextMeshPro costText;                // ��� �ؽ�Ʈ
    public TextMeshPro attackText;              // ���ݷ�/ȭ�� �ؽ�Ʈ
    public TextMeshPro descriptionText;         // ���� �ؽ�Ʈ

    // ī�� ����
    public bool isDragging = false;
    private Vector3 originalPosition;            // �巹�� �� ���� ��ġ

    // ���̾� ����ũ
    public LayerMask enemyLayer;                // �� ���̾�
    public LayerMask playerLayer;               // �÷��̾� ���̾�

    private CardManager cardManager;            // ī�� �Ŵ��� ���� �߰�
    private void Start()
    {
        // ���̾� ����ũ ����
        playerLayer = LayerMask.GetMask("Player");
        enemyLayer = LayerMask.GetMask("Enemy");

        cardManager = FindObjectOfType<CardManager>();

        SetuoCard(cardData);
    }

    public void SetuoCard(CardData data)
    {
        // 3D �ؽ�Ʈ ������Ʈ
        if (nameText != null) nameText.text = data.cardName;
        if (costText != null) costText.text = data.manaCost.ToString();
        if (attackText != null) attackText.text = data.effectAmount.ToString();
        if (descriptionText != null) descriptionText.text = data.description;
        
        // ī�� �ؽ��� ����
        if (cardRenderer != null && data.artwork != null)
        {
            Material cardMaterial = cardRenderer.material;
            cardMaterial.mainTexture = data.artwork.texture;
        }

        // SetupCard �޼��忡�� ī�� ���� �ؽ�Ʈ�� �߰� ȿ�� ���� �߰�
        if(descriptionText != null)
        {
            descriptionText.text = data.description + data.getAdditionalEffectDescripton();
        }
    }

    private void OnMouseDown()
    {
        // �巹�� ���� �� ���� ��ġ ����
        originalPosition = transform.position;
        isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            // ���콺 ��ġ�� ī�� �̵�
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 wordPos = Camera.main.ScreenToWorldPoint(mousePos);
            transform.position = new Vector3(wordPos.x, wordPos.y, transform.position.z);
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;

        // ���� ī�� ���� ��ó ��� �ߴ��� �˻� (���� üũ��)
        if (cardManager != null)
        {
            float distToDiscard = Vector3.Distance(transform.position, cardManager.discardPosition.position);

            if ( distToDiscard < 2.0f)
            {
                cardManager.DiscardCard(cardIndex);
                return;
            }
        }

        // ���⼭ ���� ī�� ��� ���� (���� üũ)
        CharacterStats playerStats = null;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerStats = playerObj.GetComponent<CharacterStats>();
        }

        if(playerStats != null || playerStats.currentMana < cardData.manaCost)
        {
            Debug.Log($"������ �����մϴ�.! (�ʿ� : {cardData.manaCost}, guswo :{playerStats?.currentMana ?? 0}");
            transform.position = originalPosition;
            return;
        }

        // �����ɽ�Ʈ�� Ÿ�� ����
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // ī�� ��� ���� ���� ����
        bool cardUsed = false;

        // �� ���� ��� �ߴ��� �˻�
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, enemyLayer))
        {
            CharacterStats enemyStats = hit.collider.GetComponent<CharacterStats>();         // ������ ���� ȿ�� ����

            if (enemyStats != null)                                                        // ī�忡 ȿ���� ����
            {
                enemyStats.takeDamage(cardData.effectAmount);
                Debug.Log($"{cardData.cardName} ī��� ������ {cardData.effectAmount} �������� �������ϴ�.");
                cardUsed = true;
            }
            else
            {
                Debug.Log("�� ī��� ������ ����� �� �����ϴ�.");
            }
        }
        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, playerLayer))
        {
            if (playerStats != null)
            {
                if (cardData.cardtype == CardData.CardType.Heal)
                {
                    playerStats.Heal(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName} ī��� �÷��̾� ü���� {cardData.effectAmount} ȸ�� �߽��ϴ�.");
                }
                else
                {
                    Debug.Log("�� ī��� �÷��̾� ���� ��� �� �� �����ϴ�.");
                }
            }

            if (!cardUsed)
            {
                transform.position = originalPosition;
                if (cardManager != null)
                    cardManager.ArrangHand();
                return;
            }

            // ī�� ��� �� ���� �Ҹ�
            playerStats.UseMana(cardData.manaCost);
            Debug.Log($"������ {cardData.manaCost} ����߽��ϴ�. (���� ���� : {playerStats.currentMana})");

            // �߰� ȿ���� �̾� ��� ó��
            if(cardData.additionalEffects != null && cardData.additionalEffects.Count > 0)
            {
                ProcessAdditionIEffectsAndDiscard();                        // �߰� ȿ�� ����
            }
            else
            {
                if (cardManager != null)
                    cardManager.DiscardCard(cardIndex);             // �߰� ȿ���� ������ ������
            }
        }
    }

    private void ProcessAdditionIEffectsAndDiscard()
    {
        // ī�� ������ �� �ε��� ����
        CardData cardDataCopy = cardData;
        int cardIndexCopy = cardIndex;

        // �߰� ȿ�� ����
        foreach (var effect in cardDataCopy.additionalEffects)
        {
            switch (effect.effectType)
            {
                case CardData.AdditionalEffectType.DrawCard:                 // ��ο� ī�� ����
                    for (int i = 0; i < effect.effectAmount; i++)
                    {
                        if (cardManager != null)
                        {
                            cardManager.DrawCard();
                        }
                    }
                    Debug.Log($"{effect.effectAmount} ���� ī�带 ��ο� �߽��ϴ�.");
                    break;

                case CardData.AdditionalEffectType.DiscardCard:
                    // ī�� ������ ����(���� ������)

                    for (int i = 0; i < effect.effectAmount; i++)
                    {
                        if (cardManager != null && cardManager.handCards.Count > 0)
                        {
                            int randomIndex = UnityEngine.Random.Range(0, cardManager.handCards.Count);         // ���� ũ�� �������� ���� �ε��� ����

                            Debug.Log($"���� ī�� ������ : ���õ� �ε��� {randomIndex}, ���� ���� ũ�� : {cardManager.handCards.Count}");

                            if (cardIndexCopy < cardManager.handCards.Count)
                            {
                                if (randomIndex != cardIndexCopy)
                                {
                                    cardManager.DiscardCard(randomIndex);
                                }

                                // ���� ���� ī���� �ε��� ���� �۴ٸ� ���� ī���� �ε����� 1 ���� ���Ѿ� ��
                                if (randomIndex < cardIndexCopy)
                                {
                                    cardIndexCopy--;
                                }
                                else if (cardManager.handCards.Count > 1)
                                {
                                    // �ٸ� ī�� ����
                                    int newIndex = (randomIndex + 1) & cardManager.handCards.Count;
                                    cardManager.DiscardCard(newIndex);

                                    if (randomIndex < cardIndexCopy)
                                    {
                                        cardIndexCopy--;
                                    }

                                }
                            }
                            else
                            {
                                // cardIndexCopy �� ���̻� ��ȿ���� ���� ��� , �ƹ� ī�峪 ����
                                cardManager.DiscardCard(randomIndex );
                            }
                        }
                       
                 }

                    Debug.Log($"�������� {effect.effectAmount} â�� ī�带 ���ǽ��ϴ�.");
                    break;

                case CardData.AdditionalEffectType.GainMana:
                    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                    if (playerObj != null)
                    {
                        CharacterStats playerStats = playerObj.GetComponent<CharacterStats>();
                        if (playerStats != null)
                        {
                            playerStats.Gainmana(effect.effectAmount);
                            Debug.Log($"������ {effect.effectAmount} ȹ�� �߽��ϴ�. (���� ���� : {playerStats.currentMana})");
                        }
                    }
                    break;

                case CardData.AdditionalEffectType.ReduceEnemyMana:
                    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Ebentg");        // �ױ׸� ����Ͽ� �÷��̾� ĳ���� ã��
                    foreach (var enemy in enemies)
                    {
                        CharacterStats enemyhStats = enemy.GetComponent<CharacterStats>();
                        if (enemyhStats != null)
                        {
                            enemyhStats.UseMana(effect.effectAmount);
                            Debug.Log($"������ {enemyhStats.characterName} �� ������ {effect.effectAmount} ���� ���׽��ϴ�. ");

                        }
                    }
                    break;

                case CardData.AdditionalEffectType.ReduceCardCost:
                    for (int i = 0; i < cardManager.cardObjects.Count; i++)
                    {
                        CardDisplay display = cardManager.cardObjects[i].GetComponent<CardDisplay>();
                        if (display != null && display != this)
                        {
                            TextMeshPro costText = display.costText;
                            if (costText != null)
                            {
                                int originalCost = display.cardData.manaCost;
                                int newCost = math.max(0, originalCost - effect.effectAmount);
                                costText.text = newCost.ToString();
                                costText.color = Color.green;
                            }
                        }
                    }
                    break;
            }
        }
        // ȿ�� ���� �� ���� ī�� ������
        if (cardManager != null){
            cardManager.DiscardCard(cardIndexCopy);
        }
    }


}
