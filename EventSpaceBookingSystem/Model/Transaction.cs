using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSpaceBookingSystem.Model
{
    public class Transaction
    {
        public string ID { get; set; }
        public string Date { get; set; }
        public string Owner { get; set; }
        public string Space { get; set; }
        public string Amount { get; set; }
    }
}
