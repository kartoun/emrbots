using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EMR_Simulator
{
    public class LabItem
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

        Guid labInstanceID;
        public Guid LabInstanceID
        {
            get { return labInstanceID; }
            set { labInstanceID = value; }
        }

        DateTime labDateTime;
        public DateTime LabDateTime
        {
            get { return labDateTime; }
            set { labDateTime = value; }
        }

        string labName;
        public string LabName
        {
            get { return labName; }
            set { labName = value; }
        }

        string labValue;
        public string LabValue
        {
            get { return labValue; }
            set { labValue = value; }
        }

        string labUnits;
        public string LabUnits
        {
            get { return labUnits; }
            set { labUnits = value; }
        }
    }
}
