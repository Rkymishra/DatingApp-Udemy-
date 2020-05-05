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

        public async Task<Photo> GetMainPhotoForUser(int id)
        {
            return await _context.Photos.Where(a=>a.UserId == id).FirstOrDefaultAsync(ph=>ph.IsMain);
        }

        public async Task<Photo> GetPhoto (int id) {
            return await _context.Photos.FirstOrDefaultAsync (q => q.Id == id);
        }

        public async Task<User> GetUser (int id) {
            return await _context.Users.Include (x => x.Photos).FirstOrDefaultAsync (z => z.Id == id);

        }

        public async Task<PagedList<User>> GetUsers (UserParams userParams) {
            var users =  _context.Users.Include(x => x.Photos);
            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        public async Task<bool> SaveAll () {
            return await _context.SaveChangesAsync () > 0;
        }
        

    }
}