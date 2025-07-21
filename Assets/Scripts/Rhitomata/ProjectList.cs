using Rhitomata;
using Rhitomata.Data;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProjectList : MonoBehaviour {
    public static string ProjectsDir => Path.Combine(Application.persistentDataPath, @"Projects");

    [SerializeField] private LevelManager levelManager;

    // New project
    [SerializeField] private TMP_InputField projectNameInputField;
    [SerializeField] private TMP_InputField authorInputField;
    [SerializeField] private Button createButton;

    [SerializeField] private Transform projectUIHolder;
    [SerializeField] private GameObject projectUIPrefab;

    private void Awake() {
        projectNameInputField.text = "Untitled";
        authorInputField.text = "Unknown";
        //authorInputField.text = Environment.UserName;
    }

    private void OnEnable() {
        UpdateProjectListUI();
    }

    private void UpdateProjectListUI() {
        if (!Directory.Exists(ProjectsDir)) Directory.CreateDirectory(ProjectsDir);

        foreach (Transform t in projectUIHolder) {
            Destroy(t.gameObject);
        }

        // TODO: Read project data from json file?
        var projectFilePaths = Directory.GetFiles(ProjectsDir);
        foreach (var projectPath in projectFilePaths) {
            var projectName = Path.GetFileName(projectPath);
            var projectInfo = new ProjectData(projectName, "Unknown", "Unknown", "Untitled") {
                directoryPath = projectPath
            };

            var projectUI = Instantiate(projectUIPrefab, projectUIHolder).GetComponent<ProjectUI>();
            projectUI.Initialize(projectInfo);
        }
    }

    public void CreateProject() {
        var pName = projectNameInputField.text;
        var author = authorInputField.text;
        var songArtist = "Unknown Artist";// TODO?
        var directoryPath = Path.Combine(ProjectsDir, pName);

        var projectInfo = new ProjectData(pName, author, songArtist, "Untitled") {
            directoryPath = directoryPath
        };
        levelManager.CreateProject(projectInfo);
    }
}