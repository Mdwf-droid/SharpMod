using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Xml.Linq;

namespace SharpMod.Setup.Utils
{
    public class PostSetupModifyProject : Task
    {
        public string FileName { get; set; }

        public override bool Execute()
        {
            bool toReturn = false;

            try
            {
                XDocument loaded = XDocument.Load(FileName);
                XNamespace xns = loaded.Root.Name.Namespace;

                XName n = XName.Get("Link", xns.NamespaceName);
                var q = (from c in loaded.Root.Descendants(n)

                         select c).ToList();
                /*from itmG in c.Elements("ItemGroup")
                from cmpl in itmG.Elements()
                where cmpl.Elements("Link") != null            
        select cmpl;*/

                XName includeName = XName.Get("Include", xns.NamespaceName);

                foreach (var v in q)
                {
                    var parentNode = v.Parent;

                    parentNode.Attribute("Include").SetValue(String.Format("{0}", v.Value));
                    v.Remove();
                }

                loaded.Save(FileName);
                toReturn = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return toReturn;
        }
    }    
}
