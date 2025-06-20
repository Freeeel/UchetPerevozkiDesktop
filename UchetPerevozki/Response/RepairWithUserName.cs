using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UchetPerevozki.Response
{
    public class RepairWithUserName : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        // Вспомогательный метод для вызова события PropertyChanged
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public int Id { get; set; }
        public string description_breakdown { get; set; } 
        public DateTime date_and_time_repair { get; set; } 
        private int _status_id;
        public int status_id
        {
            get { return _status_id; }
            set
            {
                if (_status_id != value)
                {
                    _status_id = value;
                    OnPropertyChanged(nameof(status_id));

                    OnPropertyChanged(nameof(status_text));
                }
            }
        }
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
                    return "Неизвестный статус";
                }
            }
        }
    }
}
