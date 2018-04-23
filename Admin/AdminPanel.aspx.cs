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
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Admin;
using Nevoweb.DNN.NBrightBuy.Components;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
using System.Web.UI;
using System.IO;

namespace Nevoweb.DNN.NBrightBuy
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class AdminPanel : Base.NBrightBuyCDefault
    {


        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {

            base.OnInit(e);

        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                // check for new plugins
                PluginUtils.UpdateSystemPlugins();

                #region "load templates"
               
                // insert page header text
                var nbi = new NBrightInfo();
                nbi.Lang = Utils.GetCurrentCulture();
                nbi.PortalId = PortalSettings.Current.PortalId;
                var pageheaderTempl = NBrightBuyUtils.RazorTemplRender("Admin_Panel_head.cshtml", 0, "", nbi, "/DesktopModules/NBright/NBrightBuy", "config", Utils.GetCurrentCulture(), StoreSettings.Current.Settings());
                this.Header.Controls.Add(new LiteralControl(pageheaderTempl));


                // We've split the html page into 3 parts, so we can inject the controls without changing them.
                // Admin_Panel_top.cshtml and Admin_Panel_foot.cshtml make up the whole html, with the ctrl injected as a control onto the page.
                // I think this could be done in all razor, but would mean a rewriter of all the controls, so we use this method.


                var strOut = NBrightBuyUtils.RazorTemplRender("Admin_Panel_top.cshtml", 0, "", nbi, "/DesktopModules/NBright/NBrightBuy", "config", Utils.GetCurrentCulture(), StoreSettings.Current.Settings());
                var lit = new Literal();
                lit.Text = strOut;
                phData.Controls.Add(lit);


                var pluginData = new PluginData(PortalSettings.Current.PortalId);
                var ctrl = Utils.RequestParam(Context, "ctrl");
                if (StoreSettings.Current.Settings().Count == 0) ctrl = "settings";
                if (ctrl == "")
                {
                    ctrl = "dashsummary";
                }
                var ctlpath = AdminPanelUtils.GetControlPath(ctrl);
                if (ctlpath != "" && AdminPanelUtils.CheckSecurity(UserController.Instance.GetCurrentUserInfo(), pluginData.GetPluginByCtrl(ctrl)))
                {
                    // make compatible with running DNN in virtual directory
                    if (HttpContext.Current.Request.ApplicationPath != null && !ctlpath.StartsWith(HttpContext.Current.Request.ApplicationPath)) ctlpath = HttpContext.Current.Request.ApplicationPath + ctlpath;
                    var c2 = LoadControl(ctlpath);
                    phData.Controls.Add(c2);
                }

                var strOut2 = NBrightBuyUtils.RazorTemplRender("Admin_Panel_foot.cshtml", 0, "", nbi, "/DesktopModules/NBright/NBrightBuy", "config", Utils.GetCurrentCulture(), StoreSettings.Current.Settings());
                var lit2 = new Literal();
                lit2.Text = strOut2;
                phData.Controls.Add(lit2);




                #endregion

            }
            catch (Exception exc) //Module failed to load
            {
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                phData.Controls.Add(l);
            }
        }

        #endregion
    }

}
