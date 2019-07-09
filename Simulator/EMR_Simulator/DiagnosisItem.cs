using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EMR_Simulator
{
    public class DiagnosisItem
    {
        Guid patientID;
        public Guid PatientID
        {
            get { return patientID; }
            set { patientID = value; }
        }

        int admissionID;
        public int AdmissionID
        {
            get { return admissionID; }
            set { admissionID = value; }
        }

        string iCD_Code = null;
        public string ICD_Code
        {
            get { return iCD_Code; }
            set { iCD_Code = value; }
        }

        string iCD_Description = null;
        public string ICD_Description
        {
            get { return iCD_Description; }
            set { iCD_Description = value; }
        }        
    }
}
