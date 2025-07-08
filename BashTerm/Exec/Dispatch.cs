using System.Reflection;
using System.Text;
using BashTerm.Exec.Runnables;
using BashTerm.Parsers;
using BashTerm.Utils;
using LevelGeneration;

namespace BashTerm.Exec;

public interface IRunnable {
	string CommandName { get; }
	string Desc { get; }
	string Manual { get; }

	FlagSchema FSchema { get; }

	Task<PipedPayload> Run(string cmd, List<string> args, CmdOpts opts, PipedPayload payload, LG_ComputerTerminal term);
	bool TryGetVarValue(LG_ComputerTerminal term, string varName, out string value);
	bool TryExpandArg(LG_ComputerTerminal term, string arg, out string expanded);
}

public static class Dispatch {
	internal static Dictionary<string, IRunnable> Handlers = new();
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
					BshSystem.LogWarn("dispatch", $"{type.FullName} with handler name '{attr.Name}' was not hooked because a handler for that name already exists.");
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

	public static async Task<PipedPayload> Exec(VarCommand cmd, LG_ComputerTerminal term) {
		return await Exec(cmd, new EmptyPayload(), term);
	}

	public static async Task<PipedPayload> Exec(VarCommand cmd, PipedPayload payload, LG_ComputerTerminal term) {
		if (!IsInitialized) {
			throw new ExecException("executing commands before initialization");
		}

		switch(cmd)
		{
			case null:
				throw new ExecException("cannot execute null command");

			case VarExecve(TokenWord wName, List<TokenWord> wArgs):
				string name = Arg2Str(wName, term);
				if (Handlers.TryGetValue(name, out IRunnable? handler)) {
					List<string> args = CtxArgs2Str(wArgs, term, handler);
					FlagParser fp = new FlagParser(args, handler.FSchema);
					CmdOpts opts = new CmdOpts(fp.Flags);
					return await handler.Run(name, fp.Positionals, opts, payload, term);
				}
				var argsNoCtx = Args2Str(wArgs, term);
				return await Fallback!.Run(name, argsNoCtx, CmdOpts.EmptyOpts(), payload, term);

			case VarPipe(VarCommand first, VarCommand post):
				return await Exec(post, await Exec(first, payload, term), term);

			case VarSequence(VarCommand first, VarCommand second):
				await Exec(first, payload, term);
				return await Exec(second, term);

			default:
				throw new UnknownParserCommandTypeException(cmd.GetType());
		}
	}

	private static List<string> Args2Str(List<TokenWord> wArgs, LG_ComputerTerminal term) {
		List<string> args = new();
		foreach (TokenWord tw in wArgs) {
			args.Add(Arg2Str(tw, term));
		}

		return args;
	}

	private static string Arg2Str(TokenWord word, LG_ComputerTerminal term) {
		VarProvider vp = new VarProvider(term);
		StringBuilder sb = new();
		foreach (WordPart part in word.parts) {
			switch (part) {
				case WordVar wv:
					if (vp.TryGetVarValue(wv.varName, out string? value)) {
						sb.Append(value);
					}
					break;
				case WordText wt:
					sb.Append(wt.text);
					break;
			}
		}
		return sb.ToString();
	}

	private static List<string> CtxArgs2Str(List<TokenWord> wArgs, LG_ComputerTerminal term, IRunnable handler) {
		List<string> args = new();
		foreach (TokenWord tw in wArgs) {
			args.Add(CtxArg2Str(tw, term, handler));
		}
		return args;
	}

	private static string CtxArg2Str(TokenWord word, LG_ComputerTerminal term, IRunnable handler) {
		VarProvider vp = new VarProvider(term);
		StringBuilder sb = new();
		foreach (WordPart part in word.parts) {
			switch (part) {
				case WordVar wv:
					if (vp.TryGetVarValue(wv.varName, out string? value) ||
					    handler.TryGetVarValue(term, wv.varName, out value)) {
						sb.Append(value);
					}
					break;
				case WordText wt:
					sb.Append(wt.text);
					break;
			}
		}

		if (handler.TryExpandArg(term, sb.ToString(), out string? expanded)) {
			return expanded;
		}
		return sb.ToString();
	}
}
