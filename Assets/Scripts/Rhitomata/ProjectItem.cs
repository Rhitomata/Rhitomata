using System.IO;
using Rhitomata.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rhitomata {
    public class ProjectItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        [SerializeField] private TMP_Text projectNameText;
        [SerializeField] private TMP_Text projectPathText;
        [SerializeField] private Button closeButton;

        private ProjectData _projectInfo;

        public void Initialize(ProjectData projectInfo) {
            _projectInfo = projectInfo;

            projectNameText.text = projectInfo.name;
            projectPathText.text = projectInfo.directoryPath;
            closeButton.gameObject.SetActive(false);
        }

        public void Delete() {
            // TODO: Implement a custom storage system that's cross-platform and add this feature 

            // i think deleting, should move the project to the recycling bin instead of deleting the project completely
            //FileSystem.DeleteFile(projectInfo.path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            Destroy(gameObject);
            Directory.Delete(_projectInfo.directoryPath, true);
        }

        public void OnPointerEnter(PointerEventData eventData) {
            closeButton.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData) {
            closeButton.gameObject.SetActive(false);
        }
    }
}