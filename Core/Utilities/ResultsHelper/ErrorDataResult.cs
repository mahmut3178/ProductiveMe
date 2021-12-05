using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.ResultsHelper
{
    public class ErrorDataResult<T> : DataResult<T>, IDataResult<T>
    {
        public ErrorDataResult(T data, string message) : base(true, message, data)
        {

        }
        public ErrorDataResult(T data) : base(false, data)
        {

        }
    }
}
