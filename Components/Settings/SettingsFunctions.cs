using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using System.Xml;
using DotNetNuke.Common.Utilities;

namespace Nevoweb.DNN.NBrightBuy.Components.Settings
{

    public static class SettingsFunctions
    {
        #region "Client Admin Methods"

        public static string TemplateRelPath = "/DesktopModules/NBright/NBrightBuy";

        public static string ProcessCommand(string paramCmd,HttpContext context)
        {

            var ModCtrl = new NBrightBuyController();
            var strOut = "CLIENT - ERROR!! - No Security rights for current user!";
            if (NBrightBuyUtils.CheckManagerRights())
            {
                var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
                var userId = ajaxInfo.GetXmlPropertyInt("genxml/hidden/userid");

                switch (paramCmd)
                {
                    case "settings_admin_get":
                        if (!NBrightBuyUtils.CheckRights()) break;
                        strOut = SettingsAdminDetail(context);
                        break;
                    case "settings_admin_save":
                        Update(context);
                        ShareProducts();
                        break;
                    case "settings_removelogo":
                        var settings = ModCtrl.GetByGuidKey(PortalSettings.Current.PortalId, 0, "SETTINGS", "NBrightBuySettings");
                        if (settings != null && settings.GetXmlProperty("genxml/hidden/hidemaillogo") != "")
                        {
                            settings.SetXmlProperty("genxml/hidden/hidemaillogo", "");
                            settings.SetXmlProperty("genxml/hidden/emaillogourl", "");
                            settings.SetXmlProperty("genxml/hidden/emaillogopath", "");
                            ModCtrl.Update(settings);
                        }
                        break;
                }
            }
            return strOut;
        }




        private static void ShareProducts()
        {
            var ModCtrl = new NBrightBuyController();
            var settings = ModCtrl.GetByGuidKey(PortalSettings.Current.PortalId, 0, "SETTINGS", "NBrightBuySettings");
            if (settings != null)
            {
                StoreSettings.Refresh(); // make sure we pickup changes.
                var shareproducts = StoreSettings.Current.GetBool("shareproducts");
                var sharedproductsflag = StoreSettings.Current.GetBool("sharedproductsflag");
                if (shareproducts)
                {
                    // we only want to do this if the shareproducts has changed, so use a flag.
                    if (!sharedproductsflag)
                    {
                        var l = ModCtrl.GetList(PortalSettings.Current.PortalId, -1, "PRD");
                        foreach (var i in l)
                        {
                            SharedRecord(i);
                        }
                        l = ModCtrl.GetList(PortalSettings.Current.PortalId, -1, "PRDLANG");
                        foreach (var i in l)
                        {
                            SharedRecord(i);
                        }
                        l = ModCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATEGORY");
                        foreach (var i in l)
                        {
                            SharedRecord(i);
                        }
                        l = ModCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATEGORYLANG");
                        foreach (var i in l)
                        {
                            SharedRecord(i);
                        }

                        settings.SetXmlProperty("genxml/checkbox/sharedproductsflag", "True"); // set flag
                        ModCtrl.Update(settings);
                    }
                }
                else
                {
                    // test if want to reverse the share products, by using the flag.
                    if (sharedproductsflag)
                    {
                        var l = ModCtrl.GetList(PortalSettings.Current.PortalId, -1, "PRD");
                        foreach (var i in l)
                        {
                            UnSharedRecord(i);
                        }
                        l = ModCtrl.GetList(PortalSettings.Current.PortalId, -1, "PRDLANG");
                        foreach (var i in l)
                        {
                            UnSharedRecord(i);
                        }
                        l = ModCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATEGORY");
                        foreach (var i in l)
                        {
                            UnSharedRecord(i);
                        }
                        l = ModCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATEGORYLANG");
                        foreach (var i in l)
                        {
                            UnSharedRecord(i);
                        }

                        settings.SetXmlProperty("genxml/checkbox/sharedproductsflag", "False"); // set flag
                        ModCtrl.Update(settings);
                    }
                }
            }
        }

        private static void SharedRecord(NBrightInfo i)
        {
            var ModCtrl = new NBrightBuyController();
            var createdportalid = i.PortalId;
            if (createdportalid == -1) createdportalid = PortalSettings.Current.PortalId; // previously shared record, so defualt to current.
            i.SetXmlProperty("genxml/createdportalid", createdportalid.ToString(""));
            i.PortalId = -1;
            ModCtrl.Update(i);
        }
        private static void UnSharedRecord(NBrightInfo i)
        {
            var ModCtrl = new NBrightBuyController();
            var createdportalid = PortalSettings.Current.PortalId; // default previously shared record to this portal.
            if (Utils.IsNumeric(i.GetXmlProperty("genxml/createdportalid"))) createdportalid = i.GetXmlPropertyInt("genxml/createdportalid");
            i.PortalId = createdportalid;
            ModCtrl.Update(i);
        }

        private static void Update(HttpContext context)
        {
            var ModCtrl = new NBrightBuyController();

            ModCtrl.SavePluginSinglePageData(context);

            var settings = ModCtrl.GetPluginSinglePageData("NBrightBuySettings", "SETTINGS",Utils.GetCurrentCulture());

            var sharedflag = settings.GetXmlProperty("genxml/checkbox/sharedproductsflag"); //maintain shared flag

            if (settings.GetXmlProperty("genxml/hidden/hidemaillogo") != "")
            {
                settings.SetXmlProperty("genxml/hidden/emaillogourl", StoreSettings.Current.FolderImages + "/" + settings.GetXmlProperty("genxml/hidden/hidemaillogo"));
                settings.SetXmlProperty("genxml/hidden/emaillogopath", StoreSettings.Current.FolderImagesMapPath + "\\" + settings.GetXmlProperty("genxml/hidden/hidemaillogo"));
            }

            settings.SetXmlProperty("genxml/hidden/backofficetabid", PortalSettings.Current.ActiveTab.TabID.ToString(""));

            settings.SetXmlProperty("genxml/checkbox/sharedproductsflag", sharedflag); //maintain shared flag

            ModCtrl.Update(settings);

            if (StoreSettings.Current.DebugModeFileOut) settings.XMLDoc.Save(PortalSettings.Current.HomeDirectoryMapPath + "\\debug_Settings.xml");

            // create upload folders
            var folder = StoreSettings.Current.FolderNBStoreMapPath;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            folder = StoreSettings.Current.FolderImagesMapPath;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            folder = StoreSettings.Current.FolderDocumentsMapPath;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            folder = StoreSettings.Current.FolderUploadsMapPath;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            folder = StoreSettings.Current.FolderClientUploadsMapPath;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            folder = StoreSettings.Current.FolderTempMapPath;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            //Create default category grouptype
            var l = NBrightBuyUtils.GetCategoryGroups(Utils.GetCurrentCulture(), true);
            var g = from i in l where i.GetXmlProperty("genxml/textbox/groupref") == "cat" select i;
            if (!g.Any()) CreateGroup("cat", "Categories", "2");
            if (l.Count == 0)
            {
                // read init xml config file.
                string relPath = "/DesktopModules/NBright/NBrightBuy/Themes/config/default";
                var fullpath = HttpContext.Current.Server.MapPath(relPath);
                var strXml = Utils.ReadFile(fullpath + "\\setup.config");
                var nbi = new NBrightInfo();
                nbi.XMLData = strXml;

                var xmlNodes = nbi.XMLDoc?.SelectNodes("root/group");
                if (xmlNodes != null)
                {
                    foreach (XmlNode xmlNod in xmlNodes)
                    {
                        if (xmlNod.Attributes?["groupref"] != null)
                        {
                            g = from i in l where i.GetXmlProperty("genxml/textbox/groupref") == xmlNod.Attributes["groupref"].Value select i;
                            if (!g.Any()) CreateGroup(xmlNod.Attributes["groupref"].Value, xmlNod.Attributes["groupname"].Value, xmlNod.Attributes["grouptype"].Value);
                        }
                    }
                }
            }

            //remove all cahce setting from cache for reload
            //DNN is sticky with some stuff (had some issues with email addresses not updating), so to be sure clear it all. 
            DataCache.ClearCache();

        }

        private static void CreateGroup(String groupref, String name, String groupType)
        {
            var n = new GroupData(-1, StoreSettings.Current.EditLanguage);
            n.Ref = groupref;
            n.Name = name;
            n.Type = groupType;
            n.DataRecord.GUIDKey = groupref;
            n.Save();
            n.Validate();
        }

        public static String SettingsAdminDetail(HttpContext context)
        {
            try
            {
                if (NBrightBuyUtils.CheckRights())
                {
                    var ModCtrl = new NBrightBuyController();
                    var ajaxInfo = NBrightBuyUtils.GetAjaxInfo(context);
                    var settings = ModCtrl.GetPluginSinglePageData("NBrightBuySettings", "SETTINGS", Utils.GetCurrentCulture());

                    var strOut = "";
                    var themeFolder = ajaxInfo.GetXmlProperty("genxml/hidden/themefolder");
                    if (themeFolder == "") themeFolder = "config";
                    var razortemplate = ajaxInfo.GetXmlProperty("genxml/hidden/razortemplate");

                    var passSettings = NBrightBuyUtils.GetPassSettings(ajaxInfo);

                    var objCtrl = new NBrightBuyController();
                    strOut = NBrightBuyUtils.RazorTemplRender(razortemplate, 0, "", settings, TemplateRelPath, themeFolder, Utils.GetCurrentCulture(), passSettings);
                    return strOut;
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        #endregion


    }
}
