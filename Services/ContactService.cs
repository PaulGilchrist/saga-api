using API.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace API.Services {
    public class ContactService {
        private readonly IMongoCollection<Contact> _contacts;

        public ContactService(ApplicationSettings applicationSettings) {
            var client = new MongoClient(applicationSettings.DatabaseConnectionString);
            var database = client.GetDatabase(applicationSettings.DatabaseName);
            _contacts = database.GetCollection<Contact>(applicationSettings.ContactsCollectionName);
        }

        public IMongoQueryable<Contact> Get() {
            return _contacts.AsQueryable();
        }

        public async Task<Contact> Get(string id) {
            return await _contacts.Find<Contact>(contact => contact.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Contact> Create(Contact contact) {
            await _contacts.InsertOneAsync(contact);
            return contact;
        }

        public async Task<ReplaceOneResult> Update(string id,Contact contactIn) {
            return await _contacts.ReplaceOneAsync(contact => contact.Id == id,contactIn);
        }

        public async Task<DeleteResult> Remove(Contact contactIn) {
            return await _contacts.DeleteOneAsync(contact => contact.Id == contactIn.Id);
        }

        public async Task<DeleteResult> Remove(string id) {
            return await _contacts.DeleteOneAsync(contact => contact.Id == id);
        }
    }
}
