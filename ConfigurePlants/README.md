# Configure Plants

Ever wanted to change the most simple values on plants in Valheim? This is the mod you can easily use!

## Features

This mod does not use any .cfg file to configure it. Instead it does provide configuration via YAML files:

1. Ability to simply write out the currently in-game loaded configuration of all Plants (following the `Plant` Valheim
   type).
2. Ability to read 1 or many files that contain configuration values in the same model that can be written, it will then
   change the in-game values of the plants while loading into the world.

YAML file contents are `ServerSync`ed.

### Supported config values for plants

The implementation currently provides config changes for these values:

| field name           | description                                                     | type   | example              |
|----------------------|-----------------------------------------------------------------|--------|----------------------|
| name                 | the in-game name, usually using localization tokens             | string | `$prop_pine_sapling` | 
| growTime             | values of seconds this plants need for growing                  | float  | 3000                 |
| growRadius           | meters of space a plants requires around it to be able to grow  | float  | 2                    |
| minScale             | minimum scale for grown plant                                   | float  | 1.5                  |
| maxScale             | maximum scale for grown plant                                   | float  | 2.5                  |
| needCultivatedGround | plant can only grow be planted on previously cultivated ground  | bool   | false                |
| destroyIfCantGrow    | plant will be destroyed if any condition not met during growing | bool   | true                 |

### Writing plant config defaults

To write the defaults from how they were loaded into the game you can use the console command:

```
configure_plants_print_defaults
```

The file containing all the defaults is then written to a simple YAML file inside your BepInEx config folder:
```
.../BepInEx/config/FixItFelix.ConfigurePlants.defaults.yaml
```

#### Example file contents

This is a shorted example for what you get from vanilla Valheim:

```yaml
PineTree_Sapling:
  name: $prop_pine_sapling
  growTime: 3000
  growRadius: 2
  minScale: 1.5
  maxScale: 2.5
  needCultivatedGround: false
  destroyIfCantGrow: true
FirTree_Sapling:
  name: $prop_fir_sapling
  growTime: 3000
  growRadius: 2
  minScale: 1
  maxScale: 2.5
  needCultivatedGround: false
  destroyIfCantGrow: true
sapling_onion:
  name: $piece_sapling_onion
  growTime: 4000
  growRadius: 0.5
  minScale: 0.9
  maxScale: 1.1
  needCultivatedGround: true
  destroyIfCantGrow: true
```

You can use the written data to simply create a custom config file by copying and changing file name accordingly.

#### How to use Valheim console

On how to use the Valheim console, there are many articles that help you enabling the console in details.
See for example the [Valheim wiki](https://valheim.fandom.com/wiki/Console_Commands).
To make it short: add the parameter `-console` to calling your `valheim.exe` file -> `valheim.exe -console` (this can be
done in Steam, too)

### Reading your custom config

The mod can read any YAML file that follows this naming pattern: `FixItFelix.ConfigurePlants.custom.*.yaml`

The schema of the file needs to follow the schema of the output, see above.

# Miscellaneous

<details>
  <summary>Attributions</summary>

* https://valheim.thunderstore.io/package/ValheimModding/Jotunn/
* icon -> https://www.flaticon.com/free-icons/plant

</details>

<details>
  <summary>Contact</summary>

* https://github.com/FelixReuthlinger/ConfigurePlants
* Discord: Flux#0062 (you can find me around some of the Valheim modding discords, too)

</details>
