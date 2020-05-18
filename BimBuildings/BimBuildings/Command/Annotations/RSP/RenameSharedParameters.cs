using Autodesk.Revit.ApplicationServices;
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
            UIApplication uiapp = commandData.Application;
            Application app = uiapp.Application;
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;

            //Shared parameter file
            string oldlFile = app.SharedParametersFilename;
            string newFile = @"C:\RenameSharedParameters\FAC_shared_parameters.txt";

            app.SharedParametersFilename = newFile;
            DefinitionFile sharedParameterFile = app.OpenSharedParameterFile();

            //Shared parameter group settings
            DefinitionGroups defGroups = sharedParameterFile.Groups;

            sb.Append(defGroups.Size + "\n");

            DefinitionGroup defGroup = defGroups.get_Item("Facedo");


            FamilyManager fman = doc.FamilyManager;

            using (Transaction tx = new Transaction(doc, "to family parameter"))
            {
                tx.Start();

                foreach (FamilyParameter p in fman.Parameters)
                {
                    if (p.Definition.Name.Contains("MDK"))
                    {
                        try
                        {
                            fman.ReplaceParameter(p, p.Definition.Name.Replace("MDK", "TEMP"), p.Definition.ParameterGroup, p.IsInstance);

                            sb.Append(p.Definition.Name + "\n");
                        }
                        catch { }
                    }
                }

                tx.Commit();
            }

            sb.Append("\n");

            using (Transaction tx = new Transaction(doc, "to shared parameter"))
            { 
                tx.Start();

                foreach (FamilyParameter p in fman.Parameters)
                {
                    if (p.Definition.Name.Contains("TEMP"))
                    {
                        try
                        {
                            string newname = p.Definition.Name.Replace("TEMP", "FAC");

                            ExternalDefinitionCreationOptions opt = new ExternalDefinitionCreationOptions(newname, p.Definition.ParameterType);

                            defGroup.Definitions.Create(opt);

                            ExternalDefinition newExtDef = defGroup.Definitions.get_Item(newname) as ExternalDefinition;

                            fman.ReplaceParameter(p, newname, p.Definition.ParameterGroup, p.IsInstance);

                            sb.Append(p.Definition.Name + "\n");
                        }
                        catch { }
                    }
                }

                tx.Commit();
            }
            
            TaskDialog.Show("Final Dialog", sb.ToString());

            return Result.Succeeded;
        }
    }
}
