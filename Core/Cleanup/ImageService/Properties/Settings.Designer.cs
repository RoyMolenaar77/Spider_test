﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4927
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ImageService.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=mercurius;Initial Catalog=Spider;Persist Security Info=True;User ID=s" +
            "pider;Password=spider")]
        public string SpiderConnectionString {
            get {
                return ((string)(this["SpiderConnectionString"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("d")]
        public string Setting {
            get {
                return ((string)(this["Setting"]));
            }
            set {
                this["Setting"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("50, 50")]
        public string ImageSizeSmall {
            get {
                return ((string)(this["ImageSizeSmall"]));
            }
            set {
                this["ImageSizeSmall"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("150, 150")]
        public string ImageSizeMedium {
            get {
                return ((string)(this["ImageSizeMedium"]));
            }
            set {
                this["ImageSizeMedium"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("350, 350")]
        public string ImageSizeLarge {
            get {
                return ((string)(this["ImageSizeLarge"]));
            }
            set {
                this["ImageSizeLarge"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("110, 110")]
        public string ImageSizeMasterGroupThumbNail {
            get {
                return ((string)(this["ImageSizeMasterGroupThumbNail"]));
            }
            set {
                this["ImageSizeMasterGroupThumbNail"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("100, 100")]
        public string ImageSizeAuctionOverview {
            get {
                return ((string)(this["ImageSizeAuctionOverview"]));
            }
            set {
                this["ImageSizeAuctionOverview"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("70, 70")]
        public string ImageSizePhotoView {
            get {
                return ((string)(this["ImageSizePhotoView"]));
            }
            set {
                this["ImageSizePhotoView"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("50, 50")]
        public string ImageSizeSearch {
            get {
                return ((string)(this["ImageSizeSearch"]));
            }
            set {
                this["ImageSizeSearch"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("70, 70")]
        public string ImageSizeProductOverview {
            get {
                return ((string)(this["ImageSizeProductOverview"]));
            }
            set {
                this["ImageSizeProductOverview"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("40, 40")]
        public string ImageSizeExtraSmall {
            get {
                return ((string)(this["ImageSizeExtraSmall"]));
            }
            set {
                this["ImageSizeExtraSmall"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("210, 199")]
        public string ImageSizeSubSiteProductGroup {
            get {
                return ((string)(this["ImageSizeSubSiteProductGroup"]));
            }
            set {
                this["ImageSizeSubSiteProductGroup"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=bramlivesrv;Initial Catalog=Bram_Live;Persist Security Info=True;User" +
            " ID=bram_portal;Password=Br@m1e")]
        public string Bram_LiveConnectionString {
            get {
                return ((string)(this["Bram_LiveConnectionString"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=bramlivesrv;Initial Catalog=Bram_Dev_Test;Persist Security Info=True;" +
            "User ID=replication;Password=oct0pu$$y")]
        public string Bram_Dev_TestConnectionString {
            get {
                return ((string)(this["Bram_Dev_TestConnectionString"]));
            }
        }
    }
}
