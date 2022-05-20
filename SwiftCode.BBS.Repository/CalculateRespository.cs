using SwiftCode.BBS.IRepository;

namespace SwiftCode.BBS.Repository
{
    public class CalculateRespository : ICalculateRespository
    {
        public int Sum(int i, int j)
        {
            return i + j;
        }
    }
}