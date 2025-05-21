using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UchetPerevozki.Response
{
    public class CarResponse
    {
        public int CarId { get; set; } // ID автомобиля
        public string state_number { get; set; } // Госномер
        public string model { get; set; } // Модель
        public string stamp { get; set; } // Марка автомобиля
    }
}
