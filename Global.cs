// Global usings
#if USE_POLLY
global using Polly;
global using Polly.Retry;
#endif
#if USE_FUSIONCACHE && !IS_LIB
global using ZiggyCreatures.Caching.Fusion;
global using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;
global using ZiggyCreatures.Caching.Fusion.MicrosoftHybridCache;
global using NeoSmart.Caching.Sqlite;
#endif
#if USE_DI && !IS_LIB
global using Microsoft.Extensions.DependencyInjection;
#endif
#if (USE_DI && USE_FUSIONCACHE && USE_POLLY && !IS_LIB)
global using Microsoft.Extensions.Caching.Hybrid;
global using Axion.Extensions.Caching.Hybrid.Serialization.Http;
global using Axion.Extensions.Http.Resilience;
#endif
#if USE_LOG && !IS_LIB
global using Serilog;
#endif

// Project-type usings
#if IS_GUI && !USE_DI
// GUI 在未启用 DI 时仍需要 ServiceCollection（模板语义），已启用 DI 则由 USE_DI 块提供
global using Microsoft.Extensions.DependencyInjection;
#endif
#if IS_CLI
global using System.CommandLine;
#endif
#if IS_GUI
global using Masa.Blazor;
global using Photino.Blazor;
#endif
#if IS_TEST
global using Xunit;
#endif
