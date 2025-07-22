using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Rhitomata.Data;

namespace Rhitomata {
    public class ProjectList : MonoBehaviour {
        public static string projectsDir => "Projects".GetPathLocal();

        // New project
        [SerializeField] private TMP_InputField projectNameInputField;
        [SerializeField] private TMP_InputField authorInputField;
        [SerializeField] private Button createButton;

        [SerializeField] private Transform projectUIHolder;
        [SerializeField] private GameObject projectUIPrefab;

        public List<ProjectItem> items = new();

        private void Awake() {
            projectNameInputField.text = "Untitled";
            authorInputField.text = System.Environment.UserName;
        }

        private void OnEnable() {
            UpdateProjectListUI();
        }

        private void UpdateProjectListUI() {
            Storage.CheckDirectory(projectsDir);

            Clear();

            var directories = Storage.GetDirectories(projectsDir);
            foreach (var dir in directories) {
                var projectFilePath = dir.Combine("project.json");
                if (!Storage.FileExists(projectFilePath)) continue;

                var contents = Storage.ReadAllText(projectFilePath);
                if (string.IsNullOrWhiteSpace(contents)) continue;
                
                var data = RhitomataSerializer.Deserialize<ProjectData>(contents);
                if (data == null) continue;

                data.filePath = projectFilePath;
                data.directoryPath = dir;

                var projectUI = Instantiate(projectUIPrefab, projectUIHolder).GetComponent<ProjectItem>();
                projectUI.Initialize(data);
                items.Add(projectUI);
            }
        }

        public void CreateProject() {
            // TODO: Make it spawn a whole new window to input all the necessary info
            var projectName = projectNameInputField.text;
            var projectAuthor = authorInputField.text;
            var directoryPath = projectsDir.Combine(projectName);

            var projectInfo = new ProjectData(projectName, projectAuthor, "Unknown Artist", "Untitled") {
                directoryPath = directoryPath,
                filePath = directoryPath.Combine("project.json")
            };
            References.Instance.manager.CreateProject(projectInfo);
        }
        
        public void Clear() {
            foreach (var item in items) {
                Destroy(item.gameObject);
            }
            items.Clear();
        }
    }
}