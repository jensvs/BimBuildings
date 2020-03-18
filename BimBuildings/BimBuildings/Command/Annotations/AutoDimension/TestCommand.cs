using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using BimBuildings.Command.Annotations.AutoDimension;
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

                // Options
                Options options = new Options();
                options.IncludeNonVisibleObjects = true;
                options.ComputeReferences = true;
                options.View = doc.ActiveView;

                ReferenceArray references = new ReferenceArray();
                StringBuilder sb = new StringBuilder();
                
                foreach(var e in selectionFamily.GetReferences(FamilyInstanceReferenceType.StrongReference))
                {
                    references.Append(e);
                }

                LocationPoint point = selectionElement.Location as LocationPoint;
                if(null != point)
                {
                    XYZ aa = point.Point;
                    XYZ bb = new XYZ(aa.X, aa.Y, aa.Z + 10);

                    sb.Append(aa);
                    sb.Append(bb);
                }

                XYZ dir = new XYZ(0, 1, 0);

                using (Transaction t = new Transaction(doc))
                {
                    t.Start("dimension");

                    TaskDialog.Show("ttt", sb.ToString());

                    //var plane = Plane.CreateByNormalAndOrigin(activeView.ViewDirection, activeView.Origin);
                    //var sketchPlane = SketchPlane.Create(doc, plane);
                    //activeView.SketchPlane = sketchPlane;
                    
                    //XYZ pickPoint = uidoc.Selection.PickPoint();
                    //Line line = Line.CreateBound(pickPoint, pickPoint + dir * 100);

                    //doc.Create.NewDimension(doc.ActiveView, line, references);

                    t.Commit();
                }
            }

            return Result.Succeeded;
        }
    }
}
