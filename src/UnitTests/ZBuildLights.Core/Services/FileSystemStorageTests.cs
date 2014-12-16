﻿using NUnit.Framework;
using Rhino.Mocks;
using Should;
using UnitTests._Bases;
using UnitTests._Stubs;
using ZBuildLights.Core.Models;
using ZBuildLights.Core.Services;
using ZBuildLights.Core.Wrappers;

namespace UnitTests.ZBuildLights.Core.Services
{
    public class FileSystemStorageTests
    {
        [TestFixture]
        public class When_saving_model : TestBase
        {
            private string _result;
            private string _stubbedJson;

            [SetUp]
            public void ContextSetup()
            {
                var filePath = @"C:\some\dir";
                _stubbedJson = "I am some json, I promise (just kidding)";
                var model = new MasterModel();

                var fileSystem = new FileSystemStub();
                var jsonSerializer = S<IJsonSerializerService>();
                jsonSerializer.Stub(x => x.SerializeMasterModel(model)).Return(_stubbedJson);
                var configuration = new StubApplicationConfiguration {StorageFilePath = filePath};

                var storage = new FileSystemStorage(fileSystem, jsonSerializer, configuration);
                storage.Save(model);

                _result = fileSystem.GetLastWriteTo(filePath);
            }


            [Test]
            public void Should_save_json_serialized_file_to_the_configured_file_path()
            {
                _result.ShouldEqual(_stubbedJson);
            }
        }

        [TestFixture]
        public class When_reading_model_and_file_exists : TestBase
        {
            private MasterModel _result;
            private MasterModel _stubbedModel;

            [SetUp]
            public void ContextSetup()
            {
                const string filePath = @"C:\some\dir";
                const string stubbedJson = "I am some json, I promise (just kidding)";
                _stubbedModel = new MasterModel();

                var fileSystem = new FileSystemStub();
                fileSystem.StubContentForPath(filePath, stubbedJson);

                var jsonSerializer = S<IJsonSerializerService>();
                jsonSerializer.Stub(x => x.DeserializeMasterModel(stubbedJson)).Return(_stubbedModel);
                var configuration = new StubApplicationConfiguration {StorageFilePath = filePath};

                var storage = new FileSystemStorage(fileSystem, jsonSerializer, configuration);
                _result = storage.ReadMasterModel();
            }


            [Test]
            public void Should_deserialize_json_from_the_configured_file_path()
            {
                _result.ShouldBeSameAs(_stubbedModel);
            }
        }
        
        [TestFixture]
        public class When_reading_model_and_file_does_not_exist : TestBase
        {
            private MasterModel _result;
            private MasterModel _stubbedModel;

            [SetUp]
            public void ContextSetup()
            {
                const string filePath = @"C:\some\dir";
                var configuration = new StubApplicationConfiguration { StorageFilePath = filePath };
                _stubbedModel = new MasterModel();

                var fileSystem = S<IFileSystem>();
                fileSystem.Stub(x => x.FileExists(filePath)).Return(false);

                IJsonSerializerService doNotUseJsonSerializer = null;

                var storage = new FileSystemStorage(fileSystem, doNotUseJsonSerializer, configuration);
                _result = storage.ReadMasterModel();
            }

            [Test]
            public void Should_return_null_result()
            {
                _result.ShouldBeNull();
            }
        }
    }
}