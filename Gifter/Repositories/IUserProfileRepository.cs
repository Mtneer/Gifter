using Gifter.Models;
using System.Collections.Generic;

namespace Gifter.Repositories
{
    public interface IUserProfileRepository
    {
        void Add(UserProfile userProfile);
        void Delete(int id);
        List<UserProfile> GetAll();
        UserProfile GetById(int id);
        UserProfile GetUserByIdWithPosts(int id);
        UserProfile GetByFirebaseUserId(string firebaseId);
        void Update(UserProfile userProfile);
    }
}