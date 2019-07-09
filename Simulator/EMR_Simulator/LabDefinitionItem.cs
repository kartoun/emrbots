using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EMR_Simulator
{
    public class LabDefinitionItem
    {
        string labName = null;
        public string LabName
        {
            get { return labName; }
            set { labName = value; }
        }

        string labMinValue = null;
        public string LabMinValue
        {
            get { return labMinValue; }
            set { labMinValue = value; }
        }

        string labMaxValue = null;
        public string LabMaxValue
        {
            get { return labMaxValue; }
            set { labMaxValue = value; }
        }

        string labUnits = null;
        public string LabUnits
        {
            get { return labUnits; }
            set { labUnits = value; }
        }
    }
}
