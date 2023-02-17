using PX.Data;
using PX.Data.BQL;
using PX.Objects.SO;

namespace HackathonRoute.Customization.DAC
{
    public class SOSetupExt : PXCacheExtension<SOSetup>
    {
        [PXDBString(256,IsUnicode = true)]
        [PXUIField(DisplayName = "Route API Key")]
        public virtual string UsrAPIKey { get; set; }
        public abstract class usrAPIKey : BqlString.Field<usrAPIKey> { }
    }
}
