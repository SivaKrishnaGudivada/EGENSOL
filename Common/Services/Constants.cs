namespace Common.Services
{
    public static class Constants 
    {
        public static string KafkaBulkOrderCreateTopicString = "bulk-order-created";
        public static string KafkaBulkOrderUpdateTopicString = "bulk-order-status-updated";

        public static class ConfigurationKeys
        {
            public static string KafkaPortKey = "KafkaPort";
        }
    }
}