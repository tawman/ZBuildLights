﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using ZBuildLights.Core.Extensions;
using ZBuildLights.Core.Models;
using ZBuildLights.Core.Repository;
using ZBuildLights.Core.Validation;

namespace ZBuildLights.Core.Builders
{
    public class LightLightGroupManager : ILightGroupManager
    {
        private readonly IMasterModelRepository _masterModelRepository;

        public LightLightGroupManager(IMasterModelRepository masterModelRepository)
        {
            _masterModelRepository = masterModelRepository;
        }

        public CreationResult<LightGroup> CreateLightGroup(Guid projectId, string name)
        {
            var masterModel = _masterModelRepository.GetCurrent();
            if (masterModel.Projects.None(x => x.Id.Equals(projectId)))
                return CreationResult.Fail<LightGroup>(string.Format("Cannot create group for project '{0}' that doesn't exist", projectId));

            var project = masterModel.Projects.Single(x => x.Id.Equals(projectId));
            if (project.Groups.Any(x => x.Name.Equals(name)))
                return CreationResult.Fail<LightGroup>("A group with this name already exists");

            project.AddGroup(new LightGroup {Id = Guid.NewGuid(), Name = name});
            _masterModelRepository.Save(masterModel);

            return CreationResult.Success(new LightGroup());
        }

        public EditResult<LightGroup> UpdateLightGroup(Guid groupId, string name)
        {
            var masterModel = _masterModelRepository.GetCurrent();
            var allGroups = masterModel.Projects.SelectMany(x => x.Groups).ToArray();
            if (allGroups.None(x => x.Id.Equals(groupId)))
                return EditResult.Fail<LightGroup>(BadId(groupId));

            var group = allGroups.Single(x => x.Id.Equals(groupId));
            if (group.Name.Equals(name))
                return EditResult.Success(group);

            var parentProject = group.ParentProject;
            if (parentProject.Groups.Any(x => x.Name.Equals(name)))
                return EditResult.Fail<LightGroup>(string.Format("There is already a group named '{0}' in project '{1}'", name, parentProject.Name));

            group.Name = name;
            _masterModelRepository.Save(masterModel);

            return EditResult.Success(group);
        }

        private static string BadId(Guid groupId)
        {
            return string.Format("Could not locate a light group with ID '{0}'", groupId);
        }

        public EditResult<LightGroup> DeleteLightGroup(Guid groupId)
        {
            var masterModel = _masterModelRepository.GetCurrent();
            var allGroups = masterModel.Projects.SelectMany(x => x.Groups).ToArray();
            if (allGroups.None(x => x.Id.Equals(groupId)))
                return EditResult.Fail<LightGroup>(BadId(groupId));

            var group = allGroups.Single(x => x.Id.Equals(groupId));
            var parent = group.ParentProject;
            parent.RemoveGroup(group);

            return EditResult.Success<LightGroup>(null);
        }
    }
}