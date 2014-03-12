using System;
using System.Collections.Generic;
using System.Web;

namespace FaceBookFriends
{
    public class Friend
    {
        public string id { get; set; }
        public string name { get; set; }

        public override bool Equals(object obj)
        {
            var friend = obj as Friend;

            if (friend == null)
                return false;

            return this.id.Equals(friend.id);
        }
    }
}