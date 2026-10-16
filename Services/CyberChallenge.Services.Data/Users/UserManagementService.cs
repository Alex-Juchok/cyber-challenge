using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CyberChallenge.Common;
using CyberChallenge.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CyberChallenge.Services.Data.Users
{
    /// <summary>
    /// Реализует операции управления пользователями для администратора.
    /// </summary>
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> userManager;

        public UserManagementService(
            UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        /// <summary>
        /// Возвращает пользователей и их основные роли.
        /// </summary>
        public async Task<IReadOnlyCollection<UserManagementDto>> GetAllAsync()
        {
            var users = await this.userManager.Users
                .OrderBy(user => user.Email)
                .ToListAsync();

            // Получаем пользователей по ролям отдельными запросами,
            // а не запрашиваем роли по одному разу для каждого пользователя.
            var administrators = await this.userManager
                .GetUsersInRoleAsync(GlobalConstants.AdministratorRoleName);

            var regularUsers = await this.userManager
                .GetUsersInRoleAsync(GlobalConstants.UserRoleName);

            var administratorIds = administrators
                .Select(user => user.Id)
                .ToHashSet();

            var regularUserIds = regularUsers
                .Select(user => user.Id)
                .ToHashSet();

            return users.Select(user => new UserManagementDto
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,

                Role = administratorIds.Contains(user.Id)
                    ? GlobalConstants.AdministratorRoleName
                    : regularUserIds.Contains(user.Id)
                        ? GlobalConstants.UserRoleName
                        : "Без роли",

                // Постоянную административную блокировку обозначаем
                // максимальным значением даты окончания блокировки.
                IsBlocked = user.LockoutEnd == DateTimeOffset.MaxValue,
            }).ToList();
        }

        /// <summary>
        /// Создаёт аккаунт и назначает ему одну из разрешённых ролей.
        /// </summary>
        public async Task<IdentityResult> CreateAsync(
            string email,
            string password,
            string role)
        {
            if (!IsValidRole(role))
            {
                return Failed("Указана неизвестная роль.");
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = false,
            };

            var createResult = await this.userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                return createResult;
            }

            var roleResult = await this.userManager.AddToRoleAsync(user, role);

            if (!roleResult.Succeeded)
            {
                // Если назначить роль не удалось, удаляем созданный аккаунт.
                await this.userManager.DeleteAsync(user);
                return roleResult;
            }

            return IdentityResult.Success;
        }

        /// <summary>
        /// Изменяет email и роль пользователя.
        /// Нельзя менять собственную роль.
        /// </summary>
        public async Task<IdentityResult> UpdateAsync(
            string userId,
            string email,
            string role,
            string currentAdminId)
        {
            if (!IsValidRole(role))
            {
                return Failed("Указана неизвестная роль.");
            }

            var user = await this.userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return Failed("Пользователь не найден.");
            }

            var isAdministrator = await this.userManager.IsInRoleAsync(
                user,
                GlobalConstants.AdministratorRoleName);

            if (userId == currentAdminId && role != GlobalConstants.AdministratorRoleName)
            {
                return Failed("Нельзя снять роль администратора с собственной учётной записи.");
            }

            if (isAdministrator && role != GlobalConstants.AdministratorRoleName)
            {
                var adminCount = await this.userManager
                    .GetUsersInRoleAsync(GlobalConstants.AdministratorRoleName);

                if (adminCount.Count <= 1)
                {
                    return Failed("Нельзя снять роль с последнего администратора.");
                }
            }

            user.Email = email;
            user.UserName = email;

            var updateResult = await this.userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return updateResult;
            }

            var currentRoles = await this.userManager.GetRolesAsync(user);

            if (currentRoles.Count > 0)
            {
                var removeResult = await this.userManager.RemoveFromRolesAsync(
                    user,
                    currentRoles);

                if (!removeResult.Succeeded)
                {
                    return removeResult;
                }
            }

            var addResult = await this.userManager.AddToRoleAsync(user, role);

            if (!addResult.Succeeded)
            {
                // Не оставляем пользователя без роли, если назначение не удалось.
                await this.userManager.AddToRoleAsync(user, currentRoles.FirstOrDefault()
                    ?? GlobalConstants.UserRoleName);

                return addResult;
            }

            return await this.userManager.UpdateSecurityStampAsync(user);
        }

        /// <summary>
        /// Удаляет пользователя, не позволяя удалить себя или последнего администратора.
        /// </summary>
        public async Task<IdentityResult> DeleteAsync(
            string userId,
            string currentAdminId)
        {
            // Запрещаем администратору удалять собственную учётную запись.
            if (userId == currentAdminId)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "Нельзя удалить собственную учётную запись."
                    });
            }

            var user = await this.userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "Пользователь не найден."
                    });
            }

            // Не позволяем удалить последнего администратора.
            if (await this.userManager.IsInRoleAsync(
                    user,
                    GlobalConstants.AdministratorRoleName))
            {
                var administrators = await this.userManager
                    .GetUsersInRoleAsync(GlobalConstants.AdministratorRoleName);

                if (administrators.Count <= 1)
                {
                    return IdentityResult.Failed(
                        new IdentityError
                        {
                            Description = "Нельзя удалить последнего администратора."
                        });
                }
            }

            // Удаляем связи пользователя с ролями через Identity.
            var roles = await this.userManager.GetRolesAsync(user);

            if (roles.Count > 0)
            {
                var removeRolesResult =
                    await this.userManager.RemoveFromRolesAsync(user, roles);

                if (!removeRolesResult.Succeeded)
                {
                    return removeRolesResult;
                }
            }

            // После удаления связей удаляем самого пользователя.
            return await this.userManager.DeleteAsync(user);
        }


        /// <summary>
        /// Проверяет, что роль входит в разрешённый набор.
        /// </summary>
        private static bool IsValidRole(string role)
        {
            return role == GlobalConstants.UserRoleName
                || role == GlobalConstants.AdministratorRoleName;
        }

        /// <summary>
        /// Блокирует обычного пользователя.
        /// Администратор не может заблокировать самого себя
        /// или другую учётную запись администратора.
        /// </summary>
        public async Task<IdentityResult> BlockAsync(
            string userId,
            string currentAdminId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Failed("Не указан пользователь.");
            }

            if (userId == currentAdminId)
            {
                return Failed("Нельзя заблокировать собственную учётную запись.");
            }

            var user = await this.userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return Failed("Пользователь не найден.");
            }

            if (await this.userManager.IsInRoleAsync(
                user,
                GlobalConstants.AdministratorRoleName))
            {
                return Failed("Нельзя заблокировать учётную запись администратора.");
            }

            // Включаем механизм блокировки Identity и устанавливаем
            // максимальную дату, чтобы отличать бан от временной блокировки.
            var lockoutEnabledResult =
                await this.userManager.SetLockoutEnabledAsync(user, true);

            if (!lockoutEnabledResult.Succeeded)
            {
                return lockoutEnabledResult;
            }

            var lockoutResult = await this.userManager.SetLockoutEndDateAsync(
                user,
                DateTimeOffset.MaxValue);

            if (!lockoutResult.Succeeded)
            {
                return lockoutResult;
            }

            // Обновление штампа безопасности позволяет Identity
            // обнаружить изменение учётной записи при проверке cookie.
            return await this.userManager.UpdateSecurityStampAsync(user);
        }

        /// <summary>
        /// Снимает постоянную административную блокировку.
        /// </summary>
        public async Task<IdentityResult> UnblockAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Failed("Не указан пользователь.");
            }

            var user = await this.userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return Failed("Пользователь не найден.");
            }

            // Не сбрасываем временную блокировку, возникшую
            // по другой причине, например после неудачных попыток входа.
            if (user.LockoutEnd != DateTimeOffset.MaxValue)
            {
                return Failed("У пользователя нет постоянной блокировки.");
            }

            var lockoutResult =
                await this.userManager.SetLockoutEndDateAsync(user, null);

            if (!lockoutResult.Succeeded)
            {
                return lockoutResult;
            }

            return await this.userManager.UpdateSecurityStampAsync(user);
        }

        /// <summary>
        /// Создаёт результат Identity с описанием ошибки.
        /// </summary>
        private static IdentityResult Failed(string description)
        {
            return IdentityResult.Failed(
                new IdentityError { Description = description });
        }

    }
}