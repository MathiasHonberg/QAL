using Columbo.ModuleSystem;

namespace ForceInspectOnline.WebService.ColumboModules.QalGraph
{
  public class QalGraphConfiguration : ColumboModuleConfiguration
  {
    public static QalGraphConfiguration Default
    {
      get
      {
        return new QalGraphConfiguration()
        {
        };
      }
    }
  }
}