using System;
using System.Collections.Generic;
using System.Web;
using MySql.Data.MySqlClient;
using FaceBookFriends;
using System.Configuration;

namespace FacekBookFriends
{

    public class MySQL
    {
        MySqlConnection connection;
        public String INSERT_FRIEND_MESSAGE = "Added new friend: {0}";
        public String DELETED_FRIEND_MESSAGE = "Friend deleted: {0}";

        private bool? _firstTimeLoad;

        public bool? FirstTimeLoad
        {
            get
            {
                if (_firstTimeLoad == null)
                    _firstTimeLoad = HasFriendShips();

                return _firstTimeLoad;
            }
            set { _firstTimeLoad = value; }
        }

        public MySQL()
        {
           
            connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["mysql"].ConnectionString);
        }

        //open connection to database
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.

                return false;
            }
        }

        //Close connection
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {

                return false;
            }
        }

        //Insert statement
        public void Insert(string query)
        {

            //open connection
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Execute command
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection();
            }
        }

        //Update statement
        public void Update(string query)
        {

            //Open connection
            if (this.OpenConnection() == true)
            {
                //create mysql command
                MySqlCommand cmd = new MySqlCommand();
                //Assign the query using CommandText
                cmd.CommandText = query;
                //Assign the connection using Connection
                cmd.Connection = connection;

                //Execute query
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection();
            }
        }

        //Delete statement
        public void Delete(string query)
        {
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        //Select statement
        public List<Friend> SelectFromUser(string query)
        {
            //Create a list to store the result
            List<Friend> list = new List<Friend>();


            //Open connection
            if (this.OpenConnection() == true)
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();

                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    Friend friend = new Friend
                    {
                        id = dataReader["id"] + "",
                        name = dataReader["name"] + ""
                    };

                    list.Add(friend);
                }

                //close Data Reader
                dataReader.Close();

                //close Connection
                this.CloseConnection();

                //return list to be displayed
                return list;
            }
            else
            {
                return list;
            }
        }

        public String SelectSingle(String query)
        {
            String ret = "";
            //Open connection
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.Read())
                    ret = dataReader[0].ToString();

                //close Data Reader
                dataReader.Close();

                //close Connection
                this.CloseConnection();

                return ret;
            }
            else
                return ret;


        }

        public bool FriendExists(Friend friend)
        {
            return SelectFromUser(String.Format("SELECT * from facebookuser where id = '{0}'", friend.id)).Count > 0;
        }

        public void InsertFriend(Friend friend)
        {
            Insert(String.Format("INSERT into facebookuser values('{0}','{1}')", friend.id, friend.name));
        }

        public String GetFriendShipID(Friend friend)
        {
            return SelectSingle(String.Format("SELECT id from facebookfriend where owner = '{0}' and friend = '{1}'", _Default.owner.id, friend.id));
        }

        public bool HasFriendShips()
        {
            return SelectSingle(String.Format("SELECT * from facebookfriend where owner = '{0}'", _Default.owner.id)).Length > 0;
        }

        public bool FriendShipExists(Friend friend)
        {
            return !String.IsNullOrEmpty(GetFriendShipID(friend));
        }

        public void InsertFriendShip(Friend friend)
        {
            if (!FriendExists(friend))
                InsertFriend(friend);

            Insert(String.Format("INSERT into facebookfriend values('', '{0}' , '{1}')", _Default.owner.id, friend.id));

            if (FirstTimeLoad.Value) return;

            String message = String.Format(INSERT_FRIEND_MESSAGE, friend.name);
            Insert(String.Format("INSERT into facebookhistory values('', '{0}', '{1}', '{2}')",
                _Default.owner.id,
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                message));
        }

        public void DeleteFriendShip(Friend friend)
        {
            Delete(String.Format("DELETE from facebookfriend where owner = '{0}' and friend = '{1}'", _Default.owner.id, friend.id));

            String message = String.Format(DELETED_FRIEND_MESSAGE, friend.name);
            Insert(String.Format("INSERT into facebookhistory values('', '{0}', '{1}', '{2}')",
                _Default.owner.id,
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                message));


        }

        public List<String> GetHistory()
        {
            String query = String.Format("SELECT * from facebookhistory where owner = '{0}' ORDER BY date desc", _Default.owner.id);

            //Create a list to store the result
            List<String> list = new List<String>();


            //Open connection
            if (this.OpenConnection() == true)
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();

                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    string action = String.Format("{0} on {1}", dataReader["action"], dataReader["date"]);

                    list.Add(action);
                }

                //close Data Reader
                dataReader.Close();

                //close Connection
                this.CloseConnection();

                //return list to be displayed
                return list;
            }
            else
                return list;
        }
    }
}