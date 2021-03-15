namespace Common.Services
{
    public interface IKafkaPublisher
    {
        void Publish(string topic, object data);
    }
}