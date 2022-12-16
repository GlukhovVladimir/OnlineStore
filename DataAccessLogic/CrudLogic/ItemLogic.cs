using DataAccessLogic.DatabaseModels;
using DataAccessLogic.HelperServices;
using DataAccessLogic.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLogic.CrudLogic
{
    public class ItemLogic : ICrudLogic<Item>, IPagination<Item>
    {
        private readonly ApplicationContext context;

        public ItemLogic(ApplicationContext context)
        {
            this.context = context;
        }

        public async Task Create(Item model)
        {
            if (model.User == null || string.IsNullOrWhiteSpace(model.User.UserName))
            {
                throw new Exception("Пользователь не определен");
            }

            Item sameItem = await context.Items
                .Include(item => item.User)
                .FirstOrDefaultAsync(item =>
                item.User.UserName == model.User.UserName && item.Name == model.Name);
            if (sameItem != null)
            {
                throw new Exception("Уже есть товар с таким названием");
            }

            model.Status = ItemStatusProvider.GetOnModerationStatus();
            model.Id = Guid.NewGuid().ToString();
            model.Date = DateTime.Now;
            model.User = await context.Users.FirstAsync(user =>
            user.UserName == model.User.UserName);

            await context.Items.AddAsync(model);
            await context.SaveChangesAsync();
        }

        public async Task Delete(Item model)
        {
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                throw new Exception("Товар не определен");
            }

            Item toDelete = await context.Items.FirstOrDefaultAsync(item =>
            item.Id == model.Id);
            if (toDelete == null)
            {
                throw new Exception("Товар не найден");
            }

            context.Items.Remove(toDelete);
            await context.SaveChangesAsync();
        }

        public async Task Update(Item model)
        {
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                throw new Exception("Товар не определен");
            }

            if (model.User != null)
            {
                Item sameItem = await context.Items
                .Include(item => item.User)
                .FirstOrDefaultAsync(item =>
                item.User.UserName == model.User.UserName && item.Name == model.Name);
                if (sameItem != null)
                {
                    throw new Exception("Уже есть товар с таким названием");
                }
            }

            Item toUpdate = await context.Items.FirstOrDefaultAsync(item =>
            item.Id == model.Id);
            if (toUpdate == null)
            {
                throw new Exception("Товар не найден");
            }
            toUpdate.Status = string.IsNullOrWhiteSpace(model.Status) ? toUpdate.Status : model.Status;
            toUpdate.Name = string.IsNullOrWhiteSpace(model.Name) ? toUpdate.Name : model.Name;
            toUpdate.Description = string.IsNullOrWhiteSpace(model.Description) ? toUpdate.Description : model.Description;
            toUpdate.PhotoSrc = string.IsNullOrWhiteSpace(model.PhotoSrc) ? toUpdate.PhotoSrc : model.PhotoSrc;

            await context.SaveChangesAsync();
        }

        public async Task<List<Item>> Read(Item model)
        {
            return await context.Items.Include(item => item.User).Include(item => item.Note).Where(item => model == null
            || model.User != null && !string.IsNullOrWhiteSpace(model.User.UserName) && item.User.UserName == model.User.UserName
            || !string.IsNullOrWhiteSpace(model.Id) && item.Id == model.Id
            || !string.IsNullOrWhiteSpace(model.Status) && item.Status == model.Status)
            .ToListAsync();
        }

        public async Task<List<Item>> GetPage(int pageNumber, Item model)
        {
            return await context.Items.Include(item => item.User).Where(item => model == null
            || !string.IsNullOrWhiteSpace(model.Status) && item.Status == model.Status
            || model.User != null && item.User == model.User)
            .Skip((pageNumber <= 0 ? 0 : pageNumber - 1) *
            ApplicationConstantsProvider.GetPageSize())
            .Take(ApplicationConstantsProvider.GetPageSize())
            .ToListAsync();
        }

        public async Task<int> GetCount(Item model)
        {
            return await context.Items.CountAsync(item => model == null
            || !string.IsNullOrWhiteSpace(model.Status) && item.Status == model.Status
            || model.User != null && item.User == model.User);
        }
    }
}