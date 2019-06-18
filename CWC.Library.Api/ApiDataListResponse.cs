using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CWC.Library.Api
{
    public class ApiDataListResponse<T> : ApiResponse
    {
        private int _page_count;

        public new List<T> data
        {
            get;
            set;
        }

        public int result_count
        {
            get;
            set;
        }

        public int page_count
        {
            get
            {
                bool flag = this._page_count != 0;
                int result;
                if (flag)
                {
                    result = this._page_count;
                }
                else
                {
                    bool flag2 = this.result_count == 0 || this.page_size == 0;
                    if (flag2)
                    {
                        result = 0;
                    }
                    else
                    {
                        int num = (int)Math.Ceiling((double)this.result_count / (double)this.page_size);
                        result = ((num == 0) ? 1 : num);
                    }
                }
                return result;
            }
            set
            {
                _page_count = value;
            }
        }

        private int page_size
        {
            get;
            set;
        }

        private int page_index
        {
            get;
            set;
        }

        public ApiDataListResponse()
        {
        }

        public ApiDataListResponse(int page_size, int page_index)
        {
            this.page_size = page_size;
            this.page_index = page_index;
        }

        public int GetStartRecordIndex()
        {
            return (this.page_index - 1) * this.page_size;
        }
    }
}