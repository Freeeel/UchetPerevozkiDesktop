using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UchetPerevozki.Response
{
    public class CarParkResponse
    {
        public int id { get; set; }
        public string state_number { get; set; }
        public string model { get; set; }
        public string stamp { get; set; }
    }
}
