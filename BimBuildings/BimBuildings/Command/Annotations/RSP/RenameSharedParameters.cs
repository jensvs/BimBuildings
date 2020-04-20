using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimBuildings.Command.Annotations.RSP
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class RenameSharedParameters : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //String Builder
            StringBuilder sb = new StringBuilder();

            // Application context.
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;

            FamilyManager fman = doc.FamilyManager;

            using(Transaction tx = new Transaction(doc, "test"))
            {
                tx.Start();

                foreach (FamilyParameter p in fman.Parameters)
                {
                    if (p.Definition.Name.Contains("MDK"))
                    {
                        string newname = p.Definition.Name.Replace("MDK", "FAC");

                        if (p.IsShared)
                        {

                        }
                        else
                        {
                            fman.Replace
                                
                                RenameParameter(p, newname);
                        }
                           
                        
                        sb.Append(newname + "\n");
                    }
                    
                }

                TaskDialog.Show("test123", sb.ToString());

                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
