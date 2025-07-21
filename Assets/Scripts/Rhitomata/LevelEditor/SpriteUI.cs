using Rhitomata;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpriteUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private TMP_Text spriteNameText;
    [SerializeField] private Image spriteImage;
    [SerializeField] private Button deleteButton;

    private Sprite _sprite;
    private string _spriteName;

    public void Initialize(Sprite targetSprite, string targetName) {
        _sprite = targetSprite;
        _spriteName = targetName;
        spriteNameText.text = targetName;
        spriteImage.sprite = targetSprite;

        deleteButton.gameObject.SetActive(false);
    }

    public Sprite GetSprite() {
        return _sprite;
    }

    public string GetName() {
        return _spriteName;
    }

    public void SpawnSpriteObjectUI() {// for UI ref
        SpawnSpriteObject();
    }

    public SpriteObject SpawnSpriteObject() {
        var spriteObject = Instantiate(References.Instance.spriteObjectPrefab, References.Instance.gameHolder).GetComponent<SpriteObject>();
        spriteObject.transform.position = Vector3.zero;
        spriteObject.Initialize(this);
        return spriteObject;
    }

    public void Delete() {
        Destroy(gameObject);
        if (_sprite) {
            Destroy(_sprite.texture);
            Destroy(_sprite);
        }
        References.Instance.manager.sprites.Remove(this);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        deleteButton.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        deleteButton.gameObject.SetActive(false);
    }
}
