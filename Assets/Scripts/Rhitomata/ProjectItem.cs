using System.IO;
using Rhitomata.Data;
using Rhitomata.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Rhitomata.Storage;

namespace Rhitomata {
    public class ProjectItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        [SerializeField] private TMP_Text projectNameText;
        [SerializeField] private TMP_Text projectPathText;
        [SerializeField] private Button closeButton;

        private ProjectData _projectInfo;

        private ProjectList _projectList => References.Instance.projectList;

        public void Initialize(ProjectData projectInfo) {
            _projectInfo = projectInfo;

            projectNameText.text = projectInfo.name;
            projectPathText.text = projectInfo.directoryPath;
            closeButton.gameObject.SetActive(false);
        }

        public void Open() {
            References.Instance.manager.LoadProject(_projectInfo.directoryPath);
            References.Instance.projectList.GetComponent<Window>().Hide();
            closeButton.gameObject.SetActive(false);
        }

        public void Delete() {
            _projectList.items.Remove(this);
            // i think deleting, should move the project to the recycling bin instead of deleting the project completely
            // TODO: Implement a custom storage system that's cross-platform and add this feature 
            //FileSystem.DeleteFile(projectInfo.path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            DeleteDirectory(_projectInfo.directoryPath);
            Destroy(gameObject);
        }

        public void OnPointerEnter(PointerEventData eventData) {
            closeButton.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData) {
            closeButton.gameObject.SetActive(false);
        }
    }
}