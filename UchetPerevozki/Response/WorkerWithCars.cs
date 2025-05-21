using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UchetPerevozki.Response
{
    public class WorkerWithCars
    {
        public WorkerResponse Worker { get; set; }
        public CarResponse Car { get; set; } // Структура была изменена с List на CarResponse
    }
}
