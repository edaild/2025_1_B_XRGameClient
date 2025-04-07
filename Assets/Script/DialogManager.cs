using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public static DialogManager instance { get; private set; }
    [Header("Dialog Referenecs")]
    [SerializeField] private DialogDatabaseSO dialogDatabase;

    [Header("UI References")]
    [SerializeField] private GameObject dialogPanel;

    [SerializeField] private Image portraitImage;

    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Button NextButton;

    [Header("Dialog Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private bool useTypewriterEffect = true;

    [Header("DialogChoices")]
    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private GameObject choicesButtonPrefab;

    private bool isTyping = false;
    private Coroutine typingCoroutine;          // 코루틴 선언

    private DialogSO currentDialog;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;                    // 싱글톤 패턴 구현
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (dialogDatabase != null)
        {
            dialogDatabase.Initalize();                        // 초기화
        }
        else
        {
            Debug.Log("Dialog Database is not assinged to Dialog Manager");
        }
        if (NextButton != null)
        {
            NextButton.onClick.AddListener(NextDialog);         // 버튼 리스너 등록
        }
        else
        {
            Debug.LogError("Next Button is not assigned!");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //UI초깋솨 후 대화 시작 (ID 1)
        CloseDialog();
        StartDialog(1);                 // 자동으로 첫번째 대화 시작
    }

    // Update is called once per frame
    void Update()
    {

    }

    // ID 로 대화 시작
    public void StartDialog(int dialogId)
    {
        DialogSO dialog = dialogDatabase.GetDialogByld(dialogId);
        if (dialog != null)
        {
            StartDialog(dialog);
        }
        else
        {
            Debug.LogError($"Fialog with ID {dialog} not found!");
        }
    }

    // DialogSO로 대화 시작
    public void StartDialog(DialogSO dialog)
    {
        if (dialog == null) return;

        currentDialog = dialog;
        ShowDialog();
        dialogPanel.SetActive(true);
    }

    public void ShowDialog()
    {
        if (currentDialog == null) return;
        characterNameText.text = currentDialog.characterName;        // 캐릭터 이름 설정
        // 대화 텍스트 설정 부분 수정
        if (useTypewriterEffect)
        {
            StartTypingEffect(currentDialog.text);
        }
        else
        {
            dialogText.text = currentDialog.text;               // 대화 텍스트 설정
        }

        // 초상화 설정 (세로 추가된 부분)
        if (currentDialog != null)
        {
            portraitImage.sprite = currentDialog.portrait;
            portraitImage.gameObject.SetActive(true);
        }
        else if (!string.IsNullOrEmpty(currentDialog.protraitPath))
        {
            //Resoures 폴더에서 이미지 로드
            Sprite portrait = Resources.Load<Sprite>(currentDialog.protraitPath);
            if (portrait != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log($"Portrait not found at path : {currentDialog.protraitPath}");
            }
        }
        else
        {
            portraitImage.gameObject.SetActive(false);              // 초상화가 없으면 이미지 비활성화
        }

        // 선택지 표시
        ClearChoices();
        if(currentDialog.choices != null && currentDialog.choices.Count > 0)
        {
            ShowChoices();
            NextButton.gameObject.SetActive(false);
        }
        else
        {
            NextButton.gameObject.SetActive(true);
        }
    }

    public void NextDialog()                        // 다음 대화로 진행
    {
        if (isTyping)                               // 타이핑 중이면 타이핑 완료 처리
        {
            StopTypingEffect();
            dialogText.text = currentDialog.text;
            isTyping = false;
            return;
        }

        if (currentDialog != null && currentDialog.nextId > 0)
        {
            DialogSO nextDialog = dialogDatabase.GetDialogByld(currentDialog.nextId);
            if (nextDialog != null)
            {
                currentDialog = nextDialog;
                ShowDialog();
            }
            else
            {
                CloseDialog();
            }
        }
        else
        {
            CloseDialog();
        }
    }

    // 텍스트 타이핑 효과 코루틴

    private IEnumerator TypeText(string text)
    {
        dialogText.text = "";
        foreach (char c in text)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    // 타이핑 효과 중지
    private void StopTypingEffect()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    // 타이핑 효과 시작
    private void StartTypingEffect(string text)
    {
        isTyping = true;
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(text));
    }
    public void CloseDialog()                       // 대화 종료
    {
        dialogPanel.SetActive(false);
        currentDialog = null;
        StopTypingEffect();                 // 타이핑 효과 중지 추가
    }

    // 선택지 초기화
    private void ClearChoices()
    {
        foreach (Transform child in choicesPanel.transform)
        {

            Destroy(child.gameObject);
        }
        choicesPanel.SetActive(false);
    }

    // 선택지 선택 처리
    public void SelectChoice(DialogChoiceSO choice)
    {
        if(choice != null && choice.nextid > 0)
        {
            DialogSO nextDialog = dialogDatabase.GetDialogByld(choice.nextid);
            if(nextDialog != null )
            {
                currentDialog = nextDialog;
                ShowDialog();
            }
            else
            {
                CloseDialog();
            }
        }
        else
        {
            CloseDialog();
        }
    }

    // 선택지 표시
    private void ShowChoices()
    {
        choicesPanel.SetActive(true);

        foreach(var choice in currentDialog.choices)
        {
            GameObject choiceGo = Instantiate(choicesButtonPrefab, choicesPanel.transform);
            TextMeshProUGUI buttonText = choiceGo.GetComponent<TextMeshProUGUI>(); 
            Button button = buttonText.GetComponent<Button>();

            if(buttonText != null )
            {
                buttonText.text = choice.text;
            }
            if(button != null)
            {
                DialogChoiceSO choiceSO = choice;                   // 람다식에서 사용하기 위해서 지역 변수에 할당
                button.onClick.AddListener(()=> SelectChoice(choiceSO));
            }
        }
    }


}
