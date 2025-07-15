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

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
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
            var projectInfo = new ProjectInfo(Path.GetFileName(projectPath), path: projectPath);// temp

            var projectUI = Instantiate(projectUIPrefab, projectUIHolder).GetComponent<ProjectUI>();
            projectUI.Initialize(projectInfo);
        }
    }

    public void CreateProject()
    {
        var projectInfo = new ProjectInfo(projectNameInputField.text, authorInputField.text, Path.Combine(ProjectsDir, $"{projectNameInputField.text}.rhito"));
        levelManager.CreateProject(projectInfo);
    }
}

public struct ProjectInfo
{
    public string name;
    public string author;
    public string path;

    public ProjectInfo(string name = "New Project", string author = "Unknown", string path = "")
    {
        this.name = name;
        this.author = author;
        this.path = path;
    }
}