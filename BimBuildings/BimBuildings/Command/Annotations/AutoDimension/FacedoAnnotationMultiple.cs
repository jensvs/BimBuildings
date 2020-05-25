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
using Document = Autodesk.Revit.DB.Document;

namespace BimBuildings.Command.Annotations.AutoDimension
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class FacedoAnnotationMultiple : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region//Utils
            //String Builder
            StringBuilder sb = new StringBuilder();

            //Collector
            Collector collector = new Collector();

            //UnitConvertor
            LengthUnitConverter converter = new LengthUnitConverter();

            // Application context.
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;
            #endregion

            #region//Check if we are in the Revit project , not in family one.
            if (doc.IsFamilyDocument)
            {
                Message.Display("Can't use command in family document", WindowType.Warning);
                return Result.Cancelled;
            }
            #endregion

            #region//Get access to current view.
            var sectionView = uidoc.ActiveView;
            #endregion

            #region//Check if Dimension can be created in currently active project view.
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
            #endregion

            #region//Check if Dimension can be created
            if (!canCreateDimensionInView)
            {
                Message.Display("Dimension can't be created in the current view.", WindowType.Warning);
                return Result.Cancelled;
            }
            #endregion

            #region//Check if activeView is a proper view
            if (canCreateDimensionInView)
            {
                #region//Ask user to select one generic model.
                List<Element> mainWindowList = new List<Element>();
                try
                {
                    foreach(Element e in collector.GetGenericModels(doc,sectionView.Id))
                    {
                        FamilyInstance familyInstance = doc.GetElement(e.Id) as FamilyInstance;
                        if(familyInstance.SuperComponent == null)
                        {
                            mainWindowList.Add(e);
                        }
                    }
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Cancelled;
                }
                #endregion

                foreach(Element mainWindow in mainWindowList)
                {
                    #region//Get main window
                    FamilyInstance mainWindowFamily = doc.GetElement(mainWindow.Id) as FamilyInstance;
                    FamilySymbol mainWindowSymbol = mainWindowFamily.Symbol;

                    #region//Get directions for dimensions
                    XYZ widthDirection = sectionView.RightDirection.Normalize();
                    XYZ heigthDirection = new XYZ(0, 0, 1);
                    #endregion

                    #region// Checks if selection isn't empty
                    if (mainWindowFamily == null)
                    {
                        Message.Display("You haven't selected a valid element.\nPlease selected another element.", WindowType.Error);
                        return Result.Cancelled;
                    }
                    #endregion

                    #region//Check if generic model is in same direction as view
                    XYZ genericModelDir = GetReferenceDirection(mainWindowFamily.GetReferences(FamilyInstanceReferenceType.CenterFrontBack).First(), doc);

                    if (genericModelDir.ToString() != widthDirection.ToString())
                    {
                        Message.Display("The generic model isn't parallel to the active view.", WindowType.Error);
                        return Result.Cancelled;
                    }
                    #endregion

                    #region//Get Type parameters
                    double MDK_offset_vooraanzicht = mainWindowSymbol.LookupParameter("MDK_offset_vooraanzicht").AsDouble();
                    double MDK_hoogte = mainWindowSymbol.LookupParameter("MDK_hoogte").AsDouble();
                    double MDK_breedte = mainWindowSymbol.LookupParameter("MDK_breedte").AsDouble();
                    string MDK_merk = mainWindowSymbol.LookupParameter("MDK_merk").AsString();
                    #endregion

                    #region//Get locationpoint of selected element
                    LocationPoint mainWindowLocation = mainWindowFamily.Location as LocationPoint;
                    XYZ mainWindowLocationpoint = mainWindowLocation.Point;
                    #endregion

                    #region//main window references
                    ReferenceArray mainWindowHeight1 = new ReferenceArray();
                    ReferenceArray mainWindowHeight2 = new ReferenceArray();
                    ReferenceArray mainWindowWidth1 = new ReferenceArray();
                    ReferenceArray mainWindowWidth2 = new ReferenceArray();

                    foreach (var e in mainWindowFamily.GetReferences(FamilyInstanceReferenceType.Top))
                    {
                        mainWindowHeight1.Append(e);
                        mainWindowHeight2.Append(e);
                    }

                    foreach (var e in mainWindowFamily.GetReferences(FamilyInstanceReferenceType.Bottom))
                    {
                        mainWindowHeight1.Append(e);
                        mainWindowHeight2.Append(e);
                    }

                    foreach (var e in mainWindowFamily.GetReferences(FamilyInstanceReferenceType.Left))
                    {
                        mainWindowWidth1.Append(e);
                        mainWindowWidth2.Append(e);
                    }

                    foreach (var e in mainWindowFamily.GetReferences(FamilyInstanceReferenceType.Right))
                    {
                        mainWindowWidth1.Append(e);
                        mainWindowWidth2.Append(e);
                    }

                    foreach (Reference reference in mainWindowFamily.GetReferences(FamilyInstanceReferenceType.WeakReference))
                    {
                        string name = mainWindowFamily.GetReferenceName(reference);
                        if (name.Contains("center_tussenregel"))
                        {
                            mainWindowHeight1.Append(reference);
                        }

                        if (name.Contains("center_tussenstijl"))
                        {
                            mainWindowWidth1.Append(reference);
                        }
                    }
                    #endregion
                    #endregion

                    #region//Get DimensionType
                    DimensionType windowDimension = collector.GetLinearDimensionTypeByName(doc, "FAC - Diagonal - 2mm - Black");
                    DimensionType windowFrameDimension = collector.GetLinearDimensionTypeByName(doc, "FAC - Diagonal - 2mm - Stelkozijn");
                    DimensionType doorHandleDimension = collector.GetLinearDimensionTypeByName(doc, "FAC - Ordinate - 2mm - Krukhoogte");
                    DimensionType windowFrameLevelDimension = collector.GetLinearDimensionTypeByName(doc, "FAC - Ordinate - 2mm - Stelkozijn");
                    #endregion

                    #region//Get base line
                    List<DetailLine> detailLines = collector.GetDetailLines(doc, sectionView.Id);
                    string lineStyle = "FAC_vloerpeil";
                    Line detailLine = null;

                    foreach (DetailLine dl in detailLines)
                    {
                        if (dl.LineStyle.Name == lineStyle)
                        {
                            detailLine = dl.GeometryCurve as Line;
                        }
                        else
                        {
                            Message.Display("Can't find a DetailLine with the LineStyle, FAC_vloerpeil.", WindowType.Error);
                            return Result.Cancelled;
                        }
                    }
                    #endregion

                    #region//Get windowframe and window family!!!MDK WORDT NOG VERVANGEN DOOR FAC KAN ERROR VEROORZAKEN
                    ICollection<ElementId> subComponentIds = mainWindowFamily.GetSubComponentIds();

                    #region//Get windowframe family
                    string windowFrameName = "31_MDK_GM_stelkozijn_lijn";
                    FamilyInstance windowFrame = null;

                    foreach (ElementId id in subComponentIds)
                    {
                        if (doc.GetElement(id).Name == windowFrameName)
                        {
                            windowFrame = doc.GetElement(id) as FamilyInstance;
                        }
                    }

                    #region//windowframe references
                    ReferenceArray windowframeWidth = new ReferenceArray();
                    ReferenceArray windowframeHeight1 = new ReferenceArray();
                    ReferenceArray windowframeHeight2 = new ReferenceArray();

                    foreach (var e in windowFrame.GetReferences(FamilyInstanceReferenceType.Left))
                    { windowframeWidth.Append(e); }

                    foreach (var e in windowFrame.GetReferences(FamilyInstanceReferenceType.Right))
                    { windowframeWidth.Append(e); }

                    windowframeHeight2.Append(detailLine.Reference);

                    foreach (var e in windowFrame.GetReferences(FamilyInstanceReferenceType.Top))
                    {
                        windowframeHeight1.Append(e);
                        windowframeHeight2.Append(e);
                    }

                    foreach (var e in windowFrame.GetReferences(FamilyInstanceReferenceType.Bottom))
                    {
                        windowframeHeight1.Append(e);
                        windowframeHeight2.Append(e);
                    }
                    #endregion
                    #endregion

                    #region//Get window family
                    string windowName = "31_FAC_GM_vak_vleugel";
                    FamilyInstance window = null;
                    List<FamilyInstance> nestedFamilyList = new List<FamilyInstance>();

                    foreach (ElementId id in subComponentIds)
                    {
                        FamilyInstance fInstance = doc.GetElement(id) as FamilyInstance;
                        FamilySymbol fSymbol = fInstance.Symbol;

                        if (fSymbol.FamilyName == windowName)
                        {
                            window = doc.GetElement(id) as FamilyInstance;
                        }
                    }

                    ReferenceArray doorHandleHeightLevel = new ReferenceArray();
                    ReferenceArray doorHandleHeight = new ReferenceArray();

                    XYZ windowLocationPoint = null;
                    XYZ windowDimensionPoint1 = null;

                    Line windowDimension1 = null;

                    if (window != null)
                    {
                        #region//Get locationpoint of window
                        LocationPoint windowLocation = window.Location as LocationPoint;
                        windowLocationPoint = windowLocation.Point;
                        #endregion

                        #region//Create endpoint for line creation
                        windowDimensionPoint1 = GetDistance(windowLocationPoint, widthDirection, 0, -50);
                        #endregion

                        #region//Create line for dimension
                        windowDimension1 = Line.CreateBound(windowDimensionPoint1, windowDimensionPoint1 + heigthDirection * 100);
                        #endregion

                        #region//window references
                        doorHandleHeightLevel.Append(detailLine.Reference);

                        foreach (Reference reference in window.GetReferences(FamilyInstanceReferenceType.Bottom))
                        { doorHandleHeight.Append(reference); }

                        foreach (Reference reference in window.GetReferences(FamilyInstanceReferenceType.WeakReference))
                        {
                            string name = window.GetReferenceName(reference);
                            if (name.Contains("krukhoogte_binnen"))
                            {
                                doorHandleHeight.Append(reference);
                                doorHandleHeightLevel.Append(reference);
                            }
                        }
                        #endregion
                    }
                    #endregion
                    #endregion

                    #region//Create endpoints for line creation
                    XYZ RightDimensionPoint1 = GetDistance(mainWindowLocationpoint, widthDirection, MDK_breedte, 150);
                    XYZ RightDimensionPoint2 = GetDistance(mainWindowLocationpoint, widthDirection, MDK_breedte, 300);
                    XYZ RightDimensionPoint3 = GetDistance(mainWindowLocationpoint, widthDirection, MDK_breedte, 450);

                    XYZ BottomDimensionPoint1 = new XYZ(mainWindowLocationpoint.X, mainWindowLocationpoint.Y, mainWindowLocationpoint.Z + MDK_offset_vooraanzicht - converter.ConvertToFeet(150));
                    XYZ BottomDimensionPoint2 = new XYZ(mainWindowLocationpoint.X, mainWindowLocationpoint.Y, mainWindowLocationpoint.Z + MDK_offset_vooraanzicht - converter.ConvertToFeet(300));

                    XYZ LeftDimensionPoint1 = GetDistance(mainWindowLocationpoint, widthDirection, 0, -150);
                    XYZ LeftDimensionPoint2 = GetDistance(mainWindowLocationpoint, widthDirection, 0, -300);

                    XYZ TopDimensionPoint1 = new XYZ(mainWindowLocationpoint.X, mainWindowLocationpoint.Y, mainWindowLocationpoint.Z + MDK_offset_vooraanzicht + MDK_hoogte + converter.ConvertToFeet(150));
                    #endregion

                    #region//Create line for dimension
                    Line RightDimension1 = Line.CreateBound(RightDimensionPoint1, RightDimensionPoint1 + heigthDirection * 100);
                    Line RightDimension2 = Line.CreateBound(RightDimensionPoint2, RightDimensionPoint2 + heigthDirection * 100);
                    Line RightDimension3 = Line.CreateBound(RightDimensionPoint3, RightDimensionPoint3 + heigthDirection * 100);

                    Line BottomDimension1 = Line.CreateBound(BottomDimensionPoint1, BottomDimensionPoint1 + widthDirection * 100);
                    Line BottomDimension2 = Line.CreateBound(BottomDimensionPoint2, BottomDimensionPoint2 + widthDirection * 100);

                    Line LeftDimension1 = Line.CreateBound(LeftDimensionPoint1, LeftDimensionPoint1 + heigthDirection * 100);
                    Line LeftDimension2 = Line.CreateBound(LeftDimensionPoint2, LeftDimensionPoint2 + heigthDirection * 100);

                    Line TopDimension1 = Line.CreateBound(TopDimensionPoint1, TopDimensionPoint1 + widthDirection * 100);
                    #endregion

                    #region//Get selection filter
                    SelectionFilterElement filter = null;
                    List<SelectionFilterElement> filters = collector.GetSelectionFilter(doc);
                    string filtername = "Stelkozijn maatvoering";

                    foreach (SelectionFilterElement f in filters)
                    {
                        if (f.Name == filtername)
                        {
                            filter = f;
                        }
                    }
                    #endregion

                    #region//Create Annotations
                    using (Transaction tx = new Transaction(doc))
                    {
                        tx.Start("Create Annotations");

                        #region//Create and set workplane to place dimensions on
                        Plane plane = Plane.CreateByNormalAndOrigin(sectionView.ViewDirection, sectionView.Origin);
                        SketchPlane sketchPlane = SketchPlane.Create(doc, plane);
                        sectionView.SketchPlane = sketchPlane;
                        #endregion

                        #region//Create Dimensions
                        doc.Create.NewDimension(sectionView, RightDimension1, mainWindowHeight1, windowDimension);
                        doc.Create.NewDimension(sectionView, BottomDimension1, mainWindowWidth1, windowDimension);

                        Dimension windowFrameHeight1 = doc.Create.NewDimension(sectionView, LeftDimension1, windowframeHeight1, windowFrameDimension);
                        Dimension windowFrameHeight2 = doc.Create.NewDimension(sectionView, LeftDimension2, windowframeHeight2, windowFrameLevelDimension);

                        Dimension windowFrameWidth = doc.Create.NewDimension(sectionView, TopDimension1, windowframeWidth, windowFrameDimension);

                        #region//Add dimension to selection filter
                        filter.AddSingle(windowFrameWidth.Id);
                        filter.AddSingle(windowFrameHeight1.Id);
                        filter.AddSingle(windowFrameHeight2.Id);
                        #endregion

                        #region//Add prefix to window frame dimension
                        foreach (DimensionSegment seg in windowFrameHeight2.Segments)
                        {
                            seg.Prefix = "vlp +";
                        }
                        #endregion

                        #region//Add height dimension 2 and/or 3
                        if (mainWindowHeight1.Size != 2)
                        {
                            doc.Create.NewDimension(sectionView, RightDimension2, mainWindowHeight2, windowDimension);
                            if (window != null)
                            {
                                Dimension doorHandleHeight1 = doc.Create.NewDimension(sectionView, RightDimension3, doorHandleHeightLevel, doorHandleDimension);
                                Dimension doorHandleHeight2 = doc.Create.NewDimension(sectionView, windowDimension1, doorHandleHeight, doorHandleDimension);

                                foreach (DimensionSegment seg in doorHandleHeight1.Segments)
                                {
                                    seg.Prefix = "GH =";
                                    seg.Suffix = "+ vlp";
                                }

                                foreach (DimensionSegment seg in doorHandleHeight2.Segments)
                                {
                                    seg.Prefix = "GH =";
                                }
                            }
                        }
                        else
                        {
                            if (window != null)
                            {
                                Dimension doorHandleHeight1 = doc.Create.NewDimension(sectionView, RightDimension2, doorHandleHeightLevel, doorHandleDimension);
                                Dimension doorHandleHeight2 = doc.Create.NewDimension(sectionView, windowDimension1, doorHandleHeight, doorHandleDimension);

                                foreach (DimensionSegment seg in doorHandleHeight1.Segments)
                                {
                                    seg.Prefix = "GH =";
                                    seg.Suffix = "+ vlp";
                                }

                                foreach (DimensionSegment seg in doorHandleHeight2.Segments)
                                {
                                    seg.Prefix = "GH =";
                                }
                            }
                        }
                        #endregion

                        #region//Add width dimension 2
                        if (mainWindowWidth1.Size != 2)
                        {
                            doc.Create.NewDimension(sectionView, BottomDimension2, mainWindowWidth2, windowDimension);
                        }
                        #endregion

                        #endregion
                        tx.Commit();
                    }
                    #endregion
                }
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

        public XYZ GetReferenceDirection(Reference ref1, Document doc)
        // returns the direction perpendicular to reference
        // returns XYZ.Zero on error;
        {
            XYZ res = XYZ.Zero;
            XYZ workPlaneNormal = doc.ActiveView.SketchPlane.GetPlane().Normal;
            if (ref1.ElementId == ElementId.InvalidElementId) return res;
            Element elem = doc.GetElement(ref1.ElementId);
            if (elem == null) return res;
            if (ref1.ElementReferenceType == ElementReferenceType.REFERENCE_TYPE_SURFACE || ref1.ElementReferenceType == ElementReferenceType.REFERENCE_TYPE_LINEAR)
            {
                // make a dimension to a point for direction

                XYZ bEnd = new XYZ(10, 10, 10);
                ReferenceArray refArr = new ReferenceArray();
                refArr.Append(ref1);
                Dimension dim = null;
                using (Transaction t = new Transaction(doc, "test"))
                {
                    FailureHandlingOptions failureHandlingOptions = t.GetFailureHandlingOptions();
                    FailureHandler failureHandler = new FailureHandler();
                    failureHandlingOptions.SetFailuresPreprocessor(failureHandler);
                    failureHandlingOptions.SetClearAfterRollback(true);
                    t.SetFailureHandlingOptions(failureHandlingOptions);

                    t.Start();
                    using (SubTransaction st = new SubTransaction(doc))
                    {
                        st.Start();
                        ReferencePlane refPlane = doc.Create.NewReferencePlane(XYZ.Zero, bEnd, bEnd.CrossProduct(XYZ.BasisZ).Normalize(), doc.ActiveView);
                        ModelCurve mc = doc.Create.NewModelCurve(Line.CreateBound(XYZ.Zero, new XYZ(10, 10, 10)), SketchPlane.Create(doc, refPlane.Id));
                        refArr.Append(mc.GeometryCurve.GetEndPointReference(0));
                        dim = doc.Create.NewDimension(doc.ActiveView, Line.CreateBound(XYZ.Zero, new XYZ(10, 0, 0)), refArr);
                        ElementTransformUtils.MoveElement(doc, dim.Id, new XYZ(0, 0.1, 0));
                        st.Commit();
                    }
                    if (dim != null)
                    {
                        Curve cv = dim.Curve;
                        cv.MakeBound(0, 1);
                        XYZ pt1 = cv.GetEndPoint(0);
                        XYZ pt2 = cv.GetEndPoint(1);
                        res = pt2.Subtract(pt1).Normalize();
                    }
                    t.RollBack();
                }
            }
            return res;
        }
    }
}
