using DataAccessLogic.DatabaseModels;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationTechSale.Models
{
    public class ItemModerationModel
    {
        public Item Item { get; set; }
        public bool Expanded { get; set; }

        [Required(ErrorMessage = "Укажите причину отказа")]
        [MinLength(15, ErrorMessage = "Не менее 15 символов")]
        [MaxLength(250, ErrorMessage = "Не более 250 символов")]
        public string RejectNote { get; set; }
    }
}