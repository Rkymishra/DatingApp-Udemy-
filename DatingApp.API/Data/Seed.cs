using System.Collections.Generic;
using System.Linq;
using DatingApp.API.Models;
using Newtonsoft.Json;

namespace DatingApp.API.Data {
    public class Seed {
        //userseed
        public static void SeedUsers (DataContext context) {
            if (!context.Users.Any ()) {
                var userData = System.IO.File.ReadAllText ("Data/userseed.json");
                var users = JsonConvert.DeserializeObject<List<User>> (userData);
                foreach (var user in users) {
                    byte[] passwordHash, passwordSalt;
                    CreatePasswordHash ("password", out passwordHash, out passwordSalt);
                    user.PasswordHash = passwordHash;
                    user.PassswordSalt = passwordSalt;
                    user.Username = user.Username.ToLower();
                    context.Users.Add(user);
                }
                context.SaveChanges();
            }
        }
        private static void CreatePasswordHash (string password, out byte[] passwordHash, out byte[] passwordSalt) {
            using (var hMac = new System.Security.Cryptography.HMACSHA512 ()) {
                passwordSalt = hMac.Key;
                passwordHash = hMac.ComputeHash (System.Text.Encoding.UTF8.GetBytes (password));
            }
        }
    }
}