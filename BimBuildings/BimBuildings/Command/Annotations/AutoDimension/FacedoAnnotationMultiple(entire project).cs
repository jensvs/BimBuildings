using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BimBuildings.Util;

namespace BimBuildings.Command.Annotations.AutoDimension
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class FacedoAnnotationMultiple2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region//Utils
            StringBuilder sb = new StringBuilder();
            Collector collector = new Collector();
            LengthUnitConverter converter = new LengthUnitConverter();
            #endregion

            #region//Application context
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            #endregion

            #region//Check if current project is family
            if (doc.IsFamilyDocument)
            {
                Message.Display("Can't use command in family document", WindowType.Warning);
                return Result.Cancelled;
            }
            #endregion

            #region//Select elements
            List<Element> genericModelList = collector.GetGenericModels(doc);
            List<Element> List_GenericModels = new List<Element>();
            List<string> List_GenericModelStrings = new List<string>();
            FamilyInstance familyInstance = null;

            foreach (Element genericModel in genericModelList)
            {
                familyInstance = doc.GetElement(genericModel.Id) as FamilyInstance;
                if (familyInstance.SuperComponent == null)
                {
                    if (List_GenericModelStrings.Contains(genericModel.Name))
                    { }
                    else
                    {
                        List_GenericModels.Add(genericModel);
                        List_GenericModelStrings.Add(genericModel.Name);
                    }
                }
            }
            #endregion

            #region//Check if list of generic models is empty
            if (List_GenericModels.Count == 0)
            {
                Message.Display("There aren't any generic models in the project", WindowType.Warning);
                return Result.Cancelled;
            }
            #endregion

            #region//Execute task for each generic model
            foreach (Element genericModel in List_GenericModels)
            {
                FamilyInstance genericModelFamily = doc.GetElement(genericModel.Id) as FamilyInstance;
                FamilySymbol genericModelSymbol = genericModelFamily.Symbol;

                #region //Get type parameters of element
                string MDK_merk = genericModelSymbol.LookupParameter("MDK_merk").AsString();
                #endregion

                #region //Get nested family
                FamilyInstance nestedFamily = null;
                string nestedFamilyName = "Profiel_1";

                List<double> nestedFamilyOriginZ = new List<double>();
                List<FamilyInstance> nestedFamilies = new List<FamilyInstance>();

                FamilyInstance familyContainer = null;
                LocationCurve curveContainer = null;
                Line lineContainer = null;

                ICollection<ElementId> subComponentIds = genericModelFamily.GetSubComponentIds();
                foreach (ElementId id in subComponentIds)
                {
                    if (doc.GetElement(id).Name == nestedFamilyName)
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

                nestedFamily = nestedFamilies[index];

                if (nestedFamily == null)
                {
                    Message.Display("There isn't a nested family in the element with the specified name.", WindowType.Error);
                    return Result.Cancelled;
                }
                #endregion

                #region//Get direction of family
                LocationCurve locationCurve = nestedFamily.Location as LocationCurve;
                Line locationLine = locationCurve.Curve as Line;
                XYZ dir = locationLine.Direction.Normalize(); ;
                #endregion

                #region//Create Boundingbox
                XYZ lineStart = locationLine.GetEndPoint(0);
                XYZ lineEnd = locationLine.GetEndPoint(1);
                XYZ lineDifference = lineEnd - lineStart;

                BoundingBoxXYZ genicModelbb = genericModel.get_BoundingBox(null);
                double minZ = genicModelbb.Min.Z;
                double maxZ = genicModelbb.Max.Z;

                double width = lineDifference.GetLength();
                double height = maxZ - minZ;
                double thickness = converter.ConvertToFeet(500);
                double offset = converter.ConvertToFeet(500);

                XYZ min = new XYZ(-width - converter.ConvertToFeet(1000), minZ - offset, - offset);
                XYZ max = new XYZ(width, maxZ + offset, + offset);

                XYZ midpoint = lineStart + 0.5 * lineDifference;
                XYZ up = XYZ.BasisZ;
                XYZ viewDir = dir.CrossProduct(up);

                Transform t = Transform.Identity;
                t.Origin = midpoint;
                t.BasisX = dir;
                t.BasisY = up;
                t.BasisZ = viewDir;

                BoundingBoxXYZ boundingBox = new BoundingBoxXYZ();
                boundingBox.Transform = t;
                boundingBox.Min = min;
                boundingBox.Max = max;
                #endregion

                #region//Get viewFamilyType
                ViewFamilyType viewFamily = collector.GetViewFamilyType(doc, ViewFamily.Section).FirstOrDefault();
                #endregion

                #region //Create Section
                using(Transaction tx = new Transaction(doc))
                {
                    tx.Start("Create Section");

                    ViewSection section = ViewSection.CreateSection(doc, viewFamily.Id, boundingBox);
                    section.Name = genericModel.Name;
                    section.Scale = 50;
                    section.CropBoxVisible = false;
                    section.get_Parameter(BuiltInParameter.SECTION_COARSER_SCALE_PULLDOWN_METRIC).Set(1);
                    section.LookupParameter("Subdiscipline").Set("LEGEND");

                    tx.Commit();
                }
                #endregion                
            }
            #endregion

            #region//Get all section views
            List<View> sectionViewList = collector.GetViews(doc, ViewType.Section);
            List<View> List_SectionViews = new List<View>();

            foreach (View sectionView in sectionViewList)
            {
                if (sectionView.Name.Contains("Kozijn"))
                {
                    List_SectionViews.Add(sectionView);
                }
            }
            #endregion

            #region//Execute task for each sectionView
            foreach (View sectionView in List_SectionViews)
            {
                #region//Get DimensionType
                DimensionType genericModelDimension = collector.GetLinearDimensionTypeByName(doc, "hoofdmaatvoering");
                DimensionType nestedFamilyDimension = collector.GetLinearDimensionTypeByName(doc, "stelkozijn");
                #endregion

                #region//Get directions for dimensions
                XYZ widthDirection = sectionView.RightDirection.Normalize();
                XYZ heigthDirection = new XYZ(0, 0, 1);
                #endregion

                #region//Select elements
                List<Element> genericModelInViewList = collector.GetGenericModels(doc, sectionView.Id);
                List<Element> List_GenericModelsInView = new List<Element>();
                FamilyInstance familyInstanceInView = null;

                foreach (Element genericModel in genericModelInViewList)
                {
                    familyInstanceInView = doc.GetElement(genericModel.Id) as FamilyInstance;
                    if (familyInstanceInView.SuperComponent == null)
                    {
                        List_GenericModelsInView.Add(genericModel);
                    }
                }
                #endregion

                #region//Execute task for each generic model
                foreach (Element genericModel in List_GenericModelsInView)
                {
                    FamilyInstance genericModelFamily = doc.GetElement(genericModel.Id) as FamilyInstance;
                    FamilySymbol genericModelSymbol = genericModelFamily.Symbol;

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
                    #endregion

                    #region//Get instance parameters of element
                    double MDK_offset_vooraanzicht = genericModelFamily.LookupParameter("MDK_offset_vooraanzicht").AsDouble();
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

                    #region//Create endpoints for line creation
                    XYZ genericModelHeight = GetDistance(locationpoint, widthDirection, MDK_breedte, 1000);
                    XYZ genericModelWidth = new XYZ(locationpoint.X, locationpoint.Y, locationpoint.Z + MDK_offset_vooraanzicht - converter.ConvertToFeet(1000));
                    XYZ nestedFamilyHeight = GetDistance(locationpoint, widthDirection, MDK_breedte, 500);
                    XYZ nestedFamilyWidth = new XYZ(locationpoint.X, locationpoint.Y, locationpoint.Z + MDK_offset_vooraanzicht - converter.ConvertToFeet(500));
                    #endregion

                    #region//Create line for dimension
                    Line genericModelHeightLine = Line.CreateBound(genericModelHeight, genericModelHeight + heigthDirection * 100);
                    Line genericModelWidthLine = Line.CreateBound(genericModelWidth, genericModelWidth + widthDirection * 100);
                    Line nestedFamilyHeightLine = Line.CreateBound(nestedFamilyHeight, nestedFamilyHeight + heigthDirection * 100);
                    Line nestedFamilyWidthLine = Line.CreateBound(nestedFamilyWidth, nestedFamilyWidth + widthDirection * 100);
                    #endregion

                    #region//Create Dimensions
                    using(Transaction tx = new Transaction(doc))
                    {
                        tx.Start("Create Dimensions");

                        #region//Create and set workplane to place dimensions on
                        Plane plane = Plane.CreateByNormalAndOrigin(sectionView.ViewDirection, sectionView.Origin);
                        SketchPlane sketchPlane = SketchPlane.Create(doc, plane);
                        sectionView.SketchPlane = sketchPlane;
                        #endregion

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
                #endregion
            }
            #endregion
            return Result.Succeeded;
        }

        public XYZ GetDistance(XYZ locationpoint, XYZ dir, double width, double distance)
        {
            LengthUnitConverter converter = new LengthUnitConverter();

            XYZ point = XYZ.Zero;
            double totaldistance = (width + converter.ConvertToFeet(distance));

            if (dir.X == -1 || dir.X == 1)
            {
                point = new XYZ(locationpoint.X + dir.X * totaldistance,
                        locationpoint.Y,
                        locationpoint.Z);
                return point;
            }
            else if (dir.Y == -1 || dir.Y == 1)
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
                else if (degrees > 90 && degrees < 180)
                {
                    //+X, -Y
                    degrees = degrees - 90;
                    radians = degrees * (Math.PI / 180);
                    x = Math.Cos(radians) * totaldistance;
                    y = Math.Sin(radians) * totaldistance;

                    point = new XYZ(locationpoint.X + x, locationpoint.Y - y, locationpoint.Z);
                    return point;
                }
                else if (degrees > -90 && degrees < 0)
                {
                    //-X, +Y
                    degrees = degrees + 90;
                    radians = degrees * (Math.PI / 180);
                    x = Math.Cos(radians) * totaldistance;
                    y = Math.Sin(radians) * totaldistance;

                    point = new XYZ(locationpoint.X - x, locationpoint.Y + y, locationpoint.Z);
                    return point;
                }
                else if (degrees > -180 && degrees < -90)
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
