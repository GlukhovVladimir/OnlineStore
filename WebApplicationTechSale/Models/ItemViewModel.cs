using DataAccessLogic.DatabaseModels;
using System.Collections.Generic;

namespace WebApplicationTechSale.Models
{
    public class ItemViewModel
    {
        public IEnumerable<Item> Item { get; set; }
        public PageViewModel PageViewModel { get; set; }
    }
}