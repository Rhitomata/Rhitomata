using System;
using System.IO;
using System.Linq;
using SimpleFileBrowser;
using UnityEngine;

namespace Rhitomata
{
    /// <summary>
    /// A file system wrapper for Rhitomata as it may contain abstraction for platform specific code.
    /// I also don't want to keep putting System.IO everywhere I go
    /// </summary>
    public static class Storage
    {
        public static string dataPath { get; set; } = Application.persistentDataPath;

        /// <summary>
        /// Combines a path with another path
        /// </summary>
        public static string Combine(this string root, string path) => Path.Combine(root, path);
        /// <summary>
        /// Gets a path based on the specified dataPath or %ARPROOT
        /// </summary>
        public static string GetPathLocal(this string path) => string.IsNullOrEmpty(path) ? path : Path.Combine(dataPath, path);

        public static void CheckDirectoryLocal(string path) =>  CheckDirectory(GetPathLocal(path));
        /// <summary>
        /// Checks if a directory exists and if it doesn't, create it
        /// </summary>
        public static void CheckDirectory(string path)
        {
            if (!DirectoryExists(path) && !string.IsNullOrEmpty(path)) CreateDirectory(path);
        }

        public static string[] GetFilesLocal(string directory) => GetFiles(GetPathLocal(directory));
        public static string[] GetFiles(string directory)
        {
            CheckDirectory(directory);

            if (IsDirectoryWritable(directory))
                return Directory.GetFiles(directory);
            else
            {
                var entries = FileBrowserHelpers.GetEntriesInDirectory(directory, false);
                return (from entry in entries where !entry.IsDirectory select entry.Path).ToArray();
            }
        }

        public static string[] GetDirectoriesLocal(string directory) => GetDirectories(GetPathLocal(directory));
        public static string[] GetDirectories(string directory)
        {
            CheckDirectory(directory);

            if (IsDirectoryWritable(directory))
                return Directory.GetDirectories(directory);
            
            var entries = FileBrowserHelpers.GetEntriesInDirectory(directory, false);
            return (from entry in entries where entry.IsDirectory select entry.Path).ToArray();
        }

        public static bool FileExistsLocal(string path) => FileExists(GetPathLocal(path));
        public static bool FileExists(string path) => FileBrowserHelpers.FileExists(path);

        public static bool DirectoryExistsLocal(string path) => DirectoryExists(GetPathLocal(path));
        public static bool DirectoryExists(string path) => FileBrowserHelpers.DirectoryExists(path);

        public static void WriteAllTextLocal(string path, string content) => WriteAllText(GetPathLocal(path), content);
        public static void WriteAllText(string path, string content) => FileBrowserHelpers.WriteTextToFile(path, content);

        public static void WriteAllBytesLocal(string path, byte[] bytes) => WriteAllBytes(GetPathLocal(path), bytes);
        public static void WriteAllBytes(string path, byte[] bytes) => FileBrowserHelpers.WriteBytesToFile(path, bytes);

        public static string ReadAllTextLocal(string path) => ReadAllText(GetPathLocal(path));
        public static string ReadAllText(string path) => FileBrowserHelpers.ReadTextFromFile(path);

        public static byte[] ReadAllBytesLocal(string path) => ReadAllBytes(GetPathLocal(path));
        public static byte[] ReadAllBytes(string path) => FileBrowserHelpers.ReadBytesFromFile(path);

        public static string GetDirectoryName(string path) => FileBrowserHelpers.GetDirectoryName(path);

        public static void CreateDirectoryLocal(string path) => CreateDirectory(GetPathLocal(path));
        public static void CreateDirectory(string path) => FileBrowserHelpers.CreateFolderInDirectory(GetDirectoryName(path), GetFileName(path));

        public static void MoveFileLocal(string from, string to) => MoveFile(GetPathLocal(from), GetPathLocal(to));
        public static void MoveFile(string from, string to) => FileBrowserHelpers.MoveFile(from, to);

        public static void MoveDirectoryLocal(string from, string to) => MoveDirectory(GetPathLocal(from), GetPathLocal(to));
        public static void MoveDirectory(string from, string to) => FileBrowserHelpers.MoveDirectory(from, to);

        public static void DeleteFileLocal(string path) => DeleteFile(GetPathLocal(path));
        public static void DeleteFile(string path) => FileBrowserHelpers.DeleteFile(path);

        public static void DeleteDirectoryLocal(string path) => DeleteDirectory(GetPathLocal(path));
        public static void DeleteDirectory(string path) => FileBrowserHelpers.DeleteDirectory(path);

        public static void CopyFileLocal(string from, string to) => CopyFile(GetPathLocal(from), GetPathLocal(to));
        public static void CopyFile(string from, string to) => FileBrowserHelpers.CopyFile(from, to);

        public static string GetFileName(string path)
        {
            var value = FileBrowserHelpers.GetFilename(path);
            if (string.IsNullOrWhiteSpace(value))
                return Path.GetFileName(path);
            return value;
        }

        public static bool IsDirectoryWritable(string dirPath, bool throwIfFails = false)
        {
            try
            {
                using (FileStream fs = File.Create(
                    Path.Combine(
                        dirPath,
                        Path.GetRandomFileName()
                    ),
                    1,
                    FileOptions.DeleteOnClose)
                )
                { }
                return true;
            }
            catch
            {
                if (throwIfFails)
                    throw;
                else
                    return false;
            }
        }

        /// <summary>
        /// Similar functionality as Path.GetDirectoryName(string path)
        /// </summary>
        public static string GoUpFolder(string dir)
        {
            var e = dir.Split(new[] { '/', '\\' });
            var y = dir.Remove(dir.Length - e[e.Length - 1].Length, e[e.Length - 1].Length);
            if (y.EndsWith("\\", StringComparison.CurrentCulture) || y.EndsWith("/", StringComparison.CurrentCulture))
            {
                y = y.Remove(y.Length - 1, 1);
            }
            return y;
        }
    }
}