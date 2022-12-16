using System.ComponentModel.DataAnnotations;

namespace WebApplicationTechSale.Models
{
    public class UpdateAccountViewModel
    {
        [DataType(DataType.EmailAddress, ErrorMessage = "Неверный формат")]
        [Display(Name = "Новый адрес электронной почты")]
        public string NewEmail { get; set; }

    }
}