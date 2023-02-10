using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SFA.DAS.EmployerFinance.Interfaces
{
    public interface ICookieService<T>
    {
        void Create(IHttpContextAccessor contextAccessor, string name, T content, int expireDays);
        void Update(IHttpContextAccessor contextAccessor, string name, T content);
        void Delete(IHttpContextAccessor contextAccessor, string name);
        T Get(IHttpContextAccessor contextAccessor, string name);
    }
}
