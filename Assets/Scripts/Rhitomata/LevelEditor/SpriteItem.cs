using Rhitomata.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rhitomata {
    public class SpriteItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IInstanceableObject {
        public SpriteMetadata data;
        
        [SerializeField] private TMP_Text spriteNameText;
        [SerializeField] private Image spriteImage;
        [SerializeField] private Button deleteButton;

        public Sprite sprite;

        public void Initialize(Sprite targetSprite, SpriteMetadata metadata, int id = -1) {
            sprite = targetSprite;
            spriteNameText.text = targetSprite.name;
            spriteImage.sprite = targetSprite;
            data = metadata;

            deleteButton.gameObject.SetActive(false);
            data.id = SpriteManager.Register(id, this);
        }

        public void SpawnSpriteObjectUI() {
            References.Instance.manager.SpawnSpriteObject(sprite);
        }

        public void Delete() {
            SpriteManager.Remove(this);
            if (sprite) {
                Destroy(sprite.texture);
                Destroy(sprite);
            }
            Destroy(gameObject);
        }

        public void OnPointerEnter(PointerEventData eventData) {
            deleteButton.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData) {
            deleteButton.gameObject.SetActive(false);
        }

        public int GetId() => data.id;
        public void SetId(int id) => data.id = id;
        public void OnIdRedirected(int previousId, int newId) {}
    }
}
