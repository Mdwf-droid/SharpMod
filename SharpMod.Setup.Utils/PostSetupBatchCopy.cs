using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using System.IO;
using System.Xml.Linq;

namespace SharpMod.Setup.Utils
{
    public class PostSetupBatchCopy : Task
    {
        public string ProjectBaseDirectory { get; set; }
        public string ProjectDestFilename { get; set; }
        public string Extensions { get; set; }

        private DirectoryInfo _projectDestDirectory;
        private List<FileSystemInfo> _Files;
        private List<String> _extensionList;

        public override bool Execute()
        {
            _Files = new List<FileSystemInfo>();
            _extensionList = new List<string>();

            _extensionList.AddRange(Extensions.ToUpper().Split(new char[] { ';' }));

            while (_extensionList.Remove(String.Empty)) ;

            FileInfo fi = new FileInfo(ProjectDestFilename);
            _projectDestDirectory = fi.Directory;

            XDocument loaded = XDocument.Load(ProjectDestFilename);
            XNamespace xns = loaded.Root.Name.Namespace;

            var q = (from c in loaded.Root.Descendants()
                     select c).ToList();

            DirectoryInfo dirInfo = new DirectoryInfo(ProjectBaseDirectory);
            if (!dirInfo.Exists)
            {
                Console.WriteLine("Invalid directory: " + ProjectBaseDirectory);
                return false;
            }


            foreach (var v in q)
            {
                if (v.Attribute("Include") != null)
                {
                    string _fileName = v.Attribute("Include").Value;
                    FileInfo _fi = new FileInfo(_fileName);
                    if (_extensionList.Contains(_fi.Extension.ToUpper()))
                    {
                        FileInfo DestFile = new FileInfo(_projectDestDirectory.FullName + "\\" + _fileName);
                        FileInfo srcFile = new FileInfo(dirInfo.FullName + "\\" + _fileName);
                        if (!DestFile.Exists && srcFile.Exists)
                        {
                            if (!DestFile.Directory.Exists)
                                DestFile.Directory.Create();
                            Console.WriteLine("add " + DestFile.FullName);
                            File.Copy(srcFile.FullName, DestFile.FullName);
                        }
                    }
                }
            }

            return true;
        }
    }
}
