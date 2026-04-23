using Febucci.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UIStringKey;

public class DialogueUI : UIBase, IUiSetup<ChatData>
{
    
    [Header("챗 데이터")]
    public ChatData chatData;

    [SerializeField] private TAnimSoundWriter textSoundWriter;

    [Header("캐릭터 인포UI")]
    [SerializeField] private Image characterImage;
    [SerializeField] private TMP_Text characterName;

    [Header("텍스트 박스 UI")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TypewriterByCharacter typeWriter;
    [SerializeField] private TMP_Text dialogueIndexText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button closeButton;

    [Header("캐릭터 정보")]
    [SerializeField] private SpriteDataManager npcSpriteData;

    private int nowId = 0;
    private int maxId;

    // 대화 끝나고 닫힐 때 뭔가 이어서 하고 싶을 때 쓰는 콜백
    public Action onDialogueEnd;


    private void Initialize()
    {
        if (isInitialize) return;
        isInitialize = true;

        npcSpriteData = gameManager.GetManager<SpriteDataManager>();

        nextButton.onClick.AddListener(NextContent);
        previousButton.onClick.AddListener(PreviousContent);
        closeButton.onClick.AddListener(CloseDialogue);
        textSoundWriter.Initialize(gameManager);

    }

    public override void OnShow(Action<UIBase> finished = null)
    {
        base.OnShow(finished);
        Initialize();
        DialogueSetUP(chatData);
    }


    // 챗에 들어가야하는 요소가 대화 id, 캐릭터 이름, 대화 내용

    public void DialogueSetUP(ChatData chatData) // 여기 챗 데이터 쓰는 부분 인규껄로 수정
    {
        maxId = chatData.chatDataList.Count;
        nowId = 0;
        WriteContent();
    }

    // 넘길 때 마다 텍스트 출력
    private void WriteContent()
    {
        typeWriter.ShowText(chatData.chatDataList[nowId].content);
        characterName.text = chatData.chatDataList[nowId].name;

        bool showNext = isShowNextButton();
        nextButton.gameObject.SetActive(showNext);

        if (showNext)
        {
            closeButton.gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(nextButton.gameObject);
        }
        else
        {
            closeButton.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(closeButton.gameObject);
        }

        dialogueIndexText.text = $"{nowId + 1}/{maxId}"; // 대화 남은 순서 표시
        characterImage.sprite = npcSpriteData.GetNpcSprite(chatData.chatDataList[nowId].name);
    }

    #region 버튼 함수 모음
    // 다음 버튼
    public void NextContent()
    {
        nowId++;
        WriteContent();
    }

    // 이전 버튼
    public void PreviousContent()
    {
        if (nowId <= 0) return;
        
        nowId--;
        WriteContent();
        
    }

    // 닫기 버튼
    public void CloseDialogue()
    {
        nowId = 0;
        gameManager.GetManager<UIManager>().ClosePanel(UIKey.DialogueUI);

        onDialogueEnd?.Invoke();
        onDialogueEnd = null;
    }

    // 다음 버튼을 보여줘야하는지 안보여줘야하는지 알려주는 bool 함수
    private bool isShowNextButton()
    {
        bool isShow;
        if (nowId < maxId - 1) isShow = true;
        else isShow = false;
        return isShow;
    }

#endregion

    public void Setup(ChatData payload)
    {
        chatData = payload;
    }
}
