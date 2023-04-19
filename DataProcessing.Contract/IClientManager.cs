using DataProcessing.Contract.Domain;
using LaYumba.Functional;

namespace DataProcessing.Contract;

public interface IClientManager
{
    Option<Client> Get(int id);
}