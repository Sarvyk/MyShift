using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Data
{
    public interface IRequestRepository
    {
        void GetRequest();
        void ApproveRequest();
        void RejectRequest();
    }
}
