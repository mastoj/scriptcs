﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScriptCs.Tests
{
    public class VirtualFileSystem : IFileSystem
    {
        private VirtualDirectory _rootDirectory;
        private VirtualDirectory _currentDirectory;

        public VirtualFileSystem(string rootDirectoryName)
        {
            _rootDirectory = new VirtualDirectory(rootDirectoryName, null);
            _currentDirectory = _rootDirectory;
        }

        public void AddFile(string fileName, List<string> fileLines)
        {
            _currentDirectory.AddFile(fileName, fileLines);
        }

        private abstract class VirtualItem
        {
            public VirtualItem(string name, bool isFolder, VirtualDirectory parentDirectory)
            {
                ParentDirectory = parentDirectory;
                IsFolder = isFolder;
                Name = name;
            }
            public string Name { get; private set; }
            protected bool IsFolder { get; private set; }
            protected bool IsFile { get { return !IsFolder; } }
            protected VirtualDirectory ParentDirectory { get; private set;}

            public string GetFullPath()
            {
                if (ParentDirectory == null)
                {
                    return Name;
                }
                else
                {
                    return ParentDirectory.GetFullPath() + "\\" + Name;
                }
            }
        }

        private class VirtualFile : VirtualItem
        {
            public IEnumerable<string> FileLines { get; private set; }

            public VirtualFile(string fileName, IEnumerable<string> fileLines, VirtualDirectory parentDirectory)
                : base(fileName, false, parentDirectory)
            {
                FileLines = fileLines;
            }

            public string[] GetFileLines()
            {
                return FileLines.ToArray();
            }

            public VirtualDirectory GetParentDirectory()
            {
                return ParentDirectory;
            }
        }

        private class VirtualDirectory : VirtualItem
        {
            private Dictionary<string, VirtualFile> _files = new Dictionary<string, VirtualFile>();
            private Dictionary<string,VirtualDirectory> _folders = new Dictionary<string, VirtualDirectory>();

            public VirtualDirectory(string directoryName, VirtualDirectory parentDirectory) : base(directoryName, true, parentDirectory)
            { }
            public IEnumerable<VirtualItem> Items {
                get { return _files.Values.Concat<VirtualItem>(_folders.Values); }
            }

            public void AddFile(string fileName, IEnumerable<string> fileLines)
            {
                var pathSegments = SplitToSegments(fileName);
                if (pathSegments.Count() > 1)
                {
                    var directory = GetOrCreateDirectory(pathSegments[0]);
                    directory.AddFile(string.Join("/", pathSegments.Skip(1)), fileLines);
                }
                else
                {
                    _files.Add(fileName, new VirtualFile(fileName, fileLines, this));
                }
            }

            private VirtualDirectory GetOrCreateDirectory(string directoryName)
            {
                var directory = GetDirectory(directoryName);
                if (directory == null)
                {
                    directory = AddFolder(directoryName);
                }
                return directory;
            }

            private VirtualDirectory AddFolder(string directoryName)
            {
                return new VirtualDirectory(directoryName, this);
            }

            private VirtualDirectory GetDirectory(string directoryName)
            {
                if (_folders.ContainsKey(directoryName))
                {
                    return _folders[directoryName];
                }
                return null;
            }

            private List<string> SplitToSegments(string path)
            {
                var separators = new[] {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};
                return path.Split(separators).ToList();
            }

            public VirtualFile GetFile(string path)
            {
                var pathSegments = SplitToSegments(path);
                if (pathSegments.Count() > 1)
                {
                    var directory = GetDirectory(pathSegments[0]);
                    return directory.GetFile(string.Join("/", pathSegments.Skip(1)));
                }
                else
                {
                    return _files[path];
                }
            }

            public VirtualItem GetItem(string path)
            {
                var pathSegments = SplitToSegments(path);
                if (pathSegments.Count == 1)
                {
                    return Items.First(y => y.Name == pathSegments[0]);
                }
                if (pathSegments.Count > 1)
                {
                    var reminderPath = string.Join(Path.DirectorySeparatorChar.ToString(), pathSegments.Skip(1));
                    if (pathSegments[0] == ".")
                    {
                        return GetItem(reminderPath);
                    }
                    if (pathSegments[0] == "..")
                    {
                        return ParentDirectory.GetItem(reminderPath);
                    }
                    return _folders[pathSegments[0]].GetItem(reminderPath);
                }
                throw new FileNotFoundException(path);
            }
        }

        public IEnumerable<string> EnumerateFiles(string dir, string search, SearchOption searchOption = SearchOption.AllDirectories)
        {
            throw new NotImplementedException();
        }

        public void Copy(string source, string dest, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public bool DirectoryExists(string path)
        {
            throw new NotImplementedException();
        }

        public void CreateDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public void DeleteDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public string ReadFile(string path)
        {
            throw new NotImplementedException();
        }

        public string[] ReadFileLines(string path)
        {
            return _rootDirectory.GetFile(path).GetFileLines();
        }

        public DateTime GetLastWriteTime(string file)
        {
            throw new NotImplementedException();
        }

        public bool IsPathRooted(string path)
        {
            throw new NotImplementedException();
        }

        public string GetFullPath(string path)
        {
            return _currentDirectory.GetItem(path).GetFullPath();
        }

        public string CurrentDirectory
        {
            get
            {
                return _currentDirectory.GetFullPath();
            }
            set
            {
                _currentDirectory = (VirtualDirectory)_currentDirectory.GetItem(value);

            }
        }
        public string NewLine { get; private set; }
        public string GetWorkingDirectory(string path)
        {
            var file = _currentDirectory.GetFile(path);
            var workingDirectory = file.GetParentDirectory();
            return workingDirectory.GetFullPath();
        }

        public void Move(string source, string dest)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string path)
        {
            throw new NotImplementedException();
        }

        public void FileDelete(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> SplitLines(string value)
        {
            throw new NotImplementedException();
        }

        public void WriteToFile(string path, string text)
        {
            throw new NotImplementedException();
        }

        public Stream CreateFileStream(string filePath, FileMode mode)
        {
            throw new NotImplementedException();
        }

        public string ModulesFolder { get; private set; }
    }
}