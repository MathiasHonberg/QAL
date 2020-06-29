using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web.Http;
using System.Web.Routing;
using Columbo.DataModel.Models;
using Columbo.DataModel.Utility;
using Columbo.ModuleSystem;
using Columbo.StandardModules.Auth;
using Columbo.WebService;
using ForceInspectOnline.WebService.ColumboModules.QalGraph;
using Force.Utilities.ExtensionMethods;

namespace ForceInspectOnline.WebService.ColumboModules.Qal
{
  public class QalGraph : ColumboModuleBase
  {
    #region Constants
    // QALComponent configuration.
    static readonly Guid ConfigKeyName = Guid.Parse("5C92DB46-4BD5-419C-8396-B54F2DA9D36F");
    static readonly Guid ConfigKeyEmissionType = Guid.Parse("8791569A-D4CA-45D1-8740-F732F8C2507C");
    static readonly Guid ConfigKeyMeasureUnitType = Guid.Parse("8D2DF3D9-F1DB-4756-A2E4-23A14435B054");
    static readonly Guid ConfigKeySSpan = Guid.Parse("0C91B85A-50AB-4B58-A04D-A4546E49BEED");
    static readonly Guid ConfigKeySZero = Guid.Parse("FEF0EF60-E343-4C78-B7CD-3FBCDEC73B26");
    static readonly Guid ConfigKeyKKS = Guid.Parse("D7E2BE1E-B7FF-49C0-B990-774AC7555A74");

    // QAL1Inspection configuration.
    static readonly Guid ConfigKeyMaxRange = Guid.Parse("DFCAD6B5-6A74-4912-8A3A-41C6B0A0D60D");
    static readonly Guid ConfigkeyMeasuringValue = Guid.Parse("40F0BCCF-960A-4C97-B986-7073088E2CFC");

    // Reading configuration.
    static readonly Guid ConfigKeyMeasureDateTime = Guid.Parse("ABBA73BC-9801-4821-97A0-927B47BD5AFE");
    static readonly Guid ConfigKeyZero = Guid.Parse("D0183657-A131-49B9-96BA-699BEB86046D");
    static readonly Guid ConfigKeyZeroMeasuringUnitType = Guid.Parse("BCD3A927-822D-411F-98BE-C9E69FFDCE26");
    static readonly Guid ConfigKeySpan = Guid.Parse("A38C61FF-CF09-4EC1-9CEB-C9DD9B313B9D");
    static readonly Guid ConfigKeySpanMeasuringUnitType = Guid.Parse("BAA66A8A-6830-4526-90D2-8861BB3A398A");
    static readonly Guid ConfigKeySpanGas = Guid.Parse("EF6869F6-B6E2-4D90-8E8C-4666B608C682");
    static readonly Guid ConfigKeyZeroGas = Guid.Parse("F1AF840A-E87D-4BC3-AF82-8A36FCD29E86");
    #endregion Constants

    private CommonDataEntities DbCtx { get { return ColumboWebApiApplication.CurrentRequest.DbCtx; } }

    // DO NOT add variables that can be changed from any methods since those values might be changed from another thread.
    // TODO Verify above statement with TMR or THM.

    #region Overrides
    public override void RegisterColumboEvents()
    {
      ModuleHub.Instance.ApplicationStart += ApplicationStart;
    }

    public override ColumboModuleInfo GetColumboModuleInfo()
    {
      return new ColumboModuleInfo()
      {
        Name = "QalGraph",
        Description = "Allows users to generate see QAL graphs.",
        Uuid = "EDA70A60-DE69-4D3D-B588-00F8A29C7048"
      };
    }

    private void ApplicationStart()
    {
      RouteTable.Routes.MapHttpRoute(
        //name: "Graph (REST)",
        name: "QalGraph",
        routeTemplate: "qalgraph/{id}",
        defaults: new { Controller = "QalGraph" },
        constraints: new
        {
          id = @"\b[C-F0-9]{8}(-[C-F0-9]{4}){3}-[C-F0-9]{12}\b" // GUID
        }
      );

      RouteTable.Routes.MapHttpRoute(name: "QalGraph (custom actions)",
        routeTemplate: "qalgraph/{action}",
        defaults: new { Controller = "QalGraph" }
        );
    }

    public override ColumboModuleConfiguration GetDefaultConfiguration()
    {
      return QalGraphConfiguration.Default;
    }

    public static QalGraphConfiguration Configuration
    {
      get { return (QalGraphConfiguration)ModuleHub.Instance.GetConfiguration<QalGraph>(); }
    }
    #endregion Overrides

    #region Operations
    public QalGraphDataContainer GetSpanGraphData(string itemKey)
    {
      return GetGraphData(itemKey, true);
    }

    public QalGraphDataContainer GetZeroGraphData(string itemKey)
    {
      return GetGraphData(itemKey, false);
    }

    public List<Reading> GetReadings(string itemKey)
    {
      // CUSUM Objects.
      Cusum cus = GetCusum(Guid.Parse(itemKey), true);
      Cusum cuz = GetCusum(Guid.Parse(itemKey), false);

      return GetReadings(itemKey, cus, cuz);
    }

    private List<Reading> GetReadings(string itemKey,
      Cusum cus, Cusum cuz)
    {
      // Readings.
      List<Reading> rList = GetReadingCollection(itemKey);

      if (!rList.Any() || cus == null || cuz == null)
        return null;

      double? zeroAdjust;

      foreach (Reading r in rList)
      {
        cuz.Add(r.R_Zero, r.R_ZeroGas);

        double sp = r.R_Span;

        switch (cuz.Status)
        {
          case 0: r.Status = Status.NoDrift; break;
          case 1: r.Status = Status.ZeroPrecision; break;
          case 2: r.Status = Status.ZeroPositiveDriftAutomaticInternalAdjustment; break;
          case 3: r.Status = Status.ZeroNegativeDriftAutomaticInternalAdjustment; break;
          case 4:
            r.Status = Status.ZeroPositiveDriftNonAutomaticInternalAdjustment;
            zeroAdjust = cuz.D_Adjust();
            r.ZeroAdjust = zeroAdjust;
            sp = sp - zeroAdjust.Value;
          break;
          case 5:
            r.Status = Status.ZeroNegativeDriftNonAutomaticInternalAdjustment;
            zeroAdjust = cuz.D_Adjust();
            r.ZeroAdjust = zeroAdjust;
            sp = sp - zeroAdjust.Value;
            break;
          case 6: r.Status = Status.ZeroPositiveDriftNonAutomaticInternalAdjustmentFirstReading; break;
          case 7: r.Status = Status.ZeroNegativeDriftNonAutomaticInternalAdjustmentFirstReading; break;
          default: r.Status = Status.ZeroUnknownDrift; break;
        }

        cus.Add(sp, r.R_SpanGas);

        switch (cus.Status)
        {
          case 0: r.Status = Status.NoDrift; break;
          case 1: r.Status = Status.SpanPrecision; break;
          case 2: r.Status = Status.SpanPositiveDriftAutomaticInternalAdjustment; break;
          case 3: r.Status = Status.SpanNegativeDriftAutomaticInternalAdjustment; break;
          case 4:
            r.Status = Status.SpanPositiveDriftNonAutomaticInternalAdjustment;
            r.SpanAdjust = cus.D_Adjust();
            break;
          case 5:
            r.Status = Status.SpanNegativeDriftNonAutomaticInternalAdjustment;
            r.SpanAdjust = cus.D_Adjust();
            break;
          case 6: r.Status = Status.SpanPositiveDriftNonAutomaticInternalAdjustmentFirstReading; break;
          case 7: r.Status = Status.SpanNegativeDriftNonAutomaticInternalAdjustmentFirstReading; break;
          default: r.Status = Status.SpanUnknownDrift; break;
        }

        // If the current reading has any kind of drift => Stop processing readings.
        if (cuz.Status != (int)Status.NoDrift || cus.Status != (int)Status.NoDrift)
          break;
      }

      return rList;
    }

