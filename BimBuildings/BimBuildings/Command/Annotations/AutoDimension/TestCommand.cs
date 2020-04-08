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
using BimBuildings.Util;

namespace BimBuildings.Command.Annotations.AutoDimension
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class TestCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //String Builder
            StringBuilder sb = new StringBuilder();

            //Collector
            Collector collector = new Collector();

            //UnitConvertor
            LengthUnitConverter converter = new LengthUnitConverter();

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
                FamilySymbol familySymbol = selectionFamily.Symbol;

                //Get Nested families
                string nestedFamilyName = "31_MDK_GM_stelkozijn_lijn";
                FamilyInstance nestedFamily = null;

                ICollection<ElementId> subComponentIds = selectionFamily.GetSubComponentIds();
                foreach(ElementId id in subComponentIds)
                {
                    if(doc.GetElement(id).Name == nestedFamilyName)
                    {
                        nestedFamily = doc.GetElement(id) as FamilyInstance;
                    }
                }

                //Get DimensionType
                DimensionType dimType = collector.GetDimensionTypeByName(doc, "Test");

                // Checks if selection isn't empty
                if (selectionFamily == null)
                {
                    return Result.Cancelled;
                }

                // Options
                Options options = new Options();
                options.IncludeNonVisibleObjects = true;
                options.ComputeReferences = true;
                options.View = doc.ActiveView;

                //Get type parameters of element
                double MDK_breedte = familySymbol.LookupParameter("MDK_breedte").AsDouble();

                //Get instance parameters of element
                double MDK_offset_vooraanzicht = selectionFamily.LookupParameter("MDK_offset_vooraanzicht").AsDouble();

                // Get direction of Dimensions
                XYZ dir = activeView.RightDirection;
                XYZ dirTB = new XYZ(0, 0, 1);

                // Get locationpoint of selected element
                LocationPoint location = selectionFamily.Location as LocationPoint;
                XYZ locationpoint = location.Point;

                // Get references which refer to the reference planes in the family
                ReferenceArray referencesTB = new ReferenceArray();

                foreach (var e in selectionFamily.GetReferences(FamilyInstanceReferenceType.Top))
                { referencesTB.Append(e); }

                foreach (var e in selectionFamily.GetReferences(FamilyInstanceReferenceType.Bottom))
                { referencesTB.Append(e); }


                ReferenceArray referencesLR = new ReferenceArray();

                foreach (var e in selectionFamily.GetReferences(FamilyInstanceReferenceType.StrongReference))
                { referencesLR.Append(e); }

                ReferenceArray referencesLRstelkozijn = new ReferenceArray();

                foreach (var e in nestedFamily.GetReferences(FamilyInstanceReferenceType.StrongReference))
                { referencesLRstelkozijn.Append(e); }
                
                // Transaction for creating the dimensions
                using (Transaction t = new Transaction(doc))
                {
                    t.Start("dimension");

                    //Create and set workplane to place dimensions on
                    Plane plane = Plane.CreateByNormalAndOrigin(activeView.ViewDirection, activeView.Origin);
                    SketchPlane sketchPlane = SketchPlane.Create(doc, plane);
                    activeView.SketchPlane = sketchPlane;

                    //Create endpoints for line creation
                    XYZ hoogtemaatvoering = GetDistance(locationpoint, dir, MDK_breedte);
                    XYZ lengtemaatvoering = new XYZ(locationpoint.X, locationpoint.Y, locationpoint.Z + MDK_offset_vooraanzicht - converter.ConvertToFeet(1000));
                    XYZ stelkozijnlengtemaatvoering = new XYZ(locationpoint.X, locationpoint.Y, locationpoint.Z + MDK_offset_vooraanzicht - converter.ConvertToFeet(500));

                    //Create line for dimension
                    Line lineLR = Line.CreateBound(lengtemaatvoering, lengtemaatvoering + dir * 100);
                    Line lineTB = Line.CreateBound(hoogtemaatvoering, hoogtemaatvoering + dirTB * 100);
                    Line lineLRstelkozijn = Line.CreateBound(stelkozijnlengtemaatvoering, stelkozijnlengtemaatvoering + dir * 100);

                    //Create dimension
                    doc.Create.NewDimension(doc.ActiveView, lineLR, referencesLR, dimType);
                    doc.Create.NewDimension(doc.ActiveView, lineTB, referencesTB, dimType);
                    doc.Create.NewDimension(doc.ActiveView, lineLRstelkozijn, referencesLRstelkozijn, dimType);

                    //TaskDialog.Show("Info", sb.ToString());

                    t.Commit();
                }
            }
            return Result.Succeeded;
        }

        public XYZ GetDistance(XYZ locationpoint, XYZ dir, double width)
        {
            LengthUnitConverter converter = new LengthUnitConverter();

            XYZ point = XYZ.Zero;
            const int maatlijn = 500;
            double distance = (width + converter.ConvertToFeet(maatlijn));

            if(dir.X == -1 || dir.X == 1)
            {
                point = new XYZ(locationpoint.X + dir.X * distance,
                        locationpoint.Y,
                        locationpoint.Z);
                return point;
            }
            else if(dir.Y == -1 || dir.Y == 1)
            {
                point = new XYZ(locationpoint.X,
                        locationpoint.Y + dir.Y * distance,
                        locationpoint.Z);
                return point;
            }
            else
            {
                double degrees = Math.Round(Math.Atan2(dir.X, dir.Y) * (180 / Math.PI));
                double radians;
                double x;
                double y;

                if (degrees > 0 && degrees < 90)
                {
                    //+X, +Y
                    radians = degrees * (Math.PI / 180);
                    x = Math.Sin(radians) * distance;
                    y = Math.Cos(radians) * distance;

                    point = new XYZ(locationpoint.X + x, locationpoint.Y + y, locationpoint.Z);
                    return point;
                }
                else if(degrees > 90 && degrees < 180)
                {
                    //+X, -Y
                    degrees = degrees - 90;
                    radians = degrees * (Math.PI / 180);
                    x = Math.Cos(radians) * distance;
                    y = Math.Sin(radians) * distance;

                    point = new XYZ(locationpoint.X + x, locationpoint.Y - y, locationpoint.Z);
                    return point;
                }
                else if(degrees > -90 && degrees < 0)
                {
                    //-X, +Y
                    degrees = degrees + 90;
                    radians = degrees * (Math.PI / 180);
                    x = Math.Cos(radians) * distance;
                    y = Math.Sin(radians) * distance;

                    point = new XYZ(locationpoint.X - x, locationpoint.Y + y, locationpoint.Z);
                    return point;
                }
                else if(degrees > -180 && degrees < -90)
                {
                    //-X, -Y
                    degrees = degrees + 180;
                    radians = degrees * (Math.PI / 180);
                    x = Math.Sin(radians) * distance;
                    y = Math.Cos(radians) * distance;

                    point = new XYZ(locationpoint.X - x, locationpoint.Y - y, locationpoint.Z);
                    return point;
                }
                return point;
            }
        }
    }
}
