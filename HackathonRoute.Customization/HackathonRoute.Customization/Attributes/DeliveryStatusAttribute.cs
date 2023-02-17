using PX.Data;
using PX.Data.BQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackathonRoute.Customization.Attributes
{
    internal class DeliveryStatusAttribute
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(PXStringListAttribute.Pair("N", "Picked Up"),
                      PXStringListAttribute.Pair("D", "Delivered"))
            {
            }
        }

        public class pickedUp : BqlType<IBqlString, string>.Constant<pickedUp>
        {
            public pickedUp()
                : base("P")
            {
            }
        }
        public class delivered : BqlType<IBqlString, string>.Constant<delivered>
        {
            public delivered()
                : base("D")
            {
            }
        }

        public const string Delivered = "D";
        public const string PickedUp = "P";
    }
}