    public List<SummaryView> GetSummary(int objectId)
    {
      var now = DateTime.Now;
      TimeSpan ts = DateTime.Now - now;

      // Summary list.
      List<Resume> resumeList = new List<Resume>();

      // SummaryView dictionary.
      Dictionary<int, SummaryView> summaryViewDict = new Dictionary<int, SummaryView>();

      //Properties in Resume
      List<string> parentNameList = new List<string>();
      List<string> nameList = new List<string>();
      List<string> kksList = new List<string>();
      List<string> unitList = new List<string>();
      List<double?> maxValueList = new List<double?>();
      List<float?> svsList = new List<float?>();
      List<int?> measuringValueList = new List<int?>();
      List<DateTime?> latestList = new List<DateTime?>();
      List<float?> snaList = new List<float?>();
      List<float?> spaList = new List<float?>();
      List<float?> intersectList = new List<float?>();
      List<float?> hList = new List<float?>();
      List<int?> orderList = new List<int?>();

      // Connect to database.
      string connectionString;
      SqlConnection cnnFI;
      connectionString = DbCtx.Database.Connection.ConnectionString;
      cnnFI = new SqlConnection(connectionString);
      cnnFI.Open();

      using (cnnFI)
      {
        // Get all the Components belonging to the Meters under the chosen Position.
        string sql = @"
        ;WITH ObjectList AS
        (
          SELECT tl.ObjectID, tl.ParentObjectID, tl.ExternalNo, tl.InternalDescription, oot.ObjectTypeID, ot.[Name], tP.ExternalNo AS ParentExternalNo,
          1 AS ListLevel
          FROM T_Object AS tl
          INNER JOIN T_Object_ObjectTypes AS oot ON tl.ObjectID = oot.ObjectID
          INNER JOIN T_ObjectTypes AS ot ON oot.ObjectTypeID = ot.ObjectTypeID
          LEFT JOIN T_Object AS tP ON tl.ParentObjectID = tP.ObjectID
          WHERE tl.ObjectId = @topPositionObjectId

          UNION ALL

          SELECT l.ObjectID, l.ParentObjectID, l.ExternalNo, l.InternalDescription, oot.ObjectTypeID, ot.[Name], tp.ExternalNo AS ParentExternalNo,
          ol.ListLevel + 1 AS ListLevel
          FROM T_Object AS l
          INNER JOIN T_Object_ObjectTypes AS oot ON l.ObjectID = oot.ObjectID
          INNER JOIN T_ObjectTypes AS ot ON oot.ObjectTypeID = ot.ObjectTypeID
          INNER JOIN T_Object AS tP ON l.ParentObjectID = tP.ObjectID
          INNER JOIN ObjectList AS ol ON l.ParentObjectID = ol.ObjectID

        ) SELECT * FROM ObjectList WHERE ObjectTypeId = 21 
          ORDER BY ParentObjectID, ObjectID, ListLevel";

        Dictionary<int, string> cIDList = new Dictionary<int, string>();

        using (SqlCommand sc = new SqlCommand(sql, cnnFI))
        {
          sc.Parameters.AddWithValue("@topPositionObjectId", objectId);

          using (SqlDataReader dr = sc.ExecuteReader())
          {
            while (dr.Read())
            {
              int componentID = Convert.ToInt32(dr["ObjectID"]);
              string parentName = Convert.ToString(dr["ParentExternalNo"]);

              // Only get the Meter name.
              if (parentName.Contains("("))
              {
                int start = parentName.IndexOf("(");

                parentName = parentName.Substring(start + 1);
                parentName = parentName.Substring(0, 6);
              }

              cIDList.Add(componentID, parentName);

            }
          }
        }

        // Create a SummaryView objekt for each Component in the list, and add the ParentName to each.
        foreach (int c in cIDList.Keys)
        {
          SummaryView rv = new SummaryView();
          rv.ParentName = cIDList[c];
          summaryViewDict.Add(c, rv);
        }

        if(cIDList.Any())
        { 
          // Get all the needed Component-properties for the Summary table.
          using (SqlCommand sc = new SqlCommand("SELECT GVR.ItemId, gvf.GenericConfigFieldKey, gvf.ValueDecimal, gvf.ValueNvarchar, gclf.[Name] " +
            "FROM T_GenericValueRow AS GVR INNER JOIN T_GenericValueField AS gvf " +
            "ON gvr.GenericValueRowKey = gvf.GenericValueRowKey " +
            "LEFT JOIN T_GenericConfigLookupField AS gclf ON gvf.GenericConfigLookupFieldKey = gclf.GenericConfigLookupFieldKey " +
            "WHERE GVR.ItemId IN (" + string.Join(",", cIDList.Keys.Select(rtr => rtr.ToString())) + ") " +
            "AND gvf.GenericConfigFieldKey IN (@configKeySSpan, @configKeyKKS, @ConfigKeyMeasureUnitType, @ConfigEmissionType) " +
            "ORDER BY itemid, gvf.GenericConfigFieldKey ", cnnFI))
          {
            sc.Parameters.AddWithValue("@configKeySSpan", ConfigKeySSpan);
            sc.Parameters.AddWithValue("@configKeyKKS", ConfigKeyKKS);
            sc.Parameters.AddWithValue("@ConfigKeyMeasureUnitType", ConfigKeyMeasureUnitType);
            sc.Parameters.AddWithValue("@ConfigEmissionType", ConfigKeyEmissionType);

            using (SqlDataReader dr = sc.ExecuteReader())
            {
              while (dr.Read())
              {
                Guid g1 = Guid.Empty;
                int objectID = Convert.ToInt32(dr["ItemId"]);

                g1 = (Guid)dr["GenericConfigFieldKey"];

                if (g1 == ConfigKeySSpan)
                  summaryViewDict[objectID].SVaerdi = (float)Convert.ToInt32(dr["ValueDecimal"]);

                if (g1 == ConfigKeyKKS)
                  summaryViewDict[objectID].KKS = Convert.ToString(dr["ValueNvarchar"]);

                if (g1 == ConfigKeyMeasureUnitType)
                  summaryViewDict[objectID].Enhed = Convert.ToString(dr["Name"]);

                if (g1 == ConfigKeyEmissionType)
                  summaryViewDict[objectID].Komponent = Convert.ToString(dr["Name"]);
              }
            }
          }
        }

        //Get the NEWEST QAL1 and the MaxRange and MeasuringValue
        foreach (int c in summaryViewDict.Keys)
        {
          using (SqlCommand sc = new SqlCommand("SELECT * FROM T_GenericValueField WHERE GenericValueRowKey = " +
            "(SELECT GenericValueRowKey FROM T_GenericValueRow WHERE ItemKey = " +
            "(SELECT TOP(1) i.InspectionGUID FROM T_Inspection as i " +
            "INNER JOIN T_Inspection_InspectionTypes iit on iit.InspectionGUID = i.InspectionGUID " +
            "WHERE i.ObjectID = @c " +
            "AND iit.InspectionTypeID = 99 " +
            "ORDER BY i.InspectionDateTime DESC))", cnnFI))
          {
            sc.Parameters.AddWithValue("@c", c);

            using (SqlDataReader dr = sc.ExecuteReader())
            {
              while (dr.Read())
              {
                Guid gvf = new Guid();
                gvf = (Guid)dr["GenericConfigFieldKey"];

                if (gvf == ConfigKeyMaxRange)
                {
                  summaryViewDict[c].Maaleomraade = Convert.ToDouble(dr["ValueDecimal"]);
                }
                if (gvf == ConfigkeyMeasuringValue)
                {
                  summaryViewDict[c].GraenseVaerdi = Convert.ToInt32(dr["ValueDecimal"]);
                }
              }
            }
          }

        }

        //Get the newest QAL3/Readings for all the Components
        string sqlTEst = @"
          ;WITH ObjectList AS
          (
            SELECT tl.ObjectID, tl.ParentObjectID,  tl.ExternalNo AS ExternalNumber, tl.InternalDescription, oot.ObjectTypeID, ot.[Name] AS ObjectType,
            1 AS ListLevel
            FROM T_Object AS tl
            INNER JOIN T_Object_ObjectTypes AS oot ON tl.ObjectID = oot.ObjectID
            INNER JOIN T_ObjectTypes AS ot ON oot.ObjectTypeID = ot.ObjectTypeID
            WHERE tl.ObjectId = @topPositionObjectId

            UNION ALL

            SELECT l.ObjectID, l.ParentObjectID, l.ExternalNo AS ExternalNumber, l.InternalDescription, oot.ObjectTypeID, ot.[Name] AS ObjectType,
            ol.ListLevel + 1 AS ListLevel
            FROM T_Object AS l
            INNER JOIN T_Object_ObjectTypes AS oot ON l.ObjectID = oot.ObjectID
            INNER JOIN T_ObjectTypes AS ot ON oot.ObjectTypeID = ot.ObjectTypeID
            INNER JOIN ObjectList AS ol ON l.ParentObjectID = ol.ObjectID
          )

          -- SIMPLIFIED Get all reading values on the newest reading for all components on a position
          SELECT 
            --ReadingValueDateTimeOffset, -- Just for debugging, no need to retrieve in final version.
            tmp3.ObjectId,
            tmp3.ParentObjectId,
            tmp3.ExternalNumber,
            --gcfReading.GenericConfigFieldKey AS ReadingGenericConfigFieldKey3,
            --EmissionTypeGenericConfigLookupFieldKey,
            EmissionTypeName, -- Just for debugging, no need to retrieve in final version.
            EmissionTypeOrder, -- Just for debugging, no need to retrieve in final version.
            --tmp3.InspectionKey, -- Just for debugging, no need to retrieve in final version.
            tmp3.InspectionDateTime, -- Just for debugging, no need to retrieve in final version.
            gvfReading.GenericValueFieldKey AS ReadingGenericValueFieldKey, 
            gvfReading.GenericValueRowKey AS ReadingGenericValueRowKey, 
            gvfReading.GenericConfigFieldKey AS ReadingGenericConfigFieldKey, 
            gvfReading.GenericConfigLookupFieldKey AS ReadingGenericConfigLookupFieldKey, 
            gcfReading.[Name] AS ReadingFieldName, -- Just for debugging, no need to retrieve in final version.
            --gcfReading.[Order] AS ReadingFieldOrder, -- Just for debugging, no need to retrieve in final version.
            gvfReading.ValueDateTimeOffset AS ReadingValueDateTimeOffset, 
            gvfReading.ValueDecimal AS ReadingValueDecimal  
          FROM
          (
            SELECT 
              -- Used to retrieve the newest reading.
              ROW_NUMBER() OVER (PARTITION BY tmp2.InspectionGenericValueRowKey ORDER BY gvfReading.ValueDateTimeOffSet DESC) AS ReadingRowNumber,
              gvfReading.ValueDateTimeOffset AS ReadingValueDateTimeOffset,
              tmp2.ObjectId,
              tmp2.ParentObjectId,
              tmp2.ExternalNumber,
              tmp2.EmissionTypeGenericConfigLookupFieldKey,
              tmp2.EmissionTypeName,
              tmp2.EmissionTypeOrder,
              tmp2.InspectionKey,
              tmp2.InspectionDateTime,
              gvfReading.GenericValueRowKey AS GenericValueRowKey2 
            FROM
            (
              SELECT
                tmp1.ObjectId,
                tmp1.ParentObjectId,
                tmp1.ExternalNumber,
                tmp1.InspectionKey, 
                tmp1.InspectionDateTime, 
                tmp1.InspectionGenericValueRowKey,
                tmp1.EmissionTypeGenericConfigLookupFieldKey,
                tmp1.EmissionTypeOrder,
                tmp1.EmissionTypeName
              FROM
              (
                SELECT
                  -- Used to retrieve the newest QAL 3 inspection.
                  ROW_NUMBER() OVER (PARTITION BY ol.ObjectId ORDER BY i.InspectionDateTime DESC) AS InspectionRowNumber,
                  ol.ObjectId,
                  ol.ParentObjectId,
                  ol.ExternalNumber,
                  i.InspectionGUID AS InspectionKey, 
                  i.InspectionDateTime, 
                  gvrInspection.GenericValueRowKey AS InspectionGenericValueRowKey,
                  gclfEmissionType.GenericConfigLookupFieldKey AS EmissionTypeGenericConfigLookupFieldKey,
                  gclfEmissionType.[Order] AS EmissionTypeOrder,
                  gclfEmissionType.[Name] AS EmissionTypeName
                FROM
                  ObjectList AS ol
                  LEFT JOIN T_Inspection AS i ON ol.ObjectId = i.ObjectID
                  LEFT JOIN T_Inspection_InspectionTypes AS iit ON i.InspectionGUID = iit.InspectionGUID
                  LEFT JOIN T_GenericValueRow AS gvrInspection ON i.InspectionGUID = gvrInspection.ItemKey
                  -- To get the EmissionType of the Component we need to join with the tables: T_GenericValueRow, T_GenericValueField, T_GenericConfigLookupField
                  LEFT JOIN T_GenericValueRow AS gvrObjectInfo ON ol.ObjectId = gvrObjectInfo.ItemId
                  LEFT JOIN T_GenericValueField AS gvfObjectInfo ON gvrObjectInfo.GenericValueRowKey = gvfObjectInfo.GenericValueRowKey
                  LEFT JOIN T_GenericConfigLookupField AS gclfEmissionType ON gvfObjectInfo.GenericConfigLookupFieldKey = gclfEmissionType.GenericConfigLookupFieldKey
                WHERE
                  ol.ObjectTypeId = 21 -- QAL Component
                  AND
                  gvfObjectInfo.GenericConfigFieldKey = '8791569A-D4CA-45D1-8740-F732F8C2507C' -- EmissionType
                  AND
                  (
                    i.InspectionGUID IS NULL 
                    OR 
                    iit.InspectionTypeID = 101 -- Quality Assurance Level 3
                  )
              ) AS tmp1
              WHERE
                -- Only retrieve the newest QAL 3 inspection.
                tmp1.InspectionRowNumber = 1
            ) AS tmp2
            -- Join with multivalue Reading field that are defined on a QAL 3 inspection.
            LEFT JOIN T_GenericValueField AS gvfQal3Inspection ON tmp2.InspectionGenericValueRowKey = gvfQal3Inspection.GenericValueRowKey
            -- Perform join to get all the readings that are coupled to the QAL 3 inspection.
            LEFT JOIN T_GenericValueRow AS gvrReading ON gvfQal3Inspection.GenericValueFieldKey = gvrReading.ItemKey
            -- Perform join to get all the fields that are on these readings.
            LEFT JOIN T_GenericValueField AS gvfReading ON gvrReading.GenericValueRowKey = gvfReading.GenericValueRowKey
            WHERE
              (
                -- Inlcude QAL 3 inspections without readings.
                gvfQal3Inspection.GenericConfigFieldKey IS NULL 
                AND 
                gvfReading.GenericConfigFieldKey IS NULL
              )
              OR
              (
                gvfQal3Inspection.GenericConfigFieldKey = '7929189E-86D2-4295-987E-BA106B43C4D9' -- ReadingType.
                AND
                gvfReading.GenericConfigFieldKey = 'ABBA73BC-9801-4821-97A0-927B47BD5AFE' -- Reading MeasureDateTime field.
              )
          ) AS tmp3
            LEFT JOIN T_GenericValueField AS gvfReading ON tmp3.GenericValueRowKey2 = gvfReading.GenericValueRowKey
            LEFT JOIN T_GenericConfigField AS gcfReading ON gvfReading.GenericConfigFieldKey = gcfReading.GenericConfigFieldKey
          WHERE
            -- Only retrieve the newest reading that are coupled to the QAL 3 inspection.
            tmp3.ReadingRowNumber = 1 
            --AND tmp3.ObjectId != 897926
          ORDER BY
            -- Ensure that each Component is grouped together with other Components coupled to the same Meter
            tmp3.ParentObjectId,
            -- Ensure that the listing of Components follow the order specified for the Emission lookup type.
            EmissionTypeOrder, 
            tmp3.ObjectId, 
            -- Ensure that all Reading fields are grouped together on the same Reading row.
            gvfReading.GenericValueRowKey, 
            -- Ensure that all Reading fields are ordered by the order defined for the Reading configuration.
            gcfReading.[Order]";

        using (SqlCommand sc = new SqlCommand(sqlTEst, cnnFI))
        {
          sc.Parameters.AddWithValue("@topPositionObjectId", objectId);
          DateTimeOffset d = DateTimeOffset.Now;
          decimal? vd = 0;
          float? x1 = 0;
          float? x2 = 0;
          float? y1 = 0;
          float? y2 = 0;

          using (SqlDataReader dr = sc.ExecuteReader())
          {
            while (dr.Read())
            {
              if (dr["ReadingGenericConfigFieldKey"] != DBNull.Value)
              {
                Guid gcfk = new Guid();

                int currentObjectId = Convert.ToInt32(dr["ObjectID"]);

                gcfk = (Guid)dr["ReadingGenericConfigFieldKey"];

                if (dr["ReadingValueDateTimeOffset"] != DBNull.Value)
                {
                  d = (DateTimeOffset)dr["ReadingValueDateTimeOffset"];
                  summaryViewDict[currentObjectId].SenesteQAL3 = d.DateTime;
                }
                if (dr["ReadingValueDecimal"] != DBNull.Value)
                {
                  vd = Convert.ToDecimal(dr["ReadingValueDecimal"]);
                }

                if (gcfk == ConfigKeyZero)      //Zero 
                {
                  x1 = (float)vd;
                }
                if (gcfk == ConfigKeyZeroGas)   //ZeroGas
                {
                  y1 = (float)vd;
                }
                if (gcfk == ConfigKeySpan)      //Span
                {
                  x2 = (float)vd;
                }
                if (gcfk == ConfigKeySpanGas)   //SpanGas
                {
                  y2 = (float)vd;
                }

                // Calculate slope = a
                summaryViewDict[currentObjectId].Slope = (y2 - y1) / (x2 - x1);

                // Calculate intersection with y-axis = b 
                summaryViewDict[currentObjectId].SkaeringMedY = (y1 - (summaryViewDict[currentObjectId].Slope * x1));

                //if (float.IsNaN(r.SkaeringMedY))
                //{
                //  r.SkaeringMedY = 0;
                //}

                //Udregn Seneste nulpunkt afvigelse
                summaryViewDict[currentObjectId].SenesteNAfv = (0 - x1);

                //Udregn seneste span punkts afvigelse
                summaryViewDict[currentObjectId].SenesteSAfv = (y2 - x2);

                if (dr["EmissionTypeOrder"] != DBNull.Value)
                {
                  summaryViewDict[currentObjectId].Order = Convert.ToInt32(dr["EmissionTypeOrder"]);
                }
              }
            }
          }
        }

        List<SummaryView> rvList = new List<SummaryView>();

        foreach (int objectid in summaryViewDict.Keys)
        {
          rvList.Add(summaryViewDict[objectid]);
        }

        List<SummaryView> sortedList = new List<SummaryView>();
        sortedList = rvList.OrderBy(o => o.Order).ToList();

        return sortedList;
      }
    }

