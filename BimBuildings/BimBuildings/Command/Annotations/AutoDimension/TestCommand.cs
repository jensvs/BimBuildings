using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Creation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimBuildings.Command.Annotations.AutoDimension
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class TestCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Application context.
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;

            // Check if we are in the Revit project , not in family one.
            if (doc.IsFamilyDocument)
            {
                Message.Display("Can't use command in family document", WindowType.Warning);
                return Result.Cancelled;
            }

            // Get access to current view.
            var activeView = uidoc.ActiveView;

            // Check if Dimension can be created in currently active project view.
            bool canCreateDimensionInView = false;
            switch (activeView.ViewType)
            {
                case ViewType.FloorPlan:
                    canCreateDimensionInView = false;
                    break;
                case ViewType.CeilingPlan:
                    canCreateDimensionInView = false;
                    break;
                case ViewType.Detail:
                    canCreateDimensionInView = false;
                    break;
                case ViewType.Elevation:
                    canCreateDimensionInView = true;
                    break;
                case ViewType.Section:
                    canCreateDimensionInView = false;
                    break;
            }

            // Check if Dimension can be created
            if (!canCreateDimensionInView)
            {
                Message.Display("Dimension can't be created in the current view.", WindowType.Error);
                return Result.Cancelled;
            }

            //Check if activeView is an Elevation
            if(activeView.ViewType == ViewType.Elevation)
            {
                // Ask user to select one generic model.
                var selectionReference = uidoc.Selection.PickObject(ObjectType.Element, new SelectionFilterByCategory("Generic Models"), "Select one generic model.");
                Element selectionElement = doc.GetElement(selectionReference);
                FamilyInstance selectionFamily = doc.GetElement(selectionReference) as FamilyInstance;

                // Checks if selection isn't empty
                if(selectionFamily == null)
                {
                    return Result.Cancelled;
                }

                string data = string.Empty;

                // Options
                Options options = new Options();
                options.IncludeNonVisibleObjects = true;
                options.ComputeReferences = true;
                options.View = doc.ActiveView;

                StringBuilder sb = new StringBuilder();

                selectionFamily.GetReferences(FamilyInstanceReferenceType.Left);
                selectionFamily.GetReferences(FamilyInstanceReferenceType.Right);

                ItemFactoryBase.NewDimension(doc.ActiveView, line, references);


                //foreach(GeometryObject geometryObject in selectionFamily.get_Geometry(options))
                //{
                //    sb.Append(geometryObject + "\n");
                //}

                TaskDialog.Show("test", selectionFamily.GetReferences(FamilyInstanceReferenceType.Left).ToString());
            }

            return Result.Succeeded;
        }
    }
}
