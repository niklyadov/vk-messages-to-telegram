using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Telegram.Bot;
using VkToTg.Commands;
using VkToTg.Commands.Core;

namespace VkToTg.Services.Telegram
{
    public class CommandsManager
    {
        IServiceScopeFactory _serviceScopeFactory;
        ILogger<CommandsManager> _logger;

        public CommandsManager(IServiceScopeFactory serviceScopeFactory, ILogger<CommandsManager> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public Type FindCommandType(string commandName)
        {
            Type commandType = null;

            try
            {
                commandType = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsDefined(typeof(Attributes.CommandAttribute)))
                .First(t => t.GetCustomAttributes<Attributes.CommandAttribute>()
                                .Count(a => a.Name == commandName) > 0);
            }
            catch (Exception ex) when (commandType == null)
            {
                if (ex != null)
                {
                    _logger.LogError($"Failed to find command {ex.Message}.");
                }
            }

            return commandType;
        }

        public ICollection<Attributes.CommandAttribute> GetAllCommandsAttribute()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsDefined(typeof(Attributes.CommandAttribute)))
                .Select(t => t.GetCustomAttributes<Attributes.CommandAttribute>().FirstOrDefault())
                .ToList();
        }

        public BaseCommand GetCommandImplementation(string commandText, ITelegramBotClient botClient)
        {
            var commandType = FindCommandType(commandText);
            var command = commandType != null ? Activator.CreateInstance(commandType, _serviceScopeFactory, botClient) as BaseCommand
                : new DefaultCommand(_serviceScopeFactory, botClient);

            return command;
        }
    }
}