using Autodesk.Revit.DB;

namespace BimBuildings
{
    public class AutoDimensionCommandData
    {
        public XYZ Direction { get; set; }
        public double Rotation { get; }

        public AutoDimensionCommandData()
        {        }
    }
}
