using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Mathematics;

public class CardDisplay : MonoBehaviour
{
    public CardData cardData;               // 카드 데이터
    public int cardIndex;                   // 손페에서의 인덱스 (나중에 사용)

    // 3D 카드 요소
    public MeshRenderer cardRenderer;           // 카드 렌더링(ion or 일러스트)
    public TextMeshPro nameText;                // 이름 텍스트
    public TextMeshPro costText;                // 비용 텍스트
    public TextMeshPro attackText;              // 공격력/화과 텍스트
    public TextMeshPro descriptionText;         // 설명 텍스트

    // 카드 상태
    public bool isDragging = false;
    private Vector3 originalPosition;            // 드레그 전 원래 위치

    // 레이어 마스크
    public LayerMask enemyLayer;                // 적 레이어
    public LayerMask playerLayer;               // 플레이어 레이어

    private CardManager cardManager;            // 카드 매니저 참조 추가
    private void Start()
    {
        // 레이어 마스크 설정
        playerLayer = LayerMask.GetMask("Player");
        enemyLayer = LayerMask.GetMask("Enemy");

        cardManager = FindObjectOfType<CardManager>();

        SetyoCard(cardData);
    }

    public void SetyoCard(CardData data)
    {
        // 3D 텍스트 업데이트
        if (nameText != null) nameText.text = data.cardName;
        if (costText != null) costText.text = data.manaCost.ToString();
        if (attackText != null) attackText.text = data.effectAmount.ToString();
        if (descriptionText != null) descriptionText.text = data.description;
        
        // 카드 텍스쳐 설정
        if (cardRenderer != null && data.artwork != null)
        {
            Material cardMaterial = cardRenderer.material;
            cardMaterial.mainTexture = data.artwork.texture;
        }
    }

    private void OnMouseDown()
    {
        // 드레그 시작 시 원래 위치 저장
        originalPosition = transform.position;
        isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            // 마우스 위치로 카드 이동
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 wordPos = Camera.main.ScreenToWorldPoint(mousePos);
            transform.position = new Vector3(wordPos.x, wordPos.y, transform.position.z);
        }
    }

    private void OnMouseUp()
    {

        CaracterStats playerStats = FindObjectOfType<CaracterStats>();
        if(playerStats != null || playerStats.currentMana < cardData.manaCost)                      // 마나 검사
        {
            Debug.Log($"마나가 부족 합니다.! (필요 : {cardData.manaCost}, 현재 : {playerStats?.currentMana ?? 0}");
            transform.position = originalPosition;
            return;
        }

        isDragging = false;

        // 레이케스트로 타겟 감지
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // 카드 사용 판정 지역 변수
        bool cardUsed = false;

        // 적 위에 드롭 했는지 검사
        if(Physics.Raycast(ray, out hit, Mathf.Infinity, enemyLayer))
        {
            // 적에게 공격 효과 적용
            CaracterStats enemyStats = hit.collider.GetComponent<CaracterStats>();

            if(enemyStats != null)
            {
                if(cardData.cardtype == CardData.CardType.Attack)
                {
                    // 공격 카드면 데미지 주기
                    enemyStats.takeDamage(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName} 카드로 적에게 {cardData.effectAmount} 데미지를 입혀습니다.");
                    cardUsed = true;
                }
                else
                {
                    Debug.Log("이 카드는 적에게 사용할 수 없습니다.");
                }
            }
        }
        else if(Physics.Raycast(ray, out hit, Mathf.Infinity, playerLayer)) 
        {
            // 플레이어에게 힐 효과 적용
           // CaracterStats playerStats = hit.collider.GetComponent<CaracterStats>();

            if(playerStats != null)
            {
                playerStats.Heal(cardData.effectAmount);
                Debug.Log($"{cardData.cardName} 카드로 플레이어의 체력을 {cardData.effectAmount} 회복했습니다.");
            }
            else
            {
                Debug.Log("이 카드는 적에게 사용할 수 없습니다.");
            }
           
        }

        else if (!cardUsed)
        {
            // 버린 카드 더미 근처에 드롭했는지 검사
            float distDiscard = Vector3.Distance(transform.position, cardManager.discardPosition.position);
            if (distDiscard > 2.0f)
            {
                // 카드를 버리기
                cardManager.DiscardCard(cardIndex);
                return;
            }
        }

        // 카드를 사용하지 않았다면 원래 위치로 되돌리기
        if (!cardUsed)
        {
            transform.position = originalPosition;
            // 손패 재정렬 (필요한 경우)
            cardManager.ArrangHand();
        }
        else
        {
            // 카드를 사용했다면 버린 카드 더미로 이도
            if(cardManager != null)
                cardManager.DiscardCard(cardIndex);

            // 카드 사용 시 마나 소모(카드가 성공적으로 사용된 후 추가)
            playerStats.UseMana(cardData.manaCost);
            Debug.Log($"마나를 {cardData.manaCost} 사용 했습니다. (남은 마나 : {playerStats.currentHealth})");
        }
    }


}
