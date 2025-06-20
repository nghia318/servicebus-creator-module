using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

public class Project
{
    private const string topicConnectionString = "Endpoint=sb://srvbusstandard.servicebus.windows.net/;SharedAccessKeyName=connection;SharedAccessKey=73jnWHoPwRFGvg+LDEVmo+ey1ur1ZQPRv+ASbFcDV8Y=;EntityPath=firsttopic";
    private const string topicName = "firsttopic";
    private const string subscriptionName = "subs3";
    public static async Task Main(string[] args)
    {
        var adminClient = new ServiceBusAdministrationClient(topicConnectionString);
        await adminClient.CreateSubscriptionAsync(topicName, subscriptionName);
        Console.WriteLine("Subscription created!");
    }
}