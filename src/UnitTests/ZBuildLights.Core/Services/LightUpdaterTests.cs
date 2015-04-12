using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Should;
using UnitTests._Bases;
using UnitTests._Stubs;
using ZBuildLights.Core.Models;
using ZBuildLights.Core.Repository;
using ZBuildLights.Core.Services;

namespace UnitTests.ZBuildLights.Core.Services
{
    public class LightUpdaterTests
    {
        [TestFixture]
        public class HappyPath : TestBase
        {
            private const int _zwaveHomeId = 1;
            private const int _zWaveDeviceId = 14;
            private MasterModel _saved;
            private Guid _destinationGroupId;

            [SetUp]
            public void ContextSetup()
            {
                _destinationGroupId = Guid.NewGuid();

                var existingMasterModel = new MasterModel();
                var project = existingMasterModel.CreateProject(x => x.Name = "Existing Project");
                project.CreateGroup(x => x.Id = _destinationGroupId).AddLight(new Light(1, 11, 123)).AddLight(new Light(1, 12, 123));
                project.CreateGroup().AddLight(new Light(1, 13, 123)).AddLight(new Light(1, 14, 123));

                project.CreateGroup()
                    .AddLight(new Light(1, 15, 123))
                    .AddLight(new Light(1, 16, 123));

                project.CreateGroup().AddLight(new Light(2, 13, 123)).AddLight(new Light(2, 14, 123));

                var project2 = existingMasterModel.CreateProject(x => x.Name = "Existing Project 2");
                project2.CreateGroup().AddLight(new Light(1, 21, 123)).AddLight(new Light(1, 22, 123));

                var project3 = existingMasterModel.CreateProject(x => x.Name = "Existing Project 3");
                project3.CreateGroup().AddLight(new Light(1, 31, 123)).AddLight(new Light(1, 32, 123));


                var repository = new StubMasterModelRepository();
                repository.UseCurrentModel(existingMasterModel);

                var updater = new LightUpdater(repository);
                updater.Update(_zwaveHomeId, _zWaveDeviceId, _destinationGroupId, LightColor.Red.Value);

                _saved = repository.LastSaved;
            }

            [Test]
            public void Should_move_the_light_to_the_indicated_group()
            {
                var light = _saved.FindLight(_zwaveHomeId, _zWaveDeviceId);
                light.ParentGroup.Id.ShouldEqual(_destinationGroupId);
            }

            [Test]
            public void Should_update_the_light_color()
            {
                var light = _saved.FindLight(_zwaveHomeId, _zWaveDeviceId);
                light.Color.ShouldEqual(LightColor.Red);
            }

            [Test]
            public void Should_save_the_updated_model()
            {
                _saved.ShouldNotBeNull();
            }
        }

        [TestFixture]
        public class When_assigning_an_unassigned_light : TestBase
        {
            private const int _zwaveHomeId = 1;
            private const int _zWaveDeviceId = 14;
            private MasterModel _masterModel;

            [SetUp]
            public void ContextSetup()
            {
                var groupId = Guid.NewGuid();

                _masterModel = new MasterModel();
                var project = _masterModel.CreateProject(x => x.Name = "Existing Project");
                project.CreateGroup(x => x.Id = groupId).AddLight(new Light(1, 11, 123)).AddLight(new Light(1, 12, 123));

                var unassignedLights = new[]
                {
                    new Light(1, 51, 123),
                    new Light(_zwaveHomeId, _zWaveDeviceId, 123),
                    new Light(1, 53, 123),
                };

                _masterModel.AddUnassignedLights(unassignedLights);

                var repository = S<IMasterModelRepository>();
                repository.Stub(x => x.GetCurrent()).Return(_masterModel);

                var updater = new LightUpdater(repository);
                updater.Update(_zwaveHomeId, _zWaveDeviceId, groupId, LightColor.Red.Value);
            }

            [Test]
            public void Should_move_the_light_to_the_indicated_group()
            {
                _masterModel.AllGroups.Single()
                    .Lights.Any(x => x.ZWaveHomeId.Equals(_zwaveHomeId) && x.ZWaveDeviceId.Equals(_zWaveDeviceId))
                    .ShouldBeTrue();
            }
        }

        [TestFixture]
        public class When_unassigning_an_assigned_light : TestBase
        {
            private MasterModel _lastSaved;

            [SetUp]
            public void ContextSetup()
            {
                const uint zwaveHomeId = 1;
                const byte zWaveDeviceId = 11;

                var masterModel = new MasterModel();
                var project = masterModel.CreateProject(x => x.Name = "Existing Project");

                project.CreateGroup().AddLight(new Light(zwaveHomeId, zWaveDeviceId, 123) { Color = LightColor.Yellow });

                var repository = new StubMasterModelRepository();
                repository.UseCurrentModel(masterModel);

                var updater = new LightUpdater(repository);
                updater.Update(zwaveHomeId, zWaveDeviceId, Guid.Empty, LightColor.Red.Value);

                _lastSaved = repository.LastSaved;
            }

            [Test]
            public void Should_unassign_the_light()
            {
                _lastSaved.UnassignedLights.Length.ShouldEqual(1);
                _lastSaved.UnassignedLights[0].Color.ShouldEqual(LightColor.Red);
            }
        }

        [TestFixture]
        public class When_indicated_light_is_not_in_master_model_and_not_unassigned_in_the_network : TestBase
        {
            private Exception _thrown;

            [SetUp]
            public void ContextSetup()
            {
                var groupId = Guid.NewGuid();

                var _masterModel = new MasterModel();
                var project = _masterModel.CreateProject(x => x.Name = "Existing Project");
                project.CreateGroup(x => x.Id = groupId);

                var repository = S<IMasterModelRepository>();
                repository.Stub(x => x.GetCurrent()).Return(_masterModel);

                var updater = new LightUpdater(repository);
                try
                {
                    updater.Update(12, 42, groupId, LightColor.Red.Value);
                }
                catch (Exception e)
                {
                    _thrown = e;
                }
            }

            [Test]
            public void Should_throw_an_exception_when_searching_for_a_light()
            {
                _thrown.GetType().ShouldEqual(typeof (InvalidOperationException));
                _thrown.Message.ShouldEqual("Could not find light with homeId: 12 and deviceId: 42");
            }
        }
    }
}