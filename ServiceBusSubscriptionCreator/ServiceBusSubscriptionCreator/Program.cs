using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

public class Project
{
    private const string TopicConnectionString = "Endpoint=sb://srvbusstandard.servicebus.windows.net/;SharedAccessKeyName=connection;SharedAccessKey=73jnWHoPwRFGvg+LDEVmo+ey1ur1ZQPRv+ASbFcDV8Y=;EntityPath=firsttopic";
    private const string TopicName = "firsttopic";
    private const string TrueFilterSubscriptionName = "true-subs";
    private const string FalseFilterSubscriptionName = "false-subs";
    // rule name 
    private const string RuleName = "My-Rule";
    // correlation filter setting
    private const string CorrelationSubscriptionName = "correlation-subs";
    private const string CorrelationIdFilterValue = "critical";
    private const string SubjectFilterValue = "subject-filter";
    private const string CustomPropKey = "test";
    private const string CustomPropValue = "true";
    // sql filter setting
    private const string SqlSubscriptionName = "sql-subs";
    private const string SqlFilterKey = "sql";
    private const string SqlFilterValue = "true";


    public static async Task Main(string[] args)
    {
        var adminClient = new ServiceBusAdministrationClient(TopicConnectionString);

        try
        {
            // create subs with true filter 
            await CreateSubscription(adminClient, TopicName, TrueFilterSubscriptionName,FilterType.True);

            // create subs with false filter 
            await CreateSubscription(adminClient, TopicName, FalseFilterSubscriptionName, FilterType.False);

            // create subs with filter
            await CreateSubscription(adminClient, TopicName, CorrelationSubscriptionName, FilterType.Correlation);

            // create subs with SQL filter
            await CreateSubscription(adminClient, TopicName, SqlSubscriptionName, FilterType.Sql);
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

    private enum FilterType
    {
        True,
        False,
        Correlation,
        Sql
    }
    private static async Task CreateSubscription(ServiceBusAdministrationClient adminClient,string topicName, string subscriptionName, FilterType filterType)
    {
        Console.WriteLine($"Checking if '{subscriptionName}' for topic '{topicName}' exists...");
        try
        {
            if (await adminClient.SubscriptionExistsAsync(topicName, subscriptionName))
            {
                Console.WriteLine($"Subscription '{subscriptionName}' for topic '{topicName}' already exists. Skipping creation.\n");
                return;
            }
            Console.WriteLine($"Subscription '{subscriptionName}' for topic '{topicName}' does not exist. Attempting to create...");
            var subscriptionOption = new CreateSubscriptionOptions(topicName, subscriptionName);
            var ruleOptions = new CreateRuleOptions(RuleName);

            switch (filterType)
            {
                case FilterType.Correlation:
                    var correlationFilter = new CorrelationRuleFilter 
                    { 
                        CorrelationId = CorrelationIdFilterValue, 
                        Subject = SubjectFilterValue
                    };
                    correlationFilter.ApplicationProperties.Add(CustomPropKey, CustomPropValue);
                    ruleOptions.Filter = correlationFilter;
                    Console.WriteLine($"+++Applying custom property filter: {CustomPropKey} = '{CustomPropValue}'");
                    break;

                case FilterType.Sql:
                    var sqlFilter = new SqlRuleFilter($"{SqlFilterKey} ='{SqlFilterValue}'");
                    ruleOptions.Filter = sqlFilter;
                    Console.WriteLine($"+++Applying SQL filter: {SqlFilterKey} = '{SqlFilterValue}'");
                    break;

                case FilterType.False:
                    ruleOptions.Filter = new FalseRuleFilter();
                    Console.WriteLine("+++Applying false filter: No messages will be received");
                    break;
                
                case FilterType.True:
                default:
                    ruleOptions.Filter = new TrueRuleFilter();
                    Console.WriteLine("+++Applying true filter: All messages will be received"); 
                    break;
            }
            await adminClient.CreateSubscriptionAsync(subscriptionOption, ruleOptions);
            Console.WriteLine($"\nSubscription '{subscriptionName}' for topic '{topicName}' created successfully!");
            Console.WriteLine("===============================\n");
        }
        catch (Exception)
        {

            throw;
        }
    }
}

