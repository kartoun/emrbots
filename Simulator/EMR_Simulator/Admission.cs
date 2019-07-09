using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EMR_Simulator
{
    public class Admission
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

        DateTime admissionStartDate;
        public DateTime AdmissionStartDate
        {
            get { return admissionStartDate; }
            set { admissionStartDate = value; }
        }

        DateTime admissionEndDate;
        public DateTime AdmissionEndDate
        {
            get { return admissionEndDate; }
            set { admissionEndDate = value; }
        }
    }
}
