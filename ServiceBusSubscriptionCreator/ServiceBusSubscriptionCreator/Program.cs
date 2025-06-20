using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

public class Project
{
    private const string TopicConnectionString = "Endpoint=sb://srvbusstandard.servicebus.windows.net/;SharedAccessKeyName=connection;SharedAccessKey=73jnWHoPwRFGvg+LDEVmo+ey1ur1ZQPRv+ASbFcDV8Y=;EntityPath=firsttopic";
    private const string TopicName = "firsttopic";
    private const string DefaultSubscriptionName = "subs5";
    private const string FilterSubscriptionName = "filter-subs2";
    private const string RuleName = "My-Rule";
    private const string CorrelationIdFilterValue = "critical";
    private const string SubjectFilterValue = "order-type";
    private const string CustomPropName = "Type";
    private const string CustomPropValue = "Filtered";

    public static async Task Main(string[] args)
    {
        var adminClient = new ServiceBusAdministrationClient(TopicConnectionString);

        try
        {
            // create subs with no filter 
            await CreateSubscription(adminClient, TopicName, DefaultSubscriptionName, isFiltered: false);

            // create subs with filter
            await CreateSubscription(adminClient, TopicName, FilterSubscriptionName, isFiltered: true);
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"\nError{ex.Message}");
        }
        finally
        {
            Console.WriteLine("\nExiting!");
        }
    }

    private static async Task CreateSubscription(ServiceBusAdministrationClient adminClient,string topicName, string subscriptionName, bool isFiltered)
    {
        Console.WriteLine($"Checking if '{subscriptionName}' for topic '{topicName}' exists...");
        try
        {
            if (await adminClient.SubscriptionExistsAsync(topicName, subscriptionName))
            {
                Console.WriteLine($"Subscription '{subscriptionName}' for topic '{topicName}' already exists. Skipping creation.");
                return;
            }
            Console.WriteLine($"Subscription '{subscriptionName}' for topic '{topicName}' does not exist. Attempting to create...");
            var subscriptionOption = new CreateSubscriptionOptions(topicName, subscriptionName);
            var ruleOptions = new CreateRuleOptions(RuleName);

            if (isFiltered)
            {
                var correlationFilter = new CorrelationRuleFilter() 
                { 
                    CorrelationId = CorrelationIdFilterValue, 
                    Subject = SubjectFilterValue
                };
                correlationFilter.ApplicationProperties.Add(CustomPropName, CustomPropValue);
                ruleOptions.Filter = correlationFilter;
                Console.WriteLine($"+++Applying custom property filter: {CustomPropName} = '{CustomPropValue}'");
            }
            else
            {
                ruleOptions.Filter = new TrueRuleFilter();
                Console.WriteLine("No specific filter provided");
            }

            await adminClient.CreateSubscriptionAsync(subscriptionOption, ruleOptions);
            Console.WriteLine($"\nSubscription '{subscriptionName}' for topic '{topicName}' created successfully!");
            Console.WriteLine("===============================");
        }
        catch (Exception)
        {

            throw;
        }
    }
}

