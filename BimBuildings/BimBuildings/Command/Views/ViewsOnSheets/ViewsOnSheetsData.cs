﻿using Autodesk.Revit.DB;


namespace BimBuildings
{
    public class ViewsOnSheetsData
    {
        public Element element { get; set; }
        public string name { get; set; }
        public string familyname { get; set; }
        public string familyandtypename { get; set; }
    }
}
