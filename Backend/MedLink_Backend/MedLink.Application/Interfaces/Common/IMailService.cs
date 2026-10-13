using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.Interfaces.Common
{
    public interface IMailService
    {
         Task sendMail(string to, string subject, string body);
    }
}
