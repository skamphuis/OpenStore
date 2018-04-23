// --- Copyright (c) notice NevoWeb ---
//  Copyright (c) 2014 SARL NevoWeb.  www.nevoweb.com. The MIT License (MIT).
// Author: D.C.Lee
// ------------------------------------------------------------------------
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ------------------------------------------------------------------------
// This copyright notice may NOT be removed, obscured or modified without written consent from the author.
// --- End copyright notice --- 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;
using DataProvider = DotNetNuke.Data.DataProvider;

namespace Nevoweb.DNN.NBrightBuy.Admin
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class DashSummary : NBrightBuyAdminBase
    {


        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);
            // inject any pageheader we need
            var nbi = new NBrightInfo();
            nbi.Lang = Utils.GetCurrentCulture();
            nbi.PortalId = PortalId;

            var pageheaderTempl = NBrightBuyUtils.RazorTemplRender("Admin_Dashboard_head.cshtml", 0, "", nbi, "/DesktopModules/NBright/NBrightBuy", "config", Utils.GetCurrentCulture(), StoreSettings.Current.Settings());
            PageIncludes.IncludeTextInHeader(Page, pageheaderTempl);
        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (Page.IsPostBack == false)
                {
                    PageLoad();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                phData.Controls.Add(l);
            }
        }

        private void PageLoad()
        {
            if (NBrightBuyUtils.CheckRights()) // limit module data to NBS security roles
            {
                bool forceRefresh = Utils.RequestParam(Context, "refresh") == "1";
                var statsInfo = GetStats(PortalId, forceRefresh);
                var statsData = new NBrightInfo(true);

                RazorTemplate = "Admin_Dashboard.cshtml";

                statsInfo.PortalId = PortalId;
                statsInfo.ModuleId = 0;
                statsInfo.Lang = Utils.GetCurrentCulture();
                statsInfo.GUIDKey = RazorTemplate;
                statsInfo.ItemID = -1;

                var strOut = NBrightBuyUtils.RazorTemplRender(RazorTemplate, 0, "", statsInfo, "/DesktopModules/NBright/NBrightBuy", "config", Utils.GetCurrentCulture(), StoreSettings.Current.Settings());
                var lit = new Literal();
                lit.Text = strOut;
                phData.Controls.Add(lit);

            }

        }

        #endregion

        private NBrightInfo GetStats(int portalId, bool forceRefresh = false)
        {
            var cachekey = "nbrightbuydashboard*" + PortalId.ToString("");
            var statsInfo = (NBrightInfo)Utils.GetCache(cachekey);

            if (statsInfo == null || StoreSettings.Current.DebugMode || forceRefresh)
            {
                var objCtrl = new NBrightBuyController();
                statsInfo = new NBrightInfo(true);
                
                var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
                var dbOwner = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;

                var statsXml = objCtrl.GetSqlxml("exec " + dbOwner + objQual + "NBrightBuy_DashboardStats " + portalId);
                statsInfo.XMLData = statsXml;
                Utils.SetCache(cachekey, statsInfo);
            }
            return statsInfo;
        }


    }

}
