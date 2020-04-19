using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BimBuildings.Util
{
    class Ribbon
    {
        public void CreateRibbonTab(UIControlledApplication application, string name)
        {
            application.CreateRibbonTab(name);
        }

        public RibbonPanel CreateRibbonPanel(UIControlledApplication application, string tabName, string panelName)
        {
            return application.CreateRibbonPanel(tabName, panelName);
        }

        public void CreateRibbonPanel(UIControlledApplication application, string tabName, RibbonPanel panel, string dataName, string buttonName, string assemblyPath, string command, string imageName, string toolTip, string ttImageName, string website)
        {
            PushButtonData bData = new PushButtonData(dataName, buttonName, assemblyPath, command);
            PushButton pb = panel.AddItem(bData) as PushButton;
            
            //Image
            BitmapImage pbImage = new BitmapImage(new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", imageName)));
            pb.LargeImage = pbImage;

            //Tooltip
            pb.ToolTip = toolTip;

            //Tooltip Image
            BitmapImage ttImage = new BitmapImage(new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", ttImageName)));
            pb.ToolTipImage = ttImage;

            //Help
            pb.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, website));
        }
    }
}
