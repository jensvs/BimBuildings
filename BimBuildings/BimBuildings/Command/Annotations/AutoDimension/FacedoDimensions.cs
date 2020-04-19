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
    class FacedoDimensions : IExternalCommand
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
            var sectionView = uidoc.ActiveView;

            // Check if Dimension can be created in currently active project view.
            bool canCreateDimensionInView = false;
            switch (sectionView.ViewType)
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
                    canCreateDimensionInView = true;
                    break;
            }

            // Check if Dimension can be created
            if (!canCreateDimensionInView)
            {
                Message.Display("Dimension can't be created in the current view.", WindowType.Warning);
                return Result.Cancelled;
            }

            //Check if activeView is an Elevation
            if(canCreateDimensionInView)
            {
                // Ask user to select one generic model.
                var selectionReference = uidoc.Selection.PickObject(ObjectType.Element, new SelectionFilterByCategory("Generic Models"), "Select one generic model.");
                Element selectionElement = doc.GetElement(selectionReference);
                FamilyInstance genericModelFamily = doc.GetElement(selectionElement.Id) as FamilyInstance;
                FamilySymbol genericModelSymbol = genericModelFamily.Symbol;

                // Checks if selection isn't empty
                if (genericModelFamily == null)
                {
                    Message.Display("You haven't selected a valid element.\nPlease selected another element.", WindowType.Error);
                    return Result.Cancelled;
                }
                #region//Get DimensionType
                DimensionType genericModelDimension = collector.GetDimensionTypeByName(doc, "hoofdmaatvoering");
                DimensionType nestedFamilyDimension = collector.GetDimensionTypeByName(doc, "stelkozijn");
                #endregion

                #region//Get directions for dimensions
                XYZ widthDirection = sectionView.RightDirection.Normalize();
                XYZ heigthDirection = new XYZ(0, 0, 1);
                #endregion

                #region//Get nested family 1
                string nestedFamilyName1 = "31_MDK_GM_stelkozijn_lijn";
                FamilyInstance nestedFamily1 = null;

                ICollection<ElementId> subComponentIds1 = genericModelFamily.GetSubComponentIds();
                foreach (ElementId id in subComponentIds1)
                {
                    if (doc.GetElement(id).Name == nestedFamilyName1)
                    {
                        nestedFamily1 = doc.GetElement(id) as FamilyInstance;
                    }
                }
                #endregion

                #region//Get nested family 2
                FamilyInstance nestedFamily2 = null;
                string nestedFamilyName2 = "Profiel_1";

                List<double> nestedFamilyOriginZ = new List<double>();
                List<FamilyInstance> nestedFamilies = new List<FamilyInstance>();

                FamilyInstance familyContainer = null;
                LocationCurve curveContainer = null;
                Line lineContainer = null;

                ICollection<ElementId> subComponentIds2 = genericModelFamily.GetSubComponentIds();
                foreach (ElementId id in subComponentIds2)
                {
                    if (doc.GetElement(id).Name == nestedFamilyName2)
                    {
                        nestedFamilies.Add(doc.GetElement(id) as FamilyInstance);
                        familyContainer = doc.GetElement(id) as FamilyInstance;
                        curveContainer = familyContainer.Location as LocationCurve;
                        lineContainer = curveContainer.Curve as Line;
                        nestedFamilyOriginZ.Add(lineContainer.Origin.Z);
                    }
                }

                double minValue = int.MaxValue;
                int minIndex;
                int index = -1;

                foreach (double num in nestedFamilyOriginZ)
                {
                    index++;
                    if (num <= minValue)
                    {
                        minValue = num;
                        minIndex = index;
                    }
                }

                nestedFamily2 = nestedFamilies[index];

                if (nestedFamily2 == null)
                {
                    Message.Display("There isn't a nested family in the element with the specified name.", WindowType.Error);
                    return Result.Cancelled;
                }
                #endregion

                #region//Get type parameters of element
                double MDK_breedte = genericModelSymbol.LookupParameter("MDK_breedte").AsDouble();
                string MDK_merk = genericModelSymbol.LookupParameter("MDK_merk").AsString();
                #endregion

                #region//Get instance parameters of element
                double MDK_offset_vooraanzicht = genericModelFamily.LookupParameter("MDK_offset_vooraanzicht").AsDouble();
                double MDK_hoogte = genericModelFamily.LookupParameter("MDK_hoogte").AsDouble();
                #endregion

                #region//Get direction of family
                LocationCurve locationCurve = nestedFamily2.Location as LocationCurve;
                Line locationLine = locationCurve.Curve as Line;
                XYZ dir = locationLine.Direction.Normalize(); ;
                #endregion

                #region//Check if generic model is in same direction as view
                double genericModelAngle = Math.Round(Math.Atan2(dir.Y, dir.X) * (180 / Math.PI), 5);
                double activeViewAngle = Math.Round(Math.Atan2(widthDirection.Y, widthDirection.X) * (180 / Math.PI), 5);
                if (genericModelAngle <= 0)
                {
                    genericModelAngle = genericModelAngle + 180;
                }
                else
                {
                    genericModelAngle = genericModelAngle - 180;
                }

                if (genericModelAngle != activeViewAngle)
                {
                    Message.Display("The generic model isn't parallel to the active view.", WindowType.Error);
                    return Result.Cancelled;
                }
                #endregion

                #region//Get locationpoint of selected element
                LocationPoint location = genericModelFamily.Location as LocationPoint;
                XYZ locationpoint = location.Point;
                #endregion

                #region//Create endpoints for line creation
                XYZ genericModelHeight = GetDistance(locationpoint, widthDirection, MDK_breedte, 1000);
                XYZ genericModelWidth = new XYZ(locationpoint.X, locationpoint.Y, locationpoint.Z + MDK_offset_vooraanzicht + MDK_hoogte + converter.ConvertToFeet(1000));
                XYZ nestedFamilyHeight = GetDistance(locationpoint, widthDirection, MDK_breedte, 500);
                XYZ nestedFamilyWidth = new XYZ(locationpoint.X, locationpoint.Y, locationpoint.Z + MDK_offset_vooraanzicht + MDK_hoogte + converter.ConvertToFeet(500));
                XYZ textNoteLevelOrigin = locationpoint - widthDirection * converter.ConvertToFeet(500) + heigthDirection * converter.ConvertToFeet(200);
                XYZ textNoteHeaderOrigin = locationpoint - heigthDirection * converter.ConvertToFeet(200);
                #endregion

                #region//Create line for dimension
                Line genericModelHeightLine = Line.CreateBound(genericModelHeight, genericModelHeight + heigthDirection * 100);
                Line genericModelWidthLine = Line.CreateBound(genericModelWidth, genericModelWidth + widthDirection * 100);
                Line nestedFamilyHeightLine = Line.CreateBound(nestedFamilyHeight, nestedFamilyHeight + heigthDirection * 100);
                Line nestedFamilyWidthLine = Line.CreateBound(nestedFamilyWidth, nestedFamilyWidth + widthDirection * 100);
                Line levelLine = Line.CreateBound(locationpoint - widthDirection * converter.ConvertToFeet(500), locationpoint + widthDirection * (MDK_breedte + converter.ConvertToFeet(1000)));
                #endregion

                #region// Get references which refer to the reference planes in the family
                ReferenceArray genericModelHeightref = new ReferenceArray();
                ReferenceArray genericModelWidthref = new ReferenceArray();
                ReferenceArray nestedFamilyHeightref = new ReferenceArray();
                ReferenceArray nestedFamilyWidthref = new ReferenceArray();

                foreach (var e in genericModelFamily.GetReferences(FamilyInstanceReferenceType.Top))
                { genericModelHeightref.Append(e); }

                foreach (var e in genericModelFamily.GetReferences(FamilyInstanceReferenceType.Bottom))
                { genericModelHeightref.Append(e); }

                foreach (var e in genericModelFamily.GetReferences(FamilyInstanceReferenceType.StrongReference))
                { genericModelWidthref.Append(e); }

                foreach (var e in nestedFamily1.GetReferences(FamilyInstanceReferenceType.StrongReference))
                { nestedFamilyWidthref.Append(e); }

                foreach (var e in nestedFamily1.GetReferences(FamilyInstanceReferenceType.Top))
                { nestedFamilyHeightref.Append(e); }

                foreach (var e in nestedFamily1.GetReferences(FamilyInstanceReferenceType.Bottom))
                { nestedFamilyHeightref.Append(e); }
                #endregion

                #region//Textnote Options
                TextNoteOptions textNoteLevelOptions = new TextNoteOptions();
                textNoteLevelOptions.HorizontalAlignment = HorizontalTextAlignment.Left;
                textNoteLevelOptions.TypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
                #endregion

                #region//Create Dimensions
                using (Transaction tx = new Transaction(doc))
                {
                    tx.Start("Create Dimensions");

                    #region//Create and set workplane to place dimensions on
                    Plane plane = Plane.CreateByNormalAndOrigin(sectionView.ViewDirection, sectionView.Origin);
                    SketchPlane sketchPlane = SketchPlane.Create(doc, plane);
                    sectionView.SketchPlane = sketchPlane;
                    #endregion

                    #region//Create Level Line
                    DetailCurve levelDetailLine = doc.Create.NewDetailCurve(sectionView, levelLine);
                    #endregion

                    #region//Create Textnotes
                    TextNote textNoteLevel = TextNote.Create(doc, sectionView.Id, textNoteLevelOrigin, "+vlp", textNoteLevelOptions);
                    TextNote textNoteHeader = TextNote.Create(doc, sectionView.Id, textNoteHeaderOrigin, MDK_merk, textNoteLevelOptions);
                    #endregion

                    if (MDK_offset_vooraanzicht > 0)
                    {
                        genericModelHeightref.Append(levelDetailLine.GeometryCurve.Reference);
                    }
  
                    #region//Create Dimensions
                    doc.Create.NewDimension(sectionView, genericModelHeightLine, genericModelHeightref, genericModelDimension);
                    doc.Create.NewDimension(sectionView, genericModelWidthLine, genericModelWidthref, genericModelDimension);
                    doc.Create.NewDimension(sectionView, nestedFamilyHeightLine, nestedFamilyHeightref, nestedFamilyDimension);
                    doc.Create.NewDimension(sectionView, nestedFamilyWidthLine, nestedFamilyWidthref, nestedFamilyDimension);
                    #endregion

                    tx.Commit();
                }
                #endregion
            }
            return Result.Succeeded;
        }

        public XYZ GetDistance(XYZ locationpoint, XYZ dir, double width, double distance)
        {
            LengthUnitConverter converter = new LengthUnitConverter();

            XYZ point = XYZ.Zero;
            double totaldistance = (width + converter.ConvertToFeet(distance));

            if(dir.X == -1 || dir.X == 1)
            {
                point = new XYZ(locationpoint.X + dir.X * totaldistance,
                        locationpoint.Y,
                        locationpoint.Z);
                return point;
            }
            else if(dir.Y == -1 || dir.Y == 1)
            {
                point = new XYZ(locationpoint.X,
                        locationpoint.Y + dir.Y * totaldistance,
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
                    x = Math.Sin(radians) * totaldistance;
                    y = Math.Cos(radians) * totaldistance;

                    point = new XYZ(locationpoint.X + x, locationpoint.Y + y, locationpoint.Z);
                    return point;
                }
                else if(degrees > 90 && degrees < 180)
                {
                    //+X, -Y
                    degrees = degrees - 90;
                    radians = degrees * (Math.PI / 180);
                    x = Math.Cos(radians) * totaldistance;
                    y = Math.Sin(radians) * totaldistance;

                    point = new XYZ(locationpoint.X + x, locationpoint.Y - y, locationpoint.Z);
                    return point;
                }
                else if(degrees > -90 && degrees < 0)
                {
                    //-X, +Y
                    degrees = degrees + 90;
                    radians = degrees * (Math.PI / 180);
                    x = Math.Cos(radians) * totaldistance;
                    y = Math.Sin(radians) * totaldistance;

                    point = new XYZ(locationpoint.X - x, locationpoint.Y + y, locationpoint.Z);
                    return point;
                }
                else if(degrees > -180 && degrees < -90)
                {
                    //-X, -Y
                    degrees = degrees + 180;
                    radians = degrees * (Math.PI / 180);
                    x = Math.Sin(radians) * totaldistance;
                    y = Math.Cos(radians) * totaldistance;

                    point = new XYZ(locationpoint.X - x, locationpoint.Y - y, locationpoint.Z);
                    return point;
                }
                return point;
            }
        }
    }
}
