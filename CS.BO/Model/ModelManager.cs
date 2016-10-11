using CS.BO.DataObjects.Collections;
using CS.MongoDB.CSDataManager;

namespace CS.BO.Model
{
    public class ModelManager
    {
        private DataManager dm = null;

        public ModelManager(DataManager dataManager)
        {
            dm = dataManager;
        }

        public void AddOrUpdateUserObject(UserObject userObject)
        {
            
        }

        public void RemoveUserObject(UserObject userObject)
        {
            
        }

        public bool UserObjectExists(string userObjectName)
        {
            return false;
        }

        public bool CollectionExists(string collectionName)
        {
            return dm.GetCollection(collectionName) != null;
        }

        public bool CollectionExists(UserObject userObject)
        {
            return CollectionExists(userObject.Name);
        }

    }
}
