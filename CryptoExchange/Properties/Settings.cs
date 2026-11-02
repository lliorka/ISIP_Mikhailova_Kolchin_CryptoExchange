using System.Configuration;
namespace CryptoExchange.Properties {
 [System.Runtime.CompilerServices.CompilerGenerated]
 internal sealed partial class Settings : ApplicationSettingsBase {
  private static Settings defaultInstance=(Settings)Synchronized(new Settings());
  public static Settings Default { get { return defaultInstance; } }
  [UserScopedSetting, DefaultSettingValue("Dark")]
  public string ThemeName { get { return (string)this["ThemeName"]; } set { this["ThemeName"]=value; } }
 }
}
