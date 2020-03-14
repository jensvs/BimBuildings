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
            string tabName = "BimBuildings";
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            string website = "www.bimbuildings.nl";

            Util.Ribbon BimBuildings = new Util.Ribbon();
            BimBuildings.CreateRibbonTab(application, tabName);

            BimBuildings.CreateRibbonPanel(application,
            /*Tab Name*/                   tabName,
            /*Panel Name*/                 "Hide",
            /*Button Name*/                "Hide" + System.Environment.NewLine + "Level",
            /*Name of dll file*/           thisAssemblyPath,
            /*Command*/                    "BimBuildings.Command.HelloWorld",
            /*Image*/                      "Building.Png",
            /*ToolTip*/                    "Just an example of tooltip info you can include",
            /*ToolTipImage*/               "Building.Png",
            /*Website*/                    website);



            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}