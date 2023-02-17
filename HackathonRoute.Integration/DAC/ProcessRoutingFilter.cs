using HackathonRoute.Customization.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.AP;
using PX.Objects.Common.Bql;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.SO;
using System;

namespace HackathonRoute.Integration.DAC
{
    [Serializable]
    [PXCacheName("Process Routing Filter")]
    public class ProcessRoutingFilter : IBqlTable
    {
        #region DepartureTime
        [PXDBDateAndTime(DisplayNameTime = "Departure Time", PreserveTime = true)]
        [PXUIField(DisplayName = "Departure Time")]
        public virtual DateTime? DepartureTime { get; set; }
        public abstract class departureTime : BqlDateTime.Field<departureTime> { }
        #endregion

        [PXDBInt()]
        [PXUIField(DisplayName = "Driver")]
        [PXSelector(typeof(Search2<EPEmployee.bAccountID, InnerJoin<VendorClass, On<EPEmployee.vendorClassID, Equal<VendorClass.vendorClassID>>>,
    Where<VendorClassExt.usrIsDriver, Equal<boolTrue>>>), SubstituteKey = typeof(EPEmployee.acctCD), DescriptionField = typeof(EPEmployee.acctName))]
        public virtual int? UsrDriver { get; set; }
        public abstract class usrDriver : BqlString.Field<usrDriver> { }


        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Ship Via")]
        [PXSelector(typeof(Search<Carrier.carrierID>), new Type[]
{
    typeof(Carrier.carrierID),
    typeof(Carrier.description),
    typeof(Carrier.isCommonCarrier),
    typeof(Carrier.confirmationRequired),
    typeof(Carrier.packageRequired)
}, DescriptionField = typeof(Carrier.description), CacheGlobal = true)]
        public virtual string ShipVia { get; set; }
        public abstract class shipVia : BqlString.Field<shipVia> { }

        [SOSiteAvail()]
        public virtual int? SiteID { get; set; }
        public abstract class siteID : BqlInt.Field<siteID> { }
    }
}
