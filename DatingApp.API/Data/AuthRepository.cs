using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data {
    public class AuthRepository : IAuthRepository {
        private readonly DataContext _context;
        public AuthRepository (DataContext context) {
            _context = context;
        }
        public async Task<User> Login (string Username, string Password) {
            var user = await _context.Users.FirstOrDefaultAsync (x => x.Username == Username);
            if (user == null) { return null; }

            if (!VerifyPasswordHash (Password, user.PasswordHash, user.PassswordSalt)) { return null; }
            return user;
        }

        public async Task<User> Register (User user, string Password) {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash (Password, out passwordHash, out passwordSalt);
            user.PassswordSalt = passwordSalt;
            user.PasswordHash = passwordHash;
            await _context.Users.AddAsync (user);
            await _context.SaveChangesAsync ();
            return user;
        }

        public async Task<bool> UserExists (string Username) {
            if (await _context.Users.AnyAsync (x => x.Username == Username)) {
                return true;
            }
            return false;
        }
        private void CreatePasswordHash (string password, out byte[] passwordHash, out byte[] passwordSalt) {
            using (var hMac = new System.Security.Cryptography.HMACSHA512 ()) {
                passwordSalt = hMac.Key;
                passwordHash = hMac.ComputeHash (System.Text.Encoding.UTF8.GetBytes (password));
            }
        }
        private bool VerifyPasswordHash (string password, byte[] passwordHash, byte[] passswordSalt) {
            using (var hMac = new System.Security.Cryptography.HMACSHA512 (passswordSalt)) {
                var ComputedHash = hMac.ComputeHash (System.Text.Encoding.UTF8.GetBytes (password));
                for (int i = 0; i < ComputedHash.Length; i++) {
                    if (ComputedHash[i] != passwordHash[i]) { return false; }
                }
            }
            return true;
        }
    }
}