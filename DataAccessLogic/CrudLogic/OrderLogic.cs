﻿using DataAccessLogic.DatabaseModels;
using DataAccessLogic.HelperServices;
using DataAccessLogic.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLogic.CrudLogic
{
    public class OrderLogic : ICrudLogic<Order>
    {
        private readonly ApplicationContext context;

        public OrderLogic(ApplicationContext context)
        {
            this.context = context;
        }

        public async Task Create(Order model)
        {
            Item antique = await context.Items
               .Include(item => item.User)
               .FirstOrDefaultAsync(item => item.Id == model.ItemId);
            User user = await context.Users
                .FirstOrDefaultAsync(user => user.UserName == model.UserName);

            SavedList existingList = await context.SavedLists
                .Include(list => list.Item)
                .FirstOrDefaultAsync(list => list.UserId == user.Id);

            Item itemToRemove = existingList.Item
                .Find(item => item.Id == antique.Id);

            existingList.Item.Remove(itemToRemove);

            model.Id = Guid.NewGuid().ToString();
            model.User = user;
            model.Item = antique;
            antique.Status = ItemStatusProvider.GetSoldStatus();

            context.Items.Update(antique);
            await context.Orders.AddAsync(model);
            await context.SaveChangesAsync();            
        }

        public async Task Delete(Order model)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Order>> Read(Order model)
        {
            return await context.Orders
                .Include(orders => orders.User)
                .Include(order => order.Item)
                .Where(orders => model == null
                || model.User != null && !string.IsNullOrWhiteSpace(model.User.UserName) 
                && orders.UserName == model.User.UserName
                || !string.IsNullOrWhiteSpace(model.Id) && orders.Id == model.Id)
                .ToListAsync();
        }

        public async Task Update(Order model)
        {
            throw new NotImplementedException();
        }
    }
}