using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThanosApp.API.Models;

namespace ThanosApp.API.Data {
    public class AuthRepository : IAuthrepository {
        public readonly DataContext _context;

        public AuthRepository (DataContext context) {
            _context = context;
        }
        public async Task<User> Login (string username, string password) {
            var user = await _context.Users.FirstOrDefaultAsync (x => x.Username == username);
            if (user == null)
                return null;

            if (!VerifyPassworHash (password, user.PasswordHash, user.PasswordSalt))
                return null;

            return user;
        }

        private bool VerifyPassworHash (string password, byte[] passwordHash, byte[] passwordSalt) {
            using (var hmac = new System.Security.Cryptography.HMACSHA512 (passwordSalt)) {
                // C
                var computedHash = hmac.ComputeHash (System.Text.Encoding.UTF8.GetBytes (password));
                for (int i = 0; i < computedHash.Length; i++) {
                    if (computedHash[i] != passwordHash[i]) return false;
                }
            }
            return true;
        }

        public async Task<User> Register (User user, string password) {

            
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash (password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.UId = Guid.NewGuid().ToString();
            await _context.Users.AddAsync (user);
            await _context.SaveChangesAsync ();

            return user;

        }

        private void CreatePasswordHash (string password, out byte[] passwordHash, out byte[] passwordSalt) {
            using (var hmac = new System.Security.Cryptography.HMACSHA512 ()) {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash (System.Text.Encoding.UTF8.GetBytes (password));
            }
        }

        public async Task<bool> UserExist (string username) {
            if (await _context.Users.AnyAsync (x => x.Username == username))
                return true;

            return false;
        }
    }
}