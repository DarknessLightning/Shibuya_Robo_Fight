using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionPanel : MonoBehaviour
{
    [Header("Interface Reference")]
    public Image selectedSprite;
    public Image selectedInfo;
    public GameObject selected;
    public Image selectBtn;

    [Header("Public Reference")]
    public CharacterData selectedCharacter;
    public bool confirm = false;
    public Image[] selectionButtons;
    public UIButtonHandler[] handlers;

    [Header("Outside Panel Button")]
    public CanvasGroup selectPanelBtn;
    public Transform pointer;

    //private reference
    private int index = -1;

    public void OpenPanel()
    {
        gameObject.SetActive(true);
        selectPanelBtn.alpha = 1.0f;
        pointer.position = new Vector3 (pointer.position.x, 
            selectPanelBtn.transform.position.y, 
            pointer.position.z);
    }
    public void ClosePanel()
    {
        gameObject.SetActive(false);
        selectPanelBtn.alpha = 0.2f;
    }

    public void SelectCharacter(CharacterData character)
    {
        if (confirm) return;
        if (character.characterModel == null)
        {
            selectedCharacter = null;
            return;
        }
        selectedCharacter = character;
        selectedSprite.gameObject.SetActive(true);
        selectedSprite.sprite = character.characterSprite;
        selectedInfo.gameObject.SetActive(true);
        selectedInfo.sprite = character.info;
    }

    public void SelectButton(int index)
    {
        if (confirm) return;
        if (index == this.index) return;

        int previousIndex = this.index;
        this.index = index;

        UpdateButton(previousIndex);
        UpdateButton(index);

        selectionButtons[index].transform.SetAsLastSibling();

    }

    void UpdateButton(int i)
    {
        if (i < 0 || i > selectionButtons.Length) return;
        bool isSelected = (i == index);

        var btn = selectionButtons[i];

        btn.sprite = isSelected
            ? CharacterSelectionManager.instance.selectedSprite[i]
            : CharacterSelectionManager.instance.notSelected[i];

        btn.transform.localScale = isSelected
            ? Vector3.one * 1.3f
            : Vector3.one;

        var handler = handlers[i];

        handler.hoverable = !isSelected;
    }

    public void ConfirmNCancel()
    {
        if (CharacterSelectionManager.instance.isStartingFight()) return;
        if (selectedCharacter == null) return;
        confirm = !confirm;
        selected.SetActive(confirm);
        selectBtn.sprite = confirm ?
            CharacterSelectionManager.instance.cancelBtnSprite :
            CharacterSelectionManager.instance.selectBtnSprite;
        for(int i = 0; i < handlers.Length; i++)
        {
            bool isSelected = (i == index);

            if (confirm)
            {
                handlers[i].hoverable = false;
            }
            else
            {
                handlers[i].hoverable = !isSelected;
            }
        }

        CharacterSelectionManager.instance.bothPanelConfirmed();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        handlers = new UIButtonHandler[selectionButtons.Length];
        for(int i = 0; i < selectionButtons.Length; i++)
        {
            handlers[i] = selectionButtons[i].GetComponent<UIButtonHandler>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
