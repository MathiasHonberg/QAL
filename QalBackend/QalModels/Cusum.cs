using System;
using System.Collections.Generic;
using System.Linq;

namespace ForceInspectOnline.WebService.ColumboModules.Qal
{

  /// <summary>
  /// Logic based on definitions in standard: DS/EN 14181:2014 second edition 2014-12-05.
  /// </summary>
  public class Cusum
  {
    #region Properties
    /// <summary>Gets/Sets if the equipment uses Automatic Internal Adjustment.</summary>
    public bool AIA { get; set; }
    /// <summary>Gets/Sets the number of samples.</summary>
    public int N { get; set; }
    public int Ns { get; set;  }          // Number of samples since standard deviation different from zero
    public double SumPos { get; set;  }   // Normalized sum for positive difference
    public double SumNeg { get; set; }    // Normalized sum for negative difference
    public double NPos { get; set; }      // Number of samples since positive difference
    public double NNeg { get; set; }      // Number of samples since negative difference
    public double st { get; set; }        // Provisional sum of the standard deviation 
    public double d { get; set; }         // Difference between actual instrument reading of the AMS and the reference value

    //0:ok ,1:præcision, 2:+drift,3:-drift,4:+drift adj.,5:-drift adj.,6:+drift,7:-drift IN JAKOBS CODE
    public int Status { get; private set; }

    /// <summary>Gets the standard deviation of the AMS (automated measuring system) used in QAL3.</summary>
    public double? Sams { get; private set; }

    //Nødvendigt for at tegne kontrolgrænser
    //HUSK!! - automatisk forøgelse af array skal implementeres!
    public List<double> PrecisionUpperLimit = new List<double>();
    public List<double> PrecisionLowerLimit = new List<double>();
    public List<double> DriftUpperLimit = new List<double>();
    public List<double> DriftLowerLimit = new List<double>();
    public double PlotYmax { get; set; }
    public double PlotYmin { get; set; }
    #endregion Properties

    #region Fields
    // Control values     From the 2003 DS/EN 14181 - status = Not valid
    private readonly double hx;     // Test value for detection of drift
    private readonly double kx;     // Constant in the calculation in the provisional sum for positive and negative differences and in the calculation of the required adjustment of the AMS
    private readonly double hs;     // Test value for detection of a decrease in precision
    private readonly double ks;     // Constant in the calculation in the provistional sum for standar deviation
    #endregion Fields

    #region Constructors
    public Cusum(double sams, bool automaticInternalAdjustment)
    {
      Sams = sams;

      // Initier Cusum med ny S (=sigma-ams)
      hx = 2.8467 * sams; // C.4 page 47, hx = 2.85. But we use a more precise value as specified in the MEL-16 document ("MEL16 2004 QA af AMS.pdf").
      kx = 0.5006 * sams; // C.5 page 47, kx = 0.501. But we use a more precise value as specified in the MEL-16 document ("MEL16 2004 QA af AMS.pdf").
      hs = 6.9000 * sams * sams; // C.6 page 48, hs = 6.90. But we use a more precise value as specified in the MEL-16 document ("MEL16 2004 QA af AMS.pdf").
      ks = 1.8484 * sams * sams; // C.7 page 48, ks = 1.85. But we use a more precise value as specified in the MEL-16 document ("MEL16 2004 QA af AMS.pdf").

      AIA = automaticInternalAdjustment;
      N = 0;
      Ns = 0;
      SumPos = 0;
      SumNeg = 0;
      NPos = 0;
      NNeg = 0;
      st = 0;
      d = 0;
      Status = 0;
      PlotYmax = 0;
      PlotYmin = 0;
    }
    #endregion Constructors

