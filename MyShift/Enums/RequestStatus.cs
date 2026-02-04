using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Enums
{
    public enum RequestStatus
    {
        [Display(Name = "Ожидание")]
        Pending,
        [Display(Name = "Принята")]
        Approved,
        [Display(Name = "Отклонена")]
        Rejected,
        [Display(Name = "Удалена")]
        Removed
    }
}
