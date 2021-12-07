using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using Microsoft.Build.Utilities;

namespace SharpMod.Setup.Utils
{
    public class PostSetupSolutionGenerator : Task
    {
        public String ProjectRoot { get; set; }
        public String SolutionFile { get; set; }

        public override bool Execute()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(ProjectRoot);
            if (!dirInfo.Exists)
            {
                Console.WriteLine("Invalid directory: " + ProjectRoot);
            }
            else
            {
                SolutionGenerator gen = new SolutionGenerator();
                int projectCount = gen.Execute(dirInfo, SolutionFile, true);

                Console.WriteLine(string.Format("Solution is generated, {0} projects added", projectCount));
            }
            return true;
        }
    }

    class SolutionGenerator
    {
        private List<FileSystemInfo> m_Projects;

        public SolutionGenerator()
        {
            this.m_Projects = new List<FileSystemInfo>();
        }

        public int Execute(DirectoryInfo rootDir, string solutionFile, bool recursive)
        {
            SearchDirectory(rootDir, recursive);

            using (StreamWriter writer = File.CreateText(solutionFile))
            {
                writer.WriteLine("Microsoft Visual Studio Solution File, Format Version 10.00");
                writer.WriteLine("# Visual Studio 2008");
                foreach (FileSystemInfo project in m_Projects)
                {
                    Guid projectGuid = GetProjectGuid(project);
                    writer.Write(string.Format(@"Project(""{{{0}}}"") = ", projectGuid.ToString().ToUpper()));
                    writer.WriteLine(string.Format(@"""{0}"", ""{1}"", ""{{{2}}}""",
                        project.Name.Substring(0, project.Name.Length - project.Extension.Length),
                        GetRelativePath(rootDir.FullName, project.FullName),
                        projectGuid.ToString().ToUpper()));
                    writer.WriteLine("EndProject");
                }
            }

            return m_Projects.Count;
        }

        private void SearchDirectory(DirectoryInfo dir, bool recursive)
        {
            FileSystemInfo[] files = dir.GetFileSystemInfos();
            foreach (FileSystemInfo file in files)
            {
                if (IsProjectFile(file))
                {


                    Console.WriteLine("Added project {0}", file.Name);
                    m_Projects.Add(file);

                }
            }

            if (recursive)
            {
                foreach (DirectoryInfo subdir in dir.GetDirectories("*"))
                {
                    SearchDirectory(subdir, recursive);
                }
            }
        }

        private bool IsProjectFile(FileSystemInfo file)
        {
            return file.Extension.ToUpper() == ".CSPROJ" && GetProjectGuid(file) != Guid.Empty;
        }

        private bool IsProjectListed(string projectPath, List<string> list)
        {
            string wildcardPattern = @"([\w|*|.|\\|-]+)";
            string project;

            foreach (var item in list)
            {
                string filter = item;
                if (filter.IndexOf(Path.DirectorySeparatorChar) < 0)
                {
                    // Filter by project name
                    filter = item.Replace("*", wildcardPattern);
                    project = Path.GetFileNameWithoutExtension(projectPath);
                }
                else
                {
                    // Filter by project path
                    filter = "[a-zA-Z]:" + wildcardPattern + item.Replace("\\", "\\\\").Replace("*", wildcardPattern);
                    project = Path.Combine(Path.GetDirectoryName(projectPath), Path.GetFileNameWithoutExtension(projectPath));
                }

                string filterRegex = "^" + filter + "$";
                Regex nameRegex = new Regex(filterRegex, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                Match nameMatch = nameRegex.Match(project);
                if (nameMatch.Success)
                {
                    return true;
                }
            }

            return false;
        }

        private Guid GetProjectGuid(FileSystemInfo file)
        {
            using (StreamReader reader = File.OpenText(file.FullName))
            {
                string text = reader.ReadToEnd();
                string pattern = "<ProjectGuid>";
                int start = text.IndexOf(pattern);
                if (start > 0)
                {
                    start += pattern.Length;
                    pattern = "</ProjectGuid>";
                    int end = text.IndexOf(pattern);
                    if (end > 0)
                    {
                        return new Guid(text.Substring(start + 1, end - start - 2));
                    }
                }
            }
            return Guid.Empty;
        }

        private string GetRelativePath(string rootDirPath, string absoluteFilePath)
        {
            string[] firstPathParts = rootDirPath.Trim(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
            string[] secondPathParts = absoluteFilePath.Trim(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);

            int sameCounter = 0;
            for (int i = 0; i < Math.Min(firstPathParts.Length,
            secondPathParts.Length); i++)
            {
                if (!firstPathParts[i].ToLower().Equals(secondPathParts[i].ToLower()))
                {
                    break;
                }
                sameCounter++;
            }

            if (sameCounter == 0)
            {
                return absoluteFilePath;
            }

            string newPath = String.Empty;
            for (int i = sameCounter; i < firstPathParts.Length; i++)
            {
                if (i > sameCounter)
                {
                    newPath += Path.DirectorySeparatorChar;
                }
                newPath += "..";
            }
            if (newPath.Length == 0)
            {
                newPath = ".";
            }
            for (int i = sameCounter; i < secondPathParts.Length; i++)
            {
                newPath += Path.DirectorySeparatorChar;
                newPath += secondPathParts[i];
            }
            return newPath;
        }
    }
}

