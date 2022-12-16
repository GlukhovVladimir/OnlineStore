using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLogic.DatabaseModels
{
    public class Order
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string ItemId { get; set; }
        public Item Item { get; set; }
        public User User { get; set; }
    }
}
