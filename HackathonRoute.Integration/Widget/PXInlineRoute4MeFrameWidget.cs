using HackathonRoute.Customization.DAC;
using PX.Dashboards.Widgets;
using PX.Data;
using PX.Objects.EP;
using PX.Objects.SO;
using PX.Web.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HackathonRoute.Integration.Widget
{
    public class PXInlineRoute4MeFrameWidget : PXWidgetBase<InlineFrameMaint, InlineFrameSettings>
    {
        public PXInlineRoute4MeFrameWidget() : base("Embedded Route4Me Widget", "Shows embedded document within a widget.", "dashboards@embed")
        {
        }
        protected override WebControl Render(PXDataSource ds, int height)
        {
            if (string.IsNullOrEmpty(base.Settings.Source))
            {
                return null;
            }
            WebControl webControl = new WebControl(HtmlTextWriterTag.Iframe);
            webControl.CssClass = "iframe";
            WebControl webControl2 = webControl;
            this._frame = webControl;
            WebControl webControl3 = webControl2;
            webControl3.Attributes["frameborder"] = "0";
            webControl3.Width = (webControl3.Height = Unit.Percentage(100.0));
            webControl3.Attributes["src"] = "javascript:void 0";
            return webControl3;
        }
        public override void RenderComplete()
        {
            if (this._frame != null)
            {
                IScriptRenderer renderer = JSManager.GetRenderer(base.Page);
                string source = base.Settings.Source;
                if (source != null)
                {
                    for (; ; )
                    {
                        source = source.TrimStart(Array.Empty<char>());
                        if (!source.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                        source = source.Remove(0, "javascript:".Length);
                    }
                }
                string routeID = "";
                EPEmployee user = PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.Select(base.DataGraph, new object[] { base.DataGraph.Accessinfo.UserID });
                if (user != null)
                {
                    SOShipment shipment = PXSelectGroupBy<SOShipment, Where<SOShipment.shipDate, Equal<Required<SOShipment.shipDate>>,
                        And<SOShipmentExt.usrDriver, Equal<Required<SOShipmentExt.usrDriver>>>>,
                        Aggregate<GroupBy<SOShipment.shipDate,GroupBy<SOShipmentExt.usrDriver>>>>.Select(base.DataGraph, new object[] { base.DataGraph.Accessinfo.BusinessDate, user.BAccountID });
                    var shipmentExt = shipment.GetExtension<SOShipmentExt>();
                    if(shipmentExt!=null && shipmentExt.UsrRouteID.HasValue)
                    {
                        routeID = shipmentExt.UsrRouteID?.ToString().Replace("-","").ToUpperInvariant();
                    }
                    
                }
                renderer.RegisterStartupScript(base.GetType(), this.GenerateControlID(base.WidgetID, ""), string.Concat(new string[]
                {
                    "px.elemByID('",
                    this._frame.ClientID,
                    "').src = '",
                    HttpUtility.JavaScriptStringEncode(source),
                    $"route_id={routeID}",
                    "';"
                }), true);
            }
            base.RenderComplete();
        }
        private WebControl _frame;
    }
}
