using DataAccessLogic.DatabaseModels;
using DataAccessLogic.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLogic.CrudLogic
{
    public class SavedListLogic : ISavedLogic
    {
        private readonly ApplicationContext context;

        public SavedListLogic(ApplicationContext context)
        {
            this.context = context;
        }

        public async Task Create(User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Id))
            {
                throw new Exception("Пользователь не определен");
            }

            if (!context.SavedLists.Any(savedList =>
            savedList.UserId == user.Id))
            {
                SavedList newList = new SavedList
                {
                    Id = Guid.NewGuid().ToString(),
                    User = user
                };
                await context.SavedLists.AddAsync(newList);
                await context.SaveChangesAsync();
            }
        }

        public async Task Remove(User user, Item savedItem)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Id))
            {
                throw new Exception("Пользователь не определен");
            }

            if (savedItem == null || string.IsNullOrWhiteSpace(savedItem.Id))
            {
                throw new Exception("Товар не определен");
            }

            SavedList existingList = await context.SavedLists
                .Include(list => list.Item)
                .FirstOrDefaultAsync(list => list.UserId == user.Id);

            if (existingList == null)
            {
                throw new Exception("Список избранного не найден");
            }

            Item itemToRemove = existingList.Item
                .Find(item => item.Id == savedItem.Id);

            existingList.Item.Remove(itemToRemove);

            await context.SaveChangesAsync();
        }

        public async Task<SavedList> Read(User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Id))
            {
                throw new Exception("Пользователь не определен");
            }

            return await context.SavedLists.Include(savedList => savedList.Item)
            .FirstOrDefaultAsync(savedList => savedList.UserId == user.Id);
        }

        public async Task Add(User user, Item savedItem)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Id))
            {
                throw new Exception("Пользователь не определен");
            }

            if (savedItem == null || string.IsNullOrWhiteSpace(savedItem.Id))
            {
                throw new Exception("Товар не определен");
            }

            SavedList existingList = await context.SavedLists
                .Include(list => list.Item)
                .FirstOrDefaultAsync(list => list.UserId == user.Id);

            Item itemToSave = await context.Items.FindAsync(savedItem.Id);

            if (existingList == null)
            {
                throw new Exception("Список избранного не найден");
            }

            existingList.Item.Add(itemToSave);

            await context.SaveChangesAsync();
        }
    }
}