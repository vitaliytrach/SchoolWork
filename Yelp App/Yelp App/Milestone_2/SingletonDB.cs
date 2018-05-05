using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Npgsql;
using System.Runtime.Remoting;
using System.Reflection;
using System.Collections;

namespace Milestone_2
{
    public sealed class SingletonDB
    {
        // Constructor
        private SingletonDB()
        {
            connection = new NpgsqlConnection(BuildConnString());
        }

        private static NpgsqlConnection connection;
        private static SingletonDB instance = null;

        public static SingletonDB GetInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SingletonDB();
                }
                return instance;
            }
        }

        // Returns the information needed to access my DB
        public string BuildConnString()
        {
            return "Host = localhost; Username = postgres; Password = sappword321; Database = YelpApp";
        }


        // This method runs a query to the database and reads the return result.
        // It then returns a dictionary of the rows and columns it read in the form
        // of a Key, which is the column name, and a value, which is a list of the 
        // columns components.
        // E.g:
        //      key:        Value:
        //      "bid"       List<string> {bid1, bid2.....}
        //      "address"   List<string> {address of bid1, address of bid2.....}
        public Dictionary<string, dynamic> RunQuery(string query)
        {
            connection.Open();
            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();

            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = query;

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var type = reader.GetValue(i);
                            data.Add(reader.GetName(i), CreateList(type.GetType()));

                            var value = reader.GetValue(i);
                            IList propertyValue = (IList)data[reader.GetName(i)];
                            propertyValue.Add(value);

                            data[reader.GetName(i)] = propertyValue;
                        }

                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var value = reader.GetValue(i);

                                IList propertyValue = (IList)data[reader.GetName(i)];
                                propertyValue.Add(value);

                                data[reader.GetName(i)] = propertyValue;
                            }
                        }
                    }                   
                }
            }

            connection.Close();
            return data;
        }

        // Method to run a query without a return
        public void RunUpdateQuery(string query)
        {
            connection.Open();
            var command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText = query;
            command.ExecuteNonQuery();
            connection.Close();
        }

        // Create a list of any given type
        public IList CreateList(Type type)
        {
            Type genericList = typeof(List<>).MakeGenericType(type);
            return (IList)Activator.CreateInstance(genericList);
        }
        

        

        
    }
}
