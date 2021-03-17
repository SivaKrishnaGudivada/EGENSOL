namespace Common.Services
{
    public interface IKafkaPublisher
    {
        void Publish(string topic, string data);
    }
}