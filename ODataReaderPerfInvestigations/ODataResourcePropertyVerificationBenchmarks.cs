using BenchmarkDotNet.Attributes;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Spatial;
using Microsoft.VSDiagnostics;
using System;


namespace ODataReaderPerfInvestigations;

[MemoryDiagnoser]
[CPUUsageDiagnoser]
public class ODataResourcePropertyVerificationBenchmarks
{
    private List<(IEdmTypeReference, object)> propertiesData = null!;
    private List<ODataProperty> properties = null!;
    private const int NumOfProperties = 1000;
    private EdmModel model = null!;
    private EdmEntityType sampleEntityType = null!;
    private EdmEntitySet samplesEntitySet = null!;
    private ODataMessageWriterSettings messageWriterSettings = null!;
    private ODataMessageReaderSettings messageReaderSettings = null!;
    internal Stream memoryStream = null!;
    internal Stream memoryStreamForReaderBenchmarks = null!;

    [GlobalSetup]
    public void Setup()
    {
        this.memoryStream = new MemoryStream();
        this.messageWriterSettings = new ODataMessageWriterSettings
        {
            Version = ODataVersion.V4,
            EnableMessageStreamDisposal = false,
            BaseUri = new Uri("https://tempuri.org"),
            ODataUri = new ODataUri { ServiceRoot = new Uri("https://tempuri.org") },
            BufferSize = 16 * 1024
        };

        this.messageReaderSettings = new ODataMessageReaderSettings
        {
            Version = ODataVersion.V4,
            BaseUri = new Uri("https://tempuri.org"),
            EnableMessageStreamDisposal = false
        };

        model = new EdmModel();

        var colorEnumType = new EdmEnumType("NS", "Color");
        colorEnumType.AddMember("Black", new EdmEnumMemberValue(1));
        colorEnumType.AddMember("White", new EdmEnumMemberValue(2));

        propertiesData =
        [
            (EdmCoreModel.Instance.GetBoolean(false), true),
            (EdmCoreModel.Instance.GetInt16(false), (short)7),
            (EdmCoreModel.Instance.GetInt32(false), 13),
            (EdmCoreModel.Instance.GetInt64(false), 6078747774547L),
            (EdmCoreModel.Instance.GetDecimal(false), 7654321m),
            (EdmCoreModel.Instance.GetSingle(false), 3.142f),
            (EdmCoreModel.Instance.GetDouble(false), 3.14159265359d),
            (EdmCoreModel.Instance.GetString(false), "Foo"),
            (EdmCoreModel.Instance.GetGuid(false), new Guid(23, 59, 59, [0, 1, 2, 3, 4, 5, 6, 7])),
            (EdmCoreModel.Instance.GetDateTimeOffset(false), new DateTimeOffset(new DateTime(1970, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc))),
            (EdmCoreModel.Instance.GetDuration(false), new TimeSpan(23, 59, 59)),
            (EdmCoreModel.Instance.GetByte(false), (byte)1),
            (EdmCoreModel.Instance.GetSByte(false), (sbyte)9),
            (EdmCoreModel.Instance.GetBinary(false), new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }),
            (EdmCoreModel.Instance.GetDate(false), new Date(1970, 1, 1)),
            (EdmCoreModel.Instance.GetTimeOfDay(false), new TimeOfDay(23, 59, 59, 0)),
            (new EdmEnumTypeReference(colorEnumType, false), new ODataEnumValue("Black")),
            (EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.GeographyPoint, false), GeographyPoint.Create(22.2, 22.2)),
            (EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.GeometryPoint, false), GeometryPoint.Create(7, 13)),
            (EdmCoreModel.GetCollection(EdmCoreModel.Instance.GetBoolean(false)), new ODataCollectionValue { Items = [true, false] }),
            (EdmCoreModel.GetCollection(EdmCoreModel.Instance.GetInt16(false)), new ODataCollectionValue { Items = [(short)7, (short)11] }),
            (EdmCoreModel.GetCollection(EdmCoreModel.Instance.GetInt32(false)), new ODataCollectionValue { Items = [13, 31] }),
            (EdmCoreModel.GetCollection(EdmCoreModel.Instance.GetInt64(false)), new ODataCollectionValue { Items = [6078747774547L, 7454777478706L] }),
            (EdmCoreModel.GetCollection(EdmCoreModel.Instance.GetDecimal(false)), new ODataCollectionValue { Items = [1.0m, 2.0m, 3.0m] }),
            (EdmCoreModel.GetCollection(EdmCoreModel.Instance.GetSingle(false)), new ODataCollectionValue { Items = [3.142f, 241.3f] }),
            (EdmCoreModel.GetCollection(EdmCoreModel.Instance.GetDouble(false)), new ODataCollectionValue { Items = [3.14159265359d, 95356295141.3d] }),
            (EdmCoreModel.GetCollection(EdmCoreModel.Instance.GetString(false)), new ODataCollectionValue { Items = ["Foo", "Bar"] }),
            (EdmCoreModel.GetCollection(EdmCoreModel.Instance.GetGuid(false)), new ODataCollectionValue { Items = [new Guid(23, 59, 59, [0, 1, 2, 3, 4, 5, 6, 7]), new Guid(11, 29, 29, [7, 6, 5, 4, 3, 2, 1, 0])] }),
            (EdmCoreModel.GetCollection(EdmCoreModel.Instance.GetDateTimeOffset(false)), new ODataCollectionValue { Items = [new DateTimeOffset(new DateTime(1970, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc)), new DateTimeOffset(new DateTime(1858, 11, 17, 11, 29, 29, 0, DateTimeKind.Utc))] }),
            (EdmCoreModel.GetCollection(EdmCoreModel.Instance.GetDuration(false)), new ODataCollectionValue { Items = [new TimeSpan(23, 59, 59), new TimeSpan(11, 29, 29)] }),
            (EdmCoreModel.GetCollection(EdmCoreModel.Instance.GetByte(false)), new ODataCollectionValue { Items = [(byte)1, (byte)9] }),
            (EdmCoreModel.GetCollection(EdmCoreModel.Instance.GetSByte(false)), new ODataCollectionValue { Items = [(sbyte)9, (sbyte)1] }),
            (EdmCoreModel.GetCollection(EdmCoreModel.Instance.GetBinary(false)), new ODataCollectionValue { Items = [new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, new byte[] { 0, 9, 8, 7, 6, 5, 4, 3, 2, 1 }] }),
            (EdmCoreModel.GetCollection(EdmCoreModel.Instance.GetDate(false)), new ODataCollectionValue { Items = [new Date(1970, 12, 31), new Date(1858, 11, 17)] }),
            (EdmCoreModel.GetCollection(EdmCoreModel.Instance.GetTimeOfDay(false)), new ODataCollectionValue { Items = [new TimeOfDay(23, 59, 59, 0), new TimeOfDay(11, 29, 29, 0)] }),
            (EdmCoreModel.GetCollection(new EdmEnumTypeReference(colorEnumType, false)), new ODataCollectionValue { Items = [new ODataEnumValue("Black"), new ODataEnumValue("White")] }),
            (EdmCoreModel.GetCollection(EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.GeographyPoint, false)), new ODataCollectionValue { Items = [GeographyPoint.Create(22.2, 22.2), GeographyPoint.Create(11.9, 11.6)] }),
            (EdmCoreModel.GetCollection(EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.GeometryPoint, false)), new ODataCollectionValue { Items = [GeometryPoint.Create(7, 13), GeometryPoint.Create(13, 7)] }),
        ];

        sampleEntityType = new EdmEntityType("NS", "Sample");
        sampleEntityType.AddKeys(sampleEntityType.AddStructuralProperty("Property0", EdmCoreModel.Instance.GetInt32(false)));

        var defaultEntityContainer = model.AddEntityContainer("Default", "Container");
        samplesEntitySet = defaultEntityContainer.AddEntitySet("Samples", sampleEntityType);

        properties = new List<ODataProperty>(NumOfProperties)
        {
            new ODataProperty { Name = "Property0", Value = 1 }
        };
        int index = 0;
        for (int i = 1; i < NumOfProperties; i++)
        {

            var propertyData = propertiesData[index];
            var propertyName = $"Property{i + 1}";

            sampleEntityType.AddStructuralProperty(propertyName, propertyData.Item1);
            properties.Add(new ODataProperty
            {
                Name = propertyName,
                Value = propertyData.Item2
            });

            index = (index + 1) % propertiesData.Count; // Loop back to start when reaching the end
        }

        WriteDataForReaderBenchmarks();
    }

    private void WriteDataForReaderBenchmarks()
    {
        memoryStreamForReaderBenchmarks = new MemoryStream();
        using var responseMessage = new InMemoryMessage(memoryStreamForReaderBenchmarks, leaveOpen: true);
        responseMessage.SetHeader(ODataConstants.ContentTypeHeader, "application/json");
        using var messageWriter = new ODataMessageWriter((IODataResponseMessage)responseMessage, this.messageWriterSettings);
        var resourceWriter = messageWriter.CreateODataResourceWriter(samplesEntitySet, sampleEntityType);
        var resource = new ODataResource
        {
            TypeName = "NS.Sample",
            SkipPropertyVerification = true,
            Properties = properties
        };
        resourceWriter.WriteStart(resource);
        resourceWriter.WriteEnd();
        resourceWriter.Flush();
    }

    [Benchmark]
    public ODataResource CreateODataResourceWithNoSkipVerification()
    {
        var resource = new ODataResource
        {
            TypeName = "NS.Sample"
        };

        resource.Properties = properties;
        return resource;
    }

    [Benchmark]
    public ODataResource CreateODataResourceWithSkipVerification()
    {
        var resource = new ODataResource
        {
            TypeName = "NS.Sample",
            SkipPropertyVerification = true
        };

        resource.Properties = properties;
        return resource;
    }

    [Benchmark]
    public void WriteWithNoSkipPropertyVerification()
    {
        var resource = new ODataResource
        {
            TypeName = "NS.Sample"
        };

        resource.Properties = properties;

        memoryStream.Position = 0;
        using var responseMessage = new InMemoryMessage(memoryStream, leaveOpen: true);
        responseMessage.SetHeader(ODataConstants.ContentTypeHeader, "application/json");
        using var messageWriter = new ODataMessageWriter((IODataResponseMessage)responseMessage, this.messageWriterSettings);

        var resourceWriter = messageWriter.CreateODataResourceWriter(samplesEntitySet, sampleEntityType);
        resourceWriter.WriteStart(resource);
        resourceWriter.WriteEnd();

        resourceWriter.Flush();
    }

    [Benchmark]
    public void WriteWithSkipPropertyVerification()
    {
        var resource = new ODataResource
        {
            TypeName = "NS.Sample",
            SkipPropertyVerification = true
        };

        resource.Properties = properties;

        memoryStream.Position = 0;
        using var responseMessage = new InMemoryMessage(memoryStream, leaveOpen: true);
        responseMessage.SetHeader(ODataConstants.ContentTypeHeader, "application/json");
        using var messageWriter = new ODataMessageWriter((IODataResponseMessage)responseMessage, this.messageWriterSettings);

        var resourceWriter = messageWriter.CreateODataResourceWriter(samplesEntitySet, sampleEntityType);
        resourceWriter.WriteStart(resource);
        resourceWriter.WriteEnd();

        resourceWriter.Flush();
    }

    [Benchmark]
    public ODataResource ReadResource()
    {
        memoryStreamForReaderBenchmarks.Position = 0;
        using var responseMessage = new InMemoryMessage(memoryStreamForReaderBenchmarks, leaveOpen: true);
        responseMessage.SetHeader(ODataConstants.ContentTypeHeader, "application/json");
        using var messageReader = new ODataMessageReader((IODataResponseMessage)responseMessage, this.messageReaderSettings, model);

        ODataResource resource = null;

        var resourceReader = messageReader.CreateODataResourceReader(samplesEntitySet, sampleEntityType);
        while (resourceReader.Read())
        {
            if (resourceReader.State == ODataReaderState.ResourceEnd)
            {
                resource = (ODataResource)resourceReader.Item;
            }
        }

        return resource;
    }
}
