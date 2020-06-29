using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QalImports
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }

    #region Constants
    //MAFH USER
    public string MAFHGuid = "2F636E2F-C03D-EA11-A835-005056A7F76B";

    //Connection strings
    public string cStringToQal = @"Data Source=qal.forcedb.dk;Initial Catalog=QAL;User ID=Qal_ReadOnly;Password=r6trdgdgasfaa4w5466ytrHRDHt";
    public string cStringToFI = "";

    //ARGO test lists
    public List<int> argoPositionList = new List<int>
    {
      203,
      240,
      264,
      320,
      323,
      343,
      344,
      592,
      593,
      634,
      635
    };
    public List<int> argoMeterList = new List<int>
    {
      1091,
      1092,
      1093,
      1094,
      1095,
      1096,
      1097,
      1098,
      1101,
      1102,
      1103,
      1149
    };

    //Amagerforbrænding test lists
    public List<int> amagerPositionList = new List<int>()
    {
      16,
      17,
      33,
      55,
      56,
      67,
      303
    };
    public List<int> amagerMeterList = new List<int>()
    {
      43,
      45,
      46,
      47,
      48,
      49,
      51,
      52,
      53,
      54,
      55,
      56,
      63,
      64,
      65,
      66,
      67,
      68,
      69,
      70,
      71,
      74,
      75,
      76,
      77,
      78,
      79,
      80,
      81,
      82,
      83,
      85,
      86,
      87,
      93,
      94,
      95,
      96,
      97,
      98,
      99,
      100,
      101
    };      //11 Meters not included cause of wrong Components

    //Position
    // Position that are connected to QAL1 or QAL2 or AST with bad repports. Dont add anything connected to those.
    public List<int> positionWithBadRepport = new List<int>
    {
      23,
      1,
      43
    };


    //Meter
    // 59 Meters can't be imported because they have more than 1 component of the same type attached. This should not be possible in the old system.
    // They should have created 1 Meter for each component.
    // SQL used to find affected Meters: SELECT * FROM Reading AS r INNER JOIN QAL3 AS q3 ON q3.QAL3_ID = r.R_QAL3_ID WHERE q3.QAL3_MID = @meterId ORDER BY R_QAL3_ID, R_ID
    public List<int> metersWithMultipleSameComponents = new List<int>
      {
        1,
        12,
        23,
        26,
        27,
        28,
        36,
        37,
        38,
        39,
        43,
        45,
        46,
        47,
        48,
        49,
        51,
        52,
        53,
        54,
        55,
        56,
        60,
        63,
        64,
        65,
        66,
        67,
        68,
        69,
        70,
        71,
        74,
        75,
        76,
        77,
        78,
        79,
        80,
        81,
        82,
        83,
        85,
        86,
        87,
        91,
        93,
        94,
        95,
        96,
        97,
        98,
        99,
        100,
        101,
        109,
        113,
        134,
        140,
        142,
        143,
        144,
        148,
        149,
        151,
        165,
        170,
        175,
        180,
        183,
        185,
        186,
        187,
        200,
        204,
        205,
        206,
        207,
        208,
        209,
        212,
        213,
        216,
        217,
        218,
        219,
        220,
        221,
        233,
        236,
        237,
        238,
        239,
        242,
        244,
        246,
        247,
        251,
        256,
        261,
        262,
        264,
        265,
        266,
        267,
        269,
        270,
        272,
        273,
        276,
        282,
        287,
        288,
        289,
        290,
        291,
        292,
        293,
        296,
        298,
        299,
        300,
        301,
        302,
        303,
        305,
        306,
        307,
        311,
        316,
        320,
        321,
        322,
        323,
        325,
        326,
        327,
        328,
        329,
        330,
        331,
        332,
        333,
        334,
        335,
        336,
        337,
        338,
        339,
        340,
        341,
        342,
        343,
        344,
        345,
        347,
        349,
        351,
        352,
        355,
        356,
        357,
        358,
        359,
        361,
        362,
        363,
        364,
        365,
        366,
        367,
        369,
        372,
        373,
        375,
        376,
        379,
        391,
        395,
        396,
        397,
        405,
        406,
        407,
        408,
        409,
        410,
        411,
        412,
        413,
        414,
        427,
        428,
        430,
        433,
        434,
        435,
        436,
        437,
        438,
        439,
        440,
        441,
        442,
        443,
        444,
        445,
        446,
        447,
        449,
        450,
        451,
        453,
        454,
        455,
        456,
        457,
        458,
        459,
        460,
        461,
        462,
        463,
        464,
        465,
        466,
        468,
        471,
        472,
        482,
        483,
        484,
        485,
        486,
        487,
        488,
        489,
        490,
        491,
        492,
        493,
        494,
        495,
        496,
        497,
        498,
        499,
        500,
        501,
        502,
        503,
        525,
        528,
        529,
        532,
        533,
        534,
        536,
        540,
        541,
        542,
        544,
        545,
        546,
        552,
        555,
        558,
        559,
        562,
        564,
        566,
        567,
        568,
        569,
        570,
        571,
        572,
        573,
        576,
        577,
        578,
        579,
        580,
        583,
        584,
        585,
        586,
        587,
        588,
        592,
        593,
        594,
        595,
        602,
        603,
        604,
        605,
        606,
        607,
        613,
        614,
        615,
        616,
        622,
        632,
        638,
        639,
        640,
        647,
        677,
        684,
        688,
        698,
        705,
        706,
        708,
        713,
        715,
        718,
        720,
        728,
        730,
        731,
        732,
        733,
        734,
        735,
        737,
        754,
        755,
        756,
        757,
        758,
        759,
        760,
        761,
        762,
        763,
        764,
        765,
        766,
        767,
        768,
        769,
        770,
        771,
        773,
        774,
        775,
        776,
        777,
        778,
        779,
        780,
        781,
        782,
        783,
        784,
        792,
        793,
        794,
        795,
        796,
        797,
        799,
        806,
        808,
        810,
        811,
        812,
        813,
        814,
        826,
        843,
        846,
        847,
        850,
        851,
        852,
        861,
        863,
        864,
        865,
        866,
        867,
        874,
        875,
        891,
        892,
        893,
        894,
        895,
        896,
        897,
        898,
        899,
        900,
        901,
        904,
        905,
        906,
        907,
        909,
        910,
        911,
        912,
        913,
        916,
        918,
        928,
        930,
        938,
        942,
        946,
        947,
        948,
        949,
        950,
        951,
        955,
        956,
        959,
        964,
        969,
        970,
        971,
        977,
        978,
        979,
        980,
        981,
        982,
        983,
        984,
        985,
        986,
        987,
        988,
        990,
        991,
        992,
        993,
        994,
        995,
        998,
        1000,
        1001,
        1002,
        1003,
        1004,
        1005,
        1006,
        1007,
        1010,
        1011,
        1012,
        1013,
        1014,
        1015,
        1017,
        1018,
        1019,
        1020,
        1021,
        1024,
        1025,
        1026,
        1039,
        1040,
        1041,
        1042,
        1043,
        1044,
        1045,
        1046,
        1047,
        1048,
        1049,
        1050,
        1051,
        1052,
        1053,
        1054,
        1057,
        1058,
        1059,
        1062,
        1071,
        1074,
        1075,
        1078,
        1080,
        1081,
        1082,
        1084,
        1085,
        1086,
        1087,
        1088,
        1089,
        1091,
        1092,
        1093,
        1094,
        1095,
        1096,
        1097,
        1098,
        1101,
        1102,
        1103,
        1104,
        1105,
        1115,
        1127,
        1129,
        1130,
        1131,
        1132,
        1133,
        1135,
        1142,
        1143,
        1146,
        1149,
        1152,
        1153,
        1154,
        1155,
        1156,
        1157,
        1158,
        1159,
        1160,
        1161,
        1162,
        1169,
        1173,
        1174,
        1175,
        1176,
        1177,
        1178,
        1179,
        1180,
        1181,
        1182,
        1183,
        1184,
        1185,
        1201,
        1202,
        1204,
        1205,
        1206,
        1207,
        1208,
        1209,
        1210,
        1212,
        1213,
        1214,
        1215,
        1216,
        1217,
        1218,
        1219,
        1220,
        1221,
        1222,
        1223
      };

    //Components
    Dictionary<int, int> cDict = new Dictionary<int, int>();        //Key = C_ID from qal, value = ObjectID from FI
    public string componentInspectionKey = "QALComponent";
    public string componentConfigTableKey = "9D94A96B-AE19-41E1-9F19-4756E63643AE";
    public string cNameConfigFieldKey = "5C92DB46-4BD5-419C-8396-B54F2DA9D36F";
    public string cEmissionTypeConfigFieldKey = "8791569A-D4CA-45D1-8740-F732F8C2507C";
    public string cMeasureUnitTypeConfigFieldKey = "8D2DF3D9-F1DB-4756-A2E4-23A14435B054";
    public string cSpanConfigFieldKey = "0C91B85A-50AB-4B58-A04D-A4546E49BEED";
    public string cZeroConfigFieldKey = "FEF0EF60-E343-4C78-B7CD-3FBCDEC73B26";

    //QAL1
    public string qal1InspectionKey = "QAL1Inspection";
    public string qal1ConfigTableKey = "A6651A00-D1A9-4E3D-AFAE-6DC7CC95073B";
    public string qal1MaxRangeConfigFieldKey = "DFCAD6B5-6A74-4912-8A3A-41C6B0A0D60D";
    public string qal1MeasuringValueConfigFieldKey = "40F0BCCF-960A-4C97-B986-7073088E2CFC";
    public string qal1AssociatedLimitValueConfigFieldKey = "40F0BCCF-960A-4C97-B986-7073088E2CFC";

    //QAL2
    public string qal2InspectionKey = "QAL2Inspection";
    public string qal2ConfigTableKey = "F58C7B67-9ACE-4E48-94F6-955B0E4C8E17";
    public string qal2FormulaAConfigFieldKey = "C4734B17-69A1-4806-A6B2-CE0B475D320E";
    public string qal2FormulaBConfigFieldKey = "AEEC4528-8A5E-4F65-8B5E-0474DC29F5EE";
    public string qal2IntervalFromConfigFieldKey = "30372C2F-3D46-4704-B0D6-6981A03E1189";
    public string qal2IntervalToConfigFieldKey = "21773FF5-C9B3-4BE4-8A82-FE65BE4B0C6E";
    public string qal2ValidPeriodTypeConfigFieldKey = "41D0BDE0-892A-42CE-A575-24B5E3E412E0";
    public string qal2ValidUntilDTConfigFieldKey = "71B8A0B0-16E1-453C-A531-5A1B96B0CC0E";
    public string qal2ValidPeriodTypeConfigLookupTabelKey = "E9A32909-CEE2-4496-8A86-7B1E1A0549BD";

    //QAL3

    //AST

    //Specific Data
    public int actorID = -1;
    public int jobID = -1;
    public int pID = -1;
    public Guid cmiGuid = Guid.NewGuid();
    static readonly Guid initiatorKey = Guid.Parse("ABCCBBFD-ABC2-ABC1-ABC9-ABC517A57B33");
    public string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
    #endregion Constants

    #region CreateObjects
    //"Create Objects" - button - Takes around 2 minute and 30 seconds
    //USING THE GETSPECIFICDATA() METHOD INSTEAD
    private void Button1_Click(object sender, EventArgs e)
    {

      SqlConnection cnnQal1;
      SqlConnection cnnQal2;
      SqlConnection cnnQal3;
      SqlConnection cnnFI1;
      SqlConnection cnnFI2;
      SqlConnection cnnFI3;

      cnnQal1 = new SqlConnection(cStringToQal);
      cnnQal2 = new SqlConnection(cStringToQal);
      cnnQal3 = new SqlConnection(cStringToQal);
      cnnFI1 = new SqlConnection(cStringToFI);
      cnnFI2 = new SqlConnection(cStringToFI);
      cnnFI3 = new SqlConnection(cStringToFI);

      cnnQal1.Open();
      cnnQal2.Open();
      cnnQal3.Open();
      cnnFI1.Open();
      cnnFI2.Open();
      cnnFI3.Open();

      //Key = P_ParentID from QAL, Value = ParentObjectID from FI
      //Dictionary<int, int> pDict = CreatePosition(cnnQal1, cnnFI1, amagerPositionList);       //Create positions in FI database - REMEMBER TO ADD P_ID 582
      //Key = M_ID from qal, Value = ObjectID from FI
      //Dictionary<int, int> mDict = CreateMeter(cnnQal2, cnnFI2, pDict,nowOffset, amagerMeterList, amagerPositionList);   //Create Meters in FI database with belonging positions - REMEMBER TO ADD M_ID 542 AND 583
      //Create Components in FI database with belonging Meters
      //CreateComponent(cnnQal3, cnnFI3, cDict, nowOffset, mDict, componentInspectionKey, componentConfigTableKey, MAFHGuid, cZeroConfigFieldKey, 
        //cSpanConfigFieldKey, cMeasureUnitTypeConfigFieldKey, cEmissionTypeConfigFieldKey, cNameConfigFieldKey, amagerMeterList);   
      
      System.Diagnostics.Debug.WriteLine("--- CREATE OBJECTS DONE ---");
    }

    //public static Dictionary<int, int> CreatePosition(SqlConnection cnnQal, SqlConnection cnnFI, List<int> amagerPositionList, DateTimeOffset nowOffset)
    //{

    //  Dictionary<int, int> pDict = new Dictionary<int, int>();
    //  List<Position> pList = new List<Position>();

    //  //SELECT Query for QAL - database
    //  //CHANGED 'IN' for ARGO
    //  string sqlQal = "SELECT [P_ID] ,[P_Pos] ,[P_ParentID] ,[P_WPID] ,[P_Description] ,[P_ListIndex] ,[P_Remarks] ,[P_WPSubID] " +
    //    "FROM [Position] WHERE P_ParentID != 581 AND P_ID in (" + string.Join(",", amagerPositionList.Select(r => r.ToString())) + ") " +
    //    "ORDER BY P_ID";

    //  //Get rows from QAL table
    //  using (cnnQal)
    //  {
    //    using (SqlCommand QalCommand = new SqlCommand(sqlQal, cnnQal))
    //    {
    //      using (SqlDataReader dr = QalCommand.ExecuteReader())
    //      {
    //        while (dr.Read())
    //        {
    //          var op = new Position();

    //          op.ID = Convert.ToInt32(dr["P_ID"]);
    //          op.Pos = Convert.ToString(dr["P_Pos"]);
    //          op.ParentID = Convert.ToInt32(dr["P_ParentID"]);
    //          op.WPID = Convert.ToInt32(dr["P_WPID"]);
    //          op.Description = Convert.ToString(dr["P_Description"]);
    //          op.ListIndex = Convert.ToInt32(dr["P_ListIndex"]);
    //          op.Remarks = Convert.ToString(dr["P_Remarks"]);
    //          op.WPSubId = Convert.ToString(dr["P_WPSubID"]);
    //          //string temp = Convert.ToString(dr["WorkItemKey"]);

    //          pList.Add(op);
    //          //System.Diagnostics.Debug.WriteLine(dr);
    //        }
    //      }
    //    }
    //  }

    //  int userID = 770;
    //  int objectID;

    //  //INSERT rows into ForceInspectTest database
    //  foreach (Position p in pList)
    //  {
    //    string newID;

    //    using (SqlCommand FICommand = new SqlCommand("INSERT INTO [T_Object] (ExternalNo, ParentObjectID, ExternalDescription, " +
    //      "InternalDescription, CreatedDateTime, LastModifiedDateTime, CreatedByUserID, LastModifiedByUserID, ObjectStatusID) " +
    //            "VALUES (@Pos, @ParentID, @newDescription, @ID, @cdt, @lmdt, @cbu, @lmbu, @ObjectStatusID)", cnnFI))
    //    {

    //      string newDescription = p.Description + ". " + p.Remarks;
    //      newID = "P_ID:" + p.ID;

    //      FICommand.Parameters.AddWithValue("@Pos", p.Pos);

    //      if (p.ParentID == 0)
    //      {
    //        FICommand.Parameters.AddWithValue("@ParentID", DBNull.Value);
    //      }
    //      else
    //      {
    //        FICommand.Parameters.AddWithValue("@ParentID", pDict[p.ParentID]);
    //      }

    //      FICommand.Parameters.AddWithValue("@newDescription", newDescription);
    //      FICommand.Parameters.AddWithValue("@ID", newID);
    //      FICommand.Parameters.AddWithValue("@cdt", nowOffset);
    //      FICommand.Parameters.AddWithValue("@lmdt", nowOffset);
    //      FICommand.Parameters.AddWithValue("@cbu", userID);
    //      FICommand.Parameters.AddWithValue("@lmbu", userID);
    //      FICommand.Parameters.AddWithValue("@ObjectStatusID", 2);

    //      FICommand.ExecuteNonQuery();

    //    }

    //    //Get the new ObjectID from the newly inserted Position object
    //    using (SqlCommand s = new SqlCommand("SELECT ObjectID FROM [T_Object] WHERE InternalDescription = @newID", cnnFI))
    //    {

    //      s.Parameters.AddWithValue("@newID", newID);

    //      objectID = (int)s.ExecuteScalar();
    //      pDict.Add(p.ID, objectID);
    //    }

    //    //Insert into the T_Object_ObjectTypes
    //    InsertIntoObjectTypes(cnnFI, objectID, 19);
    //  }

    //  System.Diagnostics.Debug.WriteLine("--- CreatePosition() completed ---");
    //  return pDict;
    //}

    //public static Dictionary<int, int> CreateMeter(SqlConnection cnnQal, SqlConnection cnnFI, Dictionary<int, int> pDict, DateTimeOffset nowOffset, 
    //  List<int> amagerMeterList, List<int> amagerPositionList)
    //{
    //  Dictionary<int, int> mDict = new Dictionary<int, int>();
    //  List<Meter> mList = new List<Meter>();


    //  //CHANGED the lists for argo
    //  string sqlQal = "SELECT [M_ID] ,[M_Name] ,[M_PID] ,[M_Description] ,[M_Listindex] ,[M_Remarks] " +
    //  "FROM [Meter] WHERE M_PID != 542 and M_PID != 583 AND M_ID IN (" + string.Join(",", amagerMeterList.Select(r => r.ToString()) )  +") " +
    //  "AND M_PID IN (" + string.Join(",", amagerPositionList.Select(r => r.ToString())) + ")";
    //  string meterInspectionKey = "QALMeterConfig";
    //  string meterConfigTableKey = "80C4DF1C-5F71-49A0-8747-D1D1492CBF21";

    //  //Get Meter rows from Meter in QAL database
    //  using (SqlCommand QalCommand = new SqlCommand(sqlQal, cnnQal))
    //  {
    //    using (SqlDataReader dr = QalCommand.ExecuteReader())
    //    {
    //      while (dr.Read())
    //      {
    //        var m = new Meter();

    //        m.ID = Convert.ToInt32(dr["M_ID"]);
    //        m.Name = Convert.ToString(dr["M_Name"]);
    //        m.PID = Convert.ToInt32(dr["M_PID"]);
    //        m.Description = Convert.ToString(dr["M_Description"]);
    //        m.Listindex = Convert.ToInt32(dr["M_ListIndex"]);
    //        m.Remarks = Convert.ToString(dr["M_Remarks"]);

    //        mList.Add(m);
    //        //System.Diagnostics.Debug.WriteLine(dr);
    //      }
    //    }
    //  }

    //  int userID = 770;
    //  int objectID;

    //  string newID;

    //  //Insert Meters into the T_Object table 
    //  //Description might be in ExternalNo
    //  foreach (Meter m in mList)
    //  {
    //    using (SqlCommand FICommand = new SqlCommand("INSERT INTO [T_Object] (ExternalNo, ParentObjectID, ManufacturerNo, " +
    //      "ExternalDescription, InternalDescription, CreatedDateTime, LastModifiedDateTime, CreatedByUserID, LastModifiedByUserID, ObjectStatusID) " +
    //      "VALUES (@Name, @ParentID, @mf, @ed, @ID, @cdt, @lmdt, @cbu, @lmbu, @ObjectStatusID)", cnnFI))
    //    {
    //      newID = "M_ID:" + m.ID;

    //      FICommand.Parameters.AddWithValue("@Name", m.Name);
    //      FICommand.Parameters.AddWithValue("@ParentID", pDict[m.PID]);
    //      FICommand.Parameters.AddWithValue("@mf", m.Description);
    //      FICommand.Parameters.AddWithValue("@ed", m.Remarks);
    //      FICommand.Parameters.AddWithValue("@ID", newID);
    //      FICommand.Parameters.AddWithValue("@cdt", nowOffset);
    //      FICommand.Parameters.AddWithValue("@lmdt", nowOffset);
    //      FICommand.Parameters.AddWithValue("@cbu", userID);
    //      FICommand.Parameters.AddWithValue("@lmbu", userID);
    //      FICommand.Parameters.AddWithValue("@ObjectStatusID", 2);

    //      FICommand.ExecuteNonQuery();
    //    }

    //    //Get the ObjectID from the newly inserted Meter object
    //    using (SqlCommand s = new SqlCommand("SELECT ObjectID FROM [T_Object] WHERE InternalDescription = @newID", cnnFI))
    //    {

    //      s.Parameters.AddWithValue("@newID", newID);

    //      objectID = (int)s.ExecuteScalar();
    //      mDict.Add(m.ID, objectID);
    //    }

    //    //Insert into T_Object_ObjectTypes
    //    InsertIntoObjectTypes(cnnFI, objectID, 20);

    //    //Get VersionID from T_GenericConfigTable
    //    int versionID = GetVersionId(cnnFI, meterInspectionKey);

    //    //Insert into T_GenericValueRow and T_GenericValueField
    //    InsertMeterObjectIntoGenericTables(cnnQal, cnnFI, m.ID, objectID, meterConfigTableKey, versionID, nowOffset);
        
    //  }
    //  System.Diagnostics.Debug.WriteLine("--- CreateMeter() completed ---");
    //  return mDict;
    //}

    //public static void CreateComponent(SqlConnection cnnQal, SqlConnection cnnFI, Dictionary<int, int> cDict, DateTimeOffset nowOffset, Dictionary<int, int> mDIct, 
    //  string componentInspectionKey, string componentConfigTableKey, string MAFHGuid, string cZeroConfigFieldKey, string cSpanConfigFieldKey, string cMeasureUnitTypeConfigFieldKey,
    //  string cEmissionTypeConfigFieldKey, string cNameConfigFieldKey, List<int> amagerMeterList)
    //{
    //  List<Component> cList = new List<Component>();

    //  //Get components from Component in QAL database
    //  using (SqlCommand sc = new SqlCommand("SELECT * FROM Component WHERE C_Name != 'Flow'", cnnQal))
    //  {
    //    using (SqlDataReader dr = sc.ExecuteReader())
    //    {
    //      while (dr.Read())
    //      {
    //        Component c = new Component();

    //        c.C_ID = Convert.ToInt32(dr["C_ID"]);
    //        c.C_Name = Convert.ToString(dr["C_Name"]);

    //        cList.Add(c);
    //      }
    //    }
    //  }

    //  int userID = 770;
    //  int objectID;

    //  foreach (Component c in cList)
    //  {
    //    string newID = "C_ID:" + c.C_ID;
    //    List<int> mIDList = new List<int>();

    //    //Key = Old QAL1_ID, Value = QAL1 object
    //    Dictionary<int, Qal1> oldIDQal = new Dictionary<int, Qal1>();
    //    //Key = New QAL1_ID, Value = QAL1 object
    //    Dictionary<int, Qal1> newIDQal = new Dictionary<int, Qal1>();

    //    //Get the needed QAL1 properties that a component needs
    //    //Changed list for ARGO
    //    using (SqlCommand sc = new SqlCommand("SELECT * FROM QAL1 WHERE QAL1_MID IN (" + string.Join(",", amagerMeterList.Select(r => r.ToString())) + ") " +
    //      "AND QAL1_CID = @C_Name AND QAL1_Unit != ''", cnnQal))
    //    {
    //      sc.Parameters.AddWithValue("@C_Name", c.C_Name);

    //      using (SqlDataReader dr = sc.ExecuteReader())
    //      {
    //        while (dr.Read())
    //        {
    //          Qal1 qal1 = new Qal1();

    //          int ID = Convert.ToInt32(dr["QAL1_ID"]);
    //          int mID = Convert.ToInt32(dr["QAL1_MID"]);
    //          qal1.Q1_SSpan = Convert.ToDecimal(dr["QAL1_SSpan"]);
    //          qal1.Q1_SZero = Convert.ToDecimal(dr["QAL1_SZero"]);
    //          qal1.Q1_Unit = Convert.ToString(dr["QAL1_Unit"]);

    //          oldIDQal.Add(ID, qal1);
    //          mIDList.Add(mID);
    //        }
    //      }
    //    }

    //    //Insert all the component in T_Object where QAL1_CID = 'C_NAME'
    //    foreach (int mid in mIDList)
    //    {

    //      //Insert into T_Object 
    //      using (SqlCommand sc = new SqlCommand("INSERT INTO [T_Object] (ParentObjectID, ExternalNo, " +
    //        "InternalDescription, CreatedDateTime, LastModifiedDateTime, CreatedByUserID, LastModifiedByUserID, ObjectStatusID) " +
    //              "VALUES (@ParentID, @Name, @ID, @cdt, @lmdt, @cbu, @lmbu, @ObjectStatusID)", cnnFI))
    //      {

    //        sc.Parameters.AddWithValue("@ParentID", mDIct[mid]);
    //        sc.Parameters.AddWithValue("@Name", c.C_Name);
    //        sc.Parameters.AddWithValue("@ID", newID);
    //        sc.Parameters.AddWithValue("@cdt", nowOffset);
    //        sc.Parameters.AddWithValue("@lmdt", nowOffset);
    //        sc.Parameters.AddWithValue("@cbu", userID);
    //        sc.Parameters.AddWithValue("@lmbu", userID);
    //        sc.Parameters.AddWithValue("@ObjectStatusID", 2);

    //        sc.ExecuteNonQuery();
    //      }
    //    }

    //    List<int> newObjectIDList = new List<int>();

    //    //Get the objectID from T_Object, from the newly inserted components
    //    //CreatedDateTime so it only gets the newest inserted Components... - REMEMBER TO REMOVE
    //    using (SqlCommand sq = new SqlCommand("SELECT * FROM [T_Object] WHERE CreatedDateTime > '2020-03-26 16:02:55.017' AND InternalDescription = @newID", cnnFI))
    //    {
    //      sq.Parameters.AddWithValue("@newID", newID);

    //      using (SqlDataReader d = sq.ExecuteReader())
    //      {
    //        while (d.Read())
    //        {
    //          objectID = Convert.ToInt32(d["ObjectID"]);

    //          newObjectIDList.Add(objectID);
    //        }
    //      }
    //    }

    //    int i = 0;

    //    //Switch the Old C_ID in the dictionary, with the new T_Object ID's, so we can use the references later
    //    foreach (var item in oldIDQal)
    //    {
    //      newIDQal.Add(newObjectIDList[i], item.Value);

    //      i++;
    //    }

    //    foreach (int newObjectID in newObjectIDList)
    //    {
    //      Guid guid = Guid.NewGuid();

    //      //Insert into T_Object_ObjectTypes 
    //      InsertIntoObjectTypes(cnnFI, newObjectID, 21);

    //      //Get VersionID from T_GenericConfigTable
    //      int versionID = GetVersionId(cnnFI, componentInspectionKey);

    //      //Insert into T_GenericValueRow and T_GenericValueField
    //      InsertComponentObjectIntoGenericTables(cnnFI, guid, componentConfigTableKey, newObjectID, versionID, nowOffset, c.C_Name, newIDQal[newObjectID], MAFHGuid, 
    //        cNameConfigFieldKey, cZeroConfigFieldKey, cSpanConfigFieldKey, cEmissionTypeConfigFieldKey, cMeasureUnitTypeConfigFieldKey);
    //    }
    //  }
    //  System.Diagnostics.Debug.WriteLine("--- CreateComponent() completed ---");
    //}

    public static void InsertIntoObjectTypes(int objectID, int oti, SqlTransaction transaction, SqlConnection cnnFI)
    {
      using (SqlCommand k = CreateCommandAndEnlistTransaction("INSERT INTO [T_Object_ObjectTypes] (ObjectID, ObjectTypeID, ObjectSubTypeGuid) " +
            "VALUES (@ObjectID, @oti, @ostg)", cnnFI, transaction))
      {
        //EnlistTransaction(k, transaction);
        k.Parameters.AddWithValue("@ObjectID", objectID);
        k.Parameters.AddWithValue("@oti", oti);             //19, 20 or 21
        k.Parameters.AddWithValue("@ostg", DBNull.Value);

        k.ExecuteNonQuery();

      }
    }

    public static void InsertMeterObjectIntoGenericTables(SqlConnection cnnQal, SqlConnection cnnFI, int m_ID, int objectID, string configTableKey, int versionID, 
      DateTimeOffset nowOffset, SqlTransaction transaction, Dictionary<int, Guid> instrumentDict)
    {
      int instrumentID = 0;
      Guid instrumentGuid = Guid.Empty;
      bool AIA = false;     //AutomaticInternalAdjustment
      Guid genericValueRowKey = Guid.NewGuid();
      Guid genericValueFieldKey = Guid.NewGuid();
      Guid genericValueFieldKey2 = Guid.NewGuid();

      using (SqlCommand sc = new SqlCommand("SELECT * FROM QAL1 WHERE QAL1_MID = @QAL1_MID", cnnQal))
      {
        sc.Parameters.AddWithValue("@QAL1_MID", m_ID);

        using (SqlDataReader dr = sc.ExecuteReader())
        {
          while (dr.Read())
          {
            instrumentID = Convert.ToInt32(dr["QAL1_IID"]);
            string description = Convert.ToString(dr["QAL1_IDescription"]);
            //string description = "abc";

            if (instrumentID != 0)
            {
              try
              {
                instrumentGuid = instrumentDict[instrumentID];
              }
              catch (Exception e)
              {
                MessageBox.Show("Not possible to import Meter with MID: " + m_ID + ". Unknown Instrumenttype: " + description);
              }
            }
            else
            {
              try
              {
                instrumentGuid = GetSpecificInstrumentType(transaction, description, cnnFI);
                if (instrumentGuid == Guid.Empty)
                {
                  throw new Exception();
                }
              }
              catch (Exception e)
              {
                MessageBox.Show("Not possible to import Meter with MID: " + m_ID + ". Unknown Instrumenttype: " + description);
              }
            }
            AIA = Convert.ToBoolean(dr["QAL1_AIA"]);
            
          }
        }
      }

      //Insert into T_GenericValueRow
      using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO [T_GenericValueRow] (GenericValueRowKey, GenericConfigTableKey, ItemId, VersionId, LastModifiedDate, LastModifiedBy) " +
        "VALUES (@gvrk, @gctk, @ItemId, @VersionId, @lmd, @lmb)", cnnFI, transaction))
      {
       
        sc.Parameters.AddWithValue("@gvrk", genericValueRowKey);
        sc.Parameters.AddWithValue("@gctk", configTableKey);
        sc.Parameters.AddWithValue("@ItemId", objectID);
        sc.Parameters.AddWithValue("@VersionId", versionID);
        sc.Parameters.AddWithValue("@lmd", nowOffset);
        sc.Parameters.AddWithValue("@lmb", "2F636E2F-C03D-EA11-A835-005056A7F76B");

        sc.ExecuteNonQuery();
      }

      //Insert the 2 value-fields in T_GenericValueField
      using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO [T_GenericValueField] (GenericValueFieldKey, GenericValueRowKey, GenericConfigFieldKey, " +
        "GenericConfigLookupFieldKey, ReadOnly, LastModifiedDate, LastModifiedBy) " +
        "VALUES (@gvfk, @gvrk, @gcfk, @GenericConfigLookupFieldKey, @ReadOnly, @lmd, @lmb)", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@gvfk", genericValueFieldKey);
        sc.Parameters.AddWithValue("@gvrk", genericValueRowKey);
        sc.Parameters.AddWithValue("@gcfk", "FBBE24C0-EB55-4379-905C-58C2DDCBFA8B");
        sc.Parameters.AddWithValue("@GenericConfigLookupFieldKey", instrumentGuid);
        sc.Parameters.AddWithValue("@ReadOnly", true);
        sc.Parameters.AddWithValue("@lmd", nowOffset);
        sc.Parameters.AddWithValue("@lmb", "2F636E2F-C03D-EA11-A835-005056A7F76B");

        sc.ExecuteNonQuery();
      }

      using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO [T_GenericValueField] (GenericValueFieldKey, GenericValueRowKey, GenericConfigFieldKey, " +
        "ValueBit, OriginalValue, ReadOnly, LastModifiedDate, LastModifiedBy) " +
        "VALUES (@gvfk, @gvrk, @gcfk, @ValueBit, @OriginalValue, @ReadOnly, @lmd, @lmb)", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@gvfk", genericValueFieldKey2);
        sc.Parameters.AddWithValue("@gvrk", genericValueRowKey);
        sc.Parameters.AddWithValue("@gcfk", "BF5FB983-A608-4C0C-AB2A-BF11577CED35");
        sc.Parameters.AddWithValue("@ValueBit", AIA);
        sc.Parameters.AddWithValue("@OriginalValue", AIA);
        sc.Parameters.AddWithValue("@ReadOnly", true);
        sc.Parameters.AddWithValue("@lmd", nowOffset);
        sc.Parameters.AddWithValue("@lmb", "2F636E2F-C03D-EA11-A835-005056A7F76B");

        sc.ExecuteNonQuery();
      }
      
    }

    public static void InsertComponentObjectIntoGenericTables(SqlConnection cnnFI, Guid guid, string componentConfigTableKey, int ItemId, 
      int versionID, DateTimeOffset nowOffset, string name, Qal1 qal1, string MAFHGuid, string cNameConfigFieldKey, string cZeroConfigFieldKey, string cSpanConfigFieldKey,
      string cEmissionTypeConfigFieldKey, string cMeasureUnitTypeConfigFieldKey, SqlTransaction transaction)
    {
      Guid g = Guid.NewGuid();
       
      using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO [T_GenericValueRow] (GenericValueRowKey, GenericConfigTableKey, ItemId, VersionId, LastModifiedDate, LastModifiedBy) " +
          "VALUES (@gvrk, @gctk, @ItemId, @VersionId, @LastModifiedDate, @LastModifiedBy)", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@gvrk", guid);
        sc.Parameters.AddWithValue("@gctk", componentConfigTableKey);
        sc.Parameters.AddWithValue("@ItemId", ItemId);
        sc.Parameters.AddWithValue("@VersionId", versionID);
        sc.Parameters.AddWithValue("@LastModifiedDate", nowOffset);
        sc.Parameters.AddWithValue("@LastModifiedBy", MAFHGuid);

        sc.ExecuteNonQuery();
      }

      //Insert the 5 properties of the components into the T_GenericValueField table
      InsertComponentWithValueName(guid, cNameConfigFieldKey, name, nowOffset, MAFHGuid, transaction, cnnFI);
      InsertComponentWithValueDecimal(guid, cSpanConfigFieldKey, qal1.Q1_SSpan, nowOffset, MAFHGuid, transaction, cnnFI);
      InsertComponentWithValueDecimal(guid, cZeroConfigFieldKey, qal1.Q1_SZero, nowOffset, MAFHGuid, transaction, cnnFI);
      InsertComponentWithoutValueEmission(guid, cEmissionTypeConfigFieldKey, name, nowOffset, MAFHGuid, transaction, cnnFI);
      InsertComponentWithoutValueMeasure(guid, cMeasureUnitTypeConfigFieldKey, qal1.Q1_Unit, nowOffset, MAFHGuid, transaction, cnnFI);

      using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO [T_GenericValueField] (GenericValueFieldKey, GenericValueRowKey, GenericConfigFieldKey, " +
          "ReadOnly, LastModifiedDate, LastModifiedBy) " +
          "VALUES (@gvfk, @gvrk, @gcfk, @ReadOnly, @lmd, @lmb)", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@gvfk", g);
        sc.Parameters.AddWithValue("@gvrk", guid);
        sc.Parameters.AddWithValue("@gcfk", "D7E2BE1E-B7FF-49C0-B990-774AC7555A74");
        sc.Parameters.AddWithValue("@ReadOnly", true);
        sc.Parameters.AddWithValue("@lmd", nowOffset);
        sc.Parameters.AddWithValue("@lmb", MAFHGuid);

        sc.ExecuteNonQuery();
      }
    }

    public static void InsertComponentWithValueName(Guid guid, string gcfk, string name, DateTimeOffset nowOffset, string MAFHGuid, SqlTransaction transaction
      , SqlConnection cnnFI)
    {
      Guid genericValueFieldKey = Guid.NewGuid();

      using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO [T_GenericValueField] (GenericValueFieldKey, GenericValueRowKey, GenericConfigFieldKey, " +
          "ValueNvarchar, OriginalValue, ReadOnly, LastModifiedDate, LastModifiedBy) " +
          "VALUES (@gvfk, @gvrk, @gcfk, @Value, @ov, @ReadOnly, @lmd, @lmb)", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@gvfk", genericValueFieldKey);
        sc.Parameters.AddWithValue("@gvrk", guid);
        sc.Parameters.AddWithValue("@gcfk", gcfk);
        sc.Parameters.AddWithValue("@Value", name);
        sc.Parameters.AddWithValue("@ov", name);
        sc.Parameters.AddWithValue("@ReadOnly", true);
        sc.Parameters.AddWithValue("@lmd", nowOffset);
        sc.Parameters.AddWithValue("@lmb", MAFHGuid);

        sc.ExecuteNonQuery();
      }
    }

    public static void InsertComponentWithValueDecimal(Guid guid, string gcfk, decimal value, DateTimeOffset nowOffset, string MAFHGuid, 
      SqlTransaction transaction, SqlConnection cnnFI)
    {
      Guid genericValueFieldKey = Guid.NewGuid();

      using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO [T_GenericValueField] (GenericValueFieldKey, GenericValueRowKey, GenericConfigFieldKey, " +
          "ValueDecimal, OriginalValue, ReadOnly, LastModifiedDate, LastModifiedBy) " +
          "VALUES (@gvfk, @gvrk, @gcfk, @Value, @ov, @ReadOnly, @lmd, @lmb)", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@gvfk", genericValueFieldKey);
        sc.Parameters.AddWithValue("@gvrk", guid);
        sc.Parameters.AddWithValue("@gcfk", gcfk);
        sc.Parameters.AddWithValue("@Value", value);
        sc.Parameters.AddWithValue("@ov", value);
        sc.Parameters.AddWithValue("@ReadOnly", true);
        sc.Parameters.AddWithValue("@lmd", nowOffset);
        sc.Parameters.AddWithValue("@lmb", MAFHGuid);

        sc.ExecuteNonQuery();
      }
    }

    public static void InsertComponentWithoutValueMeasure(Guid guid, string gcfk, string unit, DateTimeOffset nowOffset, string MAFHGuid, 
      SqlTransaction transaction, SqlConnection cnnFI)
    {
      Guid genericValueFieldKey = Guid.NewGuid();

      using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO [T_GenericValueField] (GenericValueFieldKey, GenericValueRowKey, GenericConfigFieldKey, " +
          "GenericConfigLookupFieldKey, ReadOnly, LastModifiedDate, LastModifiedBy) " +
          "VALUES (@gvfk, @gvrk, @gcfk, @gclfk, @ReadOnly, @lmd, @lmb)", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@gvfk", genericValueFieldKey);
        sc.Parameters.AddWithValue("@gvrk", guid);
        sc.Parameters.AddWithValue("@gcfk", gcfk);
        sc.Parameters.AddWithValue("@gclfk", GetUnitGenericConfigLookupFieldKeyAsObject(unit));
        sc.Parameters.AddWithValue("@ReadOnly", true);
        sc.Parameters.AddWithValue("@lmd", nowOffset);
        sc.Parameters.AddWithValue("@lmb", MAFHGuid);

        sc.ExecuteNonQuery();
      }
    }

    public static void InsertComponentWithoutValueEmission(Guid guid, string gcfk, string name, DateTimeOffset nowOffset, string MAFHGuid, 
      SqlTransaction transaction, SqlConnection cnnFI)
    {
      Guid lookupGuid = new Guid();

      using (SqlCommand sc = CreateCommandAndEnlistTransaction("SELECT GenericConfigLookupFieldKey FROM T_GenericConfigLookupField WHERE [Name] = @name", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@name", name);

        using (SqlDataReader dr = sc.ExecuteReader())
        {
          while (dr.Read())
          {
            lookupGuid = (Guid)dr["GenericConfigLookupFieldKey"];
          }
        }
      }

        Guid genericValueFieldKey = Guid.NewGuid();

      using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO [T_GenericValueField] (GenericValueFieldKey, GenericValueRowKey, GenericConfigFieldKey, " +
          "GenericConfigLookupFieldKey, ReadOnly, LastModifiedDate, LastModifiedBy) " +
          "VALUES (@gvfk, @gvrk, @gcfk, @gclfk, @ReadOnly, @lmd, @lmb)", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@gvfk", genericValueFieldKey);
        sc.Parameters.AddWithValue("@gvrk", guid);
        sc.Parameters.AddWithValue("@gcfk", gcfk);
        sc.Parameters.AddWithValue("@gclfk", lookupGuid);
        sc.Parameters.AddWithValue("@ReadOnly", true);
        sc.Parameters.AddWithValue("@lmd", nowOffset);
        sc.Parameters.AddWithValue("@lmb", MAFHGuid);

        sc.ExecuteNonQuery();
      }
    }

    public static object GetEmissionGenericConfigLookupFieldKeyAsObject(string name)
    {
      Guid? g = GetUnitGenericConfigLookupFieldKey(name);
      return g.HasValue ? (object)g.Value : DBNull.Value;
    }

    public static Guid? GetEmissionGenericConfigLookupFieldKey(string name)
    {
      Guid? guid;

      if (name.Equals("SO2"))
      {
        guid = Guid.Parse("C420C321-F7C7-479F-AE69-3941EAECB373");
      }
      else if (name.Equals("CO"))
      {
        guid = Guid.Parse("1C74E3D3-AB4F-4D3C-B9E4-DAB4FFDFDE2E");
      }
      else if (name.Equals("CO2"))
      {
        guid = Guid.Parse("98D0EED6-A346-4CB6-AB39-C7469BB5E202");
      }
      else if (name.Equals("NO"))
      {
        guid = Guid.Parse("859A6121-0E0B-44E6-ABC1-1B84FEAAA0D3");
      }
      else if (name.Equals("NO2"))
      {
        guid = Guid.Parse("9B510E1F-4C0B-411A-9C93-7FC5E647CEAF");
      }
      else if (name.Equals("NOx"))
      {
        guid = Guid.Parse("1C9A2F5C-0A8E-44F7-9572-EEE9257B0D31");
      }
      else if (name.Equals("HCL"))
      {
        guid = Guid.Parse("E09832AD-FCD4-469A-A426-8A9C06F61B7E");
      }
      else if (name.Equals("O2"))
      {
        guid = Guid.Parse("3AF1FD0E-542F-463F-9F88-17502B991220");
      }
      else if (name.Equals("Støv"))
      {
        guid = Guid.Parse("6A01D7AD-4514-497E-A8D8-5D2BE08561D2");
      }
      else if (name.Equals("TOC"))
      {
        guid = Guid.Parse("433E00F8-8D00-4EC7-BDF4-004388FED72B");
      }
      else if (name.Equals("H2O"))
      {
        guid = Guid.Parse("98D51E9F-A313-42F3-9920-6591E9B6BF12");
      }
      else if (name.Equals("NH3"))
      {
        guid = Guid.Parse("4DDDB216-0667-4AB4-BF54-CA3C672862D2");
      }
      else if (name.Equals("N2O"))
      {
        guid = Guid.Parse("9B510E1F-4C0B-411A-9C93-7FC5E647CEAF");
      }
      else
      {
        guid = null;
      }

      return guid;
    }
    #endregion CreateObjects

    #region DeleteObjects
    //"Delete Objects" - button 
    private void Button2_Click(object sender, EventArgs e)
    {

      SqlConnection cnnFI;
           
      cnnFI = new SqlConnection(cStringToFI);

      cnnFI.Open();

      //DeleteObjects(cnnFI);    //Delete all Positions, Meters and Components in FI database
      System.Diagnostics.Debug.WriteLine("--- DELETE OBJECTS DONE ---");
    }

    public static void DeleteObjects(SqlConnection cnnFI)
    {
      string firstDelete = "DELETE FROM [T_Object_ObjectTypes] WHERE ObjectTypeID in (19,20,21) AND ObjectID != 867125 AND ObjectID != 867124 AND ObjectID != 867126";

      string secondDelete = "DELETE FROM [T_Object] WHERE InternalDescription LIKE 'P_ID:%' or InternalDescription LIKE 'M_ID:%'";

      using (cnnFI)
      {
        using (SqlCommand firstDeleteCommand = new SqlCommand(firstDelete, cnnFI))
        {
          firstDeleteCommand.ExecuteNonQuery();
        }

        using (SqlCommand secondDeleteCommand = new SqlCommand(secondDelete, cnnFI))
        {
          secondDeleteCommand.ExecuteNonQuery();
        }
      }
    }
    #endregion DeleteObjects

    #region CreateInspections
    //"Create Inspections" - button - takes around 21 minutes 
    //Chance to specific data handling
    private void Button3_Click(object sender, EventArgs e)
    {
      //Dictionary for QAL2 
      Dictionary<string, int> qal2Dict = new Dictionary<string, int>();

      SqlConnection cnnQal;
      SqlConnection cnnQal2;
      SqlConnection cnnQal3;
      SqlConnection cnnAST;
      SqlConnection cnnFI0;
      SqlConnection cnnFI;
      SqlConnection cnnFI2;
      SqlConnection cnnFI3;
      SqlConnection cnnFI4;

      cnnQal = new SqlConnection(cStringToQal);
      cnnQal2 = new SqlConnection(cStringToQal);
      cnnQal3 = new SqlConnection(cStringToQal);
      cnnAST = new SqlConnection(cStringToQal);
      cnnFI0 = new SqlConnection(cStringToFI);
      cnnFI = new SqlConnection(cStringToFI);
      cnnFI2 = new SqlConnection(cStringToFI);
      cnnFI3 = new SqlConnection(cStringToFI);
      cnnFI4 = new SqlConnection(cStringToFI);

      cnnQal.Open();
      cnnQal2.Open();
      cnnQal3.Open();
      cnnAST.Open();
      cnnFI0.Open();
      cnnFI.Open();
      cnnFI2.Open();
      cnnFI3.Open();
      cnnFI4.Open();

      //CreateModifyInfo(cnnFI);                              //Create Hardcoded row - ALREADY DONE!      
      //CreateQal1(cnnQal, cnnFI, nowOffset, qal1InspectionKey, qal1ConfigTableKey, qal1MaxRangeConfigFieldKey, qal1MeasuringValueConfigFieldKey, MAFHGuid, pID);     //Create all QAL1 and insert them into T_Inspection and T_Inspection_InspectionTypes
      //CreateQal2(cnnQal2, cnnFI2, qal2Dict, nowOffset, qal2InspectionKey, qal2ConfigTableKey, qal2FormulaAConfigFieldKey, qal2FormulaBConfigFieldKey, qal2IntervalFromConfigFieldKey, 
      //qal2IntervalToConfigFieldKey, qal2ValidPeriodTypeConfigFieldKey, qal2ValidUntilDTConfigFieldKey, MAFHGuid, pID);     //Create all QAL2 and insert them into T_Inspection and T_Inspection_InspectionTypes
      //CreateQal3(cnnQal3, cnnFI3, nowOffset, pID);                          //Create all QAL3 and insert them into T_Inspection and T_Inspection_InspectionTypes
      //CreateAST(cnnQal4, cnnFI4, amagerMeterList);                           //Create all AST and insert them into T_Inspection and T_Inspection_InspectionTypes
      System.Diagnostics.Debug.WriteLine("--- CREATE INSPECTIONS COMPLETED ---");
    }

    public static void CreateQal1(SqlConnection cnnQal, DateTimeOffset nowOffset, string qal1InspectionKey, string qal1ConfigTableKey, string qal1MaxRangeConfigFieldKey, 
      string qal1MeasuringValueConfigFieldKey, string MAFHGuid, int pID, int jobID, SqlTransaction transaction, DateTime qal1DateTime, Guid cmiGuid, 
      Guid initiatorKey, SqlConnection cnnFI)
    {
      List<Qal1> q1List = new List<Qal1>();
      Guid guid3 = Guid.NewGuid();

      string sql = @"
      SELECT * FROM QAL1 WHERE QAL1_MID in 
      (
        SELECT M_ID FROM Meter WHERE M_PID in 
        (
          SELECT P_ID FROM Position WHERE P_ParentID = 
          (
            SELECT P_ID FROM Position WHERE P_ID = @pID
          ) AND P_ID NOT IN (240, 264, 320, 323, 343, 344)
        )
      )";
      //Only for testing. Will be changed to ALL the QAL1 properties in the old database
      //string testGUID = "5C92DB46-4BD5-419C-8396-B54F2DA9D36F";

      //Transform the retrieved QAL1 to Qal1 object and put them into a list
      using (SqlCommand getQal1 = new SqlCommand(sql, cnnQal))
      {
        getQal1.Parameters.AddWithValue("@pID", pID);

        using (SqlDataReader dr = getQal1.ExecuteReader())
        {
          while (dr.Read())
          {
            var qal1 = new Qal1();

            qal1.Q1_ID = Convert.ToInt32(dr["QAL1_ID"]);
            qal1.Q1_MID = Convert.ToInt32(dr["QAL1_MID"]);
            qal1.Q1_Dato = Convert.ToDateTime(dr["QAL1_Dato"]);
            qal1.Q1_Description = Convert.ToString(dr["QAL1_IDescription"]);
            qal1.Q1_SV = Convert.ToDecimal(dr["QAL1_SV"]);
            qal1.Q1_MV = Convert.ToDecimal(dr["QAL1_MV"]);
            qal1.Q1_INI = Convert.ToString(dr["QAL1_INI"]);
            qal1.Q1_Accept = Convert.ToBoolean(dr["QAL1_Accept"]);

            q1List.Add(qal1);
            //System.Diagnostics.Debug.WriteLine(dr);
          }
        }
      }
      

      int userID = 770;

      //Insert the Qal1 in the T_Inspection 
      foreach (Qal1 q in q1List)
      {
        string newID = "QAL1_ID:" + q.Q1_ID;
        //int jobID;

        Guid guid = Guid.NewGuid();
        Guid guid2 = Guid.NewGuid();
        Guid genericValueFieldKey = Guid.NewGuid();

        //Get ObjectID from T_Object 
        int cObjectID = GetObjectID(q.Q1_MID, transaction, cnnFI);

        //Insert into T_Inspection
        using (SqlCommand s = CreateCommandAndEnlistTransaction("INSERT INTO [T_Inspection] (InspectionGUID, ObjectID, InspectionDateTime, CreatedByUserID, " +
          "CreatedDateTime, LastModifiedByUserID, LastModifiedDateTime, Description, InternalComment, InspectionStatusID, JobId, CreatedByInitiatorKey, " +
          "LastModifiedByInitiatorKey) " +
          "VALUES (@InspectionGUID, @ObjectID, @InspectionDateTime, @CreatedByUserID, @CreatedDateTime, @LastModifiedByUserID, @LastModifiedDateTime, @Description, @IC, " +
          "@isi, @jobid, @CreatedByInitiatorKey, @LastModifiedByInitiatorKey)", cnnFI, transaction))
        {
          s.Parameters.AddWithValue("@InspectionGUID", guid);
          s.Parameters.AddWithValue("@ObjectID", cObjectID);
          s.Parameters.AddWithValue("@InspectionDateTime", q.Q1_Dato);
          s.Parameters.AddWithValue("@CreatedByUserID", userID);
          s.Parameters.AddWithValue("@CreatedDateTime", q.Q1_Dato);
          s.Parameters.AddWithValue("@LastModifiedByUserID", userID);
          s.Parameters.AddWithValue("@LastModifiedDateTime", nowOffset);
          s.Parameters.AddWithValue("@Description", q.Q1_Description);
          s.Parameters.AddWithValue("@IC", newID);
          if (q.Q1_Accept)
          {
            s.Parameters.AddWithValue("@isi", 8);
          }else
          {
            s.Parameters.AddWithValue("@isi", 10);
          }
          s.Parameters.AddWithValue("@jobid", jobID);
          s.Parameters.AddWithValue("@CreatedByInitiatorKey", initiatorKey);
          s.Parameters.AddWithValue("@LastModifiedByInitiatorKey", initiatorKey);

          s.ExecuteNonQuery();
        }

        // Get the VersionId from the T_GenericConfigTable
        int versionID = GetVersionId(qal1InspectionKey, transaction, cnnFI);

        //Insert into T_GenericValueRow 
        InsertIntoGenericValueRow(guid2, qal1ConfigTableKey, guid, nowOffset, versionID, transaction, cnnFI);

        //Insert the 2 QAL1 properties into T_GenericValueField
        InsertReadingWithValue(guid2, qal1MaxRangeConfigFieldKey, q.Q1_SV, nowOffset, transaction, cnnFI);
        InsertReadingWithValue(guid2, qal1MeasuringValueConfigFieldKey, q.Q1_MV, nowOffset, transaction, cnnFI);

        //Insert the Inspections in T_Inspection_InspectionTypes 
        InsertIntoInspectionTypes(guid, 99, transaction, cnnFI);
      }

      //Last insert into T_JobInspectionType
      using (SqlCommand t = CreateCommandAndEnlistTransaction("INSERT INTO [T_JobInspectionType] (JobInspectionTypeKey, JobId, InspectionTypeId, PeriodOfTestingStart, " +
        "CreateModifyInfoGuid) " +
        "VALUES (@JobInspectionTypeKey, @jobId, @iti, @pts, @cmiKey)", cnnFI, transaction))
      {
        t.Parameters.AddWithValue("@JobInspectionTypeKey", guid3);
        t.Parameters.AddWithValue("@jobId", jobID);
        t.Parameters.AddWithValue("@iti", 99);
        t.Parameters.AddWithValue("@pts", qal1DateTime);
        t.Parameters.AddWithValue("@cmiKey", cmiGuid);

        t.ExecuteNonQuery();
      }

      //Tell when the funktion is finished
      System.Diagnostics.Debug.WriteLine("--- CreateQal1() completed ---");
    }

    public static void CreateQal2(SqlConnection cnnQal2, Dictionary<string, int> qal2Dict, DateTimeOffset nowOffset, string qal2Inspection, string qal2ConfigTableKey,
      string qal2FormulaAConfigFieldKey, string qal2FormulaBConfigFieldKey, string qal2IntervalFromConfigFieldKey, string qal2IntervalToConfigFieldKey, 
      string qal2ValidPeriodTypeConfigFieldKey, string qal2ValidUntilDTConfigFieldKey, string MAFHGuid, int pID, int jobID, Guid cmiGuid, 
      SqlTransaction transaction, Guid initiatorKey, SqlConnection cnnFI)
    {
      List<int> mIdList = new List<int>();
      List<int> noq2List = new List<int>();
      List<Qal2> q2List = new List<Qal2>();

      DateTimeOffset earliestQal2 = new DateTimeOffset();

      Guid guid3 = Guid.NewGuid();

      string sql = @"
      SELECT M_ID FROM Meter WHERE M_PID in 
      (
        SELECT P_ID FROM Position WHERE P_ParentID = 
        (
          SELECT P_ID FROM Position WHERE P_ID = @pID
        ) AND P_ID NOT IN (240, 264, 320, 323, 343, 344)
      )";

      //Get all the Meters to see if they have a QAL2
      using (SqlCommand getQal2 = new SqlCommand(sql, cnnQal2))
      {
        getQal2.Parameters.AddWithValue("@pID", pID);

        using (SqlDataReader dr = getQal2.ExecuteReader())
        {
          while (dr.Read())
          {
            int meterID = Convert.ToInt32(dr["M_ID"]);

            mIdList.Add(meterID);
          }
        }
      }

      foreach (int mID in mIdList)
      {
        using (SqlCommand sc = new SqlCommand("SELECT COUNT(*) FROM QAL2 WHERE QAL2_MID = @mID", cnnQal2))
        {
          sc.Parameters.AddWithValue("@mID", mID);
          int anyData = (int)sc.ExecuteScalar();

          if (anyData < 1)
          {
            noq2List.Add(mID);
            earliestQal2 = nowOffset;
          }
          else
          {
            //Transform the retrieved QAL2 to Qal2 object and put them into a list
            using (SqlCommand getQal2 = new SqlCommand("SELECT * FROM QAL2 WHERE QAL2_MID = @mID", cnnQal2))
            {
              getQal2.Parameters.AddWithValue("@mID", mID);

              using (SqlDataReader dr = getQal2.ExecuteReader())
              {
                while (dr.Read())
                {
                  var qal2 = new Qal2();

                  //GET ALL QAL2 PROPERTIES FROM OLD DATABASE
                  qal2.Q2_ID = Convert.ToInt32(dr["QAL2_ID"]);
                  qal2.Q2_MID = Convert.ToInt32(dr["QAL2_MID"]);
                  qal2.Q2_Dato = Convert.ToDateTime(dr["QAL2_Dato"]);
                  qal2.Q2_SlutDato = Convert.ToDateTime(dr["QAL2_SlutDato"]);
                  qal2.Q2_Funktion = Convert.ToString(dr["QAL2_Funktion"]);
                  qal2.Q2_Accept = Convert.ToBoolean(dr["QAL2_Accept"]);

                  q2List.Add(qal2);
                }
              }
            }
            //Get the earliest Date for the QAL2 to insert into T_JobInspectionType
            earliestQal2 = q2List.OrderBy(a => a.Q2_Dato).First().Q2_Dato;
          }
        }
      }

      int userID = 770;

      foreach (Qal2 q2 in q2List)
      {
        string newID = "QAL2_ID:" + q2.Q2_ID;

        Guid guid = Guid.NewGuid();
        Guid guid2 = Guid.NewGuid();

        //Get ObjectID from T_Object
        int cObjectID = GetObjectID(q2.Q2_MID, transaction, cnnFI);

        //Insert the Qal2 in T_Inspection 
        using (SqlCommand z = CreateCommandAndEnlistTransaction("INSERT INTO [T_Inspection] (InspectionGUID, ObjectID, InspectionDateTime, CreatedByUserID, " +
          "CreatedDateTime, LastModifiedByUserID, LastModifiedDateTime, InternalComment, InspectionStatusID, JobId, CreatedByInitiatorKey, LastModifiedByInitiatorKey) " +
          "VALUES (@InspectionGUID, @ObjectID, @InspectionDateTime, @CreatedByUserID, @CreatedDateTime, @LastModifiedByUserID, @LastModifiedDateTime, @IC, " +
          "@isi, @jobid, @CreatedByInitiatorKey, @LastModifiedByInitiatorKey)", cnnFI, transaction))
        {
          z.Parameters.AddWithValue("@InspectionGUID", guid);
          z.Parameters.AddWithValue("@ObjectID", cObjectID);
          z.Parameters.AddWithValue("@InspectionDateTime", q2.Q2_SlutDato);
          z.Parameters.AddWithValue("@CreatedByUserID", userID);
          z.Parameters.AddWithValue("@CreatedDateTime", q2.Q2_Dato);
          z.Parameters.AddWithValue("@LastModifiedByUserID", userID);
          z.Parameters.AddWithValue("@LastModifiedDateTime", nowOffset);
          z.Parameters.AddWithValue("@IC", newID);
          if (q2.Q2_Accept)
          {
            z.Parameters.AddWithValue("@isi", 8);
          }
          else
          {
            z.Parameters.AddWithValue("@isi", 10);
          }
          z.Parameters.AddWithValue("@jobid", jobID);
          z.Parameters.AddWithValue("@CreatedByInitiatorKey", initiatorKey);
          z.Parameters.AddWithValue("@LastModifiedByInitiatorKey", initiatorKey);

          z.ExecuteNonQuery();
        }

        //Get the VersionId from the T_GenericConfigTable
        int versionID = GetVersionId(qal2Inspection, transaction, cnnFI);

        //Insert into T_GenericValueRow 
        InsertIntoGenericValueRow(guid2, qal2ConfigTableKey, guid, nowOffset, versionID, transaction, cnnFI);

        //Insert into T_GenericValueField for both a and b in the QAL2_Funktion 
        CreateAandB(cnnQal2, q2.Q2_Funktion, guid2, nowOffset, MAFHGuid, transaction, cnnFI);

        //Insert IntervalFrom and IntervalTo in T_GenericValueField
        InsertIntervalsIntoGenericValueField(cnnQal2, q2.Q2_ID, guid2, qal2IntervalFromConfigFieldKey, qal2IntervalToConfigFieldKey, nowOffset, transaction, cnnFI);

        //Insert QAL2ValidPeriodType and QAL2ValidUntilDateTime into T_GenericValueField
        InsertValidTimeIntoGenericValueField(cnnQal2, q2.Q2_ID, guid2, qal2ValidPeriodTypeConfigFieldKey, qal2ValidUntilDTConfigFieldKey, nowOffset,
                                            transaction, cnnFI);

        //Insert the Inspections in the T_Inspection_InspectionTypes
        InsertIntoInspectionTypes(guid, 100, transaction, cnnFI);

      }

      //INSERT QAL2 for all the nonexisting QAL2
      foreach (int mID in noq2List)
      {
        Guid newGuid = Guid.NewGuid();
        Guid guid2 = Guid.NewGuid();

        //Get cObjectID from T_Object
        int cObjectID = GetObjectID(mID, transaction, cnnFI);

        //Insert the Qal2 in T_Inspection 
        using (SqlCommand z = CreateCommandAndEnlistTransaction("INSERT INTO [T_Inspection] (InspectionGUID, ObjectID, InspectionDateTime, CreatedByUserID, " +
          "CreatedDateTime, LastModifiedByUserID, LastModifiedDateTime, InternalComment, InspectionStatusID, JobId, CreatedByInitiatorKey, " +
          "LastModifiedByInitiatorKey) " +
          "VALUES (@InspectionGUID, @ObjectID, @InspectionDateTime, @CreatedByUserID, @CreatedDateTime, @LastModifiedByUserID, @LastModifiedDateTime, @IC, " +
          "@isi, @jobid, @CreatedByInitiatorKey, @LastModifiedByInitiatorKey)", cnnFI, transaction))
        {
          z.Parameters.AddWithValue("@InspectionGUID", newGuid);
          z.Parameters.AddWithValue("@ObjectID", cObjectID);
          z.Parameters.AddWithValue("@InspectionDateTime", nowOffset);
          z.Parameters.AddWithValue("@CreatedByUserID", userID);
          z.Parameters.AddWithValue("@CreatedDateTime", nowOffset);
          z.Parameters.AddWithValue("@LastModifiedByUserID", userID);
          z.Parameters.AddWithValue("@LastModifiedDateTime", nowOffset);
          z.Parameters.AddWithValue("@IC", "Created as part of import, no actual data imported");
          z.Parameters.AddWithValue("@isi", 3);
          z.Parameters.AddWithValue("@jobid", jobID);
          z.Parameters.AddWithValue("@CreatedByInitiatorKey", initiatorKey);
          z.Parameters.AddWithValue("@LastModifiedByInitiatorKey", initiatorKey);

          z.ExecuteNonQuery();
        }

        //Insert the Inspections in the T_Inspection_InspectionTypes
        InsertIntoInspectionTypes(newGuid, 100, transaction, cnnFI);
      }

      //Last insert into T_JobInspectionType
      using (SqlCommand t = CreateCommandAndEnlistTransaction("INSERT INTO [T_JobInspectionType] (JobInspectionTypeKey, JobId, InspectionTypeId, PeriodOfTestingStart, " +
        "CreateModifyInfoGuid) " +
        "VALUES (@JobInspectionTypeKey, @jobId, @iti, @pts, @cmiKey)", cnnFI, transaction))
      {
        t.Parameters.AddWithValue("@JobInspectionTypeKey", guid3);
        t.Parameters.AddWithValue("@jobId", jobID);
        t.Parameters.AddWithValue("@iti", 100);
        t.Parameters.AddWithValue("@pts", earliestQal2); 
        t.Parameters.AddWithValue("@cmiKey", cmiGuid);

        t.ExecuteNonQuery();
      }

      //Tell when the funktion is finished
      System.Diagnostics.Debug.WriteLine("--- CreateQal2() completed ---");
    }

    public static void CreateQal3(SqlConnection cnnQal3,DateTimeOffset nowOffset, int pID, int jobID, SqlTransaction transaction, Guid cmiGuid, 
      Guid initiatorKey, SqlConnection cnnFI)
    {
      List<Qal3> q3List = new List<Qal3>();
      Guid guid3 = Guid.NewGuid();

      DateTimeOffset earliestQal3 = new DateTimeOffset();

      string sql = @"
      SELECT * FROM QAL3 WHERE QAL3_MID in 
      (
        SELECT M_ID FROM Meter WHERE M_PID in 
        (
          SELECT P_ID FROM Position WHERE P_ParentID = 
          (
            SELECT P_ID FROM Position WHERE P_ID = @pID
          ) AND P_ID NOT IN (240, 264, 320, 323, 343, 344)
        )
      )";
      string qal3InspectionKey = "QAL3Inspection";
      string qal3ConfigTableKey = "A864C548-C8A7-4AD2-9237-CA181346E134";
      string readingGUID = "7929189E-86D2-4295-987E-BA106B43C4D9";

      //Transform the retrieved QAL3 to Qal3 object and put them into a list 
      using (SqlCommand getQal3 = new SqlCommand(sql, cnnQal3))
      {
        getQal3.Parameters.AddWithValue("@pID", pID);

        using (SqlDataReader dr = getQal3.ExecuteReader())
        {
          while (dr.Read())
          {
            var qal3 = new Qal3();

            qal3.Q3_ID = Convert.ToInt32(dr["QAL3_ID"]);
            qal3.Q3_MID = Convert.ToInt32(dr["QAL3_MID"]);
            qal3.Q3_Dato = Convert.ToDateTime(dr["QAL3_Dato"]);
            qal3.Q3_Status = Convert.ToInt32(dr["QAL3_Status"]);

            q3List.Add(qal3);
          }
        }
      }
      //Get the earliest Date for the QAL3 to insert into T_JobInspectionType
      earliestQal3 = q3List.OrderBy(a => a.Q3_Dato).First().Q3_Dato;

      int userID = 770;

      foreach (Qal3 q3 in q3List)
      {
        
        string newID = "QAL3_ID:" + q3.Q3_ID;
        //int jobID;

        Guid guid = Guid.NewGuid();
        Guid guid2 = Guid.NewGuid();
        Guid genericValueFieldKey = Guid.NewGuid();

        //Get Component ObjectID from T_Object
        int cObjectID = GetObjectID(q3.Q3_MID, transaction, cnnFI);

        //Insert the Qal3 in T_Inspection 
        using (SqlCommand y = CreateCommandAndEnlistTransaction("INSERT INTO [T_Inspection] (InspectionGUID, ObjectID, InspectionDateTime, CreatedByUserID, " +
          "CreatedDateTime, LastModifiedByUserID, LastModifiedDateTime, InternalComment, InspectionStatusID, JobId, CreatedByInitiatorKey, LastModifiedByInitiatorKey) " +
          "VALUES (@InspectionGUID, @ObjectID, @InspectionDateTime, @CreatedByUserID, @CreatedDateTime, @LastModifiedByUserID, @LastModifiedDateTime, " +
          "@IC, @isi, @jobid, @CreatedByInitiatorKey, @LastModifiedByInitiatorKey)", cnnFI, transaction))
        {
          y.Parameters.AddWithValue("@InspectionGUID", guid);
          y.Parameters.AddWithValue("@ObjectID", cObjectID);
          y.Parameters.AddWithValue("@InspectionDateTime", q3.Q3_Dato);
          y.Parameters.AddWithValue("@CreatedByUserID", userID);
          y.Parameters.AddWithValue("@CreatedDateTime", q3.Q3_Dato);
          y.Parameters.AddWithValue("@LastModifiedByUserID", userID);
          y.Parameters.AddWithValue("@LastModifiedDateTime", nowOffset);
          y.Parameters.AddWithValue("@IC", newID);
          y.Parameters.AddWithValue("@isi", 2);
          y.Parameters.AddWithValue("@jobid", jobID);
          y.Parameters.AddWithValue("@CreatedByInitiatorKey", initiatorKey);
          y.Parameters.AddWithValue("@LastModifiedByInitiatorKey", initiatorKey);

          y.ExecuteNonQuery();
        }

        // Get the VersionId from the T_GenericConfigTable
        int versionID = GetVersionId(qal3InspectionKey, transaction, cnnFI);

        //Insert into the T_GenericValueRow 
        InsertIntoGenericValueRow(guid2, qal3ConfigTableKey, guid, nowOffset, versionID, transaction, cnnFI);

        //Insert into T_GenericValueField
        InsertIntoGenericValueField(genericValueFieldKey, guid2, readingGUID, nowOffset, transaction, cnnFI);

        //Insert the Inspection in the T_Inspection_InspectionTypes
        InsertIntoInspectionTypes(guid, 101, transaction, cnnFI);

        //Insert all the readings for the specific QAL3
        CreateReadings(cnnQal3, genericValueFieldKey, q3.Q3_MID, q3.Q3_ID, nowOffset, transaction, cnnFI);
      }

      //Last insert into T_JobInspectionType
      using (SqlCommand t = CreateCommandAndEnlistTransaction("INSERT INTO [T_JobInspectionType] (JobInspectionTypeKey, JobId, InspectionTypeId, PeriodOfTestingStart, " +
        "CreateModifyInfoGuid) " +
        "VALUES (@JobInspectionTypeKey, @jobId, @iti, @pts, @cmiKey)", cnnFI, transaction))
      {
        t.Parameters.AddWithValue("@JobInspectionTypeKey", guid3);
        t.Parameters.AddWithValue("@jobId", jobID);
        t.Parameters.AddWithValue("@iti", 101);
        t.Parameters.AddWithValue("@pts", earliestQal3);
        t.Parameters.AddWithValue("@cmiKey", cmiGuid);

        t.ExecuteNonQuery();

      }
        //Tell when the funktion is finished
        System.Diagnostics.Debug.WriteLine("--- CreateQal3() completed ---");
    }

    //TODO
    public static void CreateAST(SqlConnection cnnAST, DateTimeOffset nowOffset, string MAFHGuid, int pID, int jobID, Guid cmiGuid, 
      SqlTransaction transaction, Guid initiatorKey, SqlConnection cnnFI)
    {
      List<AST> astList = new List<AST>();
      DateTimeOffset earliestAST = new DateTimeOffset();
      Guid guid3 = Guid.NewGuid();

      string sql = @"
      SELECT * FROM AST WHERE AST_MID in 
      (
        SELECT M_ID FROM Meter WHERE M_PID in 
        (
          SELECT P_ID FROM Position WHERE P_ParentID = 
          (
            SELECT P_ID FROM Position WHERE P_ID = @pID
          ) AND P_ID NOT IN (240, 264, 320, 323, 343, 344)
        )
      )";

      //Get all the AST from AST in the QAL database
      using (SqlCommand sc = new SqlCommand(sql, cnnAST))
      {
        sc.Parameters.AddWithValue("@pID", pID);

        using (SqlDataReader dr = sc.ExecuteReader())
        {
          while (dr.Read())
          {
            AST ast = new AST();

            ast.AST_ID = Convert.ToInt32(dr["AST_ID"]);
            ast.AST_MID = Convert.ToInt32(dr["AST_MID"]);
            ast.AST_Dato = Convert.ToDateTime(dr["AST_Dato"]);
            ast.AST_Accept = Convert.ToBoolean(dr["AST_Accept"]);
            ast.AST_INI = Convert.ToString(dr["AST_INI"]);
            ast.AST_UID = Convert.ToInt32(dr["AST_UID"]);

            astList.Add(ast);
          }
        }
      }
      earliestAST = astList.OrderBy(a => a.AST_Dato).First().AST_Dato;

      int userID = 770;

      foreach (AST ast in astList)
      {

        string newID = "AST_ID:" + ast.AST_ID;
        //int jobID;

        Guid guid = Guid.NewGuid();
        Guid guid2 = Guid.NewGuid();
        Guid genericValueFieldKey = Guid.NewGuid();

        //Get ObjectID from T_Object
        int cObjectID = GetObjectID(ast.AST_MID, transaction, cnnFI);

        //Insert into T_Inspection
        using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO [T_Inspection] (InspectionGUID, ObjectID, InspectionDateTime, CreatedByUserID, " +
          "CreatedDateTime, LastModifiedByUserID, LastModifiedDateTime, InternalComment, InspectionStatusID, JobId, CreatedByInitiatorKey, LastModifiedByInitiatorKey) " +
          "VALUES (@InspectionGUID, @ObjectID, @InspectionDateTime, @CreatedByUserID, @CreatedDateTime, @LastModifiedByUserID, @LastModifiedDateTime, @IC, " +
          "@isi, @jobid, @CreatedByInitiatorKey, @LastModifiedByInitiatorKey)", cnnFI, transaction))
        {
          sc.Parameters.AddWithValue("@InspectionGUID", guid);
          sc.Parameters.AddWithValue("@ObjectID", cObjectID);
          sc.Parameters.AddWithValue("@InspectionDateTime", ast.AST_Dato);
          sc.Parameters.AddWithValue("@CreatedByUserID", userID);
          sc.Parameters.AddWithValue("@CreatedDateTime", ast.AST_Dato);
          sc.Parameters.AddWithValue("@LastModifiedByUserID", userID);
          sc.Parameters.AddWithValue("@LastModifiedDateTime", nowOffset);
          sc.Parameters.AddWithValue("@IC", newID);
          if (ast.AST_Accept)
          {
            sc.Parameters.AddWithValue("@isi", 8);
          }
          else
          {
            sc.Parameters.AddWithValue("@isi", 10);
          }
          sc.Parameters.AddWithValue("@jobid", jobID);
          sc.Parameters.AddWithValue("@CreatedByInitiatorKey", initiatorKey);
          sc.Parameters.AddWithValue("@LastModifiedByInitiatorKey", initiatorKey);

          sc.ExecuteNonQuery();
        }

        // Get the VersionId from the T_GenericConfigTable
          

        //Insert into the T_GenericValueRow 
         

        //Insert into T_GenericValueField


        //Insert the Inspections in the T_Inspection_InspectionTypes
        InsertIntoInspectionTypes(guid, 102, transaction, cnnFI);
      }

      //Last insert into T_JobInspectionType
      using (SqlCommand t = CreateCommandAndEnlistTransaction("INSERT INTO [T_JobInspectionType] (JobInspectionTypeKey, JobId, InspectionTypeId, PeriodOfTestingStart, " +
        "CreateModifyInfoGuid) " +
        "VALUES (@JobInspectionTypeKey, @jobId, @iti, @pts, @cmiKey)", cnnFI, transaction))
      {
        t.Parameters.AddWithValue("@JobInspectionTypeKey", guid3);
        t.Parameters.AddWithValue("@jobId", jobID);
        t.Parameters.AddWithValue("@iti", 102);
        t.Parameters.AddWithValue("@pts", earliestAST);   
        t.Parameters.AddWithValue("@cmiKey", cmiGuid);

        t.ExecuteNonQuery();
      }

    }

    public static void InsertIntoInspectionTypes(Guid guid, int iti, SqlTransaction transaction, SqlConnection cnnFI)
    {
      //Insert into T_Inspection_InspectionTypes
      using (SqlCommand c = CreateCommandAndEnlistTransaction("INSERT INTO [T_Inspection_InspectionTypes] (InspectionGUID, InspectionTypeID) " +
          "VALUES (@IG, @ITI)", cnnFI, transaction))
      {
        c.Parameters.AddWithValue("@IG", guid);
        c.Parameters.AddWithValue("@ITI", iti);

        c.ExecuteNonQuery();
      }
    }

    public static void InsertIntoJobs(int userID, string newID, Dictionary<string, int> qal2Dict, SqlTransaction transaction, SqlConnection cnnFI)
    {
      //Insert into T_Jobs
      using (SqlCommand j = CreateCommandAndEnlistTransaction("INSERT INTO [T_Jobs] (Comment, CreatedByUserID, Done, InspectionAreaId) " +
          "VALUES (@comment, @cbu, @done, @iai)", cnnFI, transaction))
      {
        j.Parameters.AddWithValue("@comment", newID);
        j.Parameters.AddWithValue("@cbu", userID);
        j.Parameters.AddWithValue("@done", true);
        j.Parameters.AddWithValue("@iai", 12);

        j.ExecuteNonQuery();
      }

      using ( SqlCommand d = CreateCommandAndEnlistTransaction("SELECT JobId FROM [T_Jobs] WHERE Comment = @newID", cnnFI, transaction))
      {
        d.Parameters.AddWithValue("@newID", newID);

        int jobID = (int)d.ExecuteScalar();
        qal2Dict.Add(newID, jobID);
      }
    }

    public static int GetObjectID(int id, SqlTransaction transaction, SqlConnection cnnFI)
    {
      string oldID = "M_ID:" + id;
      int objectID;
      int cObjectID;

      //Get ObjectID from the Meter on the inspections
      using (SqlCommand b = CreateCommandAndEnlistTransaction("SELECT ObjectID FROM [T_Object] WHERE InternalDescription = @oldID", cnnFI, transaction))
      {
        b.Parameters.AddWithValue("@oldID", oldID);

        objectID = (int)b.ExecuteScalar();
      }

      using (SqlCommand sc = CreateCommandAndEnlistTransaction("SELECT ObjectID FROM T_Object WHERE InternalDescription LIKE 'C_ID:%' " +
        "AND ParentObjectID = @objectID", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@objectID", objectID);

        cObjectID = (int)sc.ExecuteScalar();
      }

        return cObjectID;
    }

    public static void CreateModifyInfo(Guid cmiGuid, DateTime nowOffset, int actorId, string userName, SqlTransaction transaction, SqlConnection cnnFI)
    {
      using (SqlCommand q = CreateCommandAndEnlistTransaction("INSERT INTO [T_CreateModifyInfo] (CreateModifyInfoGuid, InitialCreateModifyInfoKey, Description, CreateModifyInitiatorGuid, CreateModifyDate) " +
        "VALUES (@CreateMInfoG, @InitialMIKey, @Description, @CreateMInitiatorGuid, @CreateModifyDate)", cnnFI, transaction))
      {
        q.Parameters.AddWithValue("@CreateMInfoG", cmiGuid);
        q.Parameters.AddWithValue("@InitialMIKey", cmiGuid);
        q.Parameters.AddWithValue("@Description", "Data imported from QAL database. ActorId: " + actorId + ". Imported by: " + userName);
        q.Parameters.AddWithValue("@CreateMInitiatorGuid", "ABCCBBFD-ABC2-ABC1-ABC9-ABC517A57B33");   //TXA Guid
        q.Parameters.AddWithValue("@CreateModifyDate", nowOffset);  

        q.ExecuteNonQuery();

      }

      System.Diagnostics.Debug.WriteLine("--- CreateModifyInfo() completed ---");
    }

    public static int GetVersionId(string qalInspectionKey, SqlTransaction transaction, SqlConnection cnnFI)
    {
      int versionID = 0;

      using (SqlCommand sc = CreateCommandAndEnlistTransaction("SELECT * FROM [T_GenericConfigTable] WHERE Name = @qal2I", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@qal2I", qalInspectionKey);
        using (SqlDataReader dr = sc.ExecuteReader())
        {
          while (dr.Read())
          {
            versionID = Convert.ToInt32(dr["VersionId"]);
          }
        }
      }

      return versionID;

    }

    public static void InsertIntoGenericValueRow(Guid guid2, string qalConfigTableKey, Guid guid, DateTimeOffset nowOffset, int versionID, 
      SqlTransaction transaction, SqlConnection cnnFI)
    {

      using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO [T_GenericValueRow] (GenericValueRowKey, GenericConfigTableKey, ItemKey, VersionId, LastModifiedDate, LastModifiedBy) " +
          "VALUES (@gvrk, @gctk, @ItemKey, @VersionId, @LastModifiedDate, @LastModifiedBy)", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@gvrk", guid2);
        sc.Parameters.AddWithValue("@gctk", qalConfigTableKey);
        sc.Parameters.AddWithValue("@ItemKey", guid);
        sc.Parameters.AddWithValue("@VersionId", versionID);
        sc.Parameters.AddWithValue("@LastModifiedDate", nowOffset);
        sc.Parameters.AddWithValue("@LastModifiedBy", "2F636E2F-C03D-EA11-A835-005056A7F76B");  //MAFHGuid

        sc.ExecuteNonQuery();
      }
    }

    public static void InsertIntoGenericValueField(Guid genericValueFieldKey, Guid genericValueRowKey, string guid, DateTimeOffset nowOffset, 
      SqlTransaction transaction, SqlConnection cnnFI)
    {

      using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO [T_GenericValueField] (GenericValueFieldKey, GenericValueRowKey, GenericConfigFieldKey, " +
          "ReadOnly, LastModifiedDate, LastModifiedBy) " +
          "VALUES (@gvfk, @gvrk, @gcfk, @ReadOnly, @lmd, @lmb)", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@gvfk", genericValueFieldKey);
        sc.Parameters.AddWithValue("@gvrk", genericValueRowKey);
        sc.Parameters.AddWithValue("@gcfk", guid);
        sc.Parameters.AddWithValue("@ReadOnly", true);
        sc.Parameters.AddWithValue("@lmd", nowOffset);
        sc.Parameters.AddWithValue("@lmb", "2F636E2F-C03D-EA11-A835-005056A7F76B"); //MAFHGuid

        sc.ExecuteNonQuery();
      }
    }

    public static void CreateAandB(SqlConnection cnnQal, string equation, Guid guid2,  DateTimeOffset nowOffset, string MAFHGuid, SqlTransaction transaction
      , SqlConnection cnnFI)
    {
      Guid g1 = Guid.NewGuid();
      Guid g2 = Guid.NewGuid();

      int x;
      int plus;
      int gange;
      string tempA = "";
      string tempB = "";
      float a = 0;
      float b = 0;

      string funktion = equation;

      //Check if the string is longer than 2 and doesnt contain the letter 'a'
      if (funktion.Length > 2 && !funktion.Contains("a"))
      {
        funktion = funktion.Remove(0, 2);
        funktion = funktion.Replace(" ", "");
        funktion = funktion.Replace("(", "");
        funktion = funktion.Replace(")", "");
        x = funktion.IndexOf("x");              //Get the x
        plus = funktion.IndexOf("+");           //Get the +
        gange = funktion.IndexOf("*");          //Get the *

        //Check if the 'x' is in a higher position than the first character
        if (x > 0)
        {
          tempA = funktion.Substring(0, x);
          tempA = tempA.Replace("*", "");
          tempA = tempA.Replace(".", ",");

          //Check if the string is higher than the x + 1 to see where the 'b' is 
          if (funktion.Length > x + 1)
          {
            tempB = funktion.Substring(x + 2);
            tempB = tempB.Replace(".", ",");
          }
          //Else if the string dont contain 'x' set b = 0
          else if (!funktion.Contains("+"))
          {
            tempB = "0";
          }
        }

        //Check if the string contains 'Xi' and see if the lenght is higher than 10
        if (funktion.Contains("Xi") && funktion.Length > 10)
        {
          tempA = funktion.Substring(plus + 1);
          tempA = tempA.Replace("*", "");
          tempA = tempA.Replace(".", ",");
          tempA = tempA.Replace("Xi", "");

          tempB = funktion.Substring(0, plus);
          tempB = tempB.Replace(".", ",");
        }

        //If tempA = "a", set tempA = "";
        if (tempA.Equals("a"))
        {
          tempA = "";
        }

        //If tempA is higher than 0, convert tempA to float
        if (tempA.Length > 0)
        {
          a = float.Parse(tempA);
          //System.Diagnostics.Debug.WriteLine(tempA);
        }

        //If tempB is higher than 0, convert tempB to float
        if (tempB.Length > 0)
        {
          b = float.Parse(tempB);
          //System.Diagnostics.Debug.WriteLine(tempB);
          //System.Diagnostics.Debug.WriteLine("");
        }
      }

      //Insert the a and b into T_GenericValueField
      using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO [T_GenericValueField] (GenericValueFieldKey, GenericValueRowKey, GenericConfigFieldKey, " +
        "ValueDecimal, OriginalValue, ReadOnly, LastModifiedDate, LastModifiedBy) " +
        "VALUES (@gvfk, @gvrk, @gcfk, @a, @a2, @ReadOnly, @lmd, @lmb)", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@gvfk", g1);
        sc.Parameters.AddWithValue("@gvrk", guid2);
        sc.Parameters.AddWithValue("@gcfk", "C4734B17-69A1-4806-A6B2-CE0B475D320E");
        sc.Parameters.AddWithValue("@a", a);
        sc.Parameters.AddWithValue("@a2", a);
        sc.Parameters.AddWithValue("@ReadOnly", true);
        sc.Parameters.AddWithValue("@lmd", nowOffset);
        sc.Parameters.AddWithValue("@lmb", MAFHGuid);

        sc.ExecuteNonQuery();
      }

      using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO [T_GenericValueField] (GenericValueFieldKey, GenericValueRowKey, GenericConfigFieldKey, " +
        "ValueDecimal, OriginalValue, ReadOnly, LastModifiedDate, LastModifiedBy) " +
        "VALUES (@gvfk, @gvrk, @gcfk, @b, @b2, @ReadOnly, @lmd, @lmb)", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@gvfk", g2);
        sc.Parameters.AddWithValue("@gvrk", guid2);
        sc.Parameters.AddWithValue("@gcfk", "AEEC4528-8A5E-4F65-8B5E-0474DC29F5EE");
        sc.Parameters.AddWithValue("@b", b);
        sc.Parameters.AddWithValue("@b2", b);
        sc.Parameters.AddWithValue("@ReadOnly", true);
        sc.Parameters.AddWithValue("@lmd", nowOffset);
        sc.Parameters.AddWithValue("@lmb", MAFHGuid);

        sc.ExecuteNonQuery();
      }
    }

    public static void InsertIntervalsIntoGenericValueField(SqlConnection cnnQal, int Q2_ID, Guid guid2, string qal2IntervalFromConfigFieldKey, 
      string qal2IntervalToConfigFieldKey, DateTimeOffset nowOffset, SqlTransaction transaction, SqlConnection cnnFI)
    {
      decimal iFrom = 0;
      decimal iTo = 0;
      string interval = "";

      //Get QAL2_Intervals from QAL2
      using (SqlCommand sc = new SqlCommand("SELECT QAL2_Interval FROM QAL2 WHERE QAL2_ID = @Q2_ID", cnnQal))
      {
        sc.Parameters.AddWithValue("Q2_ID", Q2_ID);

        using (SqlDataReader dr = sc.ExecuteReader())
        {
          while (dr.Read())
          {
            interval = Convert.ToString(dr["QAL2_Interval"]);
          }
        }
      }

      string tempI;

      //BAD CODE for not doing anything if the string i a '?' or emptry string
      if (interval.Contains("?") || interval.Equals(""))
      {
        iFrom = 10000000;
        iTo = 10000000;
      } 
      else if (interval.Contains(".."))  //Remove the dots
      {
        tempI = interval.Replace(".", "");

        iFrom = Convert.ToDecimal(interval[0].ToString());
        iTo = Convert.ToDecimal(tempI.Substring(1));
      }
      else if (interval.Contains("-"))  //Remove the line
      {
        tempI = interval.Replace(" ", "");
        tempI = tempI.Replace("-", "");
        tempI = tempI.Replace("mg/m3", "");
        tempI = tempI.Replace("mg", "");

        iFrom = Convert.ToDecimal(interval[0].ToString());
        iTo = Convert.ToDecimal(tempI.Substring(1));
      }

      //Insert the IntervalFrom and InternalTo into T_GenericValueField
      InsertReadingWithValue(guid2, qal2IntervalFromConfigFieldKey, iFrom, nowOffset, transaction, cnnFI);
      InsertReadingWithValue(guid2, qal2IntervalToConfigFieldKey, iTo, nowOffset, transaction, cnnFI);

    }

    public static void InsertValidTimeIntoGenericValueField(SqlConnection cnnQal, int Q2_ID, Guid guid2, string qal2ValidPeriodTypeConfigFieldKey,
      string qal2ValidUntilDTConfigFieldKey, DateTimeOffset nowOffset, SqlTransaction transaction, SqlConnection cnnFI)
    {
      DateTime dato = DateTime.Now;
      DateTime slutDato = DateTime.Now;

      //Get the QAL2_Dato and QAL2_SlutDato from QAL2
      using (SqlCommand sc = new SqlCommand("SELECT QAL2_Dato, QAL2_SlutDato FROM QAL2 WHERE QAL2_ID = @q2_id", cnnQal))
      {
        sc.Parameters.AddWithValue("@q2_id", Q2_ID);

        using (SqlDataReader dr = sc.ExecuteReader())
        {
          while (dr.Read())
          {
            dato = Convert.ToDateTime(dr["QAL2_Dato"]);
            slutDato = Convert.ToDateTime(dr["QAL2_SlutDato"]);
          }
        }
      }

      int validPeriodType = slutDato.Year - dato.Year;  //subtract the QAL2_Dato from the QAL2_SlutDato

      string vpt = "";

      if (validPeriodType == 3)         //3 years difference
      {
        vpt = "6EA8C05D-ED35-47C0-91BD-3330E31EF7C8";
      } else if (validPeriodType == 5)  //5 years differnce
      {
        vpt = "02CDF7A5-6F64-492B-A7F9-883E53291F8A";
      }

      //Only insert if the year difference is 3 or 5 
      if (validPeriodType == 3 || validPeriodType == 5)
      {
        InsertReadingDateTime(guid2, qal2ValidUntilDTConfigFieldKey, slutDato, nowOffset, transaction, cnnFI);
        //InsertReadingWithoutValue(cnnFI, guid2, qal2ValidPeriodTypeConfigFieldKey, vpt, nowOffset); //WHY THIS ???

        Guid guid = Guid.NewGuid();

        using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO [T_GenericValueField] (GenericValueFieldKey, GenericValueRowKey, GenericConfigFieldKey, GenericConfigLookupFieldKey, " +
          "ReadOnly, LastModifiedDate, LastModifiedBy) " +
          "VALUES (@gvfk, @gvrk, @gcfk, @gclfk, @ro, @lmd, @lmb)", cnnFI, transaction))
        {
          sc.Parameters.AddWithValue("@gvfk", guid);
          sc.Parameters.AddWithValue("@gvrk", guid2);
          sc.Parameters.AddWithValue("@gcfk", qal2ValidPeriodTypeConfigFieldKey);
          sc.Parameters.AddWithValue("@gclfk", vpt);
          sc.Parameters.AddWithValue("@ro", true);
          sc.Parameters.AddWithValue("@lmd", nowOffset);
          sc.Parameters.AddWithValue("@lmb", "2F636E2F-C03D-EA11-A835-005056A7F76B");   //MAFHGuid

          sc.ExecuteNonQuery();
        }
      }
    }

    public static void CreateReadings(SqlConnection cnnQal, Guid genericValueFieldKey, int Q3_MID, int Q3_ID, DateTimeOffset nowOffset, 
      SqlTransaction transaction, SqlConnection cnnFI)
    {
      List<Reading> rList = new List<Reading>();

      string readingInspectionKey = "Reading";
      string readingConfigTableKey = "41E973F4-E8BA-4A91-8E0E-9BBB6A3F830F";
      //string readingGUID = "7929189E-86D2-4295-987E-BA106B43C4D9";
      string measureDateTime = "ABBA73BC-9801-4821-97A0-927B47BD5AFE";
      string zero = "D0183657-A131-49B9-96BA-699BEB86046D";
      string zeroMeasuring = "BCD3A927-822D-411F-98BE-C9E69FFDCE26";
      string span = "A38C61FF-CF09-4EC1-9CEB-C9DD9B313B9D";
      string spanMeasuring = "BAA66A8A-6830-4526-90D2-8861BB3A398A";
      string spanGas = "EF6869F6-B6E2-4D90-8E8C-4666B608C682";
      string spanGasMeasuring = "A408FC2C-43EC-47ED-90F1-9F106F118AF8";
      string zeroGas = "F1AF840A-E87D-4BC3-AF82-8A36FCD29E86";
      string zeroGasMeasuring = "DA42CAD2-D470-451E-9214-3D2F23A776BA";
      string unit = "";

      //QAL1_MID id's where there are multiple Units 
      if (Q3_MID != 233 || Q3_MID != 343 || Q3_MID != 364 || Q3_MID != 677 || Q3_MID != 998)
      {
        using (SqlCommand sc = new SqlCommand("SELECT * FROM [QAL1] WHERE QAL1_MID = @Q3_MID", cnnQal))
        {
          sc.Parameters.AddWithValue("@Q3_MID", Q3_MID);

          using (SqlDataReader dr = sc.ExecuteReader())
          {
            while (dr.Read())
            {
              if (!Convert.ToString(dr["QAL1_Unit"]).Equals(""))
              {
                unit = Convert.ToString(dr["QAL1_Unit"]);
              }
            }
          }
        }

        //Trim unit-string
        unit.Trim();

        if (unit.Equals("%") || unit.Equals("mg/m3") || unit.Equals("ppm"))
        {
          using (SqlCommand sc = new SqlCommand("SELECT * FROM [Reading] WHERE R_QAL3_ID = @Q3_ID", cnnQal))
          {
            sc.Parameters.AddWithValue("@Q3_ID", Q3_ID);

            using (SqlDataReader dr = sc.ExecuteReader())
            {
              while (dr.Read())
              {
                Reading r = new Reading();

                r.R_ID = Convert.ToInt32(dr["R_ID"]);
                r.R_QAL3_ID = Convert.ToInt32(dr["R_QAL3_ID"]);
                r.R_Dato = Convert.ToDateTime(dr["R_Dato"]);
                r.R_Zero = Convert.ToDecimal(dr["R_Zero"]);
                r.R_ZeroGas = Convert.ToDecimal(dr["R_ZeroGas"]);
                r.R_Span = Convert.ToDecimal(dr["R_Span"]);
                r.R_SpanGas = Convert.ToDecimal(dr["R_SpanGas"]);
                r.R_INI = Convert.ToString(dr["R_INI"]);
                r.R_UID = Convert.ToInt32(dr["R_UID"]);
                r.R_SidstRettet = Convert.ToDateTime(dr["R_SidstRettet"]);
                r.R_Remarks = Convert.ToString(dr["R_Remarks"]);

                rList.Add(r);
              }
            }
          }

          //For each Reading make 1 insertions in T_GenericValueRow tabel, and 9 insertions in the T_GenericValueField tabel
          foreach (Reading r in rList)
          {
            Guid guid = Guid.NewGuid();

            //Get VersionID from T_GenericConfigTable
            int versionID = GetVersionId(readingInspectionKey, transaction, cnnFI);

            InsertIntoGenericValueRow(guid, readingConfigTableKey, genericValueFieldKey, nowOffset, versionID, transaction, cnnFI);

            InsertReadingDateTime(guid, measureDateTime, r.R_Dato, nowOffset, transaction, cnnFI);      //R_Dato
            InsertReadingWithValue(guid, zero, r.R_Zero, nowOffset, transaction, cnnFI);                //R_Zero
            InsertReadingWithoutValue(guid, zeroMeasuring, unit, nowOffset, transaction, cnnFI);        //ZeroMeasuring
            InsertReadingWithValue(guid, span, r.R_Span, nowOffset, transaction, cnnFI);                //R_Span
            InsertReadingWithoutValue(guid, spanMeasuring, unit, nowOffset, transaction, cnnFI);        //SpanMeasuring
            InsertReadingWithValue(guid, spanGas, r.R_SpanGas, nowOffset, transaction, cnnFI);          //R_SpanGas
            InsertReadingWithoutValue(guid, spanGasMeasuring, unit, nowOffset, transaction, cnnFI);     //SpanGasMeasuring
            InsertReadingWithValue(guid, zeroGas, r.R_ZeroGas, nowOffset, transaction, cnnFI);          //R_ZeroGas
            InsertReadingWithoutValue(guid, zeroGasMeasuring, unit, nowOffset, transaction, cnnFI);     //ZeroGasMeasuring
          }
        }
      }
    }

    public static void InsertReadingWithValue(Guid guid, string gcfk, decimal value, DateTimeOffset nowOffset, SqlTransaction transaction, SqlConnection cnnFI)
    {
      Guid guid2 = Guid.NewGuid();

      using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO [T_GenericValueField] (GenericValueFieldKey, GenericValueRowKey, GenericConfigFieldKey, " +
        "ValueDecimal, OriginalValue, ReadOnly, LastModifiedDate, LastModifiedBy) " +
        "VALUES (@gvfk, @gvrk, @gcfk, @vd, @ov, @ro, @lmd, @lmb)", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@gvfk", guid2);
        sc.Parameters.AddWithValue("@gvrk", guid);
        sc.Parameters.AddWithValue("@gcfk", gcfk);
        if (value == 10000000)
        {
          sc.Parameters.AddWithValue("@vd", DBNull.Value);
          sc.Parameters.AddWithValue("@ov", DBNull.Value);
        } else
        {
          sc.Parameters.AddWithValue("@vd", value);
          sc.Parameters.AddWithValue("@ov", value);
        }
        sc.Parameters.AddWithValue("@ro", true);
        sc.Parameters.AddWithValue("@lmd", nowOffset);
        sc.Parameters.AddWithValue("@lmb", "2F636E2F-C03D-EA11-A835-005056A7F76B"); //MAFHGuid

        sc.ExecuteNonQuery();
      }
    }

    public static void InsertReadingWithoutValue(Guid guid, string gcfk, string unit, DateTimeOffset nowOffset, SqlTransaction transaction, SqlConnection cnnFI)
    {
      Guid guid2 = Guid.NewGuid();

      using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO [T_GenericValueField] (GenericValueFieldKey, GenericValueRowKey, GenericConfigFieldKey, GenericConfigLookupFieldKey, " +
        "ReadOnly, LastModifiedDate, LastModifiedBy) " +
        "VALUES (@gvfk, @gvrk, @gcfk, @gclfk, @ro, @lmd, @lmb)", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@gvfk", guid2);
        sc.Parameters.AddWithValue("@gvrk", guid);
        sc.Parameters.AddWithValue("@gcfk", gcfk);
        sc.Parameters.AddWithValue("@gclfk", GetUnitGenericConfigLookupFieldKeyAsObject(unit));
        sc.Parameters.AddWithValue("@ro", true);
        sc.Parameters.AddWithValue("@lmd", nowOffset);
        sc.Parameters.AddWithValue("@lmb", "2F636E2F-C03D-EA11-A835-005056A7F76B"); //MAFHGuid

        sc.ExecuteNonQuery();
      }
    }

    public static void InsertReadingDateTime(Guid guid, string gcfk, DateTimeOffset value, DateTimeOffset nowOffset, SqlTransaction transaction, SqlConnection cnnFI)
    {
      Guid guid2 = Guid.NewGuid();

      using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO [T_GenericValueField] (GenericValueFieldKey, GenericValueRowKey, GenericConfigFieldKey, " +
        "ValueDateTimeOffset, OriginalValue, ReadOnly, LastModifiedDate, LastModifiedBy) " +
        "VALUES (@gvfk, @gvrk, @gcfk, @vdto, @ov, @ro, @lmd, @lmb)", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@gvfk", guid2);
        sc.Parameters.AddWithValue("@gvrk", guid);
        sc.Parameters.AddWithValue("@gcfk", gcfk);
        sc.Parameters.AddWithValue("@vdto", value);
        sc.Parameters.AddWithValue("@ov", value);
        sc.Parameters.AddWithValue("@ro", true);
        sc.Parameters.AddWithValue("@lmd", nowOffset);
        sc.Parameters.AddWithValue("@lmb", "2F636E2F-C03D-EA11-A835-005056A7F76B"); //MAFHGuid

        sc.ExecuteNonQuery();
      }
    }

    public static object GetUnitGenericConfigLookupFieldKeyAsObject(string unit)
    {
      Guid? g = GetUnitGenericConfigLookupFieldKey(unit);
      return g.HasValue ? (object)g.Value : DBNull.Value;
    }

    public static Guid? GetUnitGenericConfigLookupFieldKey(string unit)
    {
      Guid? guid;

      if (unit.Equals("%"))
      {
       guid = Guid.Parse("B7DD46AC-A174-49AF-AF72-C4C54C2B6487");
      }
      else if (unit.Equals("mg/m3"))
      {
        guid = Guid.Parse("048C0EAD-DFD3-4BA7-B0F2-CCDD8696EF79");
      }
      else if (unit.Equals("ppm"))
      {
        guid = Guid.Parse("0586FC63-D795-49EB-940C-27F0A3346DDA");
      }
      else
      {
       guid = null;
      }

      return guid;
    }
    #endregion CreateInspections

    #region DeleteInspections
    //"Delete Inspections" - button - Takes around 7 minutes and 50 seconds
    private void button6_Click(object sender, EventArgs e)
    {
      SqlConnection cnnFI;
      SqlConnection cnnFI2;

      cnnFI = new SqlConnection(cStringToFI);
      cnnFI2 = new SqlConnection(cStringToFI);

      cnnFI.Open();
      cnnFI2.Open();

      //DeleteInspections(cnnFI, cnnFI2);   //Delete all QAL1, QAL2, QAL3 and AST in the FI database
      System.Diagnostics.Debug.WriteLine("--- DELETE INSPECTIONS DONE ---");
    }

    public static void DeleteInspections(SqlConnection cnnFI, SqlConnection cnnFI2)
    {
      using (cnnFI)
      {
        using (SqlCommand sc = new SqlCommand("DELETE FROM T_Inspection WHERE CreatedByUserID = 770", cnnFI))
        {
          sc.ExecuteNonQuery();
        }

        using (SqlCommand sc = new SqlCommand("DELETE FROM T_GenericValueField WHERE LastModifiedBy = '2F636E2F-C03D-EA11-A835-005056A7F76B'", cnnFI))
        {
          sc.ExecuteNonQuery();
        }

        using (SqlCommand sc = new SqlCommand("DELETE FROM T_GenericValueRow WHERE LastModifiedBy = '2F636E2F-C03D-EA11-A835-005056A7F76B'", cnnFI))
        {
          sc.ExecuteNonQuery();
        }

        using (SqlCommand sc = new SqlCommand("DELETE FROM T_Inspection_InspectionTypes WHERE InspectionTypeID in (99,100,101) " +
          "AND InspectionGUID not in ('A59247D0-8F37-4D9E-AD67-6E9414AD3D10', 'C77A477D-5D2C-4B62-AEE1-988ECAB5217A', '1E1E4BFD-9E1F-4186-8489-853FDC8EE722')", cnnFI))
        {
          sc.ExecuteNonQuery();
        }

        using (SqlCommand sc = new SqlCommand("DELETE FROM T_JobInspectionType WHERE InspectionTypeId in (99,100,101)", cnnFI))
        {
          sc.ExecuteNonQuery();
        }

        DeleteJobs(cnnFI2);
      }
    }

    public static void DeleteJobs(SqlConnection cnnFI2)
    {
      using (SqlCommand sc = new SqlCommand("DELETE FROM T_Jobs WHERE CreatedByUserID = 770", cnnFI2))
      {
        sc.CommandTimeout = 0;
        sc.ExecuteNonQuery();
      }
    }
    #endregion DeleteInspections

    #region CreateReports 
    //Chance to specific data handling
    private void button4_Click(object sender, EventArgs e)
    {

      SqlConnection cnnQal;
      SqlConnection cnnQal2;
      SqlConnection cnnAST;
      SqlConnection cnnFI1;
      SqlConnection cnnFI2;
      SqlConnection cnnFI3;

      cnnQal = new SqlConnection(cStringToQal);
      cnnQal2 = new SqlConnection(cStringToQal);
      cnnAST = new SqlConnection(cStringToQal);
      cnnFI1 = new SqlConnection(cStringToFI);
      cnnFI2 = new SqlConnection(cStringToFI);
      cnnFI3 = new SqlConnection(cStringToFI);

      cnnQal.Open();
      cnnQal2.Open();
      cnnAST.Open();
      cnnFI1.Open();
      cnnFI2.Open();

      //CreateQAL1Reports(cnnQal, cnnFI1, pID);      //Create Reports from QAL1 table
      //CreateQAL2Reports(cnnQal2, cnnFI2, pID);     //Create Reports from QAL2 table
      //CreateASTReports(cnnAST, cnnFI3, pID);       //Create Reports from AST table
      System.Diagnostics.Debug.WriteLine("--- CREATE REPORTS DONE ---");
    }

    public static void CreateQAL1Reports(SqlConnection cnnQal, int pID, DateTimeOffset nowOffset, SqlTransaction transaction, SqlConnection cnnFI)
    {
      //Position P_ID: 1 is the parent-position for 'QAL1 rapport dkTEKNIK.doc' - Demo
      //Position P_ID: 23 is the parent-position for 'TXA_Test_Report_2.txt' - Oles Testområde 

      List<Reports> rList = new List<Reports>();

      string sql = @"
      SELECT QAL1_ID, QAL1_DocName, QAL1_DocLength, QAL1_DocContentType, QAL1_DocContent FROM QAL1 
      WHERE QAL1_DocContent IS NOT NULL AND QAL1_MID in 
      (
        SELECT M_ID FROM Meter WHERE M_PID in 
        (
          SELECT P_ID FROM Position WHERE P_ParentID = 
          (
            SELECT P_ID FROM Position WHERE P_ID = @pID
          ) AND P_ID NOT IN (240, 264, 320, 323, 343, 344)
        )
      )";

      //string sql = "SELECT QAL1_ID, QAL1_DocName, QAL1_DocLength, QAL1_DocContentType, QAL1_DocContent FROM [QAL1] " +
      //  "WHERE QAL1_DocContent is not null AND QAL1_MID IN " +
      //  "(" + string.Join(",", amagerMeterList.Select(r => r.ToString())) + ") AND QAL1_ID not in (1, 1049)" + 
      //  "ORDER BY QAL1_ID";

      using (SqlCommand s = new SqlCommand(sql, cnnQal))
      {
        s.Parameters.AddWithValue("@pID", pID);

        using (SqlDataReader dr = s.ExecuteReader())
        {
          while (dr.Read())
          {
            Reports r = new Reports();

            r.Q_ID = Convert.ToInt32(dr["QAL1_ID"]);
            r.DocName = Convert.ToString(dr["QAL1_DocName"]);
            r.DocLength = Convert.ToInt32(dr["QAL1_DocLength"]);
            r.DocContentType = Convert.ToString(dr["QAL1_DocContentType"]);
            object binaryData = dr["QAL1_DocContent"];
            r.DocContent = (byte[])binaryData;

            string res = r.DocName.ToLower().Substring(r.DocName.Length - 4);
            if (!res.Equals(".pdf"))
            {
              r.DocName = r.DocName + ".pdf";
            }

            rList.Add(r);
          }
        }
      }
      

      foreach (Reports r in rList)
      {
        Guid guid = Guid.NewGuid();
        Guid guid2 = Guid.NewGuid();
        Inspection i = new Inspection();
        string newID = "";
        string newName = r.DocName.Substring(0, r.DocName.Length - 4);
        if (newName.Contains("?"))
        {
          newName = newName.Replace("?", "");
          newName = newName.Replace(" ", "");
        }
        if (newName.Contains("."))
        {
          int dot = newName.IndexOf(".");
          newName = newName.Substring(0, dot);
        }

        newID = "QAL1_ID:" + r.Q_ID;

        using (SqlCommand sc = CreateCommandAndEnlistTransaction("SELECT * FROM T_Inspection WHERE InternalComment = @newid", cnnFI, transaction))
        {
          sc.Parameters.AddWithValue("@newid", newID);

          using (SqlDataReader dr = sc.ExecuteReader())
          {
            while (dr.Read())
            {
              i.InspectionGUID = (Guid)dr["InspectionGUID"];
              i.ObjectID = Convert.ToInt32(dr["ObjectID"]);
              i.InspectionDateTime = Convert.ToDateTime(dr["InspectionDateTime"]);
              i.LastModifiedDateTime = Convert.ToDateTime(dr["LastModifiedDateTime"]);
              i.JobId = Convert.ToInt32(dr["JobId"]);
            }
          }
        }

        //string test = BitConverter.ToString(r.DocContent);(

        byte[] bytes;
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, r.DocContent);
        bytes = ms.ToArray();
        //File.WriteAllBytes("C:\\Users\\mafh\\desktop\\QAL\\" + r.DocName.Replace("?", ""), bytes);
        //System.Diagnostics.Debug.WriteLine("Bytes: " + test);

        //System.Diagnostics.Debug.WriteLine("GUID: " + i.InspectionGUID);
        //System.Diagnostics.Debug.WriteLine("ObjectID: " + i.ObjectID);
        //System.Diagnostics.Debug.WriteLine("InspectionDateTime: " + i.InspectionDateTime);
        //System.Diagnostics.Debug.WriteLine("LastModifiedDateTime: " + i.LastModifiedDateTime);
        //System.Diagnostics.Debug.WriteLine("JobId: " + i.JobId);
        //System.Diagnostics.Debug.WriteLine("Bytes: " + bytes);

        using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO T_Report (ReportGUID, ObjectID, InspectionGUID, CreatedByUserID, CreatedDateTime, ReportStatusID, JobId) " +
          "VALUES (@reportguid, @objectid, @inspectionguid, @cbui, @createddatetime, @reportstatusid, @jobid)", cnnFI, transaction))
        {
          sc.Parameters.AddWithValue("@reportguid", guid);
          sc.Parameters.AddWithValue("@objectid", i.ObjectID);
          sc.Parameters.AddWithValue("@inspectionguid", i.InspectionGUID);
          sc.Parameters.AddWithValue("@cbui", 770);
          sc.Parameters.AddWithValue("@createddatetime", nowOffset);
          sc.Parameters.AddWithValue("@reportstatusid", 4);
          sc.Parameters.AddWithValue("@jobid", i.JobId);

          sc.ExecuteNonQuery();
        }

        using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO T_ReportDocument (ReportDocumentKey, ReportKey, Title, FileTypeId, ContentType, ContentLength, " +
          "OriginalFilename, DocumentFile, CreateModifyInfoKey) " +
          "VALUES (@rdk, @reportkey, @title, @fti, @ct, @cl, @of, @df, @cmik)", cnnFI, transaction))
        {
          sc.Parameters.AddWithValue("@rdk", guid2);
          sc.Parameters.AddWithValue("@reportkey", guid);
          sc.Parameters.AddWithValue("@title", newName);
          sc.Parameters.AddWithValue("@fti", 23);
          sc.Parameters.AddWithValue("@ct", r.DocContentType);
          sc.Parameters.AddWithValue("@cl", r.DocLength);
          sc.Parameters.AddWithValue("@of", r.DocName);
          sc.Parameters.AddWithValue("@df", bytes);
          sc.Parameters.AddWithValue("@cmik", "E21CD278-1B41-49F4-BD0D-590B0CC6800C");

          sc.ExecuteNonQuery();
        }
      }
      
    }

    public static void CreateQAL2Reports(SqlConnection cnnQal2, int pID, DateTimeOffset nowOffset, SqlTransaction transaction, SqlConnection cnnFI)
    {
      //POSITION P_ID: 43 er 'KomGodtIGang.doc'
      List<Reports> rList = new List<Reports>();

      string sql = @"
      SELECT QAL2_ID, QAL2_DocName, QAL2_DocLength, QAL2_DocContentType, QAL2_DocContent FROM QAL2 
      WHERE QAL2_DocContent IS NOT NULL AND QAL2_MID in 
      (
        SELECT M_ID FROM Meter WHERE M_PID in 
        (
          SELECT P_ID FROM Position WHERE P_ParentID = 
          (
            SELECT P_ID FROM Position WHERE P_ID = @pID
          ) AND P_ID NOT IN (240, 264, 320, 323, 343, 344)
        )
      )";

      //string sql = "SELECT QAL2_ID, QAL2_DocName, QAL2_DocLength, QAL2_DocContentType, QAL2_DocContent FROM [QAL2] " +
      //  "WHERE QAL2_DocContent is not null AND QAL2_MID IN " +
      //  "(" + string.Join(",", amagerMeterList.Select(r => r.ToString())) + ") AND QAL2_ID != 92" + 
      //  "ORDER BY QAL2_ID";

      using (SqlCommand s = new SqlCommand(sql, cnnQal2))
      {
        s.Parameters.AddWithValue("@pID", pID);

        using (SqlDataReader dr = s.ExecuteReader())
        {
          while (dr.Read())
          {
            Reports r = new Reports();

            r.Q_ID = Convert.ToInt32(dr["QAL2_ID"]);
            r.DocName = Convert.ToString(dr["QAL2_DocName"]);
            r.DocLength = Convert.ToInt32(dr["QAL2_DocLength"]);
            r.DocContentType = Convert.ToString(dr["QAL2_DocContentType"]);
            object binaryData = dr["QAL2_DocContent"];
            r.DocContent = (byte[])binaryData;

            string res = r.DocName.Substring(r.DocName.Length - 4);
            if (!res.Equals(".pdf"))
            {
              r.DocName = r.DocName + ".pdf";
            }

            rList.Add(r);
          }
        }
      }
      

      foreach (Reports r in rList)
      {
        Guid guid = Guid.NewGuid();
        Guid guid2 = Guid.NewGuid();
        Inspection i = new Inspection();
        string newID = "";
        string newName = r.DocName.Substring(0, r.DocName.Length - 4);
        if (newName.Contains("?"))
        {
          newName = newName.Replace("?", "");
          newName = newName.Replace(" ", "");
        }
        if (newName.Contains("."))
        {
          int dot = newName.IndexOf(".");
          newName = newName.Substring(0, dot);
        }

        newID = "QAL2_ID:" + r.Q_ID;

        using (SqlCommand sc = CreateCommandAndEnlistTransaction("SELECT * FROM T_Inspection WHERE InternalComment = @newid", cnnFI, transaction))
        {
          sc.Parameters.AddWithValue("@newid", newID);

          using (SqlDataReader dr = sc.ExecuteReader())
          {
            while (dr.Read())
            {
              i.InspectionGUID = (Guid)dr["InspectionGUID"];
              i.ObjectID = Convert.ToInt32(dr["ObjectID"]);
              i.InspectionDateTime = Convert.ToDateTime(dr["InspectionDateTime"]);
              i.LastModifiedDateTime = Convert.ToDateTime(dr["LastModifiedDateTime"]);
              i.JobId = Convert.ToInt32(dr["JobId"]);
            }
          }
        }

        //string test = BitConverter.ToString(r.DocContent);(

        byte[] bytes;
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, r.DocContent);
        bytes = ms.ToArray();
        //File.WriteAllBytes("C:\\Users\\mafh\\desktop\\QAL\\" + r.DocName.Replace("?", ""), bytes);
        //System.Diagnostics.Debug.WriteLine("Bytes: " + test);

        //System.Diagnostics.Debug.WriteLine("GUID: " + i.InspectionGUID);
        //System.Diagnostics.Debug.WriteLine("ObjectID: " + i.ObjectID);
        //System.Diagnostics.Debug.WriteLine("InspectionDateTime: " + i.InspectionDateTime);
        //System.Diagnostics.Debug.WriteLine("LastModifiedDateTime: " + i.LastModifiedDateTime);
        //System.Diagnostics.Debug.WriteLine("JobId: " + i.JobId);
        //System.Diagnostics.Debug.WriteLine("Bytes: " + bytes);

        using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO T_Report (ReportGUID, ObjectID, InspectionGUID, CreatedByUserID, CreatedDateTime, ReportStatusID, JobId) " +
          "VALUES (@reportguid, @objectid, @inspectionguid, @cbui, @createddatetime, @reportstatusid, @jobid)", cnnFI, transaction))
        {
          sc.Parameters.AddWithValue("@reportguid", guid);
          sc.Parameters.AddWithValue("@objectid", i.ObjectID);
          sc.Parameters.AddWithValue("@inspectionguid", i.InspectionGUID);
          sc.Parameters.AddWithValue("@cbui", 770);
          sc.Parameters.AddWithValue("@createddatetime", nowOffset);
          sc.Parameters.AddWithValue("@reportstatusid", 4);
          sc.Parameters.AddWithValue("@jobid", i.JobId);

          sc.ExecuteNonQuery();
        }

        using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO T_ReportDocument (ReportDocumentKey, ReportKey, Title, FileTypeId, ContentType, ContentLength, " +
          "OriginalFilename, DocumentFile, CreateModifyInfoKey) " +
          "VALUES (@rdk, @reportkey, @title, @fti, @ct, @cl, @of, @df, @cmik)", cnnFI, transaction))
        {
          sc.Parameters.AddWithValue("@rdk", guid2);
          sc.Parameters.AddWithValue("@reportkey", guid);
          sc.Parameters.AddWithValue("@title", newName);
          sc.Parameters.AddWithValue("@fti", 23);
          sc.Parameters.AddWithValue("@ct", r.DocContentType);
          sc.Parameters.AddWithValue("@cl", r.DocLength);
          sc.Parameters.AddWithValue("@of", r.DocName);
          sc.Parameters.AddWithValue("@df", bytes);
          sc.Parameters.AddWithValue("@cmik", "E21CD278-1B41-49F4-BD0D-590B0CC6800C");

          sc.ExecuteNonQuery();
        }
      }
      
    }

    public static void CreateASTReports(SqlConnection cnnAST, int pID, DateTimeOffset nowOffset, SqlTransaction transaction, SqlConnection cnnFI)
    {
      List<Reports> rList = new List<Reports>();

      string sql = @"
      SELECT AST_ID, AST_DocName, AST_DocLength, AST_DocContentType, AST_DocContent FROM AST 
      WHERE AST_DocContent IS NOT NULL AND AST_MID in 
      (
        SELECT M_ID FROM Meter WHERE M_PID in 
        (
          SELECT P_ID FROM Position WHERE P_ParentID = 
          (
            SELECT P_ID FROM Position WHERE P_ID = @pID
          ) AND P_ID NOT IN (240, 264, 320, 323, 343, 344)
        )
      )";

      //string sql = "SELECT AST_DocName, AST_DocLength, AST_DocContentType, AST_DocContent FROM [AST] " +
      //  "WHERE AST_DocContent is not null AND AST_MID IN " +
      //  "(" + string.Join(",", amagerMeterList.Select(r => r.ToString())) + ") " + 
      //  "ORDER BY AST_ID";

      using (SqlCommand s = new SqlCommand(sql, cnnAST))
      {
        s.Parameters.AddWithValue("@pID", pID);

        using (SqlDataReader dr = s.ExecuteReader())
        {
          while (dr.Read())
          {
            Reports r = new Reports();

            r.Q_ID = Convert.ToInt32(dr["AST_ID"]);
            r.DocName = Convert.ToString(dr["AST_DocName"]);
            r.DocLength = Convert.ToInt32(dr["AST_DocLength"]);
            r.DocContentType = Convert.ToString(dr["AST_DocContentType"]);
            object binaryData = dr["AST_DocContent"];
            r.DocContent = (byte[])binaryData;

            string res = r.DocName.Substring(r.DocName.Length - 4);
            if (!res.Equals(".pdf"))
            {
              r.DocName = r.DocName + ".pdf";
            }

            rList.Add(r);
          }
        }
      }
      
      foreach (Reports r in rList)
      {
        Guid guid = Guid.NewGuid();
        Guid guid2 = Guid.NewGuid();
        Inspection i = new Inspection();
        string newID = "";
        string newName = r.DocName.Substring(0, r.DocName.Length - 4);
        if (newName.Contains("?"))
        {
          newName = newName.Replace("?", "");
          newName = newName.Replace(" ", "");
        }
        if (newName.Contains("."))
        {
          int dot = newName.IndexOf(".");
          newName = newName.Substring(0, dot);
        }

        newID = "AST_ID:" + r.Q_ID;

        using (SqlCommand sc = CreateCommandAndEnlistTransaction("SELECT * FROM T_Inspection WHERE InternalComment = @newid", cnnFI, transaction))
        {
          sc.Parameters.AddWithValue("@newid", newID);

          using (SqlDataReader dr = sc.ExecuteReader())
          {
            while (dr.Read())
            {
              i.InspectionGUID = (Guid)dr["InspectionGUID"];
              i.ObjectID = Convert.ToInt32(dr["ObjectID"]);
              i.InspectionDateTime = Convert.ToDateTime(dr["InspectionDateTime"]);
              i.LastModifiedDateTime = Convert.ToDateTime(dr["LastModifiedDateTime"]);
              i.JobId = Convert.ToInt32(dr["JobId"]);
            }
          }
        }

        //string test = BitConverter.ToString(r.DocContent);(

        byte[] bytes;
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, r.DocContent);
        bytes = ms.ToArray();
        //File.WriteAllBytes("C:\\Users\\mafh\\desktop\\QAL\\" + r.DocName.Replace("?", ""), bytes);
        //System.Diagnostics.Debug.WriteLine("Bytes: " + test);

        //System.Diagnostics.Debug.WriteLine("GUID: " + i.InspectionGUID);
        //System.Diagnostics.Debug.WriteLine("ObjectID: " + i.ObjectID);
        //System.Diagnostics.Debug.WriteLine("InspectionDateTime: " + i.InspectionDateTime);
        //System.Diagnostics.Debug.WriteLine("LastModifiedDateTime: " + i.LastModifiedDateTime);
        //System.Diagnostics.Debug.WriteLine("JobId: " + i.JobId);
        //System.Diagnostics.Debug.WriteLine("Bytes: " + bytes);

        using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO T_Report (ReportGUID, ObjectID, InspectionGUID, CreatedByUserID, CreatedDateTime, ReportStatusID, JobId) " +
          "VALUES (@reportguid, @objectid, @inspectionguid, @cbui, @createddatetime, @reportstatusid, @jobid)", cnnFI, transaction))
        {
          sc.Parameters.AddWithValue("@reportguid", guid);
          sc.Parameters.AddWithValue("@objectid", i.ObjectID);
          sc.Parameters.AddWithValue("@inspectionguid", i.InspectionGUID);
          sc.Parameters.AddWithValue("@cbui", 770);
          sc.Parameters.AddWithValue("@createddatetime", nowOffset);
          sc.Parameters.AddWithValue("@reportstatusid", 4);
          sc.Parameters.AddWithValue("@jobid", i.JobId);

          sc.ExecuteNonQuery();
        }

        using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO T_ReportDocument (ReportDocumentKey, ReportKey, Title, FileTypeId, ContentType, ContentLength, " +
          "OriginalFilename, DocumentFile, CreateModifyInfoKey) " +
          "VALUES (@rdk, @reportkey, @title, @fti, @ct, @cl, @of, @df, @cmik)", cnnFI, transaction))
        {
          sc.Parameters.AddWithValue("@rdk", guid2);
          sc.Parameters.AddWithValue("@reportkey", guid);
          sc.Parameters.AddWithValue("@title", newName);
          sc.Parameters.AddWithValue("@fti", 23);
          sc.Parameters.AddWithValue("@ct", r.DocContentType);
          sc.Parameters.AddWithValue("@cl", r.DocLength);
          sc.Parameters.AddWithValue("@of", r.DocName);
          sc.Parameters.AddWithValue("@df", bytes);
          sc.Parameters.AddWithValue("@cmik", "E21CD278-1B41-49F4-BD0D-590B0CC6800C");

          sc.ExecuteNonQuery();
        }
      }
      
    }
    #endregion CreateReports

    #region DeleteReports
    private void button9_Click(object sender, EventArgs e)
    {      
      SqlConnection cnnFI;

      cnnFI = new SqlConnection(cStringToFI);

      cnnFI.Open();

      //DeleteReports(cnnFI);
      System.Diagnostics.Debug.WriteLine("--- DELETE REPORTS DONE ---");
    }

    public static void DeleteReports(SqlConnection cnnFI)
    {
      using (cnnFI)
      {
        using (SqlCommand sc = new SqlCommand("DELETE FROM T_ReportDocument WHERE CreateModifyInfoKey = 'E21CD278-1B41-49F4-BD0D-590B0CC6800C'", cnnFI))
        {
          sc.ExecuteNonQuery();
        }

        using (SqlCommand sc = new SqlCommand("DELETE FROM T_Report WHERE CreatedByUserID = 770", cnnFI))
        {
          sc.ExecuteNonQuery();
        }

      }
    }
    #endregion DeleteReports

    #region CreateCalendar
    //DOES NOT WORK YET!
    //Chance to specific data handling
    private void button7_Click(object sender, EventArgs e)
    {

      SqlConnection cnnQal;
      SqlConnection cnnFI;
      cnnQal = new SqlConnection(cStringToQal);
      cnnFI = new SqlConnection(cStringToFI);
      cnnQal.Open();
      cnnFI.Open();

      //InsertIntoCalendar(cnnFI, pID);
      System.Diagnostics.Debug.WriteLine("--- CREATE CALENDAR COMPLETED ---");
    }

    public static void InsertIntoCalendar(int pID, Guid cmiGuid, SqlTransaction transaction, SqlConnection cnnFI, Dictionary<int, int> mDict)
    {
      List<CalendarTemp> calendarList= new List<CalendarTemp>();
      string internalDescription = "P_ID:" + pID;

      string sql = $@"
        SELECT ObjectID FROM T_Object WHERE ParentObjectID in 
        (
          {string.Join(",", mDict.Values)}
        )";

      //Retrieve all the ParentObjectID's to see the belonging QAL1, QAL2 and QAL3.
      using (SqlCommand sc = CreateCommandAndEnlistTransaction(sql, cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@internalDescription", internalDescription);

        using (SqlDataReader dr = sc.ExecuteReader())
        {
          while (dr.Read())
          {
            CalendarTemp ct = new CalendarTemp();

            ct.ObjectID = Convert.ToInt32(dr["ObjectID"]);

            calendarList.Add(ct);
          }
        }
      }

      //Get all the belonging QAL1, QAL2 and QAL3 to insert into the T_Calendar table
      foreach (CalendarTemp ct in calendarList)
      {
        List<Inspection> inspectionList = new List<Inspection>();

        using (SqlCommand sc = CreateCommandAndEnlistTransaction("SELECT i.InspectionGUID, i.ObjectID, i.InspectionDateTime, i.InternalComment, iit.InspectionTypeID FROM T_Inspection AS i " +
          "INNER JOIN T_Inspection_InspectionTypes AS iit on i.InspectionGUID = iit.InspectionGUID " +
          "WHERE ObjectID = @cid", cnnFI, transaction))
        {
          sc.Parameters.AddWithValue("@cid", ct.ObjectID);

          using (SqlDataReader dr = sc.ExecuteReader())
          {
            while (dr.Read())
            {
              Inspection inspection = new Inspection();

              inspection.InspectionGUID = (Guid)dr["InspectionGUID"];
              inspection.ObjectID = Convert.ToInt32(dr["ObjectID"]);
              inspection.InspectionDateTime = Convert.ToDateTime(dr["InspectionDateTime"]);
              inspection.InternalComment = Convert.ToString(dr["InternalComment"]);
              inspection.InspectionTypeID = Convert.ToInt32(dr["InspectionTypeID"]);

              inspectionList.Add(inspection);
            }
          }
        }

        //Insert each QAL1, QAL2 and QAL3 in T_Calendar and T_InspectionCalendar table
        foreach (Inspection inspection in inspectionList)
        {
          Guid guid = Guid.NewGuid();
          Guid guid2 = Guid.NewGuid();

          using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO T_Calendar (CalendarGUID, ObjectID, CalendarEntryTypeID, DateTime, " +
            "Done, CreatedByInspectionGUID, ModifiedByInspectionGUID, ModifiedOriginalDateTime, ModifiedOriginalDone) " +
            "VALUES (@1, @2, @3, @4, @6, @7, @8, @9, @10)", cnnFI, transaction))
          {
            sc.Parameters.AddWithValue("@1", guid);
            sc.Parameters.AddWithValue("@2", ct.ObjectID);
            if (inspection.InspectionTypeID == 99)
            {
              sc.Parameters.AddWithValue("@3", 76);
            } else if (inspection.InspectionTypeID == 100)
            {
              sc.Parameters.AddWithValue("@3", 77);
            } else if (inspection.InspectionTypeID == 101)
            {
              sc.Parameters.AddWithValue("@3", 78);
            }
            sc.Parameters.AddWithValue("@4", inspection.InspectionDateTime);
            sc.Parameters.AddWithValue("@6", 1);
            sc.Parameters.AddWithValue("@7", inspection.InspectionGUID);
            sc.Parameters.AddWithValue("@8", inspection.InspectionGUID);
            sc.Parameters.AddWithValue("@9", DBNull.Value);
            sc.Parameters.AddWithValue("@10", DBNull.Value);

            sc.ExecuteNonQuery();
          }

          using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO T_InspectionCalendar (InspectionCalendarGuid, InspectionGuid, CalendarEntryTypeId, " +
            "InspectionDatePrevious, CalendarGuidPrevious, InspectionDateNext, CalendarGuidNext, CreateModifyInfoGuid) " +
            "VALUES (@1, @2, @3, @4, @5, @6, @7, @8)", cnnFI, transaction))
          {
            sc.Parameters.AddWithValue("@1", guid2);
            sc.Parameters.AddWithValue("@2", inspection.InspectionGUID);
            if (inspection.InspectionTypeID == 99)
            {
              sc.Parameters.AddWithValue("@3", 76);
            }
            else if (inspection.InspectionTypeID == 100)
            {
              sc.Parameters.AddWithValue("@3", 77);
            }
            else if (inspection.InspectionTypeID == 101)
            {
              sc.Parameters.AddWithValue("@3", 78);
            }
            sc.Parameters.AddWithValue("@4", inspection.InspectionDateTime);
            sc.Parameters.AddWithValue("@5", guid);
            sc.Parameters.AddWithValue("@6", DBNull.Value);
            sc.Parameters.AddWithValue("@7", DBNull.Value);
            sc.Parameters.AddWithValue("@8", cmiGuid);

            sc.ExecuteNonQuery();
          }
        }
      }

      //See when function is done 
      System.Diagnostics.Debug.WriteLine("--- InsertIntoCalendar() completed ---");
    }
    #endregion CreateCalendar

    #region DeleteCalendar
    private void button8_Click(object sender, EventArgs e)
    {
      SqlConnection cnnFI;

      cnnFI = new SqlConnection(cStringToFI);

      cnnFI.Open();

      //DeleteCalendar(cnnFI);   //Delete all QAL1, QAL2, QAL3 and AST in the FI database
      System.Diagnostics.Debug.WriteLine("--- DELETE CALENDAR DONE ---");
    }

    public static void DeleteCalendar(SqlConnection cnnFI)
    {
      using (cnnFI)
      {
        using (SqlCommand sc = new SqlCommand("DELETE FROM T_InspectionCalendar WHERE CalendarEntryTypeId in (76,77,78)", cnnFI))
        {
          sc.ExecuteNonQuery();
        }

        using (SqlCommand sc = new SqlCommand("DELETE FROM T_Calendar WHERE CalendarEntryTypeID in (76,77,78)", cnnFI))
        {
          sc.CommandTimeout = 0;
          sc.ExecuteNonQuery();
        }
      }
    }
    #endregion DeleteCalendar

    #region ImportSpecificData
    private void textBox1_TextChanged(object sender, EventArgs e)
    {
      //ActorId textBox
    }

    private void textBox2_TextChanged(object sender, EventArgs e)
    {
      //JobId textBox
    }

    private void textBox3_TextChanged(object sender, EventArgs e)
    {
      //P_ID textBox
    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
      //Include textBox
    }

    private void checkBox2_CheckedChanged(object sender, EventArgs e)
    {
      //Transaction checkBox
    }

    private void button10_Click(object sender, EventArgs e)
    {

      Dictionary<string, int> qal2Dict = new Dictionary<string, int>();

      string a = textBox1.Text;
      string j = textBox2.Text;
      string p = textBox3.Text;
      bool includeTopPId = checkBox1.Checked;
      bool transactionCheck = checkBox2.Checked;
      
      //if (!a.Equals("") && !j.Equals(""))
      if (int.TryParse(textBox1.Text, out actorID) && int.TryParse(textBox2.Text, out jobID) && int.TryParse(textBox3.Text, out pID))
      {
        if (!cStringToQal.Equals("") && !cStringToFI.Equals(""))
        {
          actorID = int.Parse(textBox1.Text);
          jobID = int.Parse(textBox2.Text);
          pID = int.Parse(textBox3.Text);
        }
        else
        {
          MessageBox.Show("You have to specify which Database to use");
          return;
        }
      }
      else
      {
        MessageBox.Show("You have to enter an integer value in the ActorId, JobId and P_ID input fields");
        return;
      }

      SqlTransaction transaction = null;

      try
      {
        using (SqlConnection connectionFI = new SqlConnection(cStringToFI))
        {
          using (SqlConnection connectionQal = new SqlConnection(cStringToQal))
          {
            connectionFI.Open();
            connectionQal.Open();
            if (transactionCheck)
            {
              transaction = connectionFI.BeginTransaction();
            }

            int dataExist = TestIfDataExist(actorID, jobID, transaction, connectionFI);
            if (dataExist == 0)
            {
              MessageBox.Show("Data does not exist!");
            }
            else
            {
              //Test(connectionQal, pID, transaction, check);
              Dictionary<int, Guid> instrumentDict = GetInstrumentTypes(connectionQal, transaction, connectionFI);
              DateTime now = DateTime.Now;
              //FOR EVERY TIME WE INSERT NEW DATA
              CreateModifyInfo(cmiGuid, now, actorID, userName, transaction, connectionFI);
              now = DateTime.Now;
              System.Threading.Thread.Sleep(80);
              DateTimeOffset nowOffset = DateTimeOffset.Now;
              //POSITION
              Dictionary<int, int> pDict = ImportSpecificPositionData(connectionQal, actorID, ref pID, nowOffset, transaction, includeTopPId, connectionFI);
              //METER
              Dictionary<int, int> mDict = ImportSpecificMeterData(connectionQal, pDict, actorID, pID, nowOffset, transaction, instrumentDict, connectionFI);
              //COMPONENT
              DateTime qal1DateTime = ImportSpecificComponentData(connectionQal, cDict, nowOffset, mDict, componentInspectionKey, componentConfigTableKey, MAFHGuid, cZeroConfigFieldKey,
                cSpanConfigFieldKey, cMeasureUnitTypeConfigFieldKey, cEmissionTypeConfigFieldKey, cNameConfigFieldKey, actorID, pID, transaction, connectionFI);
              //QAL1
              CreateQal1(connectionQal, nowOffset, qal1InspectionKey, qal1ConfigTableKey, qal1MaxRangeConfigFieldKey, qal1MeasuringValueConfigFieldKey, MAFHGuid,
                pID, jobID, transaction, qal1DateTime, cmiGuid, initiatorKey, connectionFI);     //Create all QAL1 and insert them into T_Inspection and T_Inspection_InspectionTypes
              //QAL2
              CreateQal2(connectionQal, qal2Dict, nowOffset, qal2InspectionKey, qal2ConfigTableKey, qal2FormulaAConfigFieldKey, qal2FormulaBConfigFieldKey, qal2IntervalFromConfigFieldKey,
                qal2IntervalToConfigFieldKey, qal2ValidPeriodTypeConfigFieldKey, qal2ValidUntilDTConfigFieldKey, MAFHGuid, pID, jobID, cmiGuid, transaction, initiatorKey, connectionFI);     //Create all QAL2 and insert them into T_Inspection and T_Inspection_InspectionTypes
              //QAL3
              CreateQal3(connectionQal, nowOffset, pID, jobID, transaction, cmiGuid, initiatorKey, connectionFI);   //Create all QAL3 and insert them into T_Inspection and T_Inspection_InspectionTypes
              //AST
              //CreateAST(connectionQal, nowOffset, MAFHGuid, pID, jobID, cmiGuid, transaction);        //Create all AST and insert them into T_Inspection and T_Inspection_InspectionTypes
              //REPORTS
              CreateQAL1Reports(connectionQal, pID, nowOffset, transaction, connectionFI);
              CreateQAL2Reports(connectionQal, pID, nowOffset, transaction, connectionFI);
              //CreateASTReports(connectionQal, pID, nowOffset, transaction);
              //CALENDAR
              InsertIntoCalendar(pID, cmiGuid, transaction, connectionFI, mDict);
            }
          }

          if (transactionCheck)
          {
            transaction.Commit();
          }
        }
      }
      catch(Exception ex)
      {
        MessageBox.Show("Error running the program: " + ex);
        if (transaction != null)
        {
          transaction.Rollback();
        }
      }
      
      System.Diagnostics.Debug.WriteLine("--- IMPORT SPECIFIC DATA COMPLETED ---");
    }

    public void Test(SqlConnection cnnQal, int pID, SqlTransaction transaction, bool check)
    {
      if (check)
      {
        System.Diagnostics.Debug.WriteLine("CHECKED!!");
      }
      else
      {
        System.Diagnostics.Debug.WriteLine("not!!");
      }
    }

    public int TestIfDataExist(int ActorID, int JobID, SqlTransaction transaction, SqlConnection cnnFI)
    {
      //Check if data exist in T_Actor 
      using (SqlCommand sc = CreateCommandAndEnlistTransaction("SELECT COUNT(*) FROM T_Actor WHERE ActorId = @ActorID", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@ActorID", ActorID);
        int anyData = (int)sc.ExecuteScalar();

        if (anyData < 1)
        {
          return 0;
        }
      }

      //Check if data exist in T_Jobs
      using (SqlCommand sc = CreateCommandAndEnlistTransaction("SELECT COUNT(*) FROM T_Jobs WHERE JobId = @JobID", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@JobID", JobID);
        int anyData = (int)sc.ExecuteScalar();

        if (anyData < 1)
        {
          return 0;
        }
      }

      //Check if data exist in T_JobActor
      using (SqlCommand sc = CreateCommandAndEnlistTransaction("SELECT COUNT(*) FROM T_JobActor WHERE ActorId = @ActorID AND JobId = @JobID", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@ActorID", ActorID);
        sc.Parameters.AddWithValue("@JobID", JobID);

        int anyData = (int)sc.ExecuteScalar();

        if (anyData < 1)
        {
          return 0;
        }
      }
      
      return 1;
    }

    /// <summary>Enlists an IDbCommand in a transaction.</summary>
    /// <param name="command">The command to enlist.</param>
    /// <param name="transaction">The transaction in which the command should be enlisted.</param>
    public static void EnlistTransaction(IDbCommand command, IDbTransaction transaction)
    {
      if (command == null || transaction == null || transaction.Connection == null)
        return;
      command.Connection = transaction.Connection;
      command.Transaction = transaction;
    }

    public static SqlCommand CreateCommandAndEnlistTransaction(string sql, SqlConnection connection = null, IDbTransaction transaction = null)
    {
      if (connection == null && transaction == null)
        throw new Exception($"Either the {nameof(connection)} or {nameof(transaction)} parameters must have a value. Both were null.");

      SqlCommand command = new SqlCommand(sql);
      if (transaction != null)
        EnlistTransaction(command, transaction);
      else
        command.Connection = connection;

      return command;
    }

    public static Dictionary<int, int> ImportSpecificPositionData(SqlConnection cnnQal, object ActorID, ref int pID, DateTimeOffset nowOffset, 
      SqlTransaction transaction, bool includeTopPId, SqlConnection cnnFI)
    {
      Dictionary<int, int> pDict = new Dictionary<int, int>();
      List<Position> pList = new List<Position>();
      string sql;

      if (includeTopPId)
      {
        sql = @"
        SELECT * FROM Position WHERE P_ID = @pID OR P_ParentID = 
        (
            SELECT P_ID FROM Position WHERE P_ID = @pID
        ) 
        AND P_ID NOT IN (240, 264, 320, 323, 343, 344)";
      } 
      else
      {
        sql = @"
        SELECT * FROM Position WHERE P_ParentID = 
        (
            SELECT P_ID FROM Position WHERE P_ID = @pID
        ) 
        AND P_ID NOT IN (240, 264, 320, 323, 343, 344)";
      }

      using (SqlCommand sc = new SqlCommand(sql, cnnQal))
      {
        sc.Parameters.AddWithValue("@pID", pID);

        using (SqlDataReader dr = sc.ExecuteReader())
        {
          while (dr.Read())
          {
            var op = new Position();

            op.ID = Convert.ToInt32(dr["P_ID"]);
            op.Pos = Convert.ToString(dr["P_Pos"]);
            op.ParentID = Convert.ToInt32(dr["P_ParentID"]);
            op.WPID = Convert.ToInt32(dr["P_WPID"]);
            op.Description = Convert.ToString(dr["P_Description"]);
            op.ListIndex = Convert.ToInt32(dr["P_ListIndex"]);
            op.Remarks = Convert.ToString(dr["P_Remarks"]);
            op.WPSubId = Convert.ToString(dr["P_WPSubID"]);
            //string temp = Convert.ToString(dr["WorkItemKey"]);

            pList.Add(op);
            //System.Diagnostics.Debug.WriteLine(dr);
          }
        }
      }
      

      int userID = 770;
      int objectID;

      //INSERT rows into ForceInspectTest database
      foreach (Position p in pList)
      {
        string newID;

        using (SqlCommand FICommand = CreateCommandAndEnlistTransaction("INSERT INTO [T_Object] (ExternalNo, ParentObjectID, ExternalDescription, " +
          "InternalDescription, CreatedDateTime, LastModifiedDateTime, CreatedByUserID, LastModifiedByUserID, ObjectStatusID) " +
                "VALUES (@Pos, @ParentID, @newDescription, @ID, @cdt, @lmdt, @cbu, @lmbu, @ObjectStatusID)", cnnFI, transaction))
        {
         
          string newDescription = p.Description + ". " + p.Remarks;
          newID = "P_ID:" + p.ID;

          FICommand.Parameters.AddWithValue("@Pos", p.Pos);

          if (!includeTopPId)
          {
            if (p.ParentID == pID)
            {
              FICommand.Parameters.AddWithValue("@ParentID", DBNull.Value);
            }
            else
            {
              FICommand.Parameters.AddWithValue("@ParentID", pDict[p.ParentID]);
            }
          }
          else
          {
            if (p.ParentID == 0)
            {
              FICommand.Parameters.AddWithValue("@ParentID", DBNull.Value);
            }
            else
            {
              FICommand.Parameters.AddWithValue("@ParentID", pDict[p.ParentID]);
            }
          }

          FICommand.Parameters.AddWithValue("@newDescription", newDescription);
          FICommand.Parameters.AddWithValue("@ID", newID);
          FICommand.Parameters.AddWithValue("@cdt", nowOffset);
          FICommand.Parameters.AddWithValue("@lmdt", nowOffset);
          FICommand.Parameters.AddWithValue("@cbu", userID);
          FICommand.Parameters.AddWithValue("@lmbu", userID);
          FICommand.Parameters.AddWithValue("@ObjectStatusID", 2);

          FICommand.ExecuteNonQuery();

        }

        //Get the new ObjectID from the newly inserted Position object
        using (SqlCommand s = CreateCommandAndEnlistTransaction("SELECT ObjectID FROM [T_Object] WHERE InternalDescription = @newID", cnnFI, transaction))
        {
          s.Parameters.AddWithValue("@newID", newID);

          objectID = (int)s.ExecuteScalar();
          pDict.Add(p.ID, objectID);
        }

        //Insert into the T_Object_ObjectTypes
        InsertIntoObjectTypes(objectID, 19, transaction, cnnFI);

        //Insert into T_Object_Actor
        InsertIntoObjectActor(objectID, ActorID, transaction, cnnFI);
      }

      System.Diagnostics.Debug.WriteLine("--- ImportSpecificPositionData() completed ---");
      return pDict;
      
    }

    public static Dictionary<int, int> ImportSpecificMeterData(SqlConnection cnnQal, Dictionary<int, int> pDict, object ActorID, int pID,
      DateTimeOffset nowOffset, SqlTransaction transaction, Dictionary<int, Guid> instrumentDict, SqlConnection cnnFI)
    {
      Dictionary<int, int> mDict = new Dictionary<int, int>();
      List<Meter> mList = new List<Meter>();
      //Meter Keys
      string meterInspectionKey = "QALMeterConfig";
      string meterConfigTableKey = "80C4DF1C-5F71-49A0-8747-D1D1492CBF21";

      string sql = @"
      SELECT * FROM Meter WHERE M_PID in 
      (
          SELECT P_ID FROM Position WHERE P_ParentID = 
          (
               SELECT P_ID FROM Position WHERE P_ID = @pID
          ) 
          AND P_ID NOT IN (240, 264, 320, 323, 343, 344)
      ) ";

      using (SqlCommand sc = new SqlCommand(sql, cnnQal))
      {
        sc.Parameters.AddWithValue("@pID", pID);

        using (SqlDataReader dr = sc.ExecuteReader())
        {
          while (dr.Read())
          {
            var m = new Meter();

            m.ID = Convert.ToInt32(dr["M_ID"]);
            m.Name = Convert.ToString(dr["M_Name"]);
            m.PID = Convert.ToInt32(dr["M_PID"]);
            m.Description = Convert.ToString(dr["M_Description"]);
            m.Listindex = Convert.ToInt32(dr["M_ListIndex"]);
            m.Remarks = Convert.ToString(dr["M_Remarks"]);

            mList.Add(m);
            //System.Diagnostics.Debug.WriteLine(dr);
          }
        }
      }
      
    int userID = 770;
    int objectID;

    string newID;

      //Insert Meters into the T_Object table 
      //Description might be in ExternalNo
      foreach (Meter m in mList)
      {
        using (SqlCommand FICommand = CreateCommandAndEnlistTransaction("INSERT INTO [T_Object] (ExternalNo, ParentObjectID, ManufacturerNo, " +
          "ExternalDescription, InternalDescription, CreatedDateTime, LastModifiedDateTime, CreatedByUserID, LastModifiedByUserID, ObjectStatusID) " +
          "VALUES (@Name, @ParentID, @mf, @ed, @ID, @cdt, @lmdt, @cbu, @lmbu, @ObjectStatusID)", cnnFI, transaction))
        {
          newID = "M_ID:" + m.ID;

          FICommand.Parameters.AddWithValue("@Name", m.Name);
          FICommand.Parameters.AddWithValue("@ParentID", pDict[m.PID]);
          FICommand.Parameters.AddWithValue("@mf", m.Description);
          FICommand.Parameters.AddWithValue("@ed", m.Remarks);
          FICommand.Parameters.AddWithValue("@ID", newID);
          FICommand.Parameters.AddWithValue("@cdt", nowOffset);
          FICommand.Parameters.AddWithValue("@lmdt", nowOffset);
          FICommand.Parameters.AddWithValue("@cbu", userID);
          FICommand.Parameters.AddWithValue("@lmbu", userID);
          FICommand.Parameters.AddWithValue("@ObjectStatusID", 2);

          FICommand.ExecuteNonQuery();
        }

        //Get the ObjectID from the newly inserted Meter object
        using (SqlCommand s = CreateCommandAndEnlistTransaction("SELECT ObjectID FROM [T_Object] WHERE InternalDescription = @newID", cnnFI, transaction))
        {
          s.Parameters.AddWithValue("@newID", newID);

          objectID = (int)s.ExecuteScalar();
          mDict.Add(m.ID, objectID);
        }

        //Insert into T_Object_ObjectTypes
        InsertIntoObjectTypes(objectID, 20, transaction, cnnFI);

        //Insert into T_Object_Actor
        InsertIntoObjectActor(objectID, ActorID, transaction, cnnFI);

        //Get VersionID from T_GenericConfigTable
        int versionID = GetVersionId(meterInspectionKey, transaction, cnnFI);

        //Insert into T_GenericValueRow and T_GenericValueField
        InsertMeterObjectIntoGenericTables(cnnQal, cnnFI, m.ID, objectID, meterConfigTableKey, versionID, nowOffset, transaction, instrumentDict);
      }
      
      System.Diagnostics.Debug.WriteLine("--- ImportSpecificMeterData() completed ---");
      return mDict;
    }

    public static DateTime ImportSpecificComponentData(SqlConnection cnnQal, Dictionary<int, int> cDict, DateTimeOffset nowOffset, Dictionary<int, int> mDIct,
    string componentInspectionKey, string componentConfigTableKey, string MAFHGuid, string cZeroConfigFieldKey, string cSpanConfigFieldKey, string cMeasureUnitTypeConfigFieldKey,
    string cEmissionTypeConfigFieldKey, string cNameConfigFieldKey, object ActorID, int pID, SqlTransaction transaction, SqlConnection cnnFI)
    {
      List<Component> cList = new List<Component>();

      List<DateTime> qal1DateTimeList = new List<DateTime>();

      //Get components from Component in QAL database
      using (SqlCommand sc = new SqlCommand("SELECT * FROM Component WHERE C_Name != 'Flow'", cnnQal))
      {
        using (SqlDataReader dr = sc.ExecuteReader())
        {
          while (dr.Read())
          {
            Component c = new Component();

            c.C_ID = Convert.ToInt32(dr["C_ID"]);
            c.C_Name = Convert.ToString(dr["C_Name"]);

            cList.Add(c);
          }
        }
      }
      int userID = 770;
      int objectID;

      foreach (Component c in cList)
      {
        string newID = "C_ID:" + c.C_ID;
        List<int> mIDList = new List<int>();

        //Key = Old QAL1_ID, Value = QAL1 object
        Dictionary<int, Qal1> oldIDQal = new Dictionary<int, Qal1>();
        //Key = New QAL1_ID, Value = QAL1 object
        Dictionary<int, Qal1> newIDQal = new Dictionary<int, Qal1>();

        //Get the needed QAL1 properties that a component needs
        string sql = @"
        SELECT * FROM QAL1 WHERE QAL1_MID in 
        (
          SELECT M_ID FROM Meter WHERE M_PID in 
          (
            SELECT P_ID FROM Position WHERE P_ParentID = 
            (
              SELECT P_ID FROM Position WHERE P_ID = @pID
            ) AND P_ID NOT IN (240, 264, 320, 323, 343, 344)
          ) 
        ) AND QAL1_CID = @C_Name AND QAL1_Unit != ''";

        //Changed list for ARGO
        using (SqlCommand sc = new SqlCommand(sql, cnnQal))
        {
          sc.Parameters.AddWithValue("@C_Name", c.C_Name);
          sc.Parameters.AddWithValue("@pID", pID);
          using (SqlDataReader dr = sc.ExecuteReader())
          {
            while (dr.Read())
            {
              Qal1 qal1 = new Qal1();

              int ID = Convert.ToInt32(dr["QAL1_ID"]);
              int mID = Convert.ToInt32(dr["QAL1_MID"]);
              qal1.Q1_Dato = Convert.ToDateTime(dr["QAL1_Dato"]);
              qal1.Q1_SSpan = Convert.ToDecimal(dr["QAL1_SSpan"]);
              qal1.Q1_SZero = Convert.ToDecimal(dr["QAL1_SZero"]);
              qal1.Q1_Unit = Convert.ToString(dr["QAL1_Unit"]);

              oldIDQal.Add(ID, qal1);
              mIDList.Add(mID);
              qal1DateTimeList.Add(qal1.Q1_Dato);
            }
          }
        }

        //Insert all the component in T_Object where QAL1_CID = 'C_NAME'
        foreach (int mid in mIDList)
        {

          //Insert into T_Object 
          using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO [T_Object] (ParentObjectID, ExternalNo, " +
            "InternalDescription, CreatedDateTime, LastModifiedDateTime, CreatedByUserID, LastModifiedByUserID, ObjectStatusID) " +
                  "VALUES (@ParentID, @Name, @ID, @cdt, @lmdt, @cbu, @lmbu, @ObjectStatusID)", cnnFI, transaction))
          {
            sc.Parameters.AddWithValue("@ParentID", mDIct[mid]);
            sc.Parameters.AddWithValue("@Name", c.C_Name);
            sc.Parameters.AddWithValue("@ID", newID);
            sc.Parameters.AddWithValue("@cdt", nowOffset);
            sc.Parameters.AddWithValue("@lmdt", nowOffset);
            sc.Parameters.AddWithValue("@cbu", userID);
            sc.Parameters.AddWithValue("@lmbu", userID);
            sc.Parameters.AddWithValue("@ObjectStatusID", 2);

            sc.ExecuteNonQuery();
          }
        }

        List<int> newObjectIDList = new List<int>();

        //Get the objectID from T_Object, from the newly inserted components
        //CreatedDateTime so it only gets the newest inserted Components... - REMEMBER TO REMOVE
        using (SqlCommand sq = CreateCommandAndEnlistTransaction("SELECT * FROM [T_Object] WHERE InternalDescription = @newID", cnnFI, transaction))
        {
          sq.Parameters.AddWithValue("@newID", newID);

          using (SqlDataReader d = sq.ExecuteReader())
          {
            while (d.Read())
            {
              objectID = Convert.ToInt32(d["ObjectID"]);

              newObjectIDList.Add(objectID);
            }
          }
        }

        int i = 0;

        //Switch the Old C_ID in the dictionary, with the new T_Object ID's, so we can use the references later
        foreach (var item in oldIDQal)
        {
          newIDQal.Add(newObjectIDList[i], item.Value);

          i++;
        }

        foreach (int newObjectID in newObjectIDList)
        {
          Guid guid = Guid.NewGuid();

          //Insert into T_Object_ObjectTypes 
          InsertIntoObjectTypes(newObjectID, 21, transaction, cnnFI);

          //Insert into T_Object_Actor
          InsertIntoObjectActor(newObjectID, ActorID, transaction, cnnFI);

          //Get VersionID from T_GenericConfigTable
          int versionID = GetVersionId(componentInspectionKey, transaction, cnnFI);

          //Insert into T_GenericValueRow and T_GenericValueField
          InsertComponentObjectIntoGenericTables(cnnFI, guid, componentConfigTableKey, newObjectID, versionID, nowOffset, c.C_Name, newIDQal[newObjectID], MAFHGuid,
            cNameConfigFieldKey, cZeroConfigFieldKey, cSpanConfigFieldKey, cEmissionTypeConfigFieldKey, cMeasureUnitTypeConfigFieldKey, transaction);
        }
      }
      System.Diagnostics.Debug.WriteLine("--- ImportSpecificComponentData() completed ---");
      DateTime qal1DateTime = qal1DateTimeList.OrderBy(a => a.Date).First();
      return qal1DateTime;

    }

    public static void InsertIntoObjectActor(int objectID, object ActorID, SqlTransaction transaction, SqlConnection cnnFI)
    {
      using (SqlCommand sc = CreateCommandAndEnlistTransaction("INSERT INTO [T_Object_Actor] (ObjectID, ActorID, ActorTypeID) " +
            "VALUES (@ObjectID, @ActorID, @ActorTypeID)", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@ObjectID", objectID);
        sc.Parameters.AddWithValue("@ActorID", ActorID);            
        sc.Parameters.AddWithValue("@ActorTypeID", 1);        //Facility = 1

        sc.ExecuteNonQuery();

      }
    }

    public static Dictionary<int, Guid> GetInstrumentTypes(SqlConnection cnnQal, SqlTransaction transaction, SqlConnection cnnFI)
    {
      //Get all instrument types in T_GenericConfigLookupField
      //GØRES MERE DYNAMISK....!!!
      string sql = "SELECT * FROM T_GenericConfigLookupField WHERE GenericConfigLookupTableKey = '9E7FB53C-2B1F-4B83-84D5-009305C55D0E'";
      Dictionary<int, Guid> instrumentDict = new Dictionary<int, Guid>();
      List<Instrument> iList = new List<Instrument>();

      using (SqlCommand sc = new SqlCommand(sql, cnnFI, transaction))
      {
        using (SqlDataReader dr = sc.ExecuteReader())
        {
          while (dr.Read())
          {
            if (Convert.ToString(dr["Description"]).Equals("ACF-NT"))
            {
              Guid g = (Guid)(dr["GenericConfigLookupFieldKey"]);
              instrumentDict.Add(5, g);
            } 
            else if (Convert.ToString(dr["Description"]).Equals("Limas 11"))
            {
              Guid g = (Guid)(dr["GenericConfigLookupFieldKey"]);
              instrumentDict.Add(10, g);
            }
            else if (Convert.ToString(dr["Description"]).Equals("MAGNOS 17"))
            {
              Guid g = (Guid)(dr["GenericConfigLookupFieldKey"]);
              instrumentDict.Add(11, g);
            }
            else if (Convert.ToString(dr["Description"]).Equals("MCS 100E"))
            {
              Guid g = (Guid)(dr["GenericConfigLookupFieldKey"]);
              instrumentDict.Add(7, g);
            }
            else if (Convert.ToString(dr["Description"]).Equals("MCS 100FT"))
            {
              Guid g = (Guid)(dr["GenericConfigLookupFieldKey"]);
              instrumentDict.Add(8, g);
            }
            else if (Convert.ToString(dr["Description"]).Equals("Rosemount NGA 2000"))
            {
              Guid g = (Guid)(dr["GenericConfigLookupFieldKey"]);
              instrumentDict.Add(2, g);
            }
            else if (Convert.ToString(dr["Description"]).Equals("URAS 14"))
            {
              Guid g = (Guid)(dr["GenericConfigLookupFieldKey"]);
              instrumentDict.Add(12, g);
            }
          }
        }
      }

      return instrumentDict;
    }

    public static Guid GetSpecificInstrumentType(SqlTransaction transaction, string name, SqlConnection cnnFI)
    {
      Guid guid = Guid.Empty;

      using (SqlCommand sc = CreateCommandAndEnlistTransaction("SELECT GenericConfigLookupFieldKey FROM T_GenericConfigLookupField " +
        "WHERE GenericConfigLookupTableKey = '9E7FB53C-2B1F-4B83-84D5-009305C55D0E' AND [Name] = @name", cnnFI, transaction))
      {
        sc.Parameters.AddWithValue("@name", name);
        using (SqlDataReader dr = sc.ExecuteReader())
        {
          while (dr.Read())
          {
            guid = (Guid)dr["GenericConfigLookupFieldKey"];
          }
        }
      }

      return guid;
    }
    #endregion ImportSpecificData

    #region Database
    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {

      if (cboState.Text.Equals("Test"))
      {
        cStringToFI = @"Data Source=forceinspecttest.forcedb.dk;Initial Catalog=ForceInspect_Test;User ID=ForceInspect_Adm;Password=FI2008go";

        System.Diagnostics.Debug.WriteLine("Database set to: TEST");
      }
      else if (cboState.Text.Equals("Development"))
      {
        cStringToFI = @"Data Source=SqlDbForceInspectDev;Initial Catalog=ForceInspect_Dev;User ID=ForceInspect_Adm;Password=FI2008go";

        System.Diagnostics.Debug.WriteLine("Database set to: DEVELOPMENT");
      }
      else if (cboState.Text.Equals("Production"))
      {
        cStringToFI = @"Data Source=SqlDbForceInspect;Initial Catalog=ForceInspect;User ID=ForceInspect_Adm;Password=Linq2009";

        System.Diagnostics.Debug.WriteLine("Database set to: PRODUCTION");
      }
    }
    #endregion Database

    #region Test
    private void button5_Click(object sender, EventArgs e)
    {

      SqlConnection cnnQal;
      SqlConnection cnnFI;
      cnnQal = new SqlConnection(cStringToQal);
      cnnFI = new SqlConnection(cStringToFI);
      cnnQal.Open();
      cnnFI.Open();

      //TestGetAandB(cnnQal, cnnFI);
      //TestGetQ1Unit(cnnQal, cnnFI);
      //TestGetIntervals(cnnQal);
      //TestGetValidTime(cnnQal);
      //TestGetFormulas(cnnQal);
      //TestInsertIntoCalender(cnnFI);
      //TestGetReports(cnnFI);
      //TestInsertNewComponent(cnnFI, nowOffset, MAFHGuid);
      System.Diagnostics.Debug.WriteLine("--- TEST DONE ---");
    }

    public static void TestGetAandB(SqlConnection cnnQal)
    {

      int x;
      int plus;
      int gange;
      string tempA = "";
      string tempB = "";
      float a = 0;
      float b = 0;

      using (SqlCommand sc = new SqlCommand("SELECT DISTINCT QAL2_Funktion FROM [QAL2]", cnnQal))
      {
        using (SqlDataReader dr = sc.ExecuteReader())
        {
          while (dr.Read())
          {
            string funktion = Convert.ToString(dr["QAL2_Funktion"]);

            //Check if the string is longer than 2 and doesnt contain the letter 'a'
            if (funktion.Length > 2 && !funktion.Contains("a"))
            {
              funktion = funktion.Remove(0, 2);
              funktion = funktion.Replace(" ", "");
              funktion = funktion.Replace("(", "");
              funktion = funktion.Replace(")", "");
              x = funktion.IndexOf("x");              //Get the x
              plus = funktion.IndexOf("+");           //Get the +
              gange = funktion.IndexOf("*");          //Get the *

              //Check if the x is in a higher position than 0
              if (x > 0)
              {
                tempA = funktion.Substring(0, x);
                tempA = tempA.Replace("*", "");
                tempA = tempA.Replace(".", ",");

                //Check if the string is higher than the x + 1 to see where the 'b' is 
                if (funktion.Length > x + 1)
                {
                  tempB = funktion.Substring(x + 2);
                  tempB = tempB.Replace(".", ",");
                }
                //Else if the string dont contains 'x' set b = 0
                else if (!funktion.Contains("+"))
                {
                  tempB = "0";
                }
              }
              //Check if the string contains 'Xi' and see if the lenght is higher than 10
              if (funktion.Contains("Xi") && funktion.Length > 10)
              {
                tempA = funktion.Substring(plus + 1);
                tempA = tempA.Replace("*", "");
                tempA = tempA.Replace(".", ",");
                tempA = tempA.Replace("Xi", "");

                tempB = funktion.Substring(0, plus);
                tempB = tempB.Replace(".", ",");
              }
              if (tempA.Equals("a"))
              {
                tempA = "";
              }

              if (tempA.Length > 0)
              {
                a = float.Parse(tempA);
                System.Diagnostics.Debug.WriteLine(tempA);
              }
              if (tempB.Length > 0)
              {
                b = float.Parse(tempB);
                System.Diagnostics.Debug.WriteLine(tempB);
                System.Diagnostics.Debug.WriteLine("");
              }
            }

          }
          //y = 13,93 + 0,546 * Xi
        }
      }
      
    }

    //Take around 2 minutes and 30 seconds
    public static void TestGetQ1Unit(SqlConnection cnnQal)
    {
      List<Qal1> q1List = new List<Qal1>();
      List<int> q3IdList = new List<int>();
      int M_ID = 0;
      int count = 0;

      using (SqlCommand sc = new SqlCommand("SELECT R_QAL3_ID FROM [Reading]", cnnQal))
      {
        using (SqlDataReader dr = sc.ExecuteReader())
        {
          while (dr.Read())
          {
            int Q3_ID;

            Q3_ID = Convert.ToInt32(dr["R_QAL3_ID"]);

            q3IdList.Add(Q3_ID);
          }
        }
      }

      foreach (int id in q3IdList)
      {
        using (SqlCommand sc = new SqlCommand("SELECT QAL3_MID FROM [QAL3] WHERE QAL3_ID = @Q3_ID", cnnQal))
        {
          sc.Parameters.AddWithValue("@Q3_ID", id);

          using (SqlDataReader dr = sc.ExecuteReader())
          {
            while (dr.Read())
            {
              M_ID = Convert.ToInt32(dr["QAL3_MID"]);
            }
          }
        }

        using (SqlCommand sc = new SqlCommand("SELECT * FROM [QAL1] WHERE QAL1_MID = @M_ID", cnnQal))
        {
          sc.Parameters.AddWithValue("@M_ID", M_ID);

          using (SqlDataReader dr = sc.ExecuteReader())
          {
            while (dr.Read())
            {
              Qal1 q1 = new Qal1();

              q1.Q1_ID = Convert.ToInt32(dr["QAL1_ID"]);
              q1.Q1_MID = Convert.ToInt32(dr["QAL1_MID"]);
              q1.Q1_Dato = Convert.ToDateTime(dr["QAL1_Dato"]);
              q1.Q1_IID = Convert.ToInt32(dr["QAL1_IID"]);
              q1.Q1_Description = Convert.ToString(dr["QAL1_IDescription"]);
              q1.Q1_CID = Convert.ToString(dr["QAL1_CID"]);
              q1.Q1_IVersion = Convert.ToString(dr["QAL1_IVersion"]);
              q1.Q1_AIA = Convert.ToBoolean(dr["QAL1_AIA"]);
              q1.Q1_SV = Convert.ToDecimal(dr["QAL1_SV"]);
              q1.Q1_MV = Convert.ToDecimal(dr["QAL1_MV"]);
              q1.Q1_Unit = Convert.ToString(dr["QAL1_Unit"]);
              q1.Q1_SZero = Convert.ToDecimal(dr["QAL1_SZero"]);
              q1.Q1_SSpan = Convert.ToDecimal(dr["QAL1_SSpan"]);
              q1.Q1_INI = Convert.ToString(dr["QAL1_INI"]);
              q1.Q1_UID = Convert.ToInt32(dr["QAL1_UID"]);
              q1.Q1_Accept = Convert.ToBoolean(dr["QAL1_Accept"]);
              //q1.Q1_DocName = Convert.ToString(dr["QAL1_DocName"]);
              //q1.Q1_DocLength = Convert.ToInt32(dr["QAL1_DocLength"]);
              //q1.Q1_DocContentType = Convert.ToString(dr["QAL1_DocContentType"]);

              q1List.Add(q1);
            }
          }
        }
        count++;
      }
      

      System.Diagnostics.Debug.WriteLine("Count: " + count);
      //foreach (Qal1 q1 in q1List)
      //{
      //  System.Diagnostics.Debug.WriteLine("Unit: " + q1.Q1_Unit);
      //}


    }

    public static void TestGetIntervals(SqlConnection cnnQal)
    {
      List<string> intervalList = new List<string>();
      List<decimal?> iFromList = new List<decimal?>();
      List<decimal?> iToList = new List<decimal?>();

      decimal? iFrom;
      decimal? iTo;

      using (SqlCommand sc = new SqlCommand("SELECT QAL2_Interval FROM QAL2", cnnQal))
      {
        using (SqlDataReader dr = sc.ExecuteReader())
        {
          while (dr.Read())
          {
            string interval;

            interval = Convert.ToString(dr["QAL2_Interval"]);

            intervalList.Add(interval);
          }
        }
      }
      string tempI;

      foreach (string i in intervalList)
      {
        if (i.Contains("?") || i.Equals(""))
        {
          iFrom = null;
          iTo = null;

          iFromList.Add(iFrom);
          iToList.Add(iTo);
        }
        else if (i.Contains(".."))
        {
          tempI = i.Replace(".", "");
          iFrom = Convert.ToDecimal(i[0].ToString());
          iTo = Convert.ToDecimal(tempI.Substring(1));

          iFromList.Add(iFrom);
          iToList.Add(iTo);
        }
        else if (i.Contains("-"))
        {
          tempI = i.Replace(" ", "");
          tempI = tempI.Replace("-", "");
          tempI = tempI.Replace("mg/m3", "");
          tempI = tempI.Replace("mg", "");

          iFrom = Convert.ToDecimal(i[0].ToString());
          iTo = Convert.ToDecimal(tempI.Substring(1));

          iFromList.Add(iFrom);
          iToList.Add(iTo);
        }
      }

      foreach (decimal? i in iFromList)
      {
        System.Diagnostics.Debug.WriteLine("From: " + i);
      }
      foreach (decimal? i in iToList)
      {
        System.Diagnostics.Debug.WriteLine("To: " + i);
      }
    }

    public static void TestGetValidTime(SqlConnection cnnQal)
    {
      DateTime dato = DateTime.Now;
      DateTime slutDato = DateTime.Now;

      using (SqlCommand sc = new SqlCommand("SELECT QAL2_Dato, QAL2_SlutDato FROM QAL2 WHERE QAL2_ID = 351", cnnQal))
      {
        using (SqlDataReader dr = sc.ExecuteReader())
        {
          while (dr.Read())
          {
            dato = Convert.ToDateTime(dr["QAL2_Dato"]);
            slutDato = Convert.ToDateTime(dr["QAL2_SlutDato"]);
          }
        }
      }

      System.Diagnostics.Debug.WriteLine("Dato: " + dato);
      System.Diagnostics.Debug.WriteLine("Slut Dato: " + slutDato);
      System.Diagnostics.Debug.WriteLine("Diff: " + (slutDato.Year - dato.Year));
    }

    public static void TestGetFormulas(SqlConnection cnnQal)
    {
      decimal fA = 10101010;
      decimal fB = 10101010;

      using (SqlCommand sc = new SqlCommand("SELECT QAL3_FormulaAlfa, QAL3_FormulaBeta FROM QAL3 WHERE QAL3_ID = 188", cnnQal))
      {
        using (SqlDataReader dr = sc.ExecuteReader())
        {
          while (dr.Read())
          {
            if (!dr.IsDBNull(1))
            {
              fA = Convert.ToDecimal(dr["QAL3_FormulaAlfa"]);
              fB = Convert.ToDecimal(dr["QAL3_FormulaBeta"]);
            }
          }
        }
      }

      System.Diagnostics.Debug.WriteLine("fA: " + fA);
      System.Diagnostics.Debug.WriteLine("fB: " + fB);
    }

    public static void TestInsertIntoCalender(SqlConnection cnnFI)
    {
      List<Inspection> iList = new List<Inspection>();
      Guid guid = Guid.NewGuid();
      Guid guid2 = Guid.NewGuid();
      Guid guid3 = Guid.NewGuid();
      Guid guid4 = Guid.NewGuid();

      using (cnnFI)
      {
        using (SqlCommand sc = new SqlCommand("INSERT INTO T_Calendar (CalendarGUID, ObjectID, CalendarEntryTypeID, DateTime, AssignedToUserID, Done, " +
          "CreatedByInspectionGUID, ModifiedByInspectionGUID, ModifiedOriginalDateTime, ModifiedOriginalDone) " +
          "VALUES (@guid, @objectid, @ceti, @datetime, @atui, @done, @cbig, @mbig, @modt, @mod)", cnnFI))
        {
          sc.Parameters.AddWithValue("@guid", guid);
          sc.Parameters.AddWithValue("@objectid", 882474);
          sc.Parameters.AddWithValue("@ceti", 76);
          sc.Parameters.AddWithValue("@datetime", "2003-06-03 00:00:00");
          sc.Parameters.AddWithValue("@atui", 770);
          sc.Parameters.AddWithValue("@done", 1);
          sc.Parameters.AddWithValue("@cbig", "549214C4-DDEA-47FB-AFF6-1520357A46D7");
          sc.Parameters.AddWithValue("@mbig", "549214C4-DDEA-47FB-AFF6-1520357A46D7");
          sc.Parameters.AddWithValue("@modt", DBNull.Value);
          sc.Parameters.AddWithValue("@mod", DBNull.Value);

          sc.ExecuteNonQuery();
        }

        using (SqlCommand sc = new SqlCommand("INSERT INTO T_Calendar (CalendarGUID, ObjectID, CalendarEntryTypeID, DateTime, AssignedToUserID, Done, " +
          "CreatedByInspectionGUID, ModifiedByInspectionGUID, ModifiedOriginalDateTime, ModifiedOriginalDone) " +
          "VALUES (@guid, @objectid, @ceti, @datetime, @atui, @done, @cbig, @mbig, @modt, @mod)", cnnFI))
        {
          sc.Parameters.AddWithValue("@guid", guid2);
          sc.Parameters.AddWithValue("@objectid", 882474);
          sc.Parameters.AddWithValue("@ceti", 77);
          sc.Parameters.AddWithValue("@datetime", "2008-07-16 00:00:00");
          sc.Parameters.AddWithValue("@atui", 770);
          sc.Parameters.AddWithValue("@done", 1);
          sc.Parameters.AddWithValue("@cbig", "9510C886-7AF9-46F4-A8FA-C0A278AF1AC7");
          sc.Parameters.AddWithValue("@mbig", "9510C886-7AF9-46F4-A8FA-C0A278AF1AC7");
          sc.Parameters.AddWithValue("@modt", DBNull.Value);
          sc.Parameters.AddWithValue("@mod", DBNull.Value);

          sc.ExecuteNonQuery();
        }

        using (SqlCommand sc = new SqlCommand("INSERT INTO T_Calendar (CalendarGUID, ObjectID, CalendarEntryTypeID, DateTime, AssignedToUserID, Done, " +
          "CreatedByInspectionGUID, ModifiedByInspectionGUID, ModifiedOriginalDateTime, ModifiedOriginalDone) " +
          "VALUES (@guid, @objectid, @ceti, @datetime, @atui, @done, @cbig, @mbig, @modt, @mod)", cnnFI))
        {
          sc.Parameters.AddWithValue("@guid", guid3);
          sc.Parameters.AddWithValue("@objectid", 882474);
          sc.Parameters.AddWithValue("@ceti", 78);
          sc.Parameters.AddWithValue("@datetime", "2003-08-18 00:00:00");
          sc.Parameters.AddWithValue("@atui", 770);
          sc.Parameters.AddWithValue("@done", 1);
          sc.Parameters.AddWithValue("@cbig", "DD08526D-FAA8-49A9-AF94-8B4FAB42713A");
          sc.Parameters.AddWithValue("@mbig", "DD08526D-FAA8-49A9-AF94-8B4FAB42713A");
          sc.Parameters.AddWithValue("@modt", DBNull.Value);
          sc.Parameters.AddWithValue("@mod", DBNull.Value);

          sc.ExecuteNonQuery();
        }

        using (SqlCommand sc = new SqlCommand("INSERT INTO T_Calendar (CalendarGUID, ObjectID, CalendarEntryTypeID, DateTime, AssignedToUserID, Done, " +
          "CreatedByInspectionGUID, ModifiedByInspectionGUID, ModifiedOriginalDateTime, ModifiedOriginalDone) " +
          "VALUES (@guid, @objectid, @ceti, @datetime, @atui, @done, @cbig, @mbig, @modt, @mod)", cnnFI))
        {
          sc.Parameters.AddWithValue("@guid", guid4);
          sc.Parameters.AddWithValue("@objectid", 882474);
          sc.Parameters.AddWithValue("@ceti", 78);
          sc.Parameters.AddWithValue("@datetime", "2004-05-03 16:21:52");
          sc.Parameters.AddWithValue("@atui", 770);
          sc.Parameters.AddWithValue("@done", 1);
          sc.Parameters.AddWithValue("@cbig", "73FE4EF5-B428-460D-B6DC-C74D080004E6");
          sc.Parameters.AddWithValue("@mbig", "73FE4EF5-B428-460D-B6DC-C74D080004E6");
          sc.Parameters.AddWithValue("@modt", DBNull.Value);
          sc.Parameters.AddWithValue("@mod", DBNull.Value);

          sc.ExecuteNonQuery();
        }
      }
    }

    public static void TestGetReports(SqlConnection cnnFI)
    {
      List<Reports> rList = new List<Reports>();

      using (cnnFI)
      {
        using (SqlCommand sc = new SqlCommand("SELECT * FROM T_ReportDocument " +
          "WHERE CreateModifyInfoKey = 'E21CD278-1B41-49F4-BD0D-590B0CC6800C' ORDER BY Title", cnnFI))
        {
          using (SqlDataReader dr = sc.ExecuteReader())
          {
            while (dr.Read())
            {
              Reports r = new Reports();

              r.DocName = Convert.ToString(dr["OriginalFilename"]);
              r.DocLength = Convert.ToInt32(dr["ContentLength"]);
              r.DocContentType = Convert.ToString(dr["ContentType"]);
              object binaryData = dr["DocumentFile"];
              r.DocContent = (byte[])binaryData;

              rList.Add(r);
            }
          }
        }
      }

      foreach (Reports r in rList)
      {
        //System.Diagnostics.Debug.WriteLine("Name: " + r.DocName);
        //System.Diagnostics.Debug.WriteLine("Length: " + r.DocLength);
        //System.Diagnostics.Debug.WriteLine("Type: " + r.DocContentType);
        //System.Diagnostics.Debug.WriteLine("Content: " + r.DocContent);
        //System.Diagnostics.Debug.WriteLine("");

        byte[] bytes;
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, r.DocContent);
        bytes = ms.ToArray();
        File.WriteAllBytes("C:\\Users\\mafh\\desktop\\QAL\\" + r.DocName.Replace("?", ""), bytes);
      }
    }

    public static void TestInsertNewComponent(SqlConnection cnnFI, DateTimeOffset nowOffset, string MAFHGuid)
    {
      Guid g = Guid.NewGuid();

      using (cnnFI)
      {
        //using (SqlCommand sc = new SqlCommand("INSERT INTO [T_GenericValueRow] (GenericValueRowKey, GenericConfigTableKey, ItemKey, VersionId, LastModifiedDate, LastModifiedBy) " +
        //  "VALUES (@1, @2, @3, @4, @5, @6)", cnnFI))
        //{
        //  sc.Parameters.AddWithValue("@1", g);
        //  sc.Parameters.AddWithValue("@2", "41E973F4-E8BA-4A91-8E0E-9BBB6A3F830F");
        //  sc.Parameters.AddWithValue("@3", "3F92E193-3335-40CC-88D9-34CA9E738781");
        //  sc.Parameters.AddWithValue("@4", 13);
        //  sc.Parameters.AddWithValue("@5", nowOffset);
        //  sc.Parameters.AddWithValue("@6", "2F636E2F-C03D-EA11-A835-005056A7F76B");  //MAFHGuid

        //  sc.ExecuteNonQuery();
        //}

        using (SqlCommand sc = new SqlCommand("INSERT INTO [T_GenericValueField] (GenericValueFieldKey, GenericValueRowKey, GenericConfigFieldKey, " +
          "ValueNvarchar, ReadOnly, LastModifiedDate, LastModifiedBy) " +
            "VALUES (@1, @2, @3, @4, @5, @6, @7)", cnnFI))
        {
          sc.Parameters.AddWithValue("@1", g);
          sc.Parameters.AddWithValue("@2", "C4499A18-1AF9-4729-9DC7-3EAC92D47F4E");
          sc.Parameters.AddWithValue("@3", "D7E2BE1E-B7FF-49C0-B990-774AC7555A74");
          sc.Parameters.AddWithValue("@4", "(6HBK51CQ01)");
          sc.Parameters.AddWithValue("@5", 1);
          sc.Parameters.AddWithValue("@6", nowOffset);
          sc.Parameters.AddWithValue("@7", "2F636E2F-C03D-EA11-A835-005056A7F76B");  //MAFHGuid

          sc.ExecuteNonQuery();
        }
      }
    }

    #endregion Test

  }
}
