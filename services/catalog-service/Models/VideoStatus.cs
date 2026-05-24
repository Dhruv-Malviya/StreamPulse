using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace catalog_service.Models
{
    public enum VideoStatus
    {
        Uploading = 0,
        Processing = 1,
        Published = 2,
        Unlisted = 3,
        Deleted = 4
    }
}