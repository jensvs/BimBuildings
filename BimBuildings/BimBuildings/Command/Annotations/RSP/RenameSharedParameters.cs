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
            DefinitionGroup defGroup = sharedParameterFile.Groups.Create("Facedo");

            //Shared parameter settings
            string newname = null;
            ParameterType pType = ParameterType.Area;
            ExternalDefinitionCreationOptions options = new ExternalDefinitionCreationOptions(newname, pType);

            FamilyManager fman = doc.FamilyManager;




            using(Transaction tx = new Transaction(doc, "test"))
            {
                tx.Start();

                foreach (FamilyParameter p in fman.Parameters)
                {
                    if (p.Definition.Name.Contains("MDK"))
                    {

                        newname = p.Definition.Name.Replace("MDK", "FAC");

                        pType = p.Definition.ParameterType;

                        try
                        {

                            foreach (DefinitionGroup dg in sharedParameterFile.Groups)
                            {
                                if (dg.Name == "Facedo")
                                {
                                    ExternalDefinition externalDefinition = dg.Definitions.Create(options) as ExternalDefinition;

                                    sb.Append(externalDefinition.ToString());

                                }
                            }
                        }
                        catch { }
                    }
                }

                TaskDialog.Show("test123", sb.ToString());

                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
