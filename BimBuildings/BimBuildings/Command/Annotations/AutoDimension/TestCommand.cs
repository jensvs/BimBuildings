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
                Message.Display("Dimension can't be created in the current view.", WindowType.Warning);
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

                ReferenceArray referencesLR = new ReferenceArray();
                ReferenceArray referencesTB = new ReferenceArray();
                StringBuilder sb = new StringBuilder();
                XYZ dir = new XYZ();
                dir = activeView.ViewDirection;
                XYZ dirTB = new XYZ(0,0,1);

                LocationPoint location = selectionElement.Location as LocationPoint;
                XYZ aa = location.Point;

                foreach (var e in selectionFamily.GetReferences(FamilyInstanceReferenceType.Top))
                {
                    referencesTB.Append(e);
                    Element element = doc.GetElement(e);
                    sb.Append(element);
                }

                foreach (var e in selectionFamily.GetReferences(FamilyInstanceReferenceType.Bottom))
                {
                    referencesTB.Append(e);
                }

                foreach (var e in selectionFamily.GetReferences(FamilyInstanceReferenceType.StrongReference))
                {
                    referencesLR.Append(e);
                }
                
                using (Transaction t = new Transaction(doc))
                {
                    t.Start("dimension");

                    var plane = Plane.CreateByNormalAndOrigin(activeView.ViewDirection, activeView.Origin);
                    var sketchPlane = SketchPlane.Create(doc, plane);
                    activeView.SketchPlane = sketchPlane;

                    FilteredElementCollector dimensionTypeCollector = new FilteredElementCollector(doc).OfClass(typeof(DimensionType));
                    DimensionType dimType = null;

                    foreach (Element e in dimensionTypeCollector)
                    {                       
                        if (e.Name == "Test")
                        {
                            dimType = e as DimensionType;
                        }
                    }
                    if (dimType == null)
                    {
                        Message.Display("There is no dimension type named Test", WindowType.Warning);
                        return Result.Cancelled;
                    }

                    //XYZ pickpoint = uidoc.Selection.PickPoint();
                    TaskDialog.Show("ddd", sb.ToString());

                    //XYZ maatlijn = new XYZ(aa.X, aa.Y - 11, aa.Z - 2);

                    //Line lineLR = Line.CreateBound(maatlijn, maatlijn + GetDirection(dir) * 100);
                    //Line lineTB = Line.CreateBound(maatlijn, maatlijn + dirTB * 100);

                    //doc.Create.NewDimension(doc.ActiveView, lineLR, referencesLR, dimType);
                    //doc.Create.NewDimension(doc.ActiveView, lineTB, referencesTB, dimType);

                    t.Commit();
                }
            }

            return Result.Succeeded;
        }

        public XYZ GetDirection(XYZ viewDir)
        {
            XYZ direction = XYZ.Zero;

            if(viewDir.X == 1 || viewDir.X == -1)
            {
                direction = new XYZ(0, 1, 0);
                return direction;

            }else if(viewDir.Y == 1 || viewDir.Y == -1)
            {
                direction = new XYZ(1, 0, 0);
                return direction;
            }else
            {
                double degrees = Math.Round(Math.Atan2(viewDir.X, viewDir.Y) * (180/Math.PI));
                double radians;

                if(degrees < 0)
                {
                    degrees = degrees + 180;

                    if(degrees > 90)
                    {
                        radians = (degrees - 90) * (Math.PI / 180);
                        direction = new XYZ(Math.Tan(radians), 1, 0);
                        return direction;
                    }
                    else
                    {
                        radians = (degrees + 90) * (Math.PI / 180);
                        direction = new XYZ(Math.Tan(radians), 1, 0);
                        return direction;
                    }
                }
                else
                {
                    if (degrees > 90)
                    {
                        radians = (degrees - 90) * (Math.PI / 180);
                        direction = new XYZ(Math.Tan(radians), 1, 0);
                        return direction;
                    }
                    else
                    {
                        radians = (degrees + 90) * (Math.PI / 180);
                        direction = new XYZ(Math.Tan(radians), 1, 0);
                        return direction;
                    }
                }
            }
        }
    }
}