    public void Add(double y, double yref)
    {
      double sp = 0;
      double dt = 0;
      double x = 0;

      dt = y - yref;
      sp = st + (dt - d) * (dt - d) / 2 - ks;   //old: C.6, new: C.8

      try
      {
        //We will not need to raise the array's size of 1. We use list to auto increment space in the list - NEEDS TO BE DONE..!
        //ReDim Preserve PrecisionUpperLimit(PrecisionUpperLimit.GetUpperBound(0) + 1)
        //ReDim Preserve PrecisionLowerLimit(PrecisionLowerLimit.GetUpperBound(0) + 1)
        x = (hs - st + ks);

        if (x > 0)
        {
          PrecisionUpperLimit.Add(yref + (d + Math.Sqrt(2 * x)));
          PrecisionLowerLimit.Add(yref + (d - Math.Sqrt(2 * x)));
        }
        else
        {
          if (N > 0)
          {
            PrecisionUpperLimit[0] = PrecisionUpperLimit[N - 1];
          }
          else
          {
            PrecisionUpperLimit[N] = 0;
          }
          if (N > 0)
          {
            PrecisionLowerLimit[0] = PrecisionLowerLimit[N - 1];
          }
          else
          {
            PrecisionLowerLimit[N] = 0;
          }
        }
      }
      catch (Exception e)
      {
        PrecisionUpperLimit[N] = 0;
        PrecisionLowerLimit[N] = 0;
      }

      DriftUpperLimit.Add(hx + kx - SumPos + yref);
      DriftLowerLimit.Add(-(hx + kx - SumNeg) + yref);

      // Looks like we are doing the same twice ???
      if (DriftLowerLimit[N] > DriftUpperLimit[N])
      {
        DriftLowerLimit[N] = DriftUpperLimit[N];
      }
      if (DriftUpperLimit[N] < DriftLowerLimit[N])
      {
        DriftUpperLimit[N] = DriftLowerLimit[N];
      }

      // Husk a.h.t skalering af y-akse på kontrolkort
      if (N == 0)
      {
        PlotYmax = yref + hx;
        PlotYmin = yref - hx;
      }
      else
      {
        if (DriftUpperLimit[N] > PlotYmax)
        {
          PlotYmax = DriftUpperLimit[N];
        }
        if (DriftLowerLimit[N] < PlotYmin)
        {
          PlotYmin = DriftLowerLimit[N];
        }
        if (PrecisionUpperLimit[N] > PlotYmax)
        {
          PlotYmax = PrecisionUpperLimit[N];
        }
        if (PrecisionLowerLimit[N] < PlotYmin)
        {
          PlotYmin = PrecisionLowerLimit[N];
        }
        if (y > PlotYmax)
        {
          PlotYmax = y;
        }
        if (PlotYmin < PlotYmin)
        {
          PlotYmin = y;
        }
      }

      N += 1;
      d = dt;

      SumPos = SumPos + dt - kx;      // Old standard: C.7, new standard: C.9
      SumNeg = SumNeg - dt - kx;      // Old standard: C.8, new standard: C.10

      if (sp > 0)                     // a
      {
        st = sp;
        Ns += 1;
      }
      else
      {
        st = 0;
        Ns = 0;
      }

      if (SumPos > 0)                 // b
      {
        NPos += 1;
      }
      else
      {
        SumPos = 0;
        NPos = 0;
      }

      if (SumNeg > 0)                 // c
      {
        NNeg += 1;
      }
      else
      {
        SumNeg = 0;
        NNeg = 0;
      }

      if (Status == 0)
      {
        if (TestDecreaseOfPrecision())
        {
          Status = 1;
        }
      }

      if (Status == 0)
      {
        Status = TestPositiveDrift();
      }

      if (Status == 0)
      {
        Status = TestNegativeDrift();
      }
    }

    public bool TestDecreaseOfPrecision()
    {
      // Old: C.5, new: C.3.5 
      // Test for decrease of precision
      // If this inequality is false for both zero and span precision then these components of then AMS are in control
      // and the test for zero ang span drift shall be performed.
      // If it is true for either zero or span precision, the manufactorer shall be contacted and then drift test is not needed.  

      if (st > hs)
      {
        //Throw new QALExection(1)
      }

      return (st > hs);
    }