    private QalGraphDataContainer GetGraphData(string itemKey, bool span)
    {
      // Container object sent to the caller.
      QalGraphDataContainer result = new QalGraphDataContainer();

      // CUSUM Objects.
      Cusum cus = GetCusum(Guid.Parse(itemKey), true);
      Cusum cuz = GetCusum(Guid.Parse(itemKey), false);
      // Add reference to the Cusum object that should be used to retrieve upper and lower limit lists.
      Cusum cusum = span ? cus : cuz;

      // Readings.
      List<Reading> rList = GetReadings(itemKey, cus, cuz);

      if (!rList.Any() || cusum == null)
        return null;

      // S-Span and S-Zero.
      string titleValue = !cusum.Sams.HasValue ? "" : "S = " + cusum.Sams.Value.ToString("0.0000");
      result.SSpan = span ? titleValue : null;
      result.SZero = !span ? titleValue : null;

      // Define lists that will be insert into the Trace class.
      List<DateTime> dateCollection = new List<DateTime>();
      List<decimal> valueCollection = new List<decimal>();
      List<decimal> valueGasCollection = new List<decimal>();

      // Put the specific data into the Zero, ZeroGas, Span and SpanGas collections.
      foreach (Reading r in rList)
      {
        dateCollection.Add(r.R_Dato);
        double value = span ? r.R_Span : r.R_Zero;
        double valueGas = span ? r.R_SpanGas : r.R_ZeroGas;
        valueCollection.Add((decimal)value);
        valueGasCollection.Add((decimal)valueGas);

        // If an adjustment to zero could be made then an additional point should be added to the trace.
        // This will be rendered as a vertical line starting in the current point and going up or down.
        // We cheat and add 2 points so the trace will go vertically from the current point to the 
        // new calculated point and back to the current point. This is done so if any more readings are
        // added the trace will continue from the current point and not the new calculated point.
        // However this kind of hack should not be necessary since the user or the system should stop adding
        // readings to a QAL 3 once an adjusment state is reached, i.e. there should never be any readings 
        // after a reading with a state other that Status.NoDrift.
        if (span)
        {
          if (r.Status == Status.ZeroPositiveDriftNonAutomaticInternalAdjustment ||
             r.Status == Status.ZeroNegativeDriftNonAutomaticInternalAdjustment)
          {
            dateCollection.Add(r.R_Dato);
            valueCollection.Add((decimal)(r.R_Zero - r.ZeroAdjust)); // NOT Tested
            dateCollection.Add(r.R_Dato);
            valueCollection.Add((decimal)r.R_Zero);
          }
          else if (r.Status == Status.SpanPositiveDriftNonAutomaticInternalAdjustment ||
            r.Status == Status.SpanNegativeDriftNonAutomaticInternalAdjustment)
          {
            dateCollection.Add(r.R_Dato);
            valueCollection.Add((decimal)(r.R_Span - r.SpanAdjust)); // Worked int Test with ARGO object 897730 on inspection 2B16F156-55A9-4782-A773-9FE07A67CDB1 with AIA = false (2020-04-08).
            dateCollection.Add(r.R_Dato);
            valueCollection.Add((decimal)r.R_Span);
          }
        }
      }

      // Unit type.
      // Since there are at least one reading and all readings have been performed 
      // with the same type of unit => take the unit from the first reading.
      result.Unit = rList.First().R_Unit;

      // Add traces.
      result.TraceCollection = GetTraceCollection(span, 
        dateCollection, valueCollection, valueGasCollection,
        cusum.PrecisionUpperLimit.Select(r => (decimal)r).ToList(),
        cusum.PrecisionLowerLimit.Select(r => (decimal)r).ToList(),
        cusum.DriftUpperLimit.Select(r => (decimal)r).ToList(),
        cusum.DriftLowerLimit.Select(r => (decimal)r).ToList());

      return result;
    }

