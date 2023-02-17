using PX.Data;
using PX.Objects.SO;
using HackathonRoute.Customization.Attributes;
using PX.Data.BQL;
using PX.Objects.CS;
using PX.Objects.EP;
using System;
using PX.Objects.AP;

namespace HackathonRoute.Customization.DAC
{
    public class SOShipmentExt : PXCacheExtension<SOShipment>
    {
        [PXDBString(1, IsFixed = true)]
        [DeliveryStatusAttribute.List]
        [PXUIField(DisplayName = "Delivery Status")]
        public virtual string UsrDeliveryStatus { get; set; }
        public abstract class usrDeliveryStatus : BqlString.Field<usrDeliveryStatus> { }

        [PXDBString(25)]
        [PXUIField(DisplayName = "Route Destination ID", Enabled = false)]
        public virtual string UsrRouteDestinationID { get; set; }
        public abstract class usrRouteDestinationID : BqlString.Field<usrRouteDestinationID> { }

        [PXDBGuid]
        [PXUIField(DisplayName = "Route ID", Enabled = false)]
        public virtual Guid? UsrRouteID { get; set; }
        public abstract class usrRouteID : BqlString.Field<usrRouteID> { }

        [PXDBGuid]
        [PXUIField(DisplayName = "Optimization Problem ID", Enabled = false)]
        public virtual Guid? UsrOptimizationProblemID { get; set; }
        public abstract class usrOptimizationProblemID : BqlString.Field<usrOptimizationProblemID> { }

        [PXDBInt()]
        [PXUIField(DisplayName = "Sequence Number", Enabled = false)]
        public virtual int? UsrSequenceNbr { get; set; }
        public abstract class usrSequenceNbr : BqlString.Field<usrSequenceNbr> { }
        
        [PXDBDecimal(2)]
        [PXUIField(DisplayName = "Distance to Destination", Enabled = false)]
        public virtual decimal? UsrDistanceToDestination { get; set; }
        public abstract class usrDistanceToDestination : BqlDecimal.Field<usrDistanceToDestination> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Drive Time to Destination", Enabled = false)]
        public virtual int? UsrAbnormalTrafficTimetoDestination { get; set; }
        public abstract class usrAbnormalTrafficTimetoDestination : BqlInt.Field<usrAbnormalTrafficTimetoDestination> { }
        
        [PXDBInt]
        [PXUIField(DisplayName = "Uncongested Time to Destination")]
        public virtual int? UsrUncongestedTimetoDestination { get; set; }
        public abstract class usrUncongestedTimetoDestination : BqlInt.Field<usrUncongestedTimetoDestination> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Traffic Time to Destination")]
        public virtual int? UsrTrafficTimetoDestination { get; set; }
        public abstract class usrTrafficTimetoDestination : BqlInt.Field<usrTrafficTimetoDestination> { }

        [PXDBDateAndTime]
        [PXUIField(DisplayName = "Delivery Date And Time")]
        public virtual DateTime? UsrDeliveryDateAndTime { get; set; }
        public abstract class usrDeliveryDateAndTime : BqlDateTime.Field<usrDeliveryDateAndTime> { }

        [PXDBInt()]
        [PXUIField(DisplayName = "Driver")]
        [PXSelector(typeof(Search2<EPEmployee.bAccountID, InnerJoin<VendorClass, On<EPEmployee.vendorClassID, Equal<VendorClass.vendorClassID>>>, 
            Where<VendorClassExt.usrIsDriver, Equal<boolTrue>>>), SubstituteKey = typeof(EPEmployee.acctCD), DescriptionField = typeof(EPEmployee.acctName))]
        public virtual int? UsrDriver { get; set; }
        public abstract class usrDriver : BqlString.Field<usrDriver> { }

        [PXBool]
        public virtual bool? UsrDeliveryEnabled { get; set; }
        public abstract class usrDeliveryEnabled : BqlBool.Field<usrDeliveryEnabled> { }
    }
}
