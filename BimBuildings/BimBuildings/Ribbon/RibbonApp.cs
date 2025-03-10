#region Usings
//Standard Using statments for our code
using System;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Application = Autodesk.Revit.ApplicationServices.Application;
using System.Reflection;
using Autodesk.Revit.DB.Events;
using System.Windows.Media.Imaging;
using System.IO;
#endregion

namespace BimBuildings.Ribbon
{
    class RibbonApp : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            #region//Application info
            string tabName = "BimBuildings";
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            string website = "www.bimbuildings.nl";
            #endregion

            #region//Create RibbonTab Bimbuildings
            application.CreateRibbonTab(tabName);

            #region//Create RibbonPanel Test
            RibbonPanel test = application.CreateRibbonPanel(tabName, "Test");

            #region//Create PushButtonData
            PushButtonData HelloWorld = new PushButtonData("Test", "Test", thisAssemblyPath, "BimBuildings.ViewsOnSheets");
            #endregion

            #region//Create Buttons
            test.AddItem(HelloWorld);
            #endregion
            #endregion

            #region//Create RibbonPanel Dimensions
            RibbonPanel dimensions = application.CreateRibbonPanel(tabName, "Dimensions");
            RibbonPanel export = application.CreateRibbonPanel(tabName, "Export");

            #region//Create PushButtonData
            PushButtonData annotationSingle = new PushButtonData("AnnotationSingle", "Single\nAnnotation", thisAssemblyPath, "BimBuildings.Command.Annotations.AutoDimension.FacedoAnnotationSingle");
            PushButtonData annotationMultiple = new PushButtonData("AnnotationMultiple", "Multiple\nAnnotation", thisAssemblyPath, "BimBuildings.Command.Annotations.AutoDimension.FacedoAnnotationMultiple");
            PushButtonData annotationDelete = new PushButtonData("AnnotationDelete", "Delete", thisAssemblyPath, "BimBuildings.Command.Annotations.AutoDimension.FacedoAnnotationDelete");
            PushButtonData annotationDeleteAll = new PushButtonData("AnnotationDeleteAll", "Delete All", thisAssemblyPath, "BimBuildings.Command.Annotations.AutoDimension.FacedoAnnotationDeleteAll");
            PushButtonData annotationSettings = new PushButtonData("AnnotationSettings", "Settings", thisAssemblyPath, "BimBuildings.Command.Annotations.AutoDimension.FacedoAnnotationSettings");

            PushButtonData exportSchedules = new PushButtonData("ExportSchedules", "Export\nSchedules", thisAssemblyPath, "BimBuildings.Command.Export.ExportSchedules.ExportSchedules");
            #endregion

            #region//Set LargeImage 32x32
            BitmapImage singleLargeImage = GetBitmapImage("Dimensions 32x32.png");
            annotationSingle.LargeImage = singleLargeImage;

            BitmapImage multipleLargeImage = GetBitmapImage("Dimensions 32x32.png");
            annotationMultiple.LargeImage = multipleLargeImage;

            BitmapImage deleteLargeImage = GetBitmapImage("Delete 16x16.png");
            annotationDelete.LargeImage = deleteLargeImage;

            BitmapImage deleteAllLargeImage = GetBitmapImage("Delete 16x16.png");
            annotationDeleteAll.LargeImage = deleteAllLargeImage;

            BitmapImage settingsLargeImage = GetBitmapImage("Settings 16x16.png");
            annotationSettings.LargeImage = settingsLargeImage;

            BitmapImage exportLargeImage = GetBitmapImage("Excel 32x32.png");
            exportSchedules.LargeImage = exportLargeImage;
            #endregion

            #region//Set SmallImage 16x16
            BitmapImage singleSmallImage = GetBitmapImage("Dimensions 16x16.png");
            annotationSingle.Image = singleSmallImage;

            BitmapImage multipleSmallImage = GetBitmapImage("Dimensions 16x16.png");
            annotationMultiple.Image = multipleSmallImage;

            BitmapImage deleteSmallImage = GetBitmapImage("Delete 16x16.png");
            annotationDelete.Image = deleteSmallImage;

            BitmapImage deleteAllSmallImage = GetBitmapImage("Delete 16x16.png");
            annotationDeleteAll.Image = deleteAllSmallImage;

            BitmapImage settingsSmallImage = GetBitmapImage("Settings 16x16.png");
            annotationSettings.Image = settingsSmallImage;

            BitmapImage exportSmallImage = GetBitmapImage("Excel 16x16.png");
            exportSchedules.Image = exportSmallImage;
            #endregion

            #region//Create Buttons
            dimensions.AddItem(annotationSingle);
            dimensions.AddItem(annotationMultiple);
            dimensions.AddSeparator();
            dimensions.AddStackedItems(annotationDelete, annotationDeleteAll, annotationSettings);

            export.AddItem(exportSchedules);
            #endregion

            #endregion
            #endregion

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }

        public BitmapImage GetBitmapImage(string imageName)
        {
            BitmapImage image = new BitmapImage(new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", imageName)));
            return image;
        }
    }
}