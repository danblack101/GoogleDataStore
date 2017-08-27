
using System;
using System.Linq;
using System.Reflection;
using Google.Cloud.Datastore.V1;
using System.Collections.Generic;

namespace QuickstartIdentityServer.Models
{
    public class DatastoreGeneralStore : IGeneralStore
    {
        private readonly string _projectId;
        private readonly DatastoreDb _db;

       public DatastoreGeneralStore(string projectId)
        {
            _projectId = projectId;
            _db = DatastoreDb.Create(_projectId);
        }
     
        public void DeleteAllData()
        {

            DeleteAll<UserTenantAccess>();
            DeleteAll<User>();

        }

        public void DeleteAll<T>()
        {
            var query = new Query(typeof(T).Name);
            var results = _db.RunQuery(query);
            _db.Delete(results.Entities);
        }

        public void Delete(long id)
        {
            _db.Delete(id.ToKey());
        }

        public void Insert<T>(T poco)
        {
            var entity = new Entity();
            entity.Key = _db.CreateKeyFactory(typeof(T).Name).CreateIncompleteKey();
            foreach (var prop in typeof(T).GetProperties())
            {
                var test = prop;
                var y = poco.GetType().GetProperty(test.Name).GetValue(poco, null);
                entity[test.Name] = y.ToString();
            }
            var keys = _db.Insert(new[] { entity });

        }

        public List<T> GetAll<T>() where T : new()
        {
            var query = new Query(typeof(T).Name);
            var results = _db.RunQuery(query);
           
            var mappedEntites = new List<T>();

            foreach(var entity in results.Entities)
            {
                var destination = new T();
                
                foreach (var prop in typeof(T).GetProperties())
                {
                    if (prop.Name.Equals("Id"))
                    {
                        prop.SetValue(destination, entity.Key.Path.First().Id, null);
                    }
                    else
                    {
                        if (prop.PropertyType == typeof(Guid))
                            prop.SetValue(destination, Guid.Parse(entity[prop.Name].StringValue), null);
                        else
                            prop.SetValue(destination, (string)entity[prop.Name], null);
                    }
                }
                mappedEntites.Add(destination);
            }
            return mappedEntites;
        }
    }
}