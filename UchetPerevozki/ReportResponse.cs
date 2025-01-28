using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UchetPerevozki
{
    public class ReportResponse
    {
        public int Id { get; set; }
        public string point_departure { get; set; }
        public string type_point_departure { get; set; }
        public string sender { get; set; }
        public string point_destination { get; set; }
        public string type_point_destination { get; set; }
        public string recipient { get; set; }
        public string view_wood { get; set; }
        public int length_wood { get; set; }
        public float volume_wood { get; set; }
        public DateTime report_date_time { get; set; }
        public string assortment_wood_type { get; set; }
        public string variety_wood_type { get; set; }
        public int user_id { get; set; }
    }
}
