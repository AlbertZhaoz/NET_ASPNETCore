using SwiftCode.BBS.IRepository;
using SwiftCode.BBS.IServices;
using SwiftCode.BBS.Repository;

namespace SwiftCode.BBS.Services
{
    public class CalculateService : ICalculateService
    {
        ICalculateRespository calculateRespository = new CalculateRespository();
        public int Sum(int i, int j)
        {
            return calculateRespository.Sum(i, j);
        }
    }
}