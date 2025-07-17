using UnityEngine;

[ExecuteInEditMode]
public partial class Indicator : MonoBehaviour
{
    public SpriteRenderer sprite1, sprite2, sprite3, sprite4;
    public float progress;
    public float multiplier = 5;

    [SerializeField]
    private bool _isVisible = true;
    
    public void Adjust() {
        if (sprite1 == null || sprite2 == null || sprite3 == null || sprite4 == null)
            return;

        float clamped = Mathf.Clamp(progress, 0f, 1f);
        float eased = clamped * clamped * clamped;
        if (eased == 1 || eased == 0)
        {
            /// The indicator is either hidden inside a tail 
            /// or it's not supposed to be visible yet
            SetVisible(false);
            return;
        }

        SetVisible(true);

        float distance = multiplier * (1f - eased);

        sprite1.transform.localPosition = new Vector3(1, 0, 1) * distance;
        sprite2.transform.localPosition = new Vector3(-1, 0, 1) * distance;
        sprite3.transform.localPosition = new Vector3(1, 0, -1) * distance;
        sprite4.transform.localPosition = new Vector3(-1, 0, -1) * distance;

        Color modulation = sprite1.color;
        modulation.a = eased;

        sprite1.color = modulation;
        sprite2.color = modulation;
        sprite3.color = modulation;
        sprite4.color = modulation;
    }

    /// <summary>
    /// Probably needed so the camera doesn't have to render all the sprites
    /// </summary>
    public void SetVisible(bool to) {
        if (_isVisible == to) return;

        _isVisible = to;
        sprite1.gameObject.SetActive(to);
        sprite2.gameObject.SetActive(to);
        sprite3.gameObject.SetActive(to);
        sprite4.gameObject.SetActive(to);
    }
}
