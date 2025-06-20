using System.Reflection;
using BashTerm.Exec.Runnables;
using LevelGeneration;

namespace BashTerm.Exec;

public interface IRunnable {
	string CommandName { get; }
	string Description { get; }
	string Help { get; }

	PipedPayload Run(string cmd, List<string> args, PipedPayload payload, LG_ComputerTerminal? term);
}

public static class Dispatch {
	private static Dictionary<string, IRunnable> _handlers = new Dictionary<string, IRunnable>();
	private static bool _isInitialized = false;
	private static IRunnable? _fallback;

	public static int Initialize() {
		_fallback = new FallbackCommand();
		int count = RegisterAllCommands();
		_isInitialized = true;
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

		return _handlers.Count;
	}

	private static bool Hook(string cmd, IRunnable handler) {
		cmd = cmd.Trim().ToLower();
		if (_handlers.ContainsKey(cmd)) { return false; }
		_handlers[cmd] = handler;
		return true;
	}

	public static PipedPayload Exec(Command cmd, LG_ComputerTerminal? term = null) {
		return Exec(cmd, new EmptyPayload(), term);
	}

	public static PipedPayload Exec(Command cmd, PipedPayload payload, LG_ComputerTerminal? term = null) {
		if (!_isInitialized) {
			throw new Exception("[Exec] Executing commands before initialization");
		}

		switch(cmd)
		{
			case null:
				throw new ExecException("Exec called with null command");

			case Execve(string name, List<string> args):
				if (_handlers.TryGetValue(name, out IRunnable? handler)) {
					return handler.Run(name, args, payload, term);
				}
				return _fallback.Run(name, args, payload, term);

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
