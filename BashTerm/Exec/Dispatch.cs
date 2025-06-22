using System.Reflection;
using BashTerm.Exec.Runnables;
using LevelGeneration;

namespace BashTerm.Exec;

public interface IRunnable {
	string CommandName { get; }
	string Desc { get; }
	string Manual { get; }

	PipedPayload Run(string cmd, List<string> args, PipedPayload payload, LG_ComputerTerminal term);
}

public static class Dispatch {
	internal static Dictionary<string, IRunnable> Handlers = new Dictionary<string, IRunnable>();
	internal static bool IsInitialized = false;
	internal static IRunnable? Fallback;

	public static int Initialize() {
		Fallback = new FallbackCommand();
		int count = RegisterAllCommands();
		IsInitialized = true;
		return count;
	}

	private static int RegisterAllCommands() {
		var types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();

		// List<Type> unhookedCommands = new List<Type>();
		// TODO: Record commands that are not hooked?

		foreach (var type in types) {
			var attr = type.GetCustomAttribute<CommandHandlerAttribute>();
			if (attr != null && typeof(IRunnable).IsAssignableFrom(type)) {
				IRunnable instance = (IRunnable)Activator.CreateInstance(type);
				if (Hook(attr.Name, instance)) {
					Logger.Debug($"{type.FullName} hooked with command name '{attr.Name}'");
				} else {
					Logger.Warn($"{type.FullName} with handler name '{attr.Name}' was not hooked because a handler for that name already exists.");
				}
			}
		}

		return Handlers.Count;
	}

	private static bool Hook(string cmd, IRunnable handler) {
		cmd = cmd.Trim().ToLower();
		if (Handlers.ContainsKey(cmd)) { return false; }
		Handlers[cmd] = handler;
		return true;
	}

	public static PipedPayload Exec(Command cmd, LG_ComputerTerminal? term = null) {
		return Exec(cmd, new EmptyPayload(), term);
	}

	public static PipedPayload Exec(Command cmd, PipedPayload payload, LG_ComputerTerminal? term = null) {
		if (!IsInitialized) {
			throw new ExecException("Executing commands before initialization");
		}

		switch(cmd)
		{
			case null:
				throw new ExecException("Exec called with null command");

			case Execve(string name, List<string> args):
				if (Handlers.TryGetValue(name, out IRunnable? handler)) {
					return handler.Run(name, args, payload, term);
				}
				return Fallback.Run(name, args, payload, term);

			case Pipe(Command first, Command post):
				return Exec(post, Exec(first, payload, term), term);

			case Sequence(Command first, Command second):
				Exec(first, payload, term);
				return Exec(second, term);

			default:
				throw new UnknownParserCommandTypeException(cmd.GetType());
		}
	}
}
