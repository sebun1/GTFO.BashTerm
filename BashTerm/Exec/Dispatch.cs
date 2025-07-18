using System.Reflection;
using System.Text;
using BashTerm.Exec.Runnables;
using BashTerm.Parsers;
using BashTerm.Utils;
using BashTerm.Sys;
using LevelGeneration;

namespace BashTerm.Exec;

public static class Dispatch {
	internal static Dictionary<string, IProc> Handlers = new();
	internal static Dictionary<string, Type> HandlerTypes = new();
	internal static bool IsInitialized = false;
	internal static IProc? Fallback;

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
			var attr = type.GetCustomAttribute<BshProcAttribute>();
			if (attr != null && typeof(IProc).IsAssignableFrom(type)) {
				if (Hook(attr.Name, type)) {
					Log.Debug($"{type.FullName} hooked with command name '{attr.Name}'");
				} else {
					Log.Warn($"{type.FullName} with handler name '{attr.Name}' was not hooked because a handler for that name already exists.");
					Bsh.LogWarn("dispatch", $"{type.FullName} with handler name '{attr.Name}' was not hooked because a handler for that name already exists.");
				}
			}
		}

		return Handlers.Count;
	}

	private static bool Hook(string cmd, Type handlerType) {
		cmd = cmd.Trim().ToLower();
		if (Handlers.ContainsKey(cmd)) { return false; }
		HandlerTypes[cmd] = handlerType;
		Handlers[cmd] = (IProc)Activator.CreateInstance(handlerType);
		return true;
	}

	public static PipedPayload Exec(VarCommand cmd, LG_ComputerTerminal term) {
		return Exec(cmd, new EmptyPayload(), term);
	}

	public static PipedPayload Exec(VarCommand cmd, PipedPayload payload, LG_ComputerTerminal term) {
		if (!IsInitialized) {
			throw new ExecException("executing commands before initialization");
		}

		switch(cmd)
		{
			case null:
				throw new ExecException("cannot execute null command");

			case VarExecve(TokenWord wName, List<TokenWord> wArgs):
				string name = Arg2Str(wName, term);
				if (Handlers.TryGetValue(name, out IProc? handler) && HandlerTypes.TryGetValue(name, out Type handlerType)) {
					FieldInfo fSchemaField = handlerType.GetField("FSchema", BindingFlags.Public | BindingFlags.Static)!;
					FlagSchema? schema = (FlagSchema?)fSchemaField.GetValue(null);
					List<string> args = CtxArgs2Str(wArgs, term, handler);
					FlagParser fp = new FlagParser(args, schema ?? new FlagSchema());
					CmdOpts opts = new CmdOpts(fp.Flags);
					return handler.Run(name, fp.Positionals, opts, payload, term);
				}
				var argsNoCtx = Args2Str(wArgs, term);
				return Fallback!.Run(name, argsNoCtx, CmdOpts.EmptyOpts(), payload, term);

			case VarPipe(VarCommand first, VarCommand post):
				return Exec(post, Exec(first, payload, term), term);

			case VarSequence(VarCommand first, VarCommand second):
				Exec(first, payload, term);
				return Exec(second, term);

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

	private static List<string> CtxArgs2Str(List<TokenWord> wArgs, LG_ComputerTerminal term, IProc handler) {
		List<string> args = new();
		foreach (TokenWord tw in wArgs) {
			args.Add(CtxArg2Str(tw, term, handler));
		}
		return args;
	}

	private static string CtxArg2Str(TokenWord word, LG_ComputerTerminal term, IProc handler) {
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
