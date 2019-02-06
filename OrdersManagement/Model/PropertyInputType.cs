using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement.Model
{
    public enum PropertyInputType
    {
        [StringValue("Unknown")]
        UNKNOWN = 0,
        [StringValue("TextBox")]
        TEXT_BOX = 1,
        [StringValue("TextArea")]
        TEXT_AREA = 2,
        [StringValue("DropDown")]
        DROP_DOWN = 3,
        [StringValue("RadioButton")]
        RADIO_BUTTON = 4,
        [StringValue("FileUpload")]
        FILE_UPLOAD = 5,
        [StringValue("DateTime")]
        DATE_TIME = 6,
        [StringValue("Label")]
        LABEL = 7
    }
}
