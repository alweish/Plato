﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Plato.Internal.Layout.ModelBinding;
using Plato.Internal.Layout.ViewProviders;
using Plato.Internal.Models.Roles;
using Plato.Internal.Models.Users;
using Plato.Internal.Stores.Abstractions.Roles;
using Plato.Roles.ViewModels;

namespace Plato.Roles.ViewProviders
{
    public class RoleViewProvider : BaseViewProvider<Role>
    {

        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly IPlatoRoleStore _platoRoleStore;
        
        public RoleViewProvider(
            UserManager<User> userManager,
            IPlatoRoleStore platoRoleStore,
            RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _platoRoleStore = platoRoleStore;
            _roleManager = roleManager;
            ;
        
        }


        public override async Task<IViewProviderResult> BuildDisplayAsync(Role role, IUpdateModel updater)
        {

            return Views(
                View<Role>("Role.Display.Header", model => role).Zone("header"),
                View<Role>("Role.Display.Meta", model => role).Zone("meta"),
                View<Role>("Role.Display.Content", model => role).Zone("content"),
                View<Role>("Role.Display.Footer", model => role).Zone("footer")
            );

        }

        public override async Task<IViewProviderResult> BuildIndexAsync(Role role, IUpdateModel updater)
        {
            return Views(
                View<Role>("User.List", model => role).Zone("header").Order(3)
            );

        }

        public override async Task<IViewProviderResult> BuildEditAsync(Role role, IUpdateModel updater)
        {

            var isNewRole =await  IsNewRole(role.Id);

            return Views(
                View<EditRoleViewModel>("Role.Edit.Header", model =>
                {
                    model.Id = role.Id;
                    model.RoleName = role.Name;
                    model.IsNewRole = isNewRole;
                    return model;
                }).Zone("header"),
                View<EditRoleViewModel>("Role.Edit.Meta", model =>
                {
                    model.Id = role.Id;
                    model.RoleName = role.Name;
                    model.IsNewRole = isNewRole;
                    return model;
                }).Zone("meta"),
                View<EditRoleViewModel>("Role.Edit.Content", model =>
                {
                    model.Id = role.Id;
                    model.RoleName = role.Name;
                    model.IsNewRole = isNewRole;
                    return model;
                }).Zone("content"),
                View<EditRoleViewModel>("Role.Edit.Footer", model =>
                {
                    model.Id = role.Id;
                    model.RoleName = role.Name;
                    model.IsNewRole = isNewRole;
                    return model;
                }).Zone("footer"),
                View<EditRoleViewModel>("Role.Edit.Actions", model =>
                {
                    model.Id = role.Id;
                    model.RoleName = role.Name;
                    model.IsNewRole = isNewRole;
                    return model;
                }).Zone("actions")
            );

        }

        public override async Task<IViewProviderResult> BuildUpdateAsync(Role role, IUpdateModel updater)
        {

            var model = new EditRoleViewModel();

            if (!await updater.TryUpdateModelAsync(model))
            {
                return await BuildEditAsync(role, updater);
            }

            if (updater.ModelState.IsValid)
            {

                role.Name = model.RoleName?.Trim();
         
                //await _userManager.SetUserNameAsync(user, model.UserName);
                //await _userManager.SetEmailAsync(user, model.Email);

                var result = await _roleManager.CreateAsync(role);

                foreach (var error in result.Errors)
                {
                    updater.ModelState.AddModelError(string.Empty, error.Description);
                }

            }

            return await BuildEditAsync(role, updater);

        }

        private async Task<bool> IsNewRole(int roleId)
        {
            return await _roleManager.FindByIdAsync(roleId.ToString()) == null;
        }

    }
}