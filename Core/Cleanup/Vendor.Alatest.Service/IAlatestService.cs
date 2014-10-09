using System.ServiceModel;

namespace AlatestService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IAlatestService" in both code and config file together.
    [ServiceContract]
    public interface IAlatestService
    {
        [OperationContract]
        string GetReview(string productID);

        [OperationContract]
        string GetRating(string productID);
    }
}
