using jwt5.Models;
using System.Collections.Generic;
using System.Linq;

namespace jwt5.Repositories
{
    public static class UserRepo
    {
        public static UserModel Get(string username, string password)
        {
            var users = new List<UserModel>();

            users.Add(new UserModel { Id = 1, Username = "clarke", Password = "abc123", Role = "admin" });

            users.Add(new UserModel { Id = 2, Username = "asimov", Password = "abc123", Role = "dev" });

            users.Add(new UserModel { Id = 3, Username = "bradbury", Password = "abc123", Role = "test" });

            users.Add(new UserModel { Id = 4, Username = "dick", Password = "abc123", Role = "user" });

            return users.Where(x => x.Username.ToLower() == username.ToLower() && x.Password == password).FirstOrDefault();
        }
    }
}