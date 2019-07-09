using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EMR_Simulator
{ 
    class PatientDefinitionItem
    {
        string rangeName = "";
        public string RangeName
        {
            get { return rangeName; }
            set { rangeName = value; }
        }

        string rangeMin = null;
        public string RangeMin
        {
            get { return rangeMin; }
            set { rangeMin = value; }
        }

        string rangeMax = null;
        public string RangeMax
        {
            get { return rangeMax; }
            set { rangeMax = value; }
        }

        string rangeWeight = null;
        public string RangeWeight
        {
            get { return rangeWeight; }
            set { rangeWeight = value; }
        }
    }
}