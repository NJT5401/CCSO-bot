﻿using System;
using CCSODiscordBot.Services.Database.DataTables.SubClasses;
using CCSODiscordBot.Services.Database.Repository;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;

namespace CCSODiscordBot.Modules.Roles
{
	public class ComponentRespond : InteractionModuleBase<ShardedInteractionContext>
	{
		private readonly IGuildRepository _IGuildRepository;
        private readonly IUserRepository _IUserRepository;
        public ComponentRespond (IGuildRepository iGuildRepository, IUserRepository iUserRepository)
        {
			_IGuildRepository = iGuildRepository;
            _IUserRepository = iUserRepository;
        }

        [ComponentInteraction("toggle-role-*")]
		[RequireContext(ContextType.Guild)]
		public async Task RoleButton(ulong roleId)
		{
			await DeferAsync(true);

			// Get desired role:
			var role = Context.Guild.Roles.FirstOrDefault(x => x.Id == roleId);
			// Get the user:
			SocketGuildUser? user = Context.User as SocketGuildUser;

			// Ensure role exists and isnt null:
			if (role is null)
			{
				await FollowupAsync("Role cannot be found. Contact an admin.");
				throw new NullReferenceException("Role button role cannot be found and is null.");
			}
			// Ensure user is a SocketGuildUser.
			if (user is null)
			{
				await FollowupAsync("User cannot be found. Contact an admin.");
				throw new NullReferenceException("User is not guild user.");
			}

			// Get user's roles:
			var usersRoles = user.Roles;
			// See if user has role:
			try
			{
				if (usersRoles.Contains(role))
				{
					// Remove:
					await user.RemoveRoleAsync(role);
					await FollowupAsync("Role " + role.Name + " has been removed from your account.", ephemeral: true);
				}
				else
				{
					// Add:
					await user.AddRoleAsync(role);
					await FollowupAsync("Role " + role.Name + " has been added to your account.", ephemeral: true);
				}
			}
			catch (HttpException e) when (e.DiscordCode == Discord.DiscordErrorCode.InsufficientPermissions)
			{
				await FollowupAsync("Bot does not have permission to perform this action. Contact an admin.", ephemeral: true);
			}
		}
        [ComponentInteraction("protected-toggle-role-*")]
        [RequireContext(ContextType.Guild)]
        public async Task ProtectedRoleButton(ulong roleId)
        {
            await DeferAsync(true);

            // Get desired role:
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Id == roleId);
            // Get the user:
            SocketGuildUser? user = Context.User as SocketGuildUser;

            // Ensure role exists and isnt null:
            if (role is null)
            {
                await FollowupAsync("Role cannot be found. Please contact an admin.", ephemeral: true);
                throw new NullReferenceException("Role button role cannot be found and is null.");
            }
            // Ensure user is a SocketGuildUser.
            if (user is null)
            {
                await FollowupAsync("User cannot be found. Please contact an admin.", ephemeral: true);
                throw new NullReferenceException("User is not guild user.");
            }
            // Check for verification:
            var dbUser = await _IUserRepository.GetByDiscordIdAsync(Context.User.Id, Context.Guild.Id);
            if (dbUser == null || !dbUser.verified)
            {
                // btn
                ButtonBuilder getStartedButton = new ButtonBuilder();
                getStartedButton.WithLabel("Get Started");
                getStartedButton.Style = ButtonStyle.Success;
                getStartedButton.WithCustomId("get-started-" + user.Id);
                ComponentBuilder component = new ComponentBuilder();
                component.WithButton(getStartedButton);

                // Check to see if they already have a protected role:
                var dbGuild = await _IGuildRepository.GetByDiscordIdAsync(Context.Guild.Id);
                var protectedRoles = dbGuild.ClassStandings?.Where(_ => _.RequireVerification);
                if (protectedRoles != null)
                {
                    foreach (BtnRole dbRole in protectedRoles)
                    {
                        var socketRole = Context.Guild.GetRole(dbRole.Role);
                        if (Context.Guild.GetUser(Context.User.Id).Roles.Contains(socketRole))
                        {
                            await FollowupAsync("Welcome returning member! Please verify your membership with our new bot by clicking the button below.", components: component.Build(), ephemeral: true);
                            return;
                        }
                    }
                }

                await FollowupAsync("Error: You need to have a verified PSU email to add this role. Click the button below if you would like to update your information.", components: component.Build(), ephemeral: true);
                return;
            }
            // Get user's roles:
            var usersRoles = user.Roles;
            // See if user has role:
            try
            {
                if (usersRoles.Contains(role))
                {
                    // Remove:
                    await user.RemoveRoleAsync(role);
                    await FollowupAsync("Role " + role.Name + " has been removed from your account.", ephemeral: true);
                }
                else
                {
                    // Add:
                    await user.AddRoleAsync(role);
                    await FollowupAsync("Role " + role.Name + " has been added to your account.", ephemeral: true);
                }
            }
            catch (HttpException e) when (e.DiscordCode == Discord.DiscordErrorCode.InsufficientPermissions)
            {
                await FollowupAsync("Bot does not have permission to perform this action. Contact an admin.", ephemeral: true);
            }
        }
    }
}