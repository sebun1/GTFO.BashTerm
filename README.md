<div align="center"><img src="https://git.takina.io/gtfo/BashTerm/-/raw/main/.assets/icon.png?ref_type=heads" width="200" alt="BashTerm Icon"></div>

<h1 align="center">BashTerm</h1>

<p align="center">An Improved GTFO Terminal</p>

<p align="center"><a href="https://git.takina.io/gtfo/BashTerm">GitLab (Source)</a> | <a href="https://github.com/sebun1/GTFO.BashTerm">GitHub (Mirror, Public Issues)</a> | <a href="https://thunderstore.io/c/gtfo/p/food/BashTerm/">Thunderstore (Release)</a></p>

<p align="center"><i>e.g. Type </i><code>P AMMO 69</code> <i>instead of </i><code>PING AMMOPACK_69</code></p>

---

<h3 align="center">Issues / Suggestions</h3>
<p align="center"><b>Please</b> open issues (for suggestions, bugs, etc.) <b>on the <a href="https://github.com/sebun1/GTFO.BashTerm">GitHub repo</a></b> as the GitLab repository is not open for public contributions, thank you!!</p>

# Table of Contents

[TOC]

# Aliases / Expansions

## Command Aliases

Several shorthand aliases of commands are added to make common operations faster. Some UNIX-equivalent commands are also mapped.

| Alias                       | GTFO Command       |
| --------------------------- | ------------------ |
| `LS` / `L`                  | `LIST`             |
| `LSU` / `LU`                | `LIST U`           |
| `CAT`                       | `READ`             |
| `UC`                        | `UPLINK_CONNECT`   |
| `RS` / `START`              | `REACTOR_STARTUP`  |
| `RSD` / `SHUT` / `SHUTDOWN` | `REACTOR_SHUTDOWN` |
| `UV`[^1]                    | `UPLINK_VERIFY`    |
| `RV`[^1]                    | `REACTOR_VERIFY`   |
| `P`                         | `PING`             |
| `Q`                         | `QUERY`            |
| `CLEAR`                     | `CLS`              |

## Zone Identifier Expansion

Specifically, the `LSU` alias expands the number argument to a zone identifier if applicable (it should only take one argument).

```
# Expansion on 49
LSU 49 ==> LSU E_49 ==> LIST U E_49

# No expansion
LSU AM ==> LIST U AM
```

i.e. `LSU 49` would be interpreted as `LSU E_49` = `LIST U E_49` (`LIST RESOURCE ZONE_49`)

This function can also be turned on for `LIST` itself in the config, expanding the first number argument encountered.

## Item Name Expansion / Concatenation

Let's be honest, typing `_` is not the most convenient thing in the world, so let's not do that.

Instead of entering something like `PING AMMOPACK_594`, BashTerm allows you to type `PING AMMO 594` instead.

This is achieved through two mechanism:

1. Concatenation
2. Name Expansion

**You might be thinking, "I don't freaking care how it works!", but understanding how BashTerm works will allow you to better utilize it.**

### Concatenation

Concatenation allows you to not have to type the underscore `_` ever[^3] when using `PING` and `QUERY`.

For example:

* `PING AMMOPACK 594` becomes `PING AMMOPACK_594`
* `PING KEY BLACK 113` becomes `PING KEY_BLACK_113`

### Name Expansion

```
COMMAND ARGUMENT_1 ARGUMENT_2 ...
```

Many objects (e.g. resources, objective items, etc.) have expansions defined in BashTerm. These expansions work on the *first argument* (`ARGUMENT_1`) when using the commands `PING` and `QUERY`.

Take the earlier example `PING AMMO 594` again. After execution, `AMMO` is translated to `AMMOPACK`, then concatenation is performed to get `PING AMMOPACK_594`.

GTFO only allows these two commands to be accompanied with one argument: the item of interest (unless you consider the `-T` option), so we automatically consider expansion when you enter more than one argument. (calling `PING` or `QUERY` with only one argument, however, will not trigger expansion).

----

*The following table lists all default expansion identifiers* (note that "`WORD+`" means **any** word starting with `WORD`, aka prefix matching):

| Identifier      | Expansion              |
| --------------- | ---------------------- |
| `MED+`          | `MEDIPACK`             |
| `TO+`           | `TOOL_REFILL`          |
| `AM+`           | `AMMOPACK`             |
| `DIS+`          | `DISINFECT_PACK`       |
| `TURB+`         | `FOG_TURBINE`          |
| `NHSU`          | `NEONATE_HSU`          |
| `BK+` / `BULK+` | `BULKHEAD_KEY`         |
| `BD+`           | `BULKHEAD_DC`          |
| `HIS+`          | `HISEC_CARGO`          |
| `LOCK+`         | `LOCKER`               |
| `SEC+` / `SD+`  | `SEC_DOOR`             |
| `NFR+`          | `NFRAME`               |
| `GEN+`          | `GENERATOR`            |
| `DISS`          | `DISINFECTION_STATION` |

