using Models;

namespace Common.Services
{
    public interface ICreateOrderPublisher
    {
        void Publish(CreateOrder data);
    }
}