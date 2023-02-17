using HackathonRoute.Customization.DAC;
using PX.Data;
using PX.Data.Update.WebServices;
using PX.Objects.CS;
using PX.Objects.SO;
using System.Collections;

namespace HackathonRoute.Customization
{
    public class SOShipmentEntryExt : PXGraphExtension<SOShipmentEntry>
    {
        public static bool IsActive()
        {
            return true;
        }
        public virtual void SOShipment_RowSelecting(PXCache sender, PXRowSelectingEventArgs e, PXRowSelecting baseMethod)
        {
            baseMethod?.Invoke(sender, e);
            if (e.Row is SOShipment row)
            {
                Carrier carrier = PXSelect<Carrier, Where<Carrier.carrierID, Equal<Current<SOShipment.shipVia>>>>.Select(this.Base);
                SOShipmentExt rowExt = PXCache<SOShipment>.GetExtension<SOShipmentExt>(row);
                if(carrier == null)
                {
                    rowExt.UsrDeliveryEnabled = true;
                }
                else
                {
                    rowExt.UsrDeliveryEnabled = carrier.IsExternal;
                }
                
            }
        }
        public virtual void SOShipment_ShipVia_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, PXFieldSelecting baseMethod)
        {
            baseMethod?.Invoke(sender, e);
            if (e.Row is SOShipment row)
            {
                Carrier carrier = PXSelect<Carrier, Where<Carrier.carrierID, Equal<Current<SOShipment.shipVia>>>>.Select(this.Base);
                SOShipmentExt rowExt = PXCache<SOShipment>.GetExtension<SOShipmentExt>(row);
                if (carrier == null)
                {
                    rowExt.UsrDeliveryEnabled = true;
                }
                else
                {
                    rowExt.UsrDeliveryEnabled = carrier.IsExternal;
                }
            }
        }
        protected virtual void SOShipment_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected baseMethod)
        {
            baseMethod?.Invoke(sender, e);
            if (e.Row is SOShipment row)
            {
                PickedUp.SetEnabled(row.Status == SOShipmentStatus.Confirmed);
                //PickedUp.SetVisible(row.Status == SOShipmentStatus.Confirmed);
                UsrCompleteDelivery.SetEnabled(row.Status == "O");
               // UsrCompleteDelivery.SetVisible(row.Status == "O");
            }
        }

        public PXAction<SOShipment> PickedUp;
        [PXUIField(DisplayName = "Picked Up")]
        [PXMassAction]
        [PXButton(CommitChanges = true, SpecialType = PXSpecialButtonType.Process)]
        public IEnumerable pickedUp(PXAdapter adapter)
        {
            Base.Document.Current.Status = "O";
            Base.Document.UpdateCurrent();
            Base.Save.Press();
            return adapter.Get();
        }

        public PXAction<SOShipment> UsrCompleteDelivery;
        [PXUIField(DisplayName = "Complete Delivery")]
        [PXMassAction]
        [PXButton(CommitChanges = true,SpecialType = PXSpecialButtonType.Process)]
        public IEnumerable usrCompleteDelivery(PXAdapter adapter)
        {
            Base.Document.Current.Status = "D";
            Base.Document.UpdateCurrent();
            Base.Save.Press();
            return adapter.Get();
        }
    }
}