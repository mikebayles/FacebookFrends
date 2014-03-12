using System;
using System.Collections.Generic;
using System.Web;

namespace FaceBookFriends
{
    public class Friends
    {
        public List<Friend> data { get; set; }

        public Friends(List<Friend> friends)
        {
            data = friends;
        }

        public List<Friend> GetUniqueFriends(Friends other)
        {
            List<Friend> ret = new List<Friend>();

            foreach(Friend friend in data)
            {
                if(!other.data.Contains(friend))
                    ret.Add(friend);
            }
            return ret;
        }
    }
}