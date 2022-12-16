using System.ComponentModel.DataAnnotations;

namespace WebApplicationTechSale.Models
{
    public class PersonalAccountViewModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }

        public ItemViewModel PersonalItemList { get; set; }
    }
}