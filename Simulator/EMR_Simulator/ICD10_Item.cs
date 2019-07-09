using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EMR_Simulator
{
    public class ICD10_Item
    {
        string code = null;
        public string Code
        {
            get { return code; }
            set { code = value; }
        }

        string description = null;
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
    }
}
