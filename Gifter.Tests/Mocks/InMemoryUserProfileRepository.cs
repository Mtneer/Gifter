using System;
using System.Collections.Generic;
using System.Linq;
using Gifter.Models;
using Gifter.Repositories;

namespace Gifter.Tests.Mocks
{
    class InMemoryUserProfileRepository : IUserProfileRepository
    {
        private readonly List<UserProfile> _data;

        public List<UserProfile> InternalData
        {
            get
            {
                return _data;
            }
        }

        public InMemoryUserProfileRepository(List<UserProfile> startingData)
        {
            _data = startingData;
        }

        public void Add(UserProfile userProfile)
        {
            var lastUserProfile = _data.Last();
            userProfile.Id = lastUserProfile.Id + 1;
            _data.Add(userProfile);
        }

        public void Delete(int id)
        {
            var userProfileTodelete = _data.FirstOrDefault(up => up.Id == id);
            if (userProfileTodelete == null)
            {
                return;
            }

            _data.Remove(userProfileTodelete);
        }

        public List<UserProfile> GetAll()
        {
            return _data;
        }

        public UserProfile GetById(int id)
        {
            return _data.FirstOrDefault(up => up.Id == id);
        }

        public void Update(UserProfile userProfile)
        {
            var currentUserProfile = _data.FirstOrDefault(up => up.Id == userProfile.Id);
            if (currentUserProfile == null)
            {
                return;
            }

            currentUserProfile.Name= userProfile.Name;
            currentUserProfile.Email = userProfile.Email;
            currentUserProfile.DateCreated = userProfile.DateCreated;
            currentUserProfile.ImageUrl = userProfile.ImageUrl;
            currentUserProfile.Bio = userProfile.Bio;
        }

        public UserProfile GetUserByIdWithPosts(int id)
        {
            throw new NotImplementedException();
        }

        public UserProfile GetByFirebaseUserId(string firebaseId)
        {
            throw new NotImplementedException();
        }

    }
}