    /// <summary>Create a collection that contain all the traces necessary to render the Span graph.</summary>
    /// <param name="span">
    /// Indicates if traces for the Span graph should be generated.
    /// If false then the traces for the Zero graph will be generated.
    /// </param>
    /// <param name="dateCollection">
    /// A collection of dates on which the readings were performed.
    /// This is used as the x-values for all the traces.
    /// </param>
    /// <param name="valueCollection">
    /// A collection of the span or zero values.
    /// This is used as the y-values of the Span or Zero trace.
    /// </param>
    /// <param name="valueGasCollection">
    /// A collection of the span gas or zero gas values.
    /// This is used as the y-values of the Span Gas or Zero Gas trace.
    /// </param>
    /// <param name="precisionUpperLimit">
    /// A collection of the upper precision limit.
    /// This is used as the y-values of the Upper precision limit trace.
    /// </param>
    /// <param name="precisionLowerLimit">
    /// A collection of the lower precision limit.
    /// This is used as the y-values of the Lower precision limit trace.
    /// </param>
    /// <param name="driftUpperLimit">
    /// A collection of the upper drift limit.
    /// This is used as the y-values of the Upper drift limit trace.
    /// </param>
    /// <param name="driftLowerLimit">
    /// A collection of the lower drift limit.
    /// This is used as the y-values of the Lower drift limit trace.
    /// </param>
    /// <returns>A collection of 6 traces that can be used to render either the Span or Zero graph.</returns>
    private static List<Trace<DateTime, decimal>> GetTraceCollection(bool span,
      List<DateTime> dateCollection, List<decimal> valueCollection, List<decimal> valueGasCollection,
      List<decimal> precisionUpperLimit, List<decimal> precisionLowerLimit,
      List<decimal> driftUpperLimit, List<decimal> driftLowerLimit)
    {
      List<Trace<DateTime, decimal>> traces = new List<Trace<DateTime, decimal>>();

      traces.Add(new Trace<DateTime, decimal>(
        dateCollection.AsReadOnly(),
        precisionUpperLimit.AsReadOnly())
      {
        Name = "Max accept af præcision",
        Mode = ModeType.Lines,
        MarkerStyle = MarkerStyle.Star,
        Type = ScatterType.Scatter,
        LineDash = LineDashType.DashDot,
        LineColor = Color.FromArgb(50, 205, 50),
        LineWidth = 1
      });

      traces.Add(new Trace<DateTime, decimal>(
          dateCollection.AsReadOnly(),
          driftUpperLimit.AsReadOnly())
      {
        Name = "Max accept af drift",
        Mode = ModeType.Lines,
        MarkerStyle = MarkerStyle.Diamond,
        Type = ScatterType.Scatter,
        LineDash = LineDashType.Dot,
        LineColor = Color.FromArgb(0, 0, 255),
        LineWidth = 1
      });

      traces.Add(new Trace<DateTime, decimal>(
          dateCollection.AsReadOnly(),
          valueCollection.AsReadOnly())
      {
        Name = $"Måling {(span ? "SPAN" : "ZERO")}",
        Mode = ModeType.Lines,
        MarkerStyle = MarkerStyle.X,
        Type = ScatterType.None,
        LineDash = LineDashType.Lines,
        LineColor = Color.FromArgb(255, 0, 0),
        LineWidth = 1
      });

      // For now only add a trace for Gas if span == true.
      // OTL must decide if this is correct.
      if (span)
        traces.Add(new Trace<DateTime, decimal>(
          dateCollection.AsReadOnly(),
          valueGasCollection.AsReadOnly())
        {
          Name = $"Måling {(span ? "SPAN" : "ZERO")}-GAS",
          Mode = ModeType.Lines,
          MarkerStyle = MarkerStyle.Circle,
          Type = ScatterType.None,
          LineDash = LineDashType.DashDot,
          LineColor = Color.FromArgb(255, 0, 0),
          LineWidth = 1
        });

      traces.Add(new Trace<DateTime, decimal>(
          dateCollection.AsReadOnly(),
          driftLowerLimit.AsReadOnly())
      {
        Name = "Min. accept af drift",
        Mode = ModeType.Lines,
        MarkerStyle = MarkerStyle.Cross,
        Type = ScatterType.Scatter,
        LineDash = LineDashType.Dot,
        LineColor = Color.FromArgb(0, 0, 255),
        LineWidth = 1
      });

      traces.Add(new Trace<DateTime, decimal>(
          dateCollection.AsReadOnly(),
          precisionLowerLimit.AsReadOnly())
      {
        Name = "min. accept af præcision",
        Mode = ModeType.Lines,
        MarkerStyle = MarkerStyle.Hourglass,
        Type = ScatterType.Scatter,
        LineDash = LineDashType.DashDot,
        LineColor = Color.FromArgb(50, 205, 50),
        LineWidth = 1
      });

      return traces;
    }

