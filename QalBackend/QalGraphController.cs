using Columbo.DataModel.Models;
using Columbo.DataModel.Utility;
using Columbo.ModuleSystem;
using ForceInspectOnline.WebService.Controllers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ForceInspectOnline.WebService.ColumboModules.Qal
{
  [Columbo.WebService.ModuleSystem.BreezeIntegration.ColumboEntityController]
  public class QalGraphController : FioController
  {

    #region MafhTest
    [HttpGet]
    public HttpResponseMessage TestWithHardCode(int objectId, int? scale, DateTime? fromdate, DateTime? todate)
    {
      try
      {
        QalGraph graphModule = ModuleHub.Instance.GetModule<QalGraph>();
        Guid id = Guid.NewGuid();


        DateTime? inspectionDateFrom = null;
        DateTime? inspectionDateTo = null;
        List<Trace<string, decimal>> traceListTest = graphModule.TestWithHardCode(objectId, inspectionDateFrom, inspectionDateTo);
        return Request.CreateResponse(HttpStatusCode.OK, traceListTest);
      }
      catch (Exception ex)
      {
        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
      }
    }

    [HttpGet]
    public HttpResponseMessage TestWithDatabaseSpan(int objectId, int? scale, DateTime? fromdate, DateTime? todate)
    {
      try
      {
        QalGraph graphModule = ModuleHub.Instance.GetModule<QalGraph>();
        List<Trace<DateTime, decimal>> traceListTest = graphModule.TestWithDatabaseSpan(1080, fromdate, todate);
        return Request.CreateResponse(HttpStatusCode.OK, traceListTest);
      }
      catch (Exception ex)
      {
        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
      }
    }

    [HttpGet]
    public HttpResponseMessage TestWithDatabaseZero(int objectId, int? scale, DateTime? fromdate, DateTime? todate)
    {
      try
      {
        QalGraph graphModule = ModuleHub.Instance.GetModule<QalGraph>();
        List<Trace<DateTime, decimal>> traceListTest = graphModule.TestWithDatabaseZero(1080, fromdate, todate);
        return Request.CreateResponse(HttpStatusCode.OK, traceListTest);
      }
      catch (Exception ex)
      {
        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
      }
    }

    [HttpGet]
    public HttpResponseMessage TestWithNewDatabaseSpan(string itemKey, DateTime? fromdate, DateTime? todate)
    {
      try
      {
        QalGraph graphModule = ModuleHub.Instance.GetModule<QalGraph>();
        QalGraphDataContainer container = graphModule.GetSpanGraphData(itemKey);
        return Request.CreateResponse(HttpStatusCode.OK, container);
      }
      catch (Exception ex)
      {
        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
      }
    }

    [HttpGet]
    public HttpResponseMessage TestWithNewDatabaseZero(string itemKey, DateTime? fromdate, DateTime? todate)
    {
      try
      {
        QalGraph graphModule = ModuleHub.Instance.GetModule<QalGraph>();
        QalGraphDataContainer container = graphModule.GetZeroGraphData(itemKey);
        return Request.CreateResponse(HttpStatusCode.OK, container);
      }
      catch (Exception ex)
      {
        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
      }
    }

    [HttpGet]
    public IHttpActionResult GetReadings(string itemKey)
    {
      QalGraph graphModule = ModuleHub.Instance.GetModule<QalGraph>();
      List<Reading> readings = graphModule.GetReadings(itemKey);

      return Ok(new { readings = readings });
    }

    [HttpGet]
    public IHttpActionResult GetResumeOldDatabase(int mID)
    {
      QalGraph graphModule = ModuleHub.Instance.GetModule<QalGraph>();

      var resume = graphModule.GetResumeOldDatabase(mID);

      return Ok(new { resume = resume });
    }

    [HttpGet]
    public IHttpActionResult GetResumeNewDatabase(int ItemId)
    {
      QalGraph graphModule = ModuleHub.Instance.GetModule<QalGraph>();

      var resume = graphModule.GetSummary(ItemId);

      return Ok(new { resume = resume });
    }
    #endregion MafhTest
  }
}