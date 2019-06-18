using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CWC.Library.Api
{
    public class ApiBack: ApiResponse
    {
        public int return_code
        {
            get;
            set;
        }
    }
}