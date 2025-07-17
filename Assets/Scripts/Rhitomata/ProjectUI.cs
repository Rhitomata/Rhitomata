using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Rhitomata.Data;

public class ProjectUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private TMP_Text projectNameText;
    [SerializeField] private TMP_Text projectPathText;
    [SerializeField] private Button closeButton;

    private ProjectData projectInfo;

    public void Initialize(ProjectData projectInfo) {
        this.projectInfo = projectInfo;

        projectNameText.text = projectInfo.name;
        projectPathText.text = projectInfo.path;
        closeButton.gameObject.SetActive(false);
    }

    public void Delete() {
        // i think deleting, should move the project to the recycling bin instead of deleting the project completely
        //FileSystem.DeleteFile(projectInfo.path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
        Destroy(gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        closeButton.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        closeButton.gameObject.SetActive(false);
    }
}