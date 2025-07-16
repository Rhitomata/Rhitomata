using Newtonsoft.Json;
using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProjectList : MonoBehaviour
{
    public static string ProjectsDir => Path.Combine(Application.persistentDataPath, @"Projects");
    
    [SerializeField] private LevelManager levelManager;

    // New project
    [SerializeField] private TMP_InputField projectNameInputField;
    [SerializeField] private TMP_InputField authorInputField;
    [SerializeField] private Button createButton;

    [SerializeField] private Transform projectUIHolder;
    [SerializeField] private GameObject projectUIPrefab;

    private void Awake()
    {
        projectNameInputField.text = "Untitled";
        authorInputField.text = "Unknown";
        //authorInputField.text = Environment.UserName;
    }

    private void OnEnable()
    {
        UpdateProjectListUI();
    }

    private void UpdateProjectListUI()
    {
        if (!Directory.Exists(ProjectsDir)) Directory.CreateDirectory(ProjectsDir);

        foreach (Transform t in projectUIHolder)
        {
            Destroy(t.gameObject);
        }

        var projectFilePaths = Directory.GetFiles(ProjectsDir);
        foreach (var projectPath in projectFilePaths)
        {
            var projectInfo = new ProjectInfo() {
                name = Path.GetFileName(projectPath),
                path = projectPath
            };

            var projectUI = Instantiate(projectUIPrefab, projectUIHolder).GetComponent<ProjectUI>();
            projectUI.Initialize(projectInfo);
        }
    }

    public void CreateProject()
    {
        var name = projectNameInputField.text;

        var projectInfo = new ProjectInfo(projectNameInputField.text, authorInputField.text, Path.Combine(ProjectsDir, $"{projectNameInputField.text}.rhito"));
        levelManager.CreateProject(projectInfo);
    }
}

public class ProjectInfo
{
    /// <summary>
    /// This project name can possibly mismatch with the folder name
    /// </summary>
    public string name = "New Project";
    /// <summary>
    /// The creator's username
    /// </summary>
    public string author = "Unknown";

    /// <summary>
    /// The name of the music
    /// </summary>
    public string musicTitle = "None";
    /// <summary>
    /// The author/creator of the music
    /// </summary>
    public string musicAuthor = "Unknown";

    /// <summary>
    /// Relative audio path from the project
    /// </summary>
    public string audioPath;

    [JsonIgnore]
    public string path { get; set; }
    [JsonIgnore]
    public string infoPath => Path.Combine(); 

    public ProjectInfo() { }
    public ProjectInfo(string name, string author, string path)
    {
        this.name = name;
        this.author = author;
        this.path = path;
    }
}