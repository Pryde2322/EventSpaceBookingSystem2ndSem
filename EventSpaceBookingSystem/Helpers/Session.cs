using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSpaceBookingSystem.Helpers
{
    public static class Session
    {
        public static int CurrentOwnerId { get; set; } = -1;
        public static string CurrentOwnerName { get; set; } = string.Empty;
        public static string CurrentOwnerEmail { get; set; } = string.Empty;
    }
}
