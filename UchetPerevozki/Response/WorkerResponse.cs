﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UchetPerevozki.Response
{
    public class WorkerResponse : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        // Вспомогательный метод для вызова события PropertyChanged
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Id { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string patronymic { get; set; }
        public DateTime date_of_birthday { get; set; }
        public string phone { get; set; }
        public string address_residential { get; set; }
        public string password { get; set; }
        public string login { get; set; }
        public string bank_account_number { get; set; }
        public int role_id { get; set; }
    }
}
