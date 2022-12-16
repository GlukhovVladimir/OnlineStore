using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace DataAccessLogic.DatabaseModels
{
    public class User : IdentityUser
    {
        public SavedList SavedList { get; set; }
        public List<Item> Item { get; set; }
    }
}