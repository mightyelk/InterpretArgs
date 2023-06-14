using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpretArgs
{
    public sealed class MandatoryParameterMissingException : Exception
    {
        public MandatoryParameterMissingException(string[] parameterNames) 
            :base($"Mandatory Parameters {string.Join(",", parameterNames)} are missing.")
        {
        }
       
    }
}
