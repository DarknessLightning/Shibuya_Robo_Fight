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
        if (index == this.index) 
        { 
            return; 
        }

        for(int i = 0; i < selectionButtons.Length; i++)
        {
            if (i != index && i != this.index)
            {
                continue;
            }

            selectionButtons[i].sprite = i == index ?
                CharacterSelectionManager.instance.selectedSprite[i] :
                CharacterSelectionManager.instance.notSelected[i];

            selectionButtons[i].transform.localScale = i == index ? 
                new Vector2(1.3f, 1.3f) : 
                new Vector2(1.0f, 1.0f);
        }
        selectionButtons[index].transform.SetAsLastSibling();
        this.index = index;

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

        CharacterSelectionManager.instance.bothPanelConfirmed();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
