using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moedim.Edgar.Client;
using Moedim.Edgar.Options;
using Moedim.Edgar.Services;

namespace Moedim.Edgar.UnitTests.DependencyInjection;

public class EdgarServiceCollectionExtensionsTests
{
    [Fact(DisplayName = "AddSecEdgar with null services throws ArgumentNullException")]
    public void AddSecEdgar_NullServices_ThrowsArgumentNullException()
    {
        IServiceCollection? services = null;

        var exception = Assert.Throws<ArgumentNullException>(() =>
            services!.AddSecEdgar(options => { }));

        exception.ParamName.Should().Be("services");
    }

    [Fact(DisplayName = "AddSecEdgar with null configureOptions throws ArgumentNullException")]
    public void AddSecEdgar_NullConfigureOptions_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddSecEdgar(null!));

        exception.ParamName.Should().Be("configureOptions");
    }

    [Fact(DisplayName = "AddSecEdgar registers all required services")]
    public void AddSecEdgar_RegistersAllRequiredServices()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        services.AddSecEdgar(options =>
        {
            options.AppName = "TestApp";
            options.AppVersion = "1.0.0";
            options.Email = "test@example.com";
        });

        var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetService<ICompanyConceptService>().Should().NotBeNull();
        serviceProvider.GetService<ICompanyFactsService>().Should().NotBeNull();
        serviceProvider.GetService<ICompanyLookupService>().Should().NotBeNull();
        serviceProvider.GetService<IEdgarLatestFilingsService>().Should().NotBeNull();
        serviceProvider.GetService<IEdgarSearchService>().Should().NotBeNull();
        serviceProvider.GetService<IFilingDetailsService>().Should().NotBeNull();
        serviceProvider.GetService<ISecEdgarClient>().Should().NotBeNull();
    }

    [Fact(DisplayName = "AddSecEdgar configures options correctly")]
    public void AddSecEdgar_ConfiguresOptionsCorrectly()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        services.AddSecEdgar(options =>
        {
            options.AppName = "TestApp";
            options.AppVersion = "1.0.0";
            options.Email = "test@example.com";
            options.MaxRetryCount = 5;
        });

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SecEdgarOptions>>().Value;

        options.AppName.Should().Be("TestApp");
        options.AppVersion.Should().Be("1.0.0");
        options.Email.Should().Be("test@example.com");
        options.MaxRetryCount.Should().Be(5);
        options.UserAgent.Should().Be("TestApp/1.0.0 (test@example.com)");
    }

    [Fact(DisplayName = "AddSecEdgar sets default UserAgent from configuration")]
    public void AddSecEdgar_SetsDefaultUserAgent()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        services.AddSecEdgar(options =>
        {
            options.AppName = "MyApp";
            options.AppVersion = "2.0.0";
            options.Email = "developer@example.com";
        });

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SecEdgarOptions>>().Value;

        options.UserAgent.Should().Be("MyApp/2.0.0 (developer@example.com)");
    }

    [Fact(DisplayName = "AddSecEdgar validates AppName is required")]
    public void AddSecEdgar_ValidatesAppNameRequired()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        services.AddSecEdgar(options =>
        {
            options.AppName = "";
            options.AppVersion = "1.0.0";
            options.Email = "test@example.com";
        });

        var serviceProvider = services.BuildServiceProvider();

        var exception = Assert.Throws<Microsoft.Extensions.Options.OptionsValidationException>(() =>
            serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SecEdgarOptions>>().Value);

        exception.Message.Should().Contain("AppName is required");
    }

    [Fact(DisplayName = "AddSecEdgar validates AppVersion is required")]
    public void AddSecEdgar_ValidatesAppVersionRequired()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        services.AddSecEdgar(options =>
        {
            options.AppName = "TestApp";
            options.AppVersion = "";
            options.Email = "test@example.com";
        });

        var serviceProvider = services.BuildServiceProvider();

        var exception = Assert.Throws<Microsoft.Extensions.Options.OptionsValidationException>(() =>
            serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SecEdgarOptions>>().Value);

        exception.Message.Should().Contain("AppVersion is required");
    }

    [Fact(DisplayName = "AddSecEdgar validates Email is required")]
    public void AddSecEdgar_ValidatesEmailRequired()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        services.AddSecEdgar(options =>
        {
            options.AppName = "TestApp";
            options.AppVersion = "1.0.0";
            options.Email = "";
        });

        var serviceProvider = services.BuildServiceProvider();

        var exception = Assert.Throws<Microsoft.Extensions.Options.OptionsValidationException>(() =>
            serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SecEdgarOptions>>().Value);

        exception.Message.Should().Contain("Email is required");
    }

    [Fact(DisplayName = "AddSecEdgar validates RetryCountOverride cannot be negative")]
    public void AddSecEdgar_ValidatesRetryCountOverrideNotNegative()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        services.AddSecEdgar(options =>
        {
            options.AppName = "TestApp";
            options.AppVersion = "1.0.0";
            options.Email = "test@example.com";
            options.RetryCountOverride = -1;
        });

        var serviceProvider = services.BuildServiceProvider();

        var exception = Assert.Throws<Microsoft.Extensions.Options.OptionsValidationException>(() =>
            serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SecEdgarOptions>>().Value);

        exception.Message.Should().Contain("RetryCountOverride cannot be negative");
    }

    [Fact(DisplayName = "AddSecEdgar validates RetryBackoffMultiplier must be greater than zero")]
    public void AddSecEdgar_ValidatesRetryBackoffMultiplierGreaterThanZero()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        services.AddSecEdgar(options =>
        {
            options.AppName = "TestApp";
            options.AppVersion = "1.0.0";
            options.Email = "test@example.com";
            options.RetryBackoffMultiplier = 0;
        });

        var serviceProvider = services.BuildServiceProvider();

        var exception = Assert.Throws<Microsoft.Extensions.Options.OptionsValidationException>(() =>
            serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SecEdgarOptions>>().Value);

        exception.Message.Should().Contain("RetryBackoffMultiplier must be greater than zero");
    }

    [Fact(DisplayName = "AddSecEdgar validates RetryDelay must be greater than zero when specified")]
    public void AddSecEdgar_ValidatesRetryDelayGreaterThanZero()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        services.AddSecEdgar(options =>
        {
            options.AppName = "TestApp";
            options.AppVersion = "1.0.0";
            options.Email = "test@example.com";
            options.RetryDelay = TimeSpan.Zero;
        });

        var serviceProvider = services.BuildServiceProvider();

        var exception = Assert.Throws<Microsoft.Extensions.Options.OptionsValidationException>(() =>
            serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SecEdgarOptions>>().Value);

        exception.Message.Should().Contain("RetryDelay must be greater than zero");
    }

    [Fact(DisplayName = "AddSecEdgar registers services as transient")]
    public void AddSecEdgar_RegistersServicesAsTransient()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        services.AddSecEdgar(options =>
        {
            options.AppName = "TestApp";
            options.AppVersion = "1.0.0";
            options.Email = "test@example.com";
        });

        var serviceProvider = services.BuildServiceProvider();

        var service1 = serviceProvider.GetService<ICompanyConceptService>();
        var service2 = serviceProvider.GetService<ICompanyConceptService>();

        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service1.Should().NotBeSameAs(service2);
    }

    [Fact(DisplayName = "AddSecEdgar configures HttpClient with retry policy")]
    public void AddSecEdgar_ConfiguresHttpClientWithRetryPolicy()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        services.AddSecEdgar(options =>
        {
            options.AppName = "TestApp";
            options.AppVersion = "1.0.0";
            options.Email = "test@example.com";
        });

        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<ISecEdgarClient>();

        client.Should().NotBeNull();
        client.Should().BeOfType<Moedim.Edgar.Client.Impl.SecEdgarClient>();
    }

    [Fact(DisplayName = "AddSecEdgar returns service collection for chaining")]
    public void AddSecEdgar_ReturnsServiceCollectionForChaining()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var result = services.AddSecEdgar(options =>
        {
            options.AppName = "TestApp";
            options.AppVersion = "1.0.0";
            options.Email = "test@example.com";
        });

        result.Should().BeSameAs(services);
    }

    [Fact(DisplayName = "AddSecEdgar allows multiple registrations without duplicates")]
    public void AddSecEdgar_AllowsMultipleRegistrations()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        services.AddSecEdgar(options =>
        {
            options.AppName = "TestApp1";
            options.AppVersion = "1.0.0";
            options.Email = "test1@example.com";
        });

        services.AddSecEdgar(options =>
        {
            options.AppName = "TestApp2";
            options.AppVersion = "2.0.0";
            options.Email = "test2@example.com";
        });

        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetService<ICompanyConceptService>();

        service.Should().NotBeNull();
    }

    [Fact(DisplayName = "AddSecEdgar binds configuration from appsettings")]
    public void AddSecEdgar_BindsConfigurationFromAppSettings()
    {
        var configurationData = new Dictionary<string, string?>
        {
            {"SecEdgar:BaseApiUrl", "https://custom.api.url"},
            {"SecEdgar:MaxRetryCount", "10"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        services.AddSecEdgar(options =>
        {
            options.AppName = "TestApp";
            options.AppVersion = "1.0.0";
            options.Email = "test@example.com";
        });

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SecEdgarOptions>>().Value;

        options.BaseApiUrl.Should().Be("https://custom.api.url");
        options.MaxRetryCount.Should().Be(10);
    }

    [Fact(DisplayName = "AddSecEdgar allows RetryCountOverride to be zero")]
    public void AddSecEdgar_AllowsRetryCountOverrideZero()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        services.AddSecEdgar(options =>
        {
            options.AppName = "TestApp";
            options.AppVersion = "1.0.0";
            options.Email = "test@example.com";
            options.RetryCountOverride = 0;
        });

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SecEdgarOptions>>().Value;

        options.RetryCountOverride.Should().Be(0);
    }

    [Fact(DisplayName = "AddSecEdgar preserves custom UserAgent when provided")]
    public void AddSecEdgar_PreservesCustomUserAgent()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        services.AddSecEdgar(options =>
        {
            options.AppName = "TestApp";
            options.AppVersion = "1.0.0";
            options.Email = "test@example.com";
            options.UserAgent = "CustomAgent/1.0";
        });

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SecEdgarOptions>>().Value;

        options.UserAgent.Should().Be("CustomAgent/1.0");
    }
}