    /// <summary>
    /// Gets a CUSUM object for either S-Span or S-Zero.
    /// </summary>
    /// <param name="itemKey">The id of the QAL3 inspection.</param>
    /// <param name="getSSpan">
    /// True:  The S-Span value of the component will be retrieved.
    /// False: The S-Zero value of the component will be retrieved.
    /// </param>
    /// <returns>
    /// A CUSUM object.
    /// </returns>
    private Cusum GetCusum(Guid itemKey, bool getSSpan)
    {
      // TODO Convert aia logic to entity framework.
      // Example of left join: https://www.devcurry.com/2011/01/linq-left-join-example-in-c.html
      #region Find AIA (AutomaticInternalAdjustment) for the Meter on which the Component is located.
      bool aia = false;

      using (SqlConnection connection = new SqlConnection(DbCtx.Database.Connection.ConnectionString))
      {
        string sql = @"
          SELECT TOP 1
            -- op.ObjectID AS ObjectId, -- Just for testing
            -- Assume that AIA is false if it is not registered for the Meter on which the component is located.
            CASE WHEN gvf.ValueBit IS NULL THEN CAST(0 AS bit) ELSE gvf.ValueBit END AS AutomaticInternalAdjustment
          FROM 
            T_Object AS op
            INNER JOIN T_Object AS oc ON op.ObjectID = oc.ParentObjectID
            INNER JOIN T_Inspection AS i ON oc.ObjectID = i.ObjectID
            -- Necessary to support historical data where the AIA field was not registered for a Meter object.
            LEFT JOIN T_GenericValueRow AS gvr ON op.ObjectID = gvr.ItemId
            LEFT JOIN T_GenericValueField AS gvf ON gvr.GenericValueRowKey = gvf.GenericValueRowKey
          WHERE
            i.InspectionGUID = @itemKey
            AND
            (gvr.GenericConfigTableKey IS NULL OR gvr.GenericConfigTableKey = '80C4DF1C-5F71-49A0-8747-D1D1492CBF21') -- QALMeterConfig
            AND
            (gvf.GenericConfigFieldKey IS NULL OR gvf.GenericConfigFieldKey = 'BF5FB983-A608-4C0C-AB2A-BF11577CED35') -- AIA (AutomaticInternalAdjustment)";

        connection.Open();

        using (SqlCommand command = new SqlCommand(sql, connection))
        {
          // Add inspectionKey parameter.
          command.Parameters.AddWithValue("@itemKey", itemKey);
          aia = (bool)command.ExecuteScalar();
        }
      }
      #endregion Find AIA (AutomaticInternalAdjustment) for the Meter on which the Component is located.

      #region Get S-Span or S-Zero of the Component
      Guid genericConfigFieldKey = getSSpan ? ConfigKeySSpan : ConfigKeySZero;

      // Connect to database.
      string connectionString = DbCtx.Database.Connection.ConnectionString;
      SqlConnection cnnFI = new SqlConnection(connectionString);
      cnnFI.Open();

      using (cnnFI)
      {
        // Get the S-Span or S-Zero from the T_GenericValueField table.
        using (SqlCommand sc = new SqlCommand(@"
          SELECT 
            ValueDecimal
          FROM
            T_GenericValueField
          WHERE 
            GenericValueRowKey = 
            (
              SELECT 
                GenericValueRowKey 
              FROM 
                T_GenericValueRow 
              WHERE
                ItemId = 
                (
                  SELECT 
                    ObjectID 
                  FROM 
                    T_Inspection 
                  WHERE
                    InspectionGUID = @itemKey
                )
                AND
                GenericConfigTableKey = '9D94A96B-AE19-41E1-9F19-4756E63643AE' -- QALComponent
            )
            AND 
            GenericConfigFieldKey = @genericConfigFieldKey", cnnFI))
        {
          // Add inspectionKey parameter.
          sc.Parameters.AddWithValue("@itemKey", itemKey);
          // Add GenericConfigFieldKey parameter for either S-Span or S-Zero.
          sc.Parameters.AddWithValue("@genericConfigFieldKey", genericConfigFieldKey);
          
          using (SqlDataReader dr = sc.ExecuteReader())
          {
            while (dr.Read())
            {
              if (TryGetGenericValueFieldDecimal(dr, out double value))
                return new Cusum(value, aia);
            }
          }
        }
      }
      #endregion Get S-Span or S-Zero of the Component

      return null;
    }

    private List<Reading> GetReadingCollection(string itemKey)
    {
      Dictionary<Guid, string> measureUnitTypeDictionary = GetmeasureUnitTypeCollection();
      List<Reading> rList = new List<Reading>();
      Reading r = new Reading();
      Guid previousGVRK = Guid.Empty;

      //Connect to database
      string connectionString;
      SqlConnection cnnFI;
      connectionString = DbCtx.Database.Connection.ConnectionString;
      cnnFI = new SqlConnection(connectionString);
      cnnFI.Open();

      using (cnnFI)
      { 
        using (SqlCommand sc = new SqlCommand(@"
            SELECT
              GenericValueFieldKey,
              GenericValueRowKey,
              GenericConfigFieldKey,
              GenericConfigLookupFieldKey,
              ValueDateTimeOffset,
              ValueDecimal
            FROM
              T_GenericValueField 
            WHERE
              GenericValueRowKey IN 
              (
                SELECT GenericValueRowKey FROM T_GenericValueRow WHERE ItemKey = 
                (
                  SELECT GenericValueFieldKey FROM T_GenericValueField WHERE GenericValueRowKey = 
                  (
                    SELECT GenericValueRowKey FROM T_GenericValueRow WHERE ItemKey = @itemKey
                  )
                )
              ) 
            ORDER BY
              GenericValueRowKey", cnnFI))
        {
          sc.Parameters.AddWithValue("@itemKey", itemKey);

          using (SqlDataReader dr = sc.ExecuteReader())
          {
            while (dr.Read())
            {
              Guid genericValueRowKey = (Guid)dr[T_GenericValueField.GenericValueRowKeyPropertyName];
              Guid genericConfigFieldKey = (Guid)dr[T_GenericValueField.GenericConfigFieldKeyPropertyName];
              Guid? genericConfigLookupFieldKey =
                dr[T_GenericValueField.GenericConfigLookupFieldKeyPropertyName] == DBNull.Value ?
                new Guid?() : (Guid)dr[T_GenericValueField.GenericConfigLookupFieldKeyPropertyName];

              if (previousGVRK == Guid.Empty)
                previousGVRK = genericValueRowKey;

              if (previousGVRK != Guid.Empty && previousGVRK != genericValueRowKey)
              {
                rList.Add(r);
                previousGVRK = genericValueRowKey;
                r = new Reading();
              }

              // Unit
              if (genericConfigFieldKey == ConfigKeyZeroMeasuringUnitType
                && genericConfigLookupFieldKey.HasValue
                && measureUnitTypeDictionary.ContainsKey(genericConfigLookupFieldKey.Value))
                r.R_Unit = measureUnitTypeDictionary[genericConfigLookupFieldKey.Value];
              // Zero.
              else if (genericConfigFieldKey == ConfigKeyZero && TryGetGenericValueFieldDecimal(dr, out double d0))
                r.R_Zero = d0;
              // ZeroGas.
              else if (genericConfigFieldKey == ConfigKeyZeroGas && TryGetGenericValueFieldDecimal(dr, out double d1))
                r.R_ZeroGas = d1;
              // Span.
              else if (genericConfigFieldKey == ConfigKeySpan && TryGetGenericValueFieldDecimal(dr, out double d2))
                r.R_Span = d2;
              // SpanGas.
              else if (genericConfigFieldKey == ConfigKeySpanGas && TryGetGenericValueFieldDecimal(dr, out double d3))
                r.R_SpanGas = d3;
              // Date.
              else if (genericConfigFieldKey == ConfigKeyMeasureDateTime && dr[T_GenericValueField.ValueDateTimeOffsetPropertyName] != DBNull.Value)
                r.R_Dato = ((DateTimeOffset)dr[T_GenericValueField.ValueDateTimeOffsetPropertyName]).DateTime;
            }

            rList.Add(r);
          }
        }

        if (!rList.Any())
          return null;

        // Sort readings so the oldest readings is first.
        // This is necessary to ensure that the CUSUM logic calculates the correct status for each reading.
        rList.Sort((r1, r2) => DateTime.Compare(r1.R_Dato, r2.R_Dato));

        return rList;
      }
    }

    /// <summary>Gets a dictionary containing all the MeasureUnitTypes (e.g. ppm, % etc.)</summary>
    /// <returns>
    /// A dictionary containing:
    /// Key:   The GenericConfigLookupFieldKey
    /// Value: The name of the field (e.g. ppm, %).
    /// </returns>
    private Dictionary<Guid, string> GetmeasureUnitTypeCollection()
    {
      Guid key = Guid.Parse("3E635D54-4977-479E-9E97-9ADCA25D05DC"); // Key of the MeasureUnitType lookup type.

      return
        (from gclf in DbCtx.T_GenericConfigLookupField
         where gclf.GenericConfigLookupTableKey == key
         select new { gclf.GenericConfigLookupFieldKey, gclf.Name }).ToDictionary(r => r.GenericConfigLookupFieldKey, r => r.Name);
    }

    private static bool TryGetGenericValueFieldDecimal(SqlDataReader reader, out double result)
    {
      bool found = false;
      result = 0;

      if (reader[T_GenericValueField.ValueDecimalPropertyName] != DBNull.Value)
      {
        result = (double)Convert.ToDecimal(reader[T_GenericValueField.ValueDecimalPropertyName]);
        found = true;
      }

      return found;
    }

    #region Old QAL database
    public List<Trace<string, decimal>> TestWithHardCode(int testkey, DateTime? inspectionDateFrom = null,
      DateTime? inspectionDateTo = null)
    {
      List<Trace<string, decimal>> result = new List<Trace<string, decimal>>();

      List<decimal> r_zero = new List<decimal> { 0, 0, 1, 0, 0, 4, 0, 0, 0, 1, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0 };
      List<decimal> r_span = new List<decimal> { 103.5m, 102.6m, 104.5m, 105.4m, 102.1m, 102.3m, 102.4m, 101.2m, 99.4m, 100.6m, 102.4m, 100.8m, 94.7m, 95.5m, 97.6m, 101.7m, 103.3m, 98.5m, 92.6m, 100.5m };
      List<decimal> r_spanGas = new List<decimal> { 100.4m, 100.72m, 100.72m, 101.6m, 100.22m, 100.47m, 100.47m, 100.47m, 100.47m, 100.47m, 100.47m, 100.47m, 100.47m, 100.47m, 100.35m, 100.35m, 100.35m, 100.35m, 100.35m, 100.5m };
      List<string> r_date = new List<string> { "2016 - 02 - 27 12:39:00", "2016 - 03 - 17 14:35:00","2016 - 04 - 20 10:59:00","2016 - 06 - 25 08:49:00","2016 - 07 - 21 16:08:00",
        "2016 - 08 - 20 12:52:00","2016 - 09 - 28 12:47:00","2016 - 10 - 27 15:04:00","2016 - 12 - 22 14:06:00","2017 - 03 - 14 14:31:00","2017 - 06 - 19 02:24:00","017 - 12 - 21 11:00:00",
        "2018 - 02 - 01 12:56:00","2018 - 04 - 12 10:59:00","2018 - 05 - 27 15:02:00","2018 - 06 - 04 13:55:00","2018 - 07 - 18 12:00:00","2018 - 08 - 13 14:04:00","2018 - 09 - 06 10:42:00",
        "2018 - 10 - 24 14:13:00" };


      //result.Add(new Trace<string, decimal>(
      //    r_date.AsReadOnly(),
      //    r_zero.AsReadOnly())
      //{
      //  Name = "Måling ZERO",
      //  Mode = ModeType.Lines,
      //  Type = ScatterType.None,
      //  LineDash = LineDashType.Lines,
      //  LineColor = Color.FromArgb(255, 0, 0),
      //  LineWidth = 1
      //});

      result.Add(new Trace<string, decimal>(
          r_date.AsReadOnly(),
          r_span.AsReadOnly())
      {
        Name = "Måling SPAN",
        Mode = ModeType.Lines,
        MarkerStyle = MarkerStyle.Circle,
        Type = ScatterType.None,
        LineDash = LineDashType.Lines,
        LineColor = Color.FromArgb(255, 0, 0),
        LineWidth = 1
      });

      result.Add(new Trace<string, decimal>(
          r_date.AsReadOnly(),
          r_spanGas.AsReadOnly())
      {
        Name = "Måling SPAN-GAS",
        Mode = ModeType.Lines,
        MarkerStyle = MarkerStyle.Diamond,
        Type = ScatterType.None,
        LineDash = LineDashType.DashDot,
        LineColor = Color.FromArgb(255, 0, 0),
        LineWidth = 1
      });

      return result;
    }

    public List<Trace<DateTime, decimal>> TestWithDatabaseSpan(int testkey, DateTime? inspectionDateFrom = null,
      DateTime? inspectionDateTo = null)
    {
      //Define lists that will be insert into the Trace class
      List<Trace<DateTime, decimal>> result = new List<Trace<DateTime, decimal>>();
      List<decimal> r_span = new List<decimal>();
      List<decimal> r_spanGas = new List<decimal>();
      List<DateTime> r_date = new List<DateTime>();
      List<decimal> PrecisionUpperLimit = new List<decimal>();
      List<decimal> PrecisionLowerLimit = new List<decimal>();
      List<decimal> DriftUpperLimit = new List<decimal>();
      List<decimal> DriftLowerLimit = new List<decimal>();

      //CUSUM Objects
      Qal.Cusum Cus = new Qal.Cusum(3.58, true);

      // Connect to database
      string connetionString;
      SqlConnection cnn;
      connetionString = @"Data Source=qal.forcedb.dk;Initial Catalog=QAL;User ID=Qal_ReadOnly;Password=r6trdgdgasfaa4w5466ytrHRDHt";
      cnn = new SqlConnection(connetionString);
      cnn.Open();

      SqlCommand command;
      SqlDataReader dr;
      string sql;
      List<Qal.Reading> rList = new List<Qal.Reading>();

      sql = "SELECT r.R_ID, r.R_QAL3_ID, R_Dato, R_Zero, R_Span, R_SpanGas, R_INI, R_UID, R_SidstRettet, R_Remarks, R_ZeroGas, m.M_ID " +
            "FROM QAL3 AS q " +
            "INNER JOIN Meter AS m ON q.QAL3_MID = m.M_ID " +
            "INNER JOIN Position AS p ON m.M_PID = p.P_ID " +
            "INNER JOIN Reading AS r ON q.QAL3_ID = r.R_QAL3_ID " +
            "WHERE m.M_ID = " + testkey +
            " ORDER BY r.R_Dato";

      command = new SqlCommand(sql, cnn);
      dr = command.ExecuteReader();

      // Read everything from the database, make them an Reading object, and add them to the Reading-list.
      while (dr.Read())
      {
        var op = new Qal.Reading();

        op.R_ID = Convert.ToInt32(dr["R_ID"]);
        op.R_QAL_3 = Convert.ToInt32(dr["R_QAL3_id"]);
        op.R_Dato = Convert.ToDateTime(dr["R_Dato"]);
        op.R_Zero = Convert.ToDouble(dr["R_Zero"]);
        op.R_Span = Convert.ToDouble(dr["R_Span"]);
        op.R_SpanGas = Convert.ToDouble(dr["R_Spangas"]);
        op.R_INI = Convert.ToString(dr["R_INI"]);
        op.R_UID = Convert.ToInt32(dr["R_UID"]);
        op.R_SidstRettet = Convert.ToDateTime(dr["R_SidstRettet"]);
        op.R_Remarks = Convert.ToString(dr["R_Remarks"]);
        op.R_ZeroGas = Convert.ToDouble(dr["R_ZeroGas"]);
        op.m_M_ID = Convert.ToInt32(dr["M_ID"]);

        rList.Add(op);
        Cus.Add(op.R_Span, op.R_SpanGas);
        //System.Diagnostics.Debug.WriteLine(op.R_Span + " | " + op.R_SpanGas);
      }

      // Put the specifik data into the Zero, ZeroGas, Span and SpanGas tables.
      foreach (Qal.Reading q in rList)
      {
        r_span.Add((decimal)q.R_Span);
        r_spanGas.Add((decimal)q.R_SpanGas);
        r_date.Add(q.R_Dato);
      }

      // Convert the Cus arrays to decimals for the Trace class.
      foreach (double d in Cus.PrecisionUpperLimit)
      {
        PrecisionUpperLimit.Add((decimal)d);
      }

      foreach (decimal d in Cus.DriftUpperLimit)
      {
        DriftUpperLimit.Add((decimal)d);
      }

      foreach (decimal d in Cus.DriftLowerLimit)
      {
        DriftLowerLimit.Add((decimal)d);
      }

      foreach (decimal d in Cus.PrecisionLowerLimit)
      {
        PrecisionLowerLimit.Add((decimal)d);
      }

      //Add them to the Trace-result list and return the list to the frontend
      result.Add(new Trace<DateTime, decimal>(
          r_date.AsReadOnly(),
         PrecisionUpperLimit.AsReadOnly())
      {
        Name = "Max accept af præcision",
        Mode = ModeType.Lines,
        MarkerStyle = MarkerStyle.Star,
        Type = ScatterType.Scatter,
        LineDash = LineDashType.DashDot,
        LineColor = Color.FromArgb(50, 205, 50),
        LineWidth = 1
      });

      result.Add(new Trace<DateTime, decimal>(
          r_date.AsReadOnly(),
          DriftUpperLimit.AsReadOnly())
      {
        Name = "Max accept af drift",
        Mode = ModeType.Lines,
        MarkerStyle = MarkerStyle.Diamond,
        Type = ScatterType.Scatter,
        LineDash = LineDashType.Dot,
        LineColor = Color.FromArgb(0, 0, 255),
        LineWidth = 1
      });

      result.Add(new Trace<DateTime, decimal>(
          r_date.AsReadOnly(),
          r_span.AsReadOnly())
      {
        Name = "Måling SPAN",
        Mode = ModeType.Lines,
        MarkerStyle = MarkerStyle.X,
        Type = ScatterType.None,
        LineDash = LineDashType.Lines,
        LineColor = Color.FromArgb(255, 0, 0),
        LineWidth = 1
      });

      result.Add(new Trace<DateTime, decimal>(
          r_date.AsReadOnly(),
          r_spanGas.AsReadOnly())
      {
        Name = "Måling SPAN-GAS",
        Mode = ModeType.Lines,
        MarkerStyle = MarkerStyle.Circle,
        Type = ScatterType.None,
        LineDash = LineDashType.DashDot,
        LineColor = Color.FromArgb(255, 0, 0),
        LineWidth = 1
      });

      result.Add(new Trace<DateTime, decimal>(
          r_date.AsReadOnly(),
          DriftLowerLimit.AsReadOnly())
      {
        Name = "Min. accept af drift",
        Mode = ModeType.Lines,
        MarkerStyle = MarkerStyle.Cross,
        Type = ScatterType.Scatter,
        LineDash = LineDashType.Dot,
        LineColor = Color.FromArgb(0, 0, 255),
        LineWidth = 1
      });

      result.Add(new Trace<DateTime, decimal>(
          r_date.AsReadOnly(),
          PrecisionLowerLimit.AsReadOnly())
      {
        Name = "min. accept af præcision",
        Mode = ModeType.Lines,
        MarkerStyle = MarkerStyle.Hourglass,
        Type = ScatterType.Scatter,
        LineDash = LineDashType.DashDot,
        LineColor = Color.FromArgb(50, 205, 50),
        LineWidth = 1
      });

      return result;
    }

    public List<Trace<DateTime, decimal>> TestWithDatabaseZero(int testkey, DateTime? inspectionDateFrom = null,
      DateTime? inspectionDateTo = null)
    {
      //Define lists that will be insert into the Trace class
      List<Trace<DateTime, decimal>> result = new List<Trace<DateTime, decimal>>();
      List<decimal> r_zero = new List<decimal>();
      List<decimal> r_zeroGas = new List<decimal>();
      List<DateTime> r_date = new List<DateTime>();
      List<decimal> PrecisionUpperLimit = new List<decimal>();
      List<decimal> PrecisionLowerLimit = new List<decimal>();
      List<decimal> DriftUpperLimit = new List<decimal>();
      List<decimal> DriftLowerLimit = new List<decimal>();

      //CUSUM Objects
      Qal.Cusum Cuz = new Qal.Cusum(3.0484, true);

      //Connect to database
      string connetionString;
      SqlConnection cnn;
      connetionString = @"Data Source=qal.forcedb.dk;Initial Catalog=QAL;User ID=Qal_ReadOnly;Password=r6trdgdgasfaa4w5466ytrHRDHt";

      cnn = new SqlConnection(connetionString);
      cnn.Open();

      SqlCommand command;
      SqlDataReader dr;
      string sql;
      List<Qal.Reading> rList = new List<Qal.Reading>();

      sql = "SELECT r.R_ID, r.R_QAL3_ID, R_Dato, R_Zero, R_Span, R_SpanGas, R_INI, R_UID, R_SidstRettet, R_Remarks, R_ZeroGas, m.M_ID " +
            "FROM QAL3 AS q " +
            "INNER JOIN Meter AS m ON q.QAL3_MID = m.M_ID " +
            "INNER JOIN Position AS p ON m.M_PID = p.P_ID " +
            "INNER JOIN Reading AS r ON q.QAL3_ID = r.R_QAL3_ID " +
            "WHERE m.M_ID = " + testkey +
            " ORDER BY r.R_Dato";

      command = new SqlCommand(sql, cnn);
      dr = command.ExecuteReader();

      //Read everything from the database, make them an Reading object, and add them to the Reading-list
      while (dr.Read())
      {
        var op = new Qal.Reading();

        op.R_ID = Convert.ToInt32(dr["R_ID"]);
        op.R_QAL_3 = Convert.ToInt32(dr["R_QAL3_id"]);
        op.R_Dato = Convert.ToDateTime(dr["R_Dato"]);
        op.R_Zero = Convert.ToDouble(dr["R_Zero"]);
        op.R_Span = Convert.ToDouble(dr["R_Span"]);
        op.R_SpanGas = Convert.ToDouble(dr["R_Spangas"]);
        op.R_INI = Convert.ToString(dr["R_INI"]);
        op.R_UID = Convert.ToInt32(dr["R_UID"]);
        op.R_SidstRettet = Convert.ToDateTime(dr["R_SidstRettet"]);
        op.R_Remarks = Convert.ToString(dr["R_Remarks"]);
        op.R_ZeroGas = Convert.ToDouble(dr["R_ZeroGas"]);
        op.m_M_ID = Convert.ToInt32(dr["M_ID"]);

        rList.Add(op);
        Cuz.Add(op.R_Zero, op.R_ZeroGas);
        //System.Diagnostics.Debug.WriteLine(op.R_Span + " | " + op.R_SpanGas);
      }

      //Put the specifik data into the Zero, ZeroGas, Span and SpanGas tables
      foreach (Qal.Reading q in rList)
      {
        r_zero.Add((decimal)q.R_Zero);
        r_zeroGas.Add((decimal)q.R_ZeroGas);
        r_date.Add(q.R_Dato);
      }

      //Convert the Cuz arrays to decimals for the Trace class
      foreach (double d in Cuz.PrecisionUpperLimit)
      {
        PrecisionUpperLimit.Add((decimal)d);
      }

      foreach (decimal d in Cuz.DriftUpperLimit)
      {
        DriftUpperLimit.Add((decimal)d);
      }

      foreach (decimal d in Cuz.DriftLowerLimit)
      {
        DriftLowerLimit.Add((decimal)d);
      }

      foreach (decimal d in Cuz.PrecisionLowerLimit)
      {
        PrecisionLowerLimit.Add((decimal)d);
      }

      //Add them to the Trace-result list and return the list to the frontend
      result.Add(new Trace<DateTime, decimal>(
          r_date.AsReadOnly(),
         PrecisionUpperLimit.AsReadOnly())
      {
        Name = "Max accept af præcision",
        Mode = ModeType.Lines,
        MarkerStyle = MarkerStyle.Star,
        Type = ScatterType.Scatter,
        LineDash = LineDashType.DashDot,
        LineColor = Color.FromArgb(50, 205, 50),
        LineWidth = 1
      });

      result.Add(new Trace<DateTime, decimal>(
          r_date.AsReadOnly(),
          DriftUpperLimit.AsReadOnly())
      {
        Name = "Max accept af drift",
        Mode = ModeType.Lines,
        MarkerStyle = MarkerStyle.Diamond,
        Type = ScatterType.Scatter,
        LineDash = LineDashType.Dot,
        LineColor = Color.FromArgb(0, 0, 255),
        LineWidth = 1
      });

      result.Add(new Trace<DateTime, decimal>(
          r_date.AsReadOnly(),
          r_zero.AsReadOnly())
      {
        Name = "Måling ZERO",
        Mode = ModeType.Lines,
        MarkerStyle = MarkerStyle.Circle,
        Type = ScatterType.None,
        LineDash = LineDashType.Lines,
        LineColor = Color.FromArgb(255, 0, 0),
        LineWidth = 1
      });

      //result.Add(new Trace<DateTime, decimal>(
      //    r_date.AsReadOnly(),
      //    r_zeroGas.AsReadOnly())
      //{
      //  Name = "Måling SPAN-ZERO",
      //  Mode = ModeType.Lines,
      //  Type = ScatterType.None,
      //  LineDash = LineDashType.DashDot,
      //  LineColor = Color.FromArgb(255, 0, 0),
      //  LineWidth = 1
      //});

      result.Add(new Trace<DateTime, decimal>(
          r_date.AsReadOnly(),
          DriftLowerLimit.AsReadOnly())
      {
        Name = "Min. accept af drift",
        Mode = ModeType.Lines,
        MarkerStyle = MarkerStyle.Cross,
        Type = ScatterType.Scatter,
        LineDash = LineDashType.Dot,
        LineColor = Color.FromArgb(0, 0, 255),
        LineWidth = 1
      });

      result.Add(new Trace<DateTime, decimal>(
          r_date.AsReadOnly(),
          PrecisionLowerLimit.AsReadOnly())
      {
        Name = "min. accept af præcision",
        Mode = ModeType.Lines,
        MarkerStyle = MarkerStyle.Hourglass,
        Type = ScatterType.Scatter,
        LineDash = LineDashType.DashDot,
        LineColor = Color.FromArgb(50, 205, 50),
        LineWidth = 1
      });


      return result;
    }

    public List<SummaryView> GetResumeOldDatabase(int mID)
    {
      //Resume List
      List<Resume> resumeList = new List<Resume>();

      //Properties in Resume
      List<string> nameList = new List<string>();
      List<string> enhedList = new List<string>();
      List<double?> måleområdeList = new List<double?>();
      List<float?> svsList = new List<float?>();
      List<int?> grænsevList = new List<int?>();
      List<DateTime?> senesteList = new List<DateTime?>();
      List<float?> snaList = new List<float?>();
      List<float?> spaList = new List<float?>();
      List<float?> skæringList = new List<float?>();
      List<float?> hList = new List<float?>();

      //QAL_ID list
      List<int> mIDList = new List<int>();

      //List<Resume> resumeList = new List<Resume>();

      SqlConnection cnnQal;
      string cStringToQal = @"Data Source=qal.forcedb.dk;Initial Catalog=QAL;User ID=Qal_ReadOnly;Password=r6trdgdgasfaa4w5466ytrHRDHt";
      cnnQal = new SqlConnection(cStringToQal);
      cnnQal.Open();

      using (cnnQal)
      {


        using (SqlCommand sc = new SqlCommand("SELECT * FROM [Meter] WHERE M_PID = @mID", cnnQal))
        {
          sc.Parameters.AddWithValue("@mID", mID);

          using (SqlDataReader dr = sc.ExecuteReader())
          {
            while (dr.Read())
            {
              string s;
              int id;

              s = Convert.ToString(dr["M_Name"]);
              id = Convert.ToInt32(dr["M_ID"]);

              nameList.Add(s);
              mIDList.Add(id);

            }
          }
        }

        foreach (int m in mIDList)
        {
          using (SqlCommand sc = new SqlCommand("SELECT * FROM [QAL1] WHERE QAL1_MID = @m", cnnQal))
          {
            sc.Parameters.AddWithValue("@m", m);

            using (SqlDataReader dr = sc.ExecuteReader())
            {
              while (dr.Read())
              {
                string st;
                double d;
                float f;
                int i;

                st = Convert.ToString(dr["QAL1_Unit"]);
                d = Convert.ToDouble(dr["QAL1_SV"]);
                f = (float)Convert.ToDouble(dr["QAL1_SSPAN"]);
                i = Convert.ToInt32(dr["QAL1_MV"]);

                enhedList.Add(st);
                måleområdeList.Add(d);
                svsList.Add(f);
                grænsevList.Add(i);
              }
            }
          }

          using (SqlCommand q = new SqlCommand("SELECT TOP 1 * FROM Reading " +
                    "WHERE R_QAL3_ID IN " +
                    "(SELECT QAL3_ID FROM QAL3 WHERE QAL3_MID = " + m + ") " +
                    "ORDER BY R_Dato DESC", cnnQal))
          {
            using (SqlDataReader dr = q.ExecuteReader())
            {
              while (dr.Read())
              {
                DateTime d;
                float sna;
                float ssa;
                float a;
                float b;
                float x1;
                float x2;
                float y1;
                float y2;

                d = Convert.ToDateTime(dr["R_Dato"]);
                x1 = (float)Convert.ToDouble(dr["R_Zero"]);
                x2 = (float)Convert.ToDouble(dr["R_Span"]);
                y1 = (float)Convert.ToDouble(dr["R_ZeroGas"]);
                y2 = (float)Convert.ToDouble(dr["R_SpanGas"]);

                //Udregn Hældningskoefficient = a
                a = (y2 - y1) / (x2 - x1);

                //Udregn skæring af Y-akse = b 
                b = (y1 - (a * x1));

                if (float.IsNaN(b))
                {
                  b = 0;
                }

                //Udregn Seneste nulpunkt afvigelse
                sna = (0 - x1);

                //Udregn seneste span punkts afvigelse
                ssa = (y2 - x2);

                senesteList.Add(d);
                snaList.Add(sna);
                spaList.Add(ssa);
                skæringList.Add(b);
                hList.Add(a);
              }
            }
          }
        }
      }
      //Put lists inside the Resume object
      Resume resume = new Resume();
      resume.Komponent = nameList;
      resume.Enhed = enhedList;
      resume.Måleområde = måleområdeList;
      resume.SVærdi = svsList;
      resume.GrænseVærdi = grænsevList;
      resume.SenesteQAL3 = senesteList;
      resume.SenesteNAfv = snaList;
      resume.SenesteSAfv = spaList;
      resume.SkæringMedY = skæringList;
      resume.Slope = hList;

      List<SummaryView> rvList = new List<SummaryView>();

      int ii = 0;

      while (ii < skæringList.Count)
      {
        SummaryView rv = new SummaryView();

        if (resume.Komponent[ii].Contains("("))
        {
          rv.KKS = resume.Komponent[ii].Substring(resume.Komponent[ii].IndexOf("("));
          rv.Komponent = resume.Komponent[ii].Remove(3);
        }
        else
        {
          rv.KKS = resume.Komponent[ii];
          rv.Komponent = resume.Komponent[ii];
        }
        rv.Enhed = enhedList[ii];
        rv.Maaleomraade = måleområdeList[ii];
        rv.SVaerdi = svsList[ii];
        rv.GraenseVaerdi = grænsevList[ii];
        rv.SenesteQAL3 = senesteList[ii];
        rv.SenesteNAfv = snaList[ii];
        rv.SenesteSAfv = spaList[ii];
        rv.SkaeringMedY = skæringList[ii];
        rv.Slope = hList[ii];

        rvList.Add(rv);
        ii++;
      }

      return rvList;
    }
    #endregion Old QAL database
    #endregion Operations
  }
}