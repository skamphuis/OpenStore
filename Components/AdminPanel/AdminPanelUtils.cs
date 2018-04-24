using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;
using DotNetNuke.Common;
using DotNetNuke.Entities.Users;
using System.Web.UI.WebControls;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public static class AdminPanelUtils
    {
        private static string _resxpath = "";

        public static String GetMenu(UserInfo userInfo)
        {
            var strOut = "";
            try
            {

                _resxpath = StoreSettings.NBrightBuyPath() + "/App_LocalResources/Plugins.ascx.resx";

                if (userInfo.UserID <= 0)
                {
                    return DnnUtils.GetLocalizedString("notloggedin", _resxpath, Utils.GetCurrentCulture());
                }

                var strCacheKey = "bomenuhtml*" + Utils.GetCurrentCulture() + "*" + PortalSettings.Current.PortalId.ToString("") + "*" + userInfo.UserID.ToString("");

                var obj = Utils.GetCache(strCacheKey);
                if (obj != null) strOut = (String)obj;

                if (StoreSettings.Current.DebugMode || strOut == "")
                {
                    var pluginData = new PluginData(PortalSettings.Current.PortalId);

                    var bomenuattributes = DnnUtils.GetLocalizedString("bomenuattributes", _resxpath, Utils.GetCurrentCulture());
                    var bosubmenuattributes = DnnUtils.GetLocalizedString("bosubmenuattributes", _resxpath, Utils.GetCurrentCulture());

                    //get group list (these are the sections/first level of the menu)
                    var rootList = new Dictionary<String, String>();

                    var pluginList = PluginUtils.GetPluginList();
                    foreach (var p in pluginList)
                    {
                        var grpname = p.GetXmlProperty("genxml/textbox/group");
                        if (p.GetXmlPropertyBool("genxml/checkbox/hidden") == false)
                        {
                            var rootname = grpname;
                            if (rootname == "") rootname = p.GetXmlProperty("genxml/textbox/ctrl");
                            if (!rootList.ContainsKey(rootname))
                            {
                                var resxname = DnnUtils.GetLocalizedString(rootname.ToLower(), _resxpath, Utils.GetCurrentCulture());
                                if (resxname == "") resxname = rootname;
                                rootList.Add(rootname, resxname);
                            }
                        }
                    }

                    strOut = "<ul " + bomenuattributes + ">";

                    // clientEditor roles can only access products, so only add the exit button to the menu.
                    // the security restriuction on product ctrl is applied in the container.ascx.cs
                    //if (!NBrightBuyUtils.IsClientOnly()) 
                    //{
                    foreach (var rootname in rootList)
                    {
                        var rtnlist = pluginData.GetSubList(rootname.Key);
                        var sublist = new List<NBrightInfo>();
                        // check security
                        foreach (var p in rtnlist)
                        {
                            if (CheckSecurity(userInfo, p)) sublist.Add(p);
                        }


                        var href = "#";
                        var ctrl = "";
                        var name = "unknown";
                        var icon = "";
                        var hrefclass = "";
                        var securityrootcheck = true;
                        if (sublist.Count > 0)
                        {
                            // has sub menus
                            ctrl = rootname.Key;
                            name = rootname.Value;
                            hrefclass = "class='dropdown-toggle'";
                            icon = DnnUtils.GetLocalizedString(ctrl.ToLower() + "_icon", _resxpath, Utils.GetCurrentCulture());
                            strOut += "<li class='dropdown'>";
                        }
                        else
                        {
                            // clickable root menu
                            var rootp = pluginData.GetPluginByCtrl(rootname.Key);
                            if (rootp != null)
                            {
                                ctrl = rootp.GetXmlProperty("genxml/textbox/ctrl");
                                name = rootp.GetXmlProperty("genxml/textbox/name");
                                icon = rootp.GetXmlProperty("genxml/textbox/icon");

                                securityrootcheck = CheckSecurity(userInfo, rootp);
                                if (securityrootcheck)
                                {
                                    strOut += "<li>";
                                    var param = new string[1];
                                    param[0] = "ctrl=" + ctrl;
                                    //href = Globals.NavigateURL(TabId, "", param);
                                    href = "/DesktopModules/NBright/NBrightBuy/Admin/AdminPanel.aspx?ctrl=" + ctrl;
                                }
                            }
                            else
                            {
                                securityrootcheck = false;
                            }
                        }
                        if (securityrootcheck) strOut += GetRootLinkNode(name, ctrl, icon, href, hrefclass);

                        if (sublist.Count > 0)
                        {
                            strOut += "<ul " + bosubmenuattributes + ">";
                            foreach (var p in sublist)
                            {
                                if (p.GetXmlPropertyBool("genxml/checkbox/hidden") == false)
                                {

                                    ctrl = p.GetXmlProperty("genxml/textbox/ctrl");
                                    name = p.GetXmlProperty("genxml/textbox/name");
                                    icon = p.GetXmlProperty("genxml/textbox/icon");
                                    var param = new string[1];
                                    param[0] = "ctrl=" + ctrl;
                                    //href = Globals.NavigateURL(TabId, "", param);
                                    href = "/DesktopModules/NBright/NBrightBuy/Admin/AdminPanel.aspx?ctrl=" + ctrl;
                                    strOut += "<li>" + GetSubLinkNode(name, ctrl, icon, href) + "</li>";
                                }
                            }
                            strOut += "</ul>";
                        }
                        if (securityrootcheck) strOut += "</li>";
                    }

                    // }

                    // add exit button
                    strOut += "<li>";
                    var tabid = StoreSettings.Current.Get("exittab");
                    var exithref = "/";
                    if (Utils.IsNumeric(tabid)) exithref = Globals.NavigateURL(Convert.ToInt32(tabid));
                    strOut += GetRootLinkNode("Exit", "exit", DnnUtils.GetLocalizedString("exit_icon", _resxpath, Utils.GetCurrentCulture()), exithref, "");
                    strOut += "</li>";

                    strOut += "</ul>";

                    NBrightBuyUtils.SetModCache(0, strCacheKey, strOut);

                    if (StoreSettings.Current.DebugModeFileOut) Utils.SaveFile(PortalSettings.Current.HomeDirectoryMapPath + "\\debug_menu.html", strOut);
                }
            }
            catch (Exception exc) //Module failed to load
            {
                strOut = exc.ToString();
            }

            return strOut;
        }

        public static Boolean CheckSecurity(UserInfo userInfo, NBrightInfo pluginData)
        {
            if (pluginData.GetXmlPropertyBool("genxml/checkbox/hidden")) return false;

            var roles = pluginData.GetXmlProperty("genxml/textbox/roles");
            if (roles.Trim() == "") roles = StoreSettings.ManagerRole + "," + StoreSettings.EditorRole;
            if (userInfo.IsSuperUser) return true;
            if (userInfo.IsInRole("Administrators")) return true;
            var rlist = roles.Split(',');
            foreach (var r in rlist)
            {
                if (userInfo.IsInRole(r)) return true;
            }
            return false;
        }

        public static String GetRootLinkNode(String name, String ctrl, String icon, String href, String hrefclass)
        {
            var strOutSub = "";
            var dispname = DnnUtils.GetLocalizedString(ctrl.ToLower(), _resxpath, Utils.GetCurrentCulture());
            if (string.IsNullOrEmpty(dispname)) dispname = name;
            strOutSub += "<a " + hrefclass + " href='" + href + "'>" + icon + "<span class='hidden-xs'>" + dispname + "</span></a>";
            return strOutSub;
        }

        public static String GetSubLinkNode(String name, String ctrl, String icon, String href)
        {
            var strOutSub = "";
            var dispname = DnnUtils.GetLocalizedString(ctrl.ToLower(), _resxpath, Utils.GetCurrentCulture());
            if (string.IsNullOrEmpty(dispname)) dispname = name;
            strOutSub += "<a href='" + href + "'>" + icon + dispname + "</a>";
            return strOutSub;
        }

        public static Boolean IsInRoles(UserInfo userInfo,String roleCSV)
        {
            if (roleCSV == "") return true;
            var s = roleCSV.Split(',');
            foreach (var r in s)
            {
                if (userInfo.IsInRole(r)) return true;
            }
            return false;
        }

        public static String GetControlPath(String ctrl)
        {
            var pluginData = new PluginData(PortalSettings.Current.PortalId);
            var p = pluginData.GetPluginByCtrl(ctrl);
            return p.GetXmlProperty("genxml/textbox/path");
        }


    }
}
