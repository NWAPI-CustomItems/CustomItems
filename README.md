# CustomItems
This plugin adds some custom items to be used, this plugin depends of [CustomItem.API](https://github.com/NWAPI-CustomItems/API) to work.

# Configuration
Each custom item has its own configuration, the configuration of the custom items is in the ``CustomItems.yml`` file and will be where the plugin configuration is either if you set your plugin to global or [port].

### Spawn locations
The following list of locations are the only ones that are able to be used in the SpawnLocation configs for each item:
(Their names must be typed EXACTLY as they are listed, otherwise you will probably break your item config file)
```cs
Inside330
Inside330Chamber
Inside049Armory
Inside079Secondary
Inside096
Inside173Armory
Inside173Bottom
Inside173Connector
InsideEscapePrimary
InsideEscapeSecondary
InsideIntercom
InsideLczArmory
InsideLczCafe
InsideNukeArmory
InsideSurfaceNuke
Inside079First
Inside173Gate
Inside914
InsideGateA
InsideGateB
InsideGr18
InsideHczArmory
InsideHid
InsideHidLeft
InsideHidRight
InsideLczWc
InsideServersBottom
// Using this will make a random locker throughout the facility unless LockerZone is in a FacilityZone other than None.
InsideLocker
```
### FacilityZones
This are the values that FacilityZone enum can have
```cs
None
LightContainment
HeavyContainment
Entrance
Surface
Other
```
