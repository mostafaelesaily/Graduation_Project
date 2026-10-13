using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
            
        }
    }
}