## Customizing Aliases / Expansions

### General Syntax

Command aliases and object expansions can be customized by editing their respective fields in the configuration. The config follows a specific format:

*It is worth noting that aliases and expansions are essentially the same thing because they work the same way. The only difference between them in BashTerm is the argument(s) they apply to. "Aliases" only apply to commands, while "Object Expansions" apply to the arguments of commands*

```
# Custom Command Aliases
COMMAND, ALIAS1, ALIAS2: COMMAND2, CMD2_ALIAS1, CMD2_ALIAS2

# Custom Object Name Expansions
EXPANSION, SHORTHAND1, SHORTHAND2
```

*Alias* and *expansion* customization follow the same format, as shown above. Different **groups** are separated by a colon `:`, and **terms** within a **group** are separated by commas `,`. All definitions are case-insensitive (i.e. it doesn't matter if you capitalize), and extra spaces around **terms** are all trimmed automatically.

You can specify multiple aliases/expansions for a single command or object within a single **group**. The first term is the command/expansion for which you are defining aliases/shorthands for, while the rest of the terms in the **group** are all aliases/shorthands.

In the first group shown in the code block above (`COMMAND, ALIAS1, ALIAS2`), `ALIAS1` and `ALIAS2` are mapped to `COMMAND` and will be expanded to it.

```
# Example
ALIAS1 AMMO ==> COMMAND AMMO
```

### Escaping Characters

There might be cases where you want to include colons `:` or commas `,` in your definition. This can be easily achieve with an escape using the backslash `\`. When a backslash appears, the character right after it will be interpreted literally (including backslash `\` itself).

```
# Example Config
"\:_FORM/AT_", format3

# Interpreted Definition
FORMAT3 ==> ":_FORM/AT_"
```

The above example shows how the colon appears in the interpreted definition since it was escaped in the config file.

### Overriding Defaults

Any default mappings can be overridden; all that is needed is to provide the same alias in your configuration. If you want to override the default mapping of `AM+` to `AMMOPACK` to something like `AM+` to `NOT_AMMOPACK`, you would add the following **group** in the configuration.

```
not_ammopack, am+
```



# Raw Input `RAW` / `R`

If you ever encounter a situation where your commands are being interpreted unexpectedly (or you simply don't want it to be interpreted), pass your command to `RAW` or `R` to have it executed verbatim.

e.g. `RAW LSU 49` will be executed as `LSU 49` rather than `LIST U E_49`.

# Contributing

Any issues can be opened in the mirror repo on [GitHub](https://github.com/sebun1/GTFO.BashTerm).

This project is not currently planned for open contributions; if there are any specific ideas for contribution/collaboration, please DM me on Discord (`@uwufood`). Any suggestions can also be DMed, although opening an issue on GitHub is largely preferred.

# Roadmap
- [x] **Aliasing** (v0.1.0): Shorthands for commonly used commands or snippets
  - [x] **Customizable Aliases/Expansions** (v0.2.0): Config-defined command aliases and object name expansions
- [ ] **Objective Information**: Print the current objective information in the terminal
- [ ] **Better Autocomplete**: Pressing tab shows the different candidates if multiple matches exist
- [ ] **Better Prompt**: Update styling of command prompt (i.e. `\\ROOT\`) to include things like zone information
- [ ] **Pipe `|`**: Allows for piping command outputs to other commands (e.g. `LIST CELL | QUERY` which queries all cells returned by `LIST CELL`)
- [ ] **Advanced Filter Options**[^2]: Allows filtering multiple zones, etc. (e.g. zone 115|116|118 -> zone 115 or 116 or 118)
- [ ] **Command Preservation**[^2]: Preserve typed aliases in the terminal
  - [ ] **Command History Preservation**: Preserve the arrow-up history for commands for reuse

- [ ] **Better Help**: More detailed help and information on specific commands
- [ ] **BashTerm Help**: Help for BashTerm itself, directly in the terminal




[^1]: Since these aliases might be consider balance-breaking to some, there are options in the config to turn these mappings off. By default, however, they are enabled.

[^2]: These features may or may not be plausible/implemented

[^3]: Well, almost never.
