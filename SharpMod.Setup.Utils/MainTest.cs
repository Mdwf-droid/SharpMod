using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpMod.Setup.Utils
{
    class MainTest
    {
        static void Main()
        {
            PostSetupBatchCopy p = new PostSetupBatchCopy();
            p.ProjectBaseDirectory = @"..\..\SharpMod.SilverLight.UI";
            p.ProjectDestFilename = @"..\..\SharpMod.Setup\Version\SilverLight\SourceCode\SharpMod.SilverLight.UI\SharpMod.SilverLight.UI.csproj";
            p.Extensions = ".xaml;.fnt;";
            p.Execute();
           /* PostSetupModifyProject p = new PostSetupModifyProject();
            p.FileName = "SharpMod.SilverLight.csproj";
            p.Execute();*/
        }
    }
}
