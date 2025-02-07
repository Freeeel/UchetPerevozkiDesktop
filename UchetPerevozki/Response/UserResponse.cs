using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UchetPerevozki
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string Phone { get; set; }
        public DateTime DateOfBirthday { get; set; }
        public string AddressResidential { get; set; }
        public int BankAccountNumber { get; set; }
        public int RoleId { get; set; }

    }
}
