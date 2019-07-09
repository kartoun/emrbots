using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EMR_Simulator
{
    public class Patient
    {
        Guid patientID;
        public Guid PatientID
        {
            get { return patientID; }
            set { patientID = value; }
        }

        string patientGender = null;
        public string PatientGender
        {
            get { return patientGender; }
            set { patientGender = value; }
        }

        DateTime patientDateOfBirth;
        public DateTime PatientDateOfBirth
        {
            get { return patientDateOfBirth; }
            set { patientDateOfBirth = value; }
        }

        string patientRace = null;
        public string PatientRace
        {
            get { return patientRace; }
            set { patientRace = value; }
        }

        string patientMaritalStatus = null;
        public string PatientMaritalStatus
        {
            get { return patientMaritalStatus; }
            set { patientMaritalStatus = value; }
        }

        string patientLanguage = null;
        public string PatientLanguage
        {
            get { return patientLanguage; }
            set { patientLanguage = value; }
        }

        double patientPopulationPercentageBelowPoverty;
        public double PatientPopulationPercentageBelowPoverty
        {
            get { return patientPopulationPercentageBelowPoverty; }
            set { patientPopulationPercentageBelowPoverty = value; }
        }
    }
}
