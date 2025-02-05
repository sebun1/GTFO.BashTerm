![Icon](.assets/icon.png)
<h1 align="center">BashTerm</h1>

<p align="center">An Improved GTFO Terminal</p>

# Aliases

Several shorthand aliases of commands are added to make common operations easier

| Alias                       | GTFO Command       |
| --------------------------- | ------------------ |
| `LS` / `L`                  | `LIST`             |
| `LSU` / `LU`                | `LIST U`           |
| `UC`                        | `UPLINK_CONNECT`   |
| `RS` / `START`              | `REACTOR_STARTUP`  |
| `RSD` / `SHUT` / `SHUTDOWN` | `REACTOR_SHUTDOWN` |
| `UV`[^1]                    | `UPLINK_VERIFY`    |
| `RV`[^1]                    | `REACTOR_VERIFY`   |
| `P`                         | `PING`             |
| `Q`                         | `QUERY`            |
| `CLEAR`                     | `CLS`              |

## `LSU` / `LU`

`LSU` specifically converts number-only filter to zone identifiers if applicable.

i.e. `LSU 49` would be interpreted as `LSU E_49` = `LIST U E_49`

## Item Name Expansion / Concatenation

Let's be honest, typing `_` is not the most convenient thing in the world, so let's not do that.

Instead of entering something like `PING AMMOPACK_594`, BashTerm allows you to type `PING AMMO 594` instead.

This is achieve through two mechanism:

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
COMMAND ARGUMENT_1 ARGUMENT_2
```

Many objects (e.g. resources, object items, etc.) include expansions. These expansions works on the *first argument* (`ARGUMENT_1`) when using the commands `PING` and `QUERY`. GTFO only allows these two commands to be accompanied with one argument, the item of interest (unless you consider the `-T` option).

Take the earlier example `PING AMMO 594` again. After execution, `AMMO` is translated to `AMMOPACK`, then concatenation is performed to get `PING AMMOPACK_594`

----

*The following table lists all expansion identifiers* (note that "`WORD`+" means **any** word starting with `WORD`):

| Identifier      | Expansion              |
| --------------- | ---------------------- |
| `MED`+          | `MEDIPACK`             |
| `TO`+           | `TOOL_REFILL`          |
| `AM`+           | `AMMOPACK`             |
| `DIS`+          | `DISINFECTION_PACK`    |
| `TURB`+         | `FOG_TURBINE`          |
| `NHSU`          | `NEONATE_HSU`          |
| `BK`+ / `BULK`+ | `BULKHEAD_KEY`         |
| `BD`+           | `BULKHEAD_DC`          |
| `LOCK`+         | `LOCKER`               |
| `SEC`+ / `SD`+  | `SEC_DOOR`             |
| `GEN`+          | `GENERATOR`            |
| `DISS`          | `DISINFECTION_STATION` |

# Contributing

This project current is not planning for open contributions, if there are any specific ideas for collaboration, please DM me on discord.

Feel free to fire any suggestions on discord @`uwufood`

# Roadmap
- [x] **Aliasing**: Shorthands for commonly used commands or snippets
- [ ] **Better Autocomplete**: Pressing tab shows the different candidates if multiple matches exist
- [ ] **Better Prompt**: Update styling of command prompt (i.e. `\\ROOT\`) to include things like zone information
- [ ] **Advanced Filter Options**[^2]: Allows filtering multiple zones, etc. (e.g. zone 115|116|118 -> zone 115 or 116 or 118)
- [ ] **Command Preservation**[^2]: Preserve typed aliases in the terminal
- [ ] **Contextual Objective Information**: Display information such as uplink address in the terminal
- [ ] **Better Help**: More detailed help and information on specific commands
- [ ] **BashTerm Help**: Help for BashTerm itself, directly in the terminal




[^1]: Since these aliases might be consider balance-breaking to some, there are options in the config to turn these mappings off. By default, however, they are enabled.
[^2]: These features may or may not be plausible/implemented
[^3]: Well, almost never.
