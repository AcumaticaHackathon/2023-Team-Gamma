using PX.Data;

namespace HackathonRoute.Customization.DAC
{
    public class VendorClassExt : PXCacheExtension<PX.Objects.AP.VendorClass>
    {
        #region UsrIsDriver
        [PXDBBool]
        [PXUIField(DisplayName = "Is Driver")]

        public virtual bool? UsrIsDriver { get; set; }
        public abstract class usrIsDriver : PX.Data.BQL.BqlBool.Field<usrIsDriver> { }
        #endregion
    }
}