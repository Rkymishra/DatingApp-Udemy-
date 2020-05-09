using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data {
    public class DatingRepository : IDatingRepository {
        private readonly DataContext _context;
        public DatingRepository (DataContext context) {
            _context = context;
        }
        public void Add<T> (T entity) where T : class {
            _context.Add (entity);
        }
        public void Delete<T> (T entity) where T : class {
            _context.Remove (entity);
        }
        public async Task<Photo> GetMainPhotoForUser (int id) {
            return await _context.Photos.Where (a => a.UserId == id).FirstOrDefaultAsync (ph => ph.IsMain);
        }
        public async Task<Photo> GetPhoto (int id) {
            return await _context.Photos.FirstOrDefaultAsync (q => q.Id == id);
        }
        public async Task<User> GetUser (int id) {
            return await _context.Users.Include (x => x.Photos).FirstOrDefaultAsync (z => z.Id == id);
        }
        public async Task<PagedList<User>> GetUsers (UserParams userParams) {
            var users = _context.Users.Include (x => x.Photos).OrderByDescending (q => q.LastActive).AsQueryable ();
            users = users.Where (x => x.Id != userParams.UserId && x.Gender == userParams.Gender);
            if (userParams.Likers) {
                var userLikers = await GetUserLikes (userParams.UserId, userParams.Likers);
                users = users.Where (u => userLikers.Contains (u.Id));
            }
            if (userParams.Likees) {
                var userLikees = await GetUserLikes (userParams.UserId, userParams.Likers);
                users = users.Where (u => userLikees.Contains (u.Id));
            }
            if (userParams.MinAge != 18 || userParams.MaxAge != 99) {
                var minDob = DateTime.Today.AddYears (-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears (-userParams.MinAge);
                users = users.Where (q => q.DateOfBirth >= minDob && q.DateOfBirth <= maxDob);
            }
            if (!string.IsNullOrEmpty (userParams.Orderby)) {
                switch (userParams.Orderby.ToLower ()) {
                    case "created":
                        users = users.OrderByDescending (q => q.Created);
                        break;
                    default:
                        users = users.OrderByDescending (q => q.LastActive);
                        break;
                }
            }
            return await PagedList<User>.CreateAsync (users, userParams.PageNumber, userParams.PageSize);
        }
        private async Task<IEnumerable<int>> GetUserLikes (int id, bool likers) {
            var user = await _context.Users.Include (x => x.Likers).Include (x => x.Likees).FirstOrDefaultAsync (u => u.Id == id);
            if (likers) {
                return user.Likers.Where (c => c.LikeeId == id).Select (d => d.LikerId);
            } else {
                return user.Likees.Where (c => c.LikerId == id).Select (d => d.LikeeId);
            }
        }
        public async Task<bool> SaveAll () {
            return await _context.SaveChangesAsync () > 0;
        }
        public async Task<Like> GetLike (int userId, int recepientId) {
            return await _context.Likes.FirstOrDefaultAsync (u => u.LikerId == userId && u.LikeeId == recepientId);
        }
        public async Task<Message> GetMessage (int id) {
            return await _context.Messages.FirstOrDefaultAsync (m => m.Id == id);
        }
        public async Task<PagedList<Message>> GetMessagesForUser (MessageParams messageParams) {
            var messages = _context.Messages.Include (u => u.Sender).ThenInclude (p => p.Photos).Include (u => u.Recipient).ThenInclude (p => p.Photos).AsQueryable ();
            switch (messageParams.MessageContainer) {
                case "Inbox":
                    messages = messages.Where (u => u.RecipientId == messageParams.UserId);
                    break;
                case "Outbox":
                    messages = messages.Where (u => u.SenderId == messageParams.UserId);
                    break;
                default:
                    messages = messages.Where (u => u.RecipientId == messageParams.UserId && !u.IsRead);
                    break;
            }
            messages = messages.OrderByDescending (d => d.MessageSent);
            return await PagedList<Message>.CreateAsync (messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread (int userId, int RecipientId) {
            var messages = await _context.Messages.Include (u => u.Sender).ThenInclude (p => p.Photos).Include (u => u.Recipient).ThenInclude (p => p.Photos).
                                Where (m => m.RecipientId == userId && m.SenderId == RecipientId || m.RecipientId == RecipientId && m.SenderId == userId).
                                OrderByDescending(m=>m.MessageSent).ToListAsync();
            return messages;

        }
    }
}