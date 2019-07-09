using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace EMR_Simulator
{
    static public class Program
    {
        static string DATA_SERVER = "TBD";
        static string DATABASE_CONFIGURATION = "TBD";
        static string DATABASE_DEPLOYMENT = "TBD";
        static readonly Random rnd = new Random();

        static int POPULATION_SIZE = 10000;
        static List<Guid> patientIDs = new List<Guid>();
        static List<DateTime> patientDOBs = new List<DateTime>();
        static List<string> patientGenders = new List<string>();
        static List<string> patientRaces = new List<string>();
        static List<string> patientMaritalStatuses = new List<string>();
        static List<double> patientPopulationPercentageBelowPoverty = new List<double>();
        static List<string> patientLanguages = new List<string>();

        static List<Patient> patients = new List<Patient>();

        static void Main(string[] args)
        {
            List<string> tableNames = new List<string>();
            tableNames.Add("AdmissionsDiagnosesCorePopulatedTable");
            tableNames.Add("AdmissionsCorePopulatedTable");
            tableNames.Add("PatientCorePopulatedTable");
            tableNames.Add("LabsCorePopulatedTable");            
            DeleteCoreTables(tableNames);

            GeneratePatients();
            GenerateAdmissions();
            GenerateDiagnoses();
            GenerateLabs();
        }

        public static void GenerateLabs()
        {
            List<LabDefinitionItem> labDefinitionItems = new List<LabDefinitionItem>();
            labDefinitionItems = GetLabDefinitionItems();

            int i = 0;

            foreach (Admission a in allAdmissions)
            {
                List<LabItem> labItems = new List<LabItem>();

                DateTime current24SliceStartDate = a.AdmissionStartDate;

                var totalHoursLeft = (a.AdmissionEndDate - current24SliceStartDate).TotalHours;

                while (totalHoursLeft > 0)
                {
                    DateTime randomDate = GetRandomDate(current24SliceStartDate, current24SliceStartDate.AddDays(1));

                    int percentMissingValues = Convert.ToInt16(GetRandomNumber(5, 50));
                    int numberOfExistingLabValues = labDefinitionItems.Count - Convert.ToInt16((labDefinitionItems.Count * percentMissingValues / 100));

                    List<LabDefinitionItem> existingLabDefinitionItems = new List<LabDefinitionItem>();
                    existingLabDefinitionItems = labDefinitionItems.OrderBy(x => rnd.Next()).Take(numberOfExistingLabValues).ToList();

                    IEnumerable<LabDefinitionItem> nonExistingLabDefinitionItems = labDefinitionItems.Except(existingLabDefinitionItems);

                    // day one
                    foreach (LabDefinitionItem ldf in existingLabDefinitionItems)
                    {
                        LabItem li = new LabItem();
                        li.PatientID = a.PatientID;
                        li.AdmissionID = a.AdmissionID;
                        li.LabInstanceID = Guid.NewGuid();
                        li.LabName = ldf.LabName;
                        li.LabValue = Math.Round(GetRandomNumber(Convert.ToDouble(ldf.LabMinValue), Convert.ToDouble(ldf.LabMaxValue)), 1).ToString();
                        li.LabUnits = ldf.LabUnits;

                        if (totalHoursLeft < 24)
                        {
                            li.LabDateTime = GetRandomDate(current24SliceStartDate, a.AdmissionEndDate);
                        }
                        else
                        {
                            li.LabDateTime = GetRandomDate(current24SliceStartDate, current24SliceStartDate.AddDays(1));
                        }

                        labItems.Add(li);
                    }
                                        
                    current24SliceStartDate = current24SliceStartDate.AddDays(1);
                    totalHoursLeft = (a.AdmissionEndDate - current24SliceStartDate).TotalHours;
                }

                i++;
                                
                //Insert admission's labs   
                InsertLabItems(labItems);
            }            
        }

        public static void InsertLabItems(List<LabItem> admissionLabItems)
        {
            try
            {
                DataTable listRows = new DataTable();
                listRows.Columns.Add("PatientID", typeof(Guid));
                listRows.Columns.Add("AdmissionID", typeof(Int32));
                listRows.Columns.Add("LabInstanceID", typeof(Guid));                
                listRows.Columns.Add("LabName", typeof(String));
                listRows.Columns.Add("LabValue", typeof(String));
                listRows.Columns.Add("LabUnits", typeof(String));
                listRows.Columns.Add("LabDateTime", typeof(DateTime));

                foreach (LabItem li in admissionLabItems)
                {
                    DataRow dr = listRows.NewRow();

                    dr["PatientID"] = li.PatientID;
                    dr["AdmissionID"] = li.AdmissionID;
                    dr["LabInstanceID"] = li.LabInstanceID;
                    dr["LabName"] = li.LabName;
                    dr["LabValue"] = li.LabValue;
                    dr["LabUnits"] = li.LabUnits;
                    dr["LabDateTime"] = li.LabDateTime;

                    listRows.Rows.Add(dr);
                }

                SqlConnection bulkSqlConnector =
                    new SqlConnection("integrated security=SSPI;persist security info=False;Trusted_Connection=Yes;server="
                        + DATA_SERVER + ";" + "database=" + DATABASE_DEPLOYMENT + ";connection timeout=0");

                using (SqlConnection connection = bulkSqlConnector)
                {
                    connection.Open();

                    using (SqlBulkCopy bulkcopy = new SqlBulkCopy(connection))
                    {
                        bulkcopy.DestinationTableName = "dbo." + "LabsCorePopulatedTable";
                        bulkcopy.BulkCopyTimeout = 30000;

                        try
                        {
                            bulkcopy.WriteToServer(listRows);
                        }
                        catch (Exception ex)
                        {
                            //  throw ex;
                        }

                        connection.Close();
                    }
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void DeleteCoreTables(List<string> tableNames)
        {
            List<PatientDefinitionItem> pdis = new List<PatientDefinitionItem>();

            OpenConnectionMain(DATABASE_DEPLOYMENT);

            SqlDataReader reader = null;

            try
            {
                foreach (string st in tableNames)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append("Delete FROM [");
                    sb.Append(DATABASE_DEPLOYMENT);
                    sb.Append("].[dbo].");
                    sb.Append(st);  

                    SqlCommand cmdIns = new SqlCommand(sb.ToString(), myConnectionMain);
                    cmdIns.CommandTimeout = 60000;
                    cmdIns.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if ((reader != null) && !reader.IsClosed)
                {
                    reader.Close();
                }
            }

            CloseConnectionMain();
        }

        public static void GenerateDiagnoses()
        {
            List<DiagnosisItem> diagnosisItems = new List<DiagnosisItem>();

            List<ICD10_Item> icdItems = new List<ICD10_Item>();
            icdItems = GetICD10Items();

            foreach (Admission a in allAdmissions)
            {
                DiagnosisItem di = new DiagnosisItem();
                di.PatientID = a.PatientID;
                di.AdmissionID = a.AdmissionID;

                //In 70% of the time pick some random diagnosis
                int int3070 = Convert.ToInt16(GetRandomNumber(1, 10));

                // main diagnosis
                ICD10_Item icd = new ICD10_Item();

                if (int3070 >= 4)
                {
                    int randInt = Convert.ToInt16(GetRandomNumber(0, icdItems.Count - 1));
                    di.ICD_Code = icdItems[randInt].Code;
                    di.ICD_Description = icdItems[randInt].Description;
                }
                else
                {
                    int int_copd_rand = Convert.ToInt16(GetRandomNumber(1, 4));
                    if (int_copd_rand == 1)
                    {
                        di.ICD_Code = "J41";
                        di.ICD_Description = "Simple and mucopurulent chronic bronchitis";
                    }
                    if (int_copd_rand == 2)
                    {
                        di.ICD_Code = "J42";
                        di.ICD_Description = "Unspecified chronic bronchitis";
                    }
                    if (int_copd_rand == 3)
                    {
                        di.ICD_Code = "J43";
                        di.ICD_Description = "Emphysema";
                    }
                    if (int_copd_rand == 4)
                    {
                        di.ICD_Code = "J44";
                        di.ICD_Description = "Other chronic obstructive pulmonary disease";
                    }                    
                }

                diagnosisItems.Add(di);
            }

            InsertDiagnosisItems(diagnosisItems);
        }

        public static void InsertDiagnosisItems(List<DiagnosisItem> diagnosisItems)
        {
            try
            {
                DataTable listRows = new DataTable();
                listRows.Columns.Add("PatientID", typeof(Guid));
                listRows.Columns.Add("AdmissionID", typeof(Int32));
                listRows.Columns.Add("ICD_Code", typeof(String));
                listRows.Columns.Add("ICD_Description", typeof(String));

                foreach (DiagnosisItem di in diagnosisItems)
                {
                    DataRow dr = listRows.NewRow();

                    dr["PatientID"] = di.PatientID;
                    dr["AdmissionID"] = di.AdmissionID;
                    dr["ICD_Code"] = di.ICD_Code;
                    dr["ICD_Description"] = di.ICD_Description;

                    listRows.Rows.Add(dr);
                }

                SqlConnection bulkSqlConnector =
                    new SqlConnection("integrated security=SSPI;persist security info=False;Trusted_Connection=Yes;server="
                        + DATA_SERVER + ";" + "database=" + DATABASE_DEPLOYMENT + ";connection timeout=0");

                using (SqlConnection connection = bulkSqlConnector)
                {
                    connection.Open();

                    using (SqlBulkCopy bulkcopy = new SqlBulkCopy(connection))
                    {
                        bulkcopy.DestinationTableName = "dbo." + "AdmissionsDiagnosesCorePopulatedTable";
                        bulkcopy.BulkCopyTimeout = 30000;

                        try
                        {
                            bulkcopy.WriteToServer(listRows);
                        }
                        catch (Exception ex)
                        {
                            //  throw ex;
                        }

                        connection.Close();
                    }
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<ICD10_Item> GetICD10Items()
        {
            List<ICD10_Item> icdItems = new List<ICD10_Item>();

            OpenConnectionMain(DATABASE_CONFIGURATION);

            SqlDataReader reader = null;

            try
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("SELECT [Code], [Description] FROM [" + DATABASE_CONFIGURATION + "].[dbo].[v_ICD10Table]");
                                
                SqlCommand cmdIns = new SqlCommand(sb.ToString(), myConnectionMain);
                cmdIns.CommandTimeout = 600;
                reader = cmdIns.ExecuteReader();

                int i = 0;
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ICD10_Item item = new ICD10_Item();

                        item.Code = reader["Code"].ToString();
                        item.Description = reader["Description"].ToString();

                        icdItems.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if ((reader != null) && !reader.IsClosed)
                {
                    reader.Close();
                }
            }

            CloseConnectionMain();

            return icdItems;
        }

        public static List<LabDefinitionItem> GetLabDefinitionItems()
        {
            List<LabDefinitionItem> labItems = new List<LabDefinitionItem>();

            OpenConnectionMain(DATABASE_CONFIGURATION);

            SqlDataReader reader = null;

            try
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("SELECT [LabName],[LabMinValue],[LabMaxValue],[LabUnits] FROM [" + DATABASE_CONFIGURATION + "].[dbo].[LabsDefinitions]");

                SqlCommand cmdIns = new SqlCommand(sb.ToString(), myConnectionMain);
                cmdIns.CommandTimeout = 600;
                reader = cmdIns.ExecuteReader();

                int i = 0;
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        LabDefinitionItem item = new LabDefinitionItem();

                        item.LabName = reader["LabName"].ToString();
                        item.LabMinValue = reader["LabMinValue"].ToString();
                        item.LabMaxValue = reader["LabMaxValue"].ToString();
                        item.LabUnits = reader["LabUnits"].ToString();

                        labItems.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if ((reader != null) && !reader.IsClosed)
                {
                    reader.Close();
                }
            }

            CloseConnectionMain();

            return labItems;
        }
        
        public static void GeneratePatients()
        {
            for (int i = 0; i < POPULATION_SIZE; i++)
            {
                patientIDs.Add(Guid.NewGuid());
            }

            List<PatientDefinitionItem> pdis = new List<PatientDefinitionItem>();

            pdis = GetPatientDefinitionItems("DateOfBirth");
            foreach (PatientDefinitionItem pdi in pdis)
            {
                int numPatients = Convert.ToInt32((Convert.ToDouble(pdi.RangeWeight) / 100) * POPULATION_SIZE);

                for (int i = 0; i < numPatients; i++)
                {
                    DateTime randomDate = GetRandomDate(Convert.ToDateTime(pdi.RangeMin), Convert.ToDateTime(pdi.RangeMax));
                    patientDOBs.Add(randomDate);
                }
            }

            pdis = GetPatientDefinitionItems("Gender");
            foreach (PatientDefinitionItem pdi in pdis)
            {
                int numPatients = Convert.ToInt32((Convert.ToDouble(pdi.RangeWeight) / 100) * POPULATION_SIZE);

                for (int i = 0; i < numPatients; i++)
                {
                    patientGenders.Add(pdi.RangeMin);
                }
            }

            pdis = GetPatientDefinitionItems("MaritalStatus");
            foreach (PatientDefinitionItem pdi in pdis)
            {
                int numPatients = Convert.ToInt32((Convert.ToDouble(pdi.RangeWeight) / 100) * POPULATION_SIZE);

                for (int i = 0; i < numPatients; i++)
                {
                    patientMaritalStatuses.Add(pdi.RangeMin);
                }
            }

            pdis = GetPatientDefinitionItems("Race");
            foreach (PatientDefinitionItem pdi in pdis)
            {
                int numPatients = Convert.ToInt32((Convert.ToDouble(pdi.RangeWeight) / 100) * POPULATION_SIZE);

                for (int i = 0; i < numPatients; i++)
                {
                    patientRaces.Add(pdi.RangeMin);
                }
            }

            pdis = GetPatientDefinitionItems("PopulationPercentageBelowPoverty");
            foreach (PatientDefinitionItem pdi in pdis)
            {
                int numPatients = Convert.ToInt32((Convert.ToDouble(pdi.RangeWeight) / 100) * POPULATION_SIZE);

                for (int i = 0; i < numPatients; i++)
                {
                    double randomNumber = Math.Round(GetRandomNumber(Convert.ToDouble(pdi.RangeMin), Convert.ToDouble(pdi.RangeMax)), 2);
                    patientPopulationPercentageBelowPoverty.Add(randomNumber);
                }
            }

            pdis = GetPatientDefinitionItems("Language");
            foreach (PatientDefinitionItem pdi in pdis)
            {
                int numPatients = Convert.ToInt32((Convert.ToDouble(pdi.RangeWeight) / 100) * POPULATION_SIZE);

                for (int i = 0; i < numPatients; i++)
                {
                    patientLanguages.Add(pdi.RangeMin);
                }
            }

            patientDOBs = patientDOBs.OrderBy(item => rnd.Next()).ToList();
            patientGenders = patientGenders.OrderBy(item => rnd.Next()).ToList();
            patientMaritalStatuses = patientMaritalStatuses.OrderBy(item => rnd.Next()).ToList();
            patientLanguages = patientLanguages.OrderBy(item => rnd.Next()).ToList();
            patientRaces = patientRaces.OrderBy(item => rnd.Next()).ToList();
            patientPopulationPercentageBelowPoverty = patientPopulationPercentageBelowPoverty.OrderBy(item => rnd.Next()).ToList();

            int j = 0;
            foreach (Guid id in patientIDs)
            {
                Patient patient = new Patient();
                patient.PatientID = id;
                patient.PatientDateOfBirth = patientDOBs[j];
                patient.PatientGender = patientGenders[j];
                patient.PatientMaritalStatus = patientMaritalStatuses[j];
                patient.PatientLanguage = patientLanguages[j];
                patient.PatientRace = patientRaces[j];
                patient.PatientPopulationPercentageBelowPoverty = patientPopulationPercentageBelowPoverty[j];

                patients.Add(patient);

                j++;
            }

            InsertPatients(patients);
        }

        static DateTime endTimeHorizonDate = Convert.ToDateTime("01/01/2013");

        static List<Admission> allAdmissions = new List<Admission>();

        public static void GenerateAdmissions()
        {            
            foreach (Patient p in patients)
            {   
                int admissionID = 1;

                DateTime dob = p.PatientDateOfBirth; //check what is the DOB

                DateTime startDateFirstEncounter = GetRandomDate(dob.AddYears(18), dob.AddYears(58)); //Have a first encounter at least 18 years after DOB but less than 58 years.
                DateTime endDateFirstEncounter = startDateFirstEncounter.AddDays(GetRandomNumber(2, 20)); // LOS between 2 to 20 days.

                Admission firstAdmission = new Admission();
                
                firstAdmission.PatientID = p.PatientID;
                firstAdmission.AdmissionID = admissionID;
                firstAdmission.AdmissionStartDate = startDateFirstEncounter;
                firstAdmission.AdmissionEndDate = endDateFirstEncounter;

                allAdmissions.Add(firstAdmission);
                //admissions.Add(firstAdmission);

                var totalDaysLeft = (endTimeHorizonDate - endDateFirstEncounter).TotalDays; //Check what is the difference between the end of the encounter to 1/1/2013
                
                DateTime mostRecentAdmissionEndDate = endDateFirstEncounter;

                while (totalDaysLeft >= 365 * 5) //If the difference is large than five years then create another encounter
                {
                    Admission anotherAdmission = new Admission();
                    anotherAdmission.PatientID = p.PatientID;

                    admissionID++;
                    anotherAdmission.AdmissionID = admissionID;


                    int int_30day = Convert.ToInt16(GetRandomNumber(1, 10));

                    if (int_30day >= 5)
                    {
                        anotherAdmission.AdmissionStartDate = mostRecentAdmissionEndDate.AddDays(GetRandomNumber(5, totalDaysLeft));
                    }
                    else
                    {
                        anotherAdmission.AdmissionStartDate = mostRecentAdmissionEndDate.AddDays(GetRandomNumber(5, 30));
                    }
                    
                    anotherAdmission.AdmissionEndDate = anotherAdmission.AdmissionStartDate.AddDays(GetRandomNumber(2, 20));

                    totalDaysLeft = (endTimeHorizonDate - anotherAdmission.AdmissionEndDate).TotalDays;
                    mostRecentAdmissionEndDate = anotherAdmission.AdmissionEndDate;

                    allAdmissions.Add(anotherAdmission);
                }               
            }
            //xxx
            InsertAdmissions(allAdmissions);
        }

        public static void InsertPatients(List<Patient> patients)
        {
            try
            {
                DataTable listRows = new DataTable();
                listRows.Columns.Add("PatientID", typeof(Guid));                
                listRows.Columns.Add("PatientGender", typeof(String));
                listRows.Columns.Add("PatientDateOfBirth", typeof(DateTime));
                listRows.Columns.Add("PatientRace", typeof(String));
                listRows.Columns.Add("PatientMaritalStatus", typeof(String));
                listRows.Columns.Add("PatientLanguage", typeof(String));                
                listRows.Columns.Add("PatientPopulationPercentageBelowPoverty", typeof(Double));

                foreach (var patient in patients)
                {
                    DataRow dr = listRows.NewRow();

                    dr["PatientID"] = patient.PatientID;
                    dr["PatientGender"] = patient.PatientGender;
                    dr["PatientDateOfBirth"] = patient.PatientDateOfBirth;
                    dr["PatientRace"] = patient.PatientRace;
                    dr["PatientMaritalStatus"] = patient.PatientMaritalStatus;
                    dr["PatientLanguage"] = patient.PatientLanguage;
                    dr["PatientPopulationPercentageBelowPoverty"] = patient.PatientPopulationPercentageBelowPoverty;

                    listRows.Rows.Add(dr);
                }

                SqlConnection bulkSqlConnector =
                    new SqlConnection("integrated security=SSPI;persist security info=False;Trusted_Connection=Yes;server="
                        + DATA_SERVER + ";" + "database=" + DATABASE_DEPLOYMENT + ";connection timeout=0");

                using (SqlConnection connection = bulkSqlConnector)
                {
                    connection.Open();

                    using (SqlBulkCopy bulkcopy = new SqlBulkCopy(connection))
                    {
                        bulkcopy.DestinationTableName = "dbo." + "PatientCorePopulatedTable";
                        bulkcopy.BulkCopyTimeout = 30;

                        try
                        {
                            bulkcopy.WriteToServer(listRows);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }

                        connection.Close();
                    }
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertAdmissions(List<Admission> allAdmissions)
        {
            try
            {
                DataTable listRows = new DataTable();
                listRows.Columns.Add("PatientID", typeof(Guid));
                listRows.Columns.Add("AdmissionID", typeof(Int32));
                listRows.Columns.Add("AdmissionStartDate", typeof(DateTime));
                listRows.Columns.Add("AdmissionEndDate", typeof(DateTime));

                foreach (Admission a in allAdmissions)
                {
                    DataRow dr = listRows.NewRow();

                    dr["PatientID"] = a.PatientID;
                    dr["AdmissionID"] = a.AdmissionID;
                    dr["AdmissionStartDate"] = a.AdmissionStartDate;
                    dr["AdmissionEndDate"] = a.AdmissionEndDate;

                    listRows.Rows.Add(dr);
                }

                SqlConnection bulkSqlConnector =
                    new SqlConnection("integrated security=SSPI;persist security info=False;Trusted_Connection=Yes;server="
                        + DATA_SERVER + ";" + "database=" + DATABASE_DEPLOYMENT + ";connection timeout=0");

                using (SqlConnection connection = bulkSqlConnector)
                {
                    connection.Open();

                    using (SqlBulkCopy bulkcopy = new SqlBulkCopy(connection))
                    {
                        bulkcopy.DestinationTableName = "dbo." + "AdmissionsCorePopulatedTable";
                        bulkcopy.BulkCopyTimeout = 30;

                        try
                        {
                            bulkcopy.WriteToServer(listRows);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }

                        connection.Close();
                    }
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertDiagnoses(List<Admission> allAdmissions)
        {
            try
            {
                DataTable listRows = new DataTable();
                listRows.Columns.Add("PatientID", typeof(Guid));
                listRows.Columns.Add("AdmissionID", typeof(Int32));
                listRows.Columns.Add("AdmissionStartDate", typeof(DateTime));
                listRows.Columns.Add("AdmissionEndDate", typeof(DateTime));

                foreach (Admission a in allAdmissions)
                {
                    DataRow dr = listRows.NewRow();

                    dr["PatientID"] = a.PatientID;
                    dr["AdmissionID"] = a.AdmissionID;
                    dr["AdmissionStartDate"] = a.AdmissionStartDate;
                    dr["AdmissionEndDate"] = a.AdmissionEndDate;

                    listRows.Rows.Add(dr);
                }

                SqlConnection bulkSqlConnector =
                    new SqlConnection("integrated security=SSPI;persist security info=False;Trusted_Connection=Yes;server="
                        + DATA_SERVER + ";" + "database=" + DATABASE_DEPLOYMENT + ";connection timeout=0");

                using (SqlConnection connection = bulkSqlConnector)
                {
                    connection.Open();

                    using (SqlBulkCopy bulkcopy = new SqlBulkCopy(connection))
                    {
                        bulkcopy.DestinationTableName = "dbo." + "AdmissionsCorePopulatedTable";
                        bulkcopy.BulkCopyTimeout = 30;

                        try
                        {
                            bulkcopy.WriteToServer(listRows);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }

                        connection.Close();
                    }
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static double GetRandomNumber(double minimum, double maximum)
        {            
            return minimum + rnd.NextDouble() * (maximum - minimum);
        }
        
        public static DateTime GetRandomDate(DateTime from, DateTime to)
        {
            var range = to - from;
            var randTimeSpan = new TimeSpan((long)(rnd.NextDouble() * range.Ticks));
            return from + randTimeSpan;
        }

        static List<PatientDefinitionItem> GetPatientDefinitionItems(string itemName)
        {
            List<PatientDefinitionItem> pdis = new List<PatientDefinitionItem>();

            OpenConnectionMain(DATABASE_CONFIGURATION);            

            SqlDataReader reader = null;

            try
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("SELECT [RangeName], [RangeMin], [RangeMax], [RangeWeight] FROM [");
                sb.Append(DATABASE_CONFIGURATION);
                sb.Append("].[dbo].[");
                sb.Append("PatientOccuranceDefinitionsTable");
                sb.Append("] Where");
                sb.Append(" [RangeName] = '");
                sb.Append(itemName);
                sb.Append("'");

                SqlCommand cmdIns = new SqlCommand(sb.ToString(), myConnectionMain);
                cmdIns.CommandTimeout = 600;
                reader = cmdIns.ExecuteReader();

                int i = 0;
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        PatientDefinitionItem pdi = new PatientDefinitionItem();
                        pdi.RangeName = reader["RangeName"].ToString();
                        pdi.RangeMin = reader["RangeMin"].ToString();
                        pdi.RangeMax = reader["RangeMax"].ToString();
                        pdi.RangeWeight = reader["RangeWeight"].ToString();

                        pdis.Add(pdi);                        
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if ((reader != null) && !reader.IsClosed)
                {
                    reader.Close();
                }
            }

            CloseConnectionMain();

            return pdis;
        }
     
        #region Connection Functions

        static SqlConnection myConnectionMain;

        static void OpenConnectionMain(string databaseType)
        {
            myConnectionMain = new SqlConnection("integrated security=SSPI;persist security info=False;Trusted_Connection=Yes;server=" + DATA_SERVER + ";" + "database=" + databaseType + ";connection timeout=0");

            try
            {
                myConnectionMain.Open();
            }
            catch (Exception e)
            {
                Console.Write("Connection problems...");
            }
        }

        static void CloseConnectionMain()
        {
            myConnectionMain.Dispose();
        }

        #endregion Connection
    }
}
