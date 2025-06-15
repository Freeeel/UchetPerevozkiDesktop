using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UchetPerevozki.Response
{
    public class RepairWithUserName
    {
        public int Id { get; set; }
        public string description_breakdown { get; set; }
        public DateTime date_and_time_repair { get; set; }
        public int status_id { get; set; }
        public int user_id { get; set; }
        public string user_name { get; set; }

        public string status_text
        {
            get
            {
                if (status_id == 1)
                {
                    return "Активна";
                }
                else if (status_id == 2)
                {
                    return "Выполнена";
                }
                else
                {
                    return "Неизвестный статус"; // Или другое значение по умолчанию
                }
            }
        }
    }
}
