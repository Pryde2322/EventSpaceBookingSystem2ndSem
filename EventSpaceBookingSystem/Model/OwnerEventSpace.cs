using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace EventSpaceBookingSystem.Model
{
    class OwnerEventSpace
    {
        //string spaceName;
        //string description;
        //int capacity;
        //int spaceRate;
        //int ratePerHour;
        //int ratePerChair;

        public string? SpaceName { get; set; }
        public string? SpaceAddress { get; set; }
        public string? Description { get; set; }
        public string? ImageURL { get; set; }
        public int Capacity { get; set; }
        public int SpaceRate { get; set; }
        public int RatePerHour { get; set; }
        public int RatePerChair { get; set; }
    }
}
