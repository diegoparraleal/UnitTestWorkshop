using DataProcessing.Contract;
using DataProcessing.Contract.Domain;
using LaYumba.Functional;

namespace DataProcessing.Logic;

public class ClientManager: IClientManager
{
    private readonly Dictionary<int, Client> _clients = new List<Client>
    {
        new(1, "Harry", "Potter"), 
        new(2, "Hermione", "Granger"), 
        new(3, "Ron", "Weasley"),
        new(4, "Draco", "Malfoy")
    }.ToDictionary(k => k.Id);

    public Option<Client> Get(int id) => _clients.Lookup(id);
}