    public int TestPositiveDrift()
    {
      // old: C.6, new: C.3.6
      // Test for drift and the need for adjustment

      if (SumPos > hx)
        // +Drift is detected
      {
        if (AIA)
        {
          // AMS with Automatic Internal Adjustment(AIA)
         // Throw New QALExection(2)           'The AMS is out of order and shall be rectified'
          return 2;
        }
        else
        {
          // AMS without Automatic Internal Adjustment(AIA)

          // In the case of a detected drift , the value of the drift can be estimated
          // and shall be used for adjustment of the AMS 

          // If a drift is detected in either Zero-value or span-value ,
          // AMS shall be adjusted and then other value shall be checked and may be adjusted.

          // If the AMS after correction still gives a signal that results in a detectable drift,
          // the AMS is regarded as out-of-order and shall be rectified.
          // (altså hvis den første aflæsning giver drift - det kan den nemlig godt)
          if (this.N > 1)
          {
            //Throw New QALExection(4)    'Positive drift is detected for a non-AIA AMS'
            return 4;
          }
          else
          {
            // Throw New QALExection(6)    'Positive drift is detected for a non-AIA AMS , 1. aflæsning -> out-of-order'
            return 6;
          }
        }
      }

      return 0;     // 'no drift'
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int TestNegativeDrift()
    {
      // old: C.6, new: C.3.6 
      // Test for drift and the for adjustment

      if (SumNeg > hx)
      // +Drift is detected
      {
        if (AIA)
        {
          // AMS with automatic internal adjustment

         // Throw New QALExection(3)    'The AMS is out of order and shall be rectified'
          return 3;
        }
        else
        {
          // AMS without Automatic Internal Adjustment (AIA)

          // In the case of a detected drift , the value of the drift can be estimated
          // and shall be used for adjustment of the AMS 

          // If a drift is detected in either Zero-value or span-value ,
          // AMS shall be adjusted and then other value shall be checked and may be adjusted.

          // If the AMS after correction still gives a signal that results in a detectable drift,
          // the AMS is regarded as out-of-order and shall be rectified.
          // (altså hvis den første aflæsning giver drift - det kan den nemlig godt)
          if (this.N > 1)
          {
            // Throw New QALExection(5)    'Negative drift is detected for a non-AIA AMS'
            return 5;
          }
          else
          {
            // Throw New QALExection(7)    'Negative drift is detected for a non-AIA AMS , 1. aflæsning -> out-of-order'
            return 7;
          }
        }
      }

      return 0;    // 'no drift'
    }

    public double D_Adjust()
    {
      if (SumPos > hx)
        return 0.7 * (kx + SumPos / NPos);    //old: C.9, new: C.11

      if (SumNeg > hx)
        return -0.7 * (kx + SumNeg / NNeg);   //old: C.10, new: C.12

      return 0;
    }

    public string StatusText()
    {
      //Ask what this is ?
      //return "msg_QALErr" + Status.ToString.Trim;
      return "MAFH TEST";
      // Slå tekster op i globalization-database

      // Select Case Status
      //  Case 1 : Return ("Der er konstateret en formindsket præcision.")
      //  Case 2 : Return ("Der er konstateret en positiv drift (AIA)")
      //  Case 3 : Return ("Der er konstateret en negativ drift (AIA)")
      //  Case 4 : Return ("Der er konstateret en positiv drift, justering skal foretages.")
      //  Case 5 : Return ("Der er konstateret en negativ drift, Justering skal foretages.")
      //  Case 6 : Return ("Der er konstateret en positiv drift på 1. aflæsning.")
      //  Case 7 : Return ("Der er konstateret en negativ drift på 1. aflæsning.")
      // End Select
    }
  }
}