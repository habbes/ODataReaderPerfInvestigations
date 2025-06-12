// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using ODataReaderPerfInvestigations;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

//DebugTestBenchmark();
//DebugReader();

void DebugTestBenchmark()
{
    var bench = new ODataResourcePropertyVerificationBenchmarks();
    bench.Setup();

    // Do it twice to ensure memory stream resets correctly.
    bench.WriteWithNoSkipPropertyVerification();
    bench.WriteWithNoSkipPropertyVerification();
    bench.memoryStream.Position = 0;
    var output = new StreamReader(bench.memoryStream, leaveOpen: true).ReadToEnd();

    //Console.WriteLine(output);

    var bench2 = new ODataResourcePropertyVerificationBenchmarks();
    bench2.Setup();

    bench2.WriteWithSkipPropertyVerification();
    bench2.WriteWithSkipPropertyVerification();

    bench2.memoryStream.Position = 0;
    var output2 = new StreamReader(bench2.memoryStream, leaveOpen: true).ReadToEnd();
    //Console.WriteLine(output2);

    Console.WriteLine($"Same output: {output == output2}");
}

void DebugReader()
{
    var bench = new ODataResourcePropertyVerificationBenchmarks();
    bench.Setup();

    var resource = bench.ReadResource();
    resource = bench.ReadResource(); // Read again to ensure the stream is reset correctly.
    Console.WriteLine(resource.TypeName);
    var prop = resource.Properties.First();
    Console.WriteLine($"Property: {prop.Name} = {prop.Value}");
    var propCount = resource.Properties.Count();
    Console.WriteLine($"Total Properties: {propCount}");
